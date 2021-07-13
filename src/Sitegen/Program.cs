using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HandlebarsDotNet;
using Sitegen.Models;
using Sitegen.Models.Config;
using Sitegen.Services;
using Sitegen.Services.MultiLanguage;
using Sitegen.Services.SingleLanguage;
using Sitegen.YamlDotNet;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sitegen
{
    /// <summary>
    /// Entry-point for `sitegen`. This is the main class, handling parsing of command line parameters and delegating
    /// the rest of the work to other parts of the application.
    /// </summary>
    public class Program
    {
        private readonly HandlebarsConverter handlebarsConverter;
        private readonly Config config;
        private readonly TopLevelConfig topLevelConfig;

        private List<BlogPostModel> allBlogPosts;
        private Dictionary<string, IEnumerable<IDictionary<string, object>>> groupedBlogPosts;

        /// <summary>
        /// Returns a string representation of <see cref="DateTime.Now"/>, including milliseconds.
        /// </summary>
        private static string NowWithMillis => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        private List<BlogPostModel> AllBlogPosts => allBlogPosts ??=
            GetBlogPostFiles()
                .Select(BlogPostConverter.ReadBlogPost)
                .OrderByDescending(p => p.Date)
                .ToList();

        private Dictionary<string, IEnumerable<IDictionary<string, object>>> GroupedBlogPosts => groupedBlogPosts ??=
            AllBlogPosts
                .GroupBy(bp => bp.Language)
                .ToDictionary(k => k.Key, v => v.Select(p => p.ToDictionary(config)));

        /// <summary>
        /// Entry-point. This is the first method that gets called by the .NET runtime.
        /// </summary>
        /// <param name="args">An array of command line arguments.</param>
        public static void Main(string[] args)
        {
            TopLevelConfig topLevelConfig = ReadConfig("config.yaml");
            var program = new Program(topLevelConfig);

            switch (args[0])
            {
                case "--build":
                    DateTime start = DateTime.Now;

                    program.ConvertPosts();
                    program.ConvertPages();
                    program.CopyStaticAssets();

                    DateTime end = DateTime.Now;

                    LogInfo($"Done rebuilding site in {(int) (end - start).TotalMilliseconds} ms");
                    Console.WriteLine();

                    break;

                case "--posts":
                    program.ConvertPosts();
                    break;

                case "--post":
                    var blogPostConverter = new BlogPostConverter(topLevelConfig, program.handlebarsConverter);

                    blogPostConverter.ProcessBlogPost(args[1]);
                    break;

                default:
                    if (args.Length == 2)
                    {
                        string sourcePath = args[0];
                        string targetPath = args[1];

                        program.ConvertHandlebarsFile(sourcePath, targetPath);
                    }
                    else
                    {
                        Console.WriteLine(
                            "Syntax: sitegen --build | --posts | --post <src-file> | <src-file> <target-file>");
                        Environment.Exit(1);
                    }

                    break;
            }
        }

        private Program(TopLevelConfig topLevelConfig)
        {
            config = topLevelConfig.Config;
            handlebarsConverter = new HandlebarsConverter(topLevelConfig);

            this.topLevelConfig = topLevelConfig;
        }

        public static TopLevelConfig ReadConfig(string path)
        {
            if (!File.Exists(path))
            {
                throw new ConfigurationException($"{path} does not exist");
            }

            var input = new StringReader(File.ReadAllText(path));

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithSerializationEventSupport()
                .Build();

            try
            {
                var config = deserializer.Deserialize<TopLevelConfig>(input);

                if (config == null)
                {
                    // While this logic might seem silly and irrelevant, it does serve a purpose. Empty YAML documents
                    // will yield a 'null' reference at this point because of how YamlDotNet works. We must have a valid
                    // non-null object we can use, so why not just instantiate a blank object at this point? If we do,
                    // our custom deserialization code powered by IDeserialized will not be executed. We choose the
                    // simple approach and make this an unsupported scenario altogether; it's very unlikely to cause any
                    // real-world problems anyway.
                    throw new ConfigurationException("config.yaml does not contain any settings");
                }

                return config;
            }
            catch (YamlException e)
            {
                Console.Write(Environment.NewLine + Environment.NewLine);

                Console.Error.WriteLine($"Error reading config.yaml: {e.Message}");
                Console.Error.WriteLine(e.InnerException?.Message);
                Environment.Exit(1);

                // Will never be reached, but must be added because of limitations in the C# language/compiler.
                return null;
            }
        }

        private void ConvertPosts()
        {
            // Pass 1: convert all Markdown files to .html
            var files = GetBlogPostFiles();
            var posts = ConvertPosts(files);

            // Pass 2: generate category listings for all categories used by these posts.
            CategoryPageCreator categoryPageCreator = config.MultipleLanguages switch
            {
                true => new MultiLanguageCategoryPageCreator(config, handlebarsConverter),
                false => new SingleLanguageCategoryPageCreator(config, handlebarsConverter)
            };

            categoryPageCreator.CreateCategoryPages(posts);
        }

        private IEnumerable<BlogPostModel> ConvertPosts(IEnumerable<string> files)
        {
            var posts = new List<BlogPostModel>();

            var blogPostConverter = new BlogPostConverter(topLevelConfig, handlebarsConverter);

            foreach (string postSourceFile in files)
            {
                BlogPostModel blogPost = blogPostConverter.ProcessBlogPost(postSourceFile);
                LogInfo($"Converted {postSourceFile} to HTML");

                if (blogPost != null)
                {
                    posts.Add(blogPost);
                }
            }

            return posts;
        }

        private string[] GetBlogPostFiles()
        {
            return Directory.GetFiles(topLevelConfig.Config.PostsDir, "*.md", SearchOption.AllDirectories);
        }

        private void ConvertPages()
        {
            var files = Directory.GetFiles(topLevelConfig.Config.SourceDir, "*.hbs", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                // file is e.g. src/sv/om/index.hbs at this stage. Make it be relative from the SourceDir, so that
                // we can create a similar folder structure in the target dir.
                string relativePath = MakeRelativePath(file);

                if (relativePath.StartsWith("/_"))
                {
                    //_layouts, _includes etc - they should not be included at this stage.
                    continue;
                }

                string targetPath = Path.ChangeExtension(
                    Path.Join(topLevelConfig.Config.OutputDir, relativePath),
                    ".html"
                );

                string targetDir = Path.GetDirectoryName(targetPath);

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                try
                {
                    ConvertHandlebarsFile(file, targetPath);
                }
                catch (HandlebarsParserException e)
                {
                    Console.Write(Environment.NewLine + Environment.NewLine);

                    Console.Error.WriteLine($"Error converting {file}. Details:");
                    Console.Error.Write(e);
                    return;
                }
            }
        }

        private void CopyStaticAssets()
        {
            var files = Directory.GetFiles(topLevelConfig.Config.SourceDir, "*", SearchOption.AllDirectories);
            var excludedExtensions = new List<string>
            {
                ".hbs",
                ".md"
            };

            foreach (string file in files)
            {
                if (excludedExtensions.Contains(Path.GetExtension(file)))
                {
                    continue;
                }

                // file is e.g. src/js/plugins.js at this stage. Make it be relative from the SourceDir, so that
                // we can create a similar folder structure in the target dir.
                string relativePath = MakeRelativePath(file);

                if (relativePath.StartsWith("/_"))
                {
                    //_layouts, _includes etc - they should not be included at this stage.
                    continue;
                }

                string targetPath = Path.Join(topLevelConfig.Config.OutputDir, relativePath);
                string targetDir = Path.GetDirectoryName(targetPath);

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                File.Copy(file, targetPath, overwrite: true);
                LogDebug($"Copied {file} to {targetPath}");
            }
        }

        private string MakeRelativePath(string file)
        {
            // The approach below is probably not the best way in the world to do this, but it works for simple,
            // relative paths and will do for now.
            return file.StartsWith(topLevelConfig.Config.SourceDir)
                ? file.Substring(topLevelConfig.Config.SourceDir.Length)
                : file;
        }

        /// <summary>
        /// Converts a Handlebars (.hbs) file to HTML (.html) format.
        /// </summary>
        /// <param name="sourcePath">the path to the source .hbs file</param>
        /// <param name="targetPath">the path to the target .html file</param>
        private void ConvertHandlebarsFile(string sourcePath, string targetPath)
        {
            var extraData = new Dictionary<string, object>
            {
                { "all_blog_posts", AllBlogPosts },
                { "blog_posts", GroupedBlogPosts }
            };

            string source = File.ReadAllText(sourcePath);
            string result = handlebarsConverter.Convert(source, extraData);

            File.WriteAllText(targetPath, result);

            LogInfo($"Converted {sourcePath} to {targetPath}");
        }

        private static void LogInfo(string message)
        {
            if (Console.IsOutputRedirected)
            {
                Console.WriteLine($"[{NowWithMillis}] {message}");
            }
            else
            {
                Console.Write($"[{NowWithMillis}] {message}".PadRight(Console.WindowWidth - 1));
                Console.Write("\r");
            }
        }

        private static void LogDebug(string message)
        {
            if (Console.IsOutputRedirected)
            {
                Console.WriteLine($"[{NowWithMillis}] {message}");
            }
            else
            {
                Console.Write($"[{NowWithMillis}] {message}".PadRight(Console.WindowWidth - 1));
                Console.Write("\r");
            }
        }
    }
}
