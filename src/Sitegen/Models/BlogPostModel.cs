using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Sitegen.Services;
using YamlDotNet.Serialization;
using static Sitegen.UrlUtils;

namespace Sitegen.Models
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

        [UsedImplicitly]
        public LineBreaks? LineBreaks { get; set; }

        // Ignore this property as it will not be part of the YAML document
        [YamlIgnore]
        public string Body { get; set; }

        private string Excerpt => Body.Split(Environment.NewLine + Environment.NewLine, 2).FirstOrDefault();

        public IDictionary<string, object> ToDictionary(Config.Config config)
        {
            string languagePrefix = config.MultipleLanguages switch
            {
                true => Language,
                false => ""
            };

            // This is the "presentation layer" for this model object. The field names below are what the .hbs
            // templates will see.
            return new Dictionary<string, object>
            {
                { "title", Title },
                { "date", Date.ToString("MMM d, yyyy") },
                { "date_iso", Date.ToString("yyyy-MM-dd") },
                { "body", MarkdownConverter.ToHtml(Body, LineBreaks ?? config.LineBreaks) },
                { "excerpt", MarkdownConverter.ToHtml(Excerpt, LineBreaks ?? config.LineBreaks) },
                { "language", Language },

                {
                    "link", Path.Join(
                        "/",
                        languagePrefix,
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
                        { "name", c },
                        { "slug", Path.Join(languagePrefix, Slugify(c)) }
                    })
                }
            };
        }

        public override string ToString()
        {
            return $"{nameof(Title)}: {Title}, " +
                   $"{nameof(Date)}: {Date}, " +
                   $"{nameof(Categories)}: {Categories}, " +
                   $"{nameof(Language)}: {Language}, " +
                   $"{nameof(Excerpt)}: {Excerpt}";
        }
    }
}
