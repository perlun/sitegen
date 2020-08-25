using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using HandlebarsDotNet;
using HandlebarsDotNet.Compiler;
using Markdig;
using SiteGenerator.ConsoleApp.Models.Config;

namespace SiteGenerator.ConsoleApp
{
    /// <summary>
    /// Converts Handlebars (.hbs) content to HTML (.html)
    /// </summary>
    public class HandlebarsConverter
    {
        private readonly IHandlebars handlebars;
        private readonly TopLevelConfig topLevelConfig;

        private Config Config => topLevelConfig.Config;

        public HandlebarsConverter(TopLevelConfig topLevelConfig)
        {
            this.topLevelConfig = topLevelConfig;

            handlebars = Handlebars.Create();

            handlebars.RegisterHelper("include", IncludeHelper);
            handlebars.RegisterHelper("markdown", MarkdownHelper);
            handlebars.RegisterHelper("set", SetHelper);
        }

        public string Convert(string source, IDictionary<string,object> extraData = null)
        {
            var template = handlebars.Compile(source);

            var data = new ExpandoObject() as IDictionary<string, object>;

            data.Add("now", DateTime.Now);
            data.Add("site", topLevelConfig.Site);

            foreach ((string key, object value) in extraData ?? new Dictionary<string, object>())
            {
                data.Add(key, value);
            }

            return template(data);
        }

        private void IncludeHelper(TextWriter writer, dynamic context, object[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new HandlebarsException("{{include}} helper must have exactly one argument");
            }

            if (!(parameters[0] is string fileName))
            {
                throw new HandlebarsException("{{include}} expected string parameter, not " +
                                              parameters[0].GetType().Name);
            }

            string templateSource = File.ReadAllText(Path.Join(Config.SourceDir, fileName));

            var template = handlebars.Compile(templateSource);
            string result = template(context);

            writer.WriteSafeString(result);
        }

        private static void MarkdownHelper(TextWriter writer, HelperOptions options, dynamic context, object[] arguments)
        {
            var stringWriter = new StringWriter();
            options.Template(stringWriter, context);

            string html = Markdown.ToHtml(stringWriter.ToString());

            writer.WriteSafeString(html);
        }

        private static void SetHelper(TextWriter writer, dynamic dynamicContext, object[] parameters)
        {
            var context = (IDictionary<string, object>) dynamicContext;

            if (parameters.Length == 0)
            {
                throw new HandlebarsException("{{set}} helper must have at least one argument");
            }

            foreach (object parameter in parameters)
            {
                if (!(parameter is HashParameterDictionary dictionary))
                {
                    throw new HandlebarsException("{{set}} parameter must use key=value notation");
                }

                foreach ((string key, object value) in dictionary)
                {
                    context.Add(key, value);
                }
            }
        }
    }
}
