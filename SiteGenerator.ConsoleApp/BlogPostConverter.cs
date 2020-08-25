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

        public PostModel ProcessBlogPost(string path)
        {
            PostModel post = ReadBlogPost(path);
            ConvertToHtml(post);

            return post;
        }

        public static PostModel ReadBlogPost(string path)
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

            var post = deserializer.Deserialize<PostModel>(frontmatterYaml);
            post.Body = blogPostBody;
            return post;
        }

        private void ConvertToHtml(PostModel post)
        {
            string layout = File.ReadAllText(Path.Join(topLevelConfig.Config.LayoutsDir, post.Layout + ".hbs"));

            string result = handlebarsConverter.Convert(layout, new Dictionary<string, object>
            {
                {"post", post.ToDictionary()}
            });

            foreach (string postCategory in post.Categories)
            {
                string outputDir = Path.Join(Config.OutputDir, Slugify(postCategory), post.Date.Year.ToString(),
                    post.Date.Month.ToString(), post.Date.Day.ToString(), Slugify(post.Title));
                string outputPath = Path.Join(outputDir, "index.html");

                Directory.CreateDirectory(outputDir);
                File.WriteAllText(outputPath, result);
            }
        }
    }
}
