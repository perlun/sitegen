using System.Collections.Generic;
using System.IO;
using SiteGenerator.ConsoleApp.Models;
using SiteGenerator.ConsoleApp.Models.Config;
using static SiteGenerator.ConsoleApp.UrlUtils;

namespace SiteGenerator.ConsoleApp.Services.SingleLanguage
{
    /// <summary>
    /// Implementation of <see cref="CategoryPageCreator"/> for multi-language scenarios.
    ///
    /// Single-language scenarios are simple; no language identifier is needed in the URLs, and the categories don't
    /// need to be grouped by language.
    /// </summary>
    internal class SingleLanguageCategoryPageCreator : CategoryPageCreator
    {
        public SingleLanguageCategoryPageCreator(Config config, HandlebarsConverter handlebarsConverter) :
            base(config, handlebarsConverter)
        {
        }

        public override void CreateCategoryPages(IEnumerable<BlogPostModel> allPosts)
        {
            var postsByCategory = new Dictionary<string, List<BlogPostModel>>();

            foreach (BlogPostModel postModel in allPosts)
            {
                foreach (string category in postModel.Categories)
                {
                    if (!postsByCategory.ContainsKey(category))
                    {
                        postsByCategory[category] = new List<BlogPostModel>();
                    }

                    postsByCategory[category].Add(postModel);
                }
            }

            foreach ((string category, var categoryPosts) in postsByCategory)
            {
                string targetPath = Path.Join(
                    Config.OutputDir,
                    Slugify(category),
                    "index.html"
                );

                WriteCategoryPage(category, categoryPosts, targetPath);
            }
        }
    }
}
