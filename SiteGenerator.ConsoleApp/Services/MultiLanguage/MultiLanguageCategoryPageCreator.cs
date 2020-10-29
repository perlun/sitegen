using System.Collections.Generic;
using System.IO;
using SiteGenerator.ConsoleApp.Models;
using SiteGenerator.ConsoleApp.Models.Config;
using static SiteGenerator.ConsoleApp.UrlUtils;

namespace SiteGenerator.ConsoleApp.Services.MultiLanguage
{
    /// <summary>
    /// Implementation of <see cref="CategoryPageCreator"/> for multi-language scenarios.
    ///
    /// In multi-language scenarios, the category pages uses the language identifier as the prefix, before the category
    /// slug.
    /// </summary>
    internal class MultiLanguageCategoryPageCreator : CategoryPageCreator
    {
        public MultiLanguageCategoryPageCreator(Config config, HandlebarsConverter handlebarsConverter) :
            base(config, handlebarsConverter)
        {
        }

        public override void CreateCategoryPages(IEnumerable<BlogPostModel> allPosts)
        {
            var postsByLanguageAndCategory = new Dictionary<string, Dictionary<string, List<BlogPostModel>>>();

            // Pass 1: Loop over all blog posts and build up the required data model.
            foreach (BlogPostModel postModel in allPosts)
            {
                var postsByCategory = postsByLanguageAndCategory.GetValueOrDefault(
                    postModel.Language,
                    new Dictionary<string, List<BlogPostModel>>()
                );

                if (!postsByLanguageAndCategory.ContainsKey(postModel.Language))
                {
                    postsByLanguageAndCategory[postModel.Language] = postsByCategory;
                }

                foreach (string category in postModel.Categories)
                {
                    if (!postsByCategory!.ContainsKey(category))
                    {
                        postsByCategory[category] = new List<BlogPostModel>();
                    }

                    postsByCategory[category].Add(postModel);
                }
            }

            // Pass 2: Loop over the created data model and create the category pages for all language/category
            // combinations in existence.
            foreach (string language in postsByLanguageAndCategory.Keys)
            {
                var postsByCategory = postsByLanguageAndCategory[language];

                foreach ((string category, var categoryPosts) in postsByCategory)
                {
                    string targetPath = Path.Join(
                        Config.OutputDir,
                        language,
                        Slugify(category),
                        "index.html"
                    );

                    WriteCategoryPage(category, categoryPosts, targetPath);
                }
            }
        }
    }
}
