using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using SiteGenerator.ConsoleApp.Services;
using YamlDotNet.Serialization;
using static SiteGenerator.ConsoleApp.UrlUtils;

namespace SiteGenerator.ConsoleApp.Models
{
    /// <summary>
    /// Representation of an individual blog post, as provided in an individual `.md` file.
    ///
    /// The header is the YAML "front matter", as separated by `---` + LF before and after the header fields. Each field
    /// populates the properties defined in this class.
    ///
    /// The final part (after the second `---`) is stored as-is (as raw Markdown) in the `Body` property.
    /// </summary>
    [UsedImplicitly]
    public class BlogPostModel
    {
        [UsedImplicitly]
        public string Title { get; set; }

        [UsedImplicitly]
        public DateTime Date { get; set; }

        [UsedImplicitly]
        public string[] Categories { get; set; }

        [UsedImplicitly]
        public string Layout { get; set; }

        /// <summary>
        /// Two-letter ISO-639-1 language code (e.g. sv, en, de etc)
        /// </summary>
        [UsedImplicitly]
        public string Language { get; set; }

        // Ignore this property as it will not be part of the YAML document
        [YamlIgnore]
        public string Body { get; set; }

        private string Excerpt => Body.Split(Environment.NewLine + Environment.NewLine, 2).FirstOrDefault();

        public IDictionary<string, object> ToDictionary()
        {
            // This is the "presentation layer" for this model object. The field names below are what the .hbs
            // templates will see.
            return new Dictionary<string, object>
            {
                { "title", Title },
                { "date", Date.ToString("MMM d, yyyy") },
                { "date_iso", Date.ToString("yyyy-MM-dd") },
                { "body", MarkdownConverter.ToHtml(Body) },
                { "excerpt", MarkdownConverter.ToHtml(Excerpt) },

                {
                    "link", Path.Join(
                        "/",
                        Slugify(Categories.First()),
                        Date.Year.ToString(),
                        Date.Month.ToString(),
                        Date.Day.ToString(),
                        Slugify(Title)
                    )
                },

                {
                    "categories", Categories.Select(c => new Dictionary<string, string>
                    {
                        {"name", c},
                        {"slug", Slugify(c)}
                    })
                }
            };
        }
    }
}
