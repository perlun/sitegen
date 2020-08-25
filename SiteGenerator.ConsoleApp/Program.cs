using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiteGenerator.ConsoleApp.Models;
using SiteGenerator.ConsoleApp.Models.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static SiteGenerator.ConsoleApp.UrlUtils;

namespace SiteGenerator.ConsoleApp
{
    public class Program
    {
        private readonly HandlebarsConverter handlebarsConverter;
        private readonly Config config;
        private readonly TopLevelConfig topLevelConfig;

        /// <summary>
        /// Returns a string representation of <see cref="DateTime.Now"/>, including milliseconds.
        /// </summary>
        private static string NowWithMillis => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        public static void Main(string[] args)
        {
            TopLevelConfig topLevelConfig = ReadConfig();
            var program = new Program(topLevelConfig);

            if (args[0] == "--post")
            {
                var blogPostConverter = new BlogPostConverter(topLevelConfig, program.handlebarsConverter);

                blogPostConverter.ProcessBlogPost(args[1]);
            }
            else if (args[0] == "--posts")
            {
                // Pass 1: convert all Markdown files to .html
                var files = Directory.GetFiles(topLevelConfig.Config.PostsDir, "*.md");
                var posts = program.ConvertPosts(files);

                // Pass 2: generate category listings for all categories used by these posts.
                program.CreateCategoryPages(posts);
            }
            else if (args.Length == 2)
            {
                string sourcePath = args[0];
                string targetPath = args[1];

                program.ConvertHandlebarsFile(sourcePath, targetPath);
            }
            else
            {
                Console.WriteLine(
                    "Syntax: sitegen --post <src-file> | <src-file> <target-file> | --posts <src-file1> <src-file2> <...>");
                Environment.Exit(1);
            }
        }

        private static void LogInfo(string message)
        {
            Console.WriteLine($"[{NowWithMillis}] {message}");
        }

        private Program(TopLevelConfig topLevelConfig)
        {
            config = topLevelConfig.Config;
            handlebarsConverter = new HandlebarsConverter(topLevelConfig);

            this.topLevelConfig = topLevelConfig;
        }

        private static TopLevelConfig ReadConfig()
        {
            var input = new StringReader(File.ReadAllText("config.yaml"));

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var config = deserializer.Deserialize<TopLevelConfig>(input);

            ValidateConfig(config);
            return config;
        }

        private static void ValidateConfig(TopLevelConfig config)
        {
            // Set default values for config settings which have not been provided
            config.Config ??= new Config();
            config.Config.SourceDir ??= "src";
            config.Config.LayoutsDir ??= Path.Join(config.Config.SourceDir, "_layouts");
            config.Config.OutputDir ??= "out";
            config.Config.PostsDir ??= "src/_posts";
        }

        private IEnumerable<PostModel> ConvertPosts(IEnumerable<string> files)
        {
            var posts = new List<PostModel>();

            var blogPostConverter = new BlogPostConverter(topLevelConfig, handlebarsConverter);

            foreach (string postSourceFile in files)
            {
                var post = blogPostConverter.ProcessBlogPost(postSourceFile);
                LogInfo($"Converted {postSourceFile} to HTML");

                posts.Add(post);
            }

            return posts;
        }

        /// <summary>
        /// Converts a Handlebars (.hbs) file to HTML (.html) format.
        /// </summary>
        /// <param name="sourcePath">the path to the source .hbs file</param>
        /// <param name="targetPath">the path to the target .html file</param>
        private void ConvertHandlebarsFile(string sourcePath, string targetPath)
        {
            var blogPosts = Directory.GetFiles(config.PostsDir, "*.md")
                .Select(BlogPostConverter.ReadBlogPost)
                .OrderByDescending(p => p.Date)
                .Select(p => p.ToDictionary());

            var extraData = new Dictionary<string, object>
            {
                {"blog_posts", blogPosts}
            };

            string source = File.ReadAllText(sourcePath);
            string result = handlebarsConverter.Convert(source, extraData);

            File.WriteAllText(targetPath, result);
        }

        private void CreateCategoryPages(IEnumerable<PostModel> allPosts)
        {
            var postsByCategory = new Dictionary<string, List<PostModel>>();

            foreach (PostModel postModel in allPosts)
            {
                foreach (string category in postModel.Categories)
                {
                    if (!postsByCategory.ContainsKey(category))
                    {
                        postsByCategory[category] = new List<PostModel>();
                    }

                    postsByCategory[category].Add(postModel);
                }
            }

            foreach ((string category, var categoryPosts) in postsByCategory)
            {
                WriteCategoryPage(category, categoryPosts);
            }
        }

        private void WriteCategoryPage(string category, List<PostModel> categoryPosts)
        {
            string sourcePath = Path.Join(config.LayoutsDir, "category_archive.hbs");
            string targetPath = Path.Join(config.OutputDir, Slugify(category), "index.html");

            var extraData = new Dictionary<string, object>
            {
                {"category_posts", categoryPosts.Select(p => p.ToDictionary())},
                {"category_name", category}
            };

            string source = File.ReadAllText(sourcePath);
            string result = handlebarsConverter.Convert(source, extraData);

            File.WriteAllText(targetPath, result);
        }
    }
}
