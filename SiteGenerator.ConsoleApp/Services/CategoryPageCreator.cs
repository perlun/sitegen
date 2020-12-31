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

        /// <param name="language">The language being used, or `null` if running in non-multi-language mode.</param>
        /// <param name="category">The name of the category whose page is being generated.</param>
        /// <param name="categoryPosts">A list of the posts in this category.</param>
        /// <param name="targetPath">The path to the output `.html` file.</param>
        protected void WriteCategoryPage(string language, string category, List<BlogPostModel> categoryPosts, string targetPath)
        {
            string sourcePath = Path.Join(Config.LayoutsDir, "category_archive.hbs");

            var extraData = new Dictionary<string, object>
            {
                { "category_posts", categoryPosts.Select(p => p.ToDictionary(Config)) },
                { "category_name", category },
                { "language", language }
            };

            string source = File.ReadAllText(sourcePath);
            string result = handlebarsConverter.Convert(source, extraData);

            File.WriteAllText(targetPath, result);
        }
    }
}
