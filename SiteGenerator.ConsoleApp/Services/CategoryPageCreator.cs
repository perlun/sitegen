using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiteGenerator.ConsoleApp.Models;
using SiteGenerator.ConsoleApp.Models.Config;

namespace SiteGenerator.ConsoleApp.Services
{
    internal abstract class CategoryPageCreator
    {
        protected Config Config { get; }

        private readonly HandlebarsConverter handlebarsConverter;

        protected CategoryPageCreator(Config config, HandlebarsConverter handlebarsConverter)
        {
            this.handlebarsConverter = handlebarsConverter;

            Config = config;
        }

        /// <summary>
        /// Creates category pages for all the categories referred to in the provided list of blog posts.
        /// </summary>
        /// <param name="allPosts">An enumerable of blog posts.</param>
        public abstract void CreateCategoryPages(IEnumerable<BlogPostModel> allPosts);

        protected void WriteCategoryPage(string category, List<BlogPostModel> categoryPosts, string targetPath)
        {
            string sourcePath = Path.Join(Config.LayoutsDir, "category_archive.hbs");

            var extraData = new Dictionary<string, object>
            {
                { "category_posts", categoryPosts.Select(p => p.ToDictionary(Config)) },
                { "category_name", category }
            };

            string source = File.ReadAllText(sourcePath);
            string result = handlebarsConverter.Convert(source, extraData);

            File.WriteAllText(targetPath, result);
        }
    }
}
