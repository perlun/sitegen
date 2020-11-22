using System;
using System.Collections.Generic;
using System.IO;
using SiteGenerator.ConsoleApp.Models;
using SiteGenerator.ConsoleApp.Models.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static SiteGenerator.ConsoleApp.UrlUtils;

namespace SiteGenerator.ConsoleApp
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
            ConvertToHtml(blogPost);

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

            var post = deserializer.Deserialize<BlogPostModel>(frontmatterYaml);
            post.Body = blogPostBody;
            return post;
        }

        private void ConvertToHtml(BlogPostModel blogPost)
        {
            string layout = File.ReadAllText(Path.Join(topLevelConfig.Config.LayoutsDir, blogPost.Layout + ".hbs"));

            string result = handlebarsConverter.Convert(layout, new Dictionary<string, object>
            {
                {"post", blogPost.ToDictionary(topLevelConfig.Config)}
            });

            foreach (string postCategory in blogPost.Categories)
            {
                string outputDir = Path.Join(Config.OutputDir, Slugify(postCategory), blogPost.Date.Year.ToString(),
                    blogPost.Date.Month.ToString(), blogPost.Date.Day.ToString(), Slugify(blogPost.Title));
                string outputPath = Path.Join(outputDir, "index.html");

                Directory.CreateDirectory(outputDir);
                File.WriteAllText(outputPath, result);
            }
        }
    }
}
