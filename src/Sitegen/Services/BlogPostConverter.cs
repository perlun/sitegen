using System;
using System.Collections.Generic;
using System.IO;
using Sitegen.Models;
using Sitegen.Models.Config;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static Sitegen.UrlUtils;

namespace Sitegen.Services
{
    public class BlogPostConverter
    {
        private readonly TopLevelConfig topLevelConfig;
        private readonly HandlebarsConverter handlebarsConverter;
        private Config Config => topLevelConfig.Config;

        public BlogPostConverter(TopLevelConfig topLevelConfig, HandlebarsConverter handlebarsConverter)
        {
            this.topLevelConfig = topLevelConfig;
            this.handlebarsConverter = handlebarsConverter;
        }

        public BlogPostModel ProcessBlogPost(string path)
        {
            BlogPostModel blogPost = ReadBlogPost(path);

            if (blogPost != null)
            {
                ConvertToHtml(blogPost);
            }

            return blogPost;
        }

        public static BlogPostModel ReadBlogPost(string path)
        {
            string blogPostWithFrontmatter = File.ReadAllText(path);

            var parts = blogPostWithFrontmatter.Split("---" + Environment.NewLine, 2,
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                throw new PostFormatException(
                    $"Blog post {path} is expected to contain exactly two parts, not {parts.Length}");
            }

            string frontmatterYaml = parts[0];
            string blogPostBody = parts[1];

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            try
            {
                var post = deserializer.Deserialize<BlogPostModel>(frontmatterYaml);
                post.Body = blogPostBody;
                return post;
            }
            catch (YamlException e)
            {
                Console.Write(Environment.NewLine + Environment.NewLine);

                Console.Error.WriteLine($"Error converting {path}. Details:");
                Console.Error.Write(e);
                return null;
            }
        }

        private void ConvertToHtml(BlogPostModel blogPost)
        {
            string layout = File.ReadAllText(Path.Join(topLevelConfig.Config.LayoutsDir, blogPost.Layout + ".hbs"));

            string result = handlebarsConverter.Convert(layout, new Dictionary<string, object>
            {
                { "post", blogPost.ToDictionary(topLevelConfig.Config) }
            });

            foreach (string postCategory in blogPost.Categories)
            {
                string languagePrefix = Config.MultipleLanguages switch
                {
                    true => blogPost.Language,
                    false => ""
                };

                string outputDir = Path.Join(
                    Config.OutputDir, languagePrefix, Slugify(postCategory),
                    blogPost.Date.Year.ToString(), blogPost.Date.Month.ToString(), blogPost.Date.Day.ToString(),
                    Slugify(blogPost.Title)
                );

                string outputPath = Path.Join(outputDir, "index.html");

                Directory.CreateDirectory(outputDir);
                File.WriteAllText(outputPath, result);
            }
        }
    }
}
