using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using HandlebarsDotNet;
using HandlebarsDotNet.Compiler;
using SiteGenerator.ConsoleApp.Models.Config;
using SiteGenerator.ConsoleApp.Services;

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
            handlebars.RegisterHelper("ifeq", IfEqHelper);
        }

        public string Convert(string source, IDictionary<string, object> extraData = null)
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

        private void MarkdownHelper(TextWriter writer, HelperOptions options, dynamic context,
            object[] arguments)
        {
            var stringWriter = new StringWriter();
            options.Template(stringWriter, context);

            string html = MarkdownConverter.ToHtml(stringWriter.ToString(), topLevelConfig.Config.LineBreaks!.Value);

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

        /// <summary>
        /// `{{#ifeq}}` block helper.
        ///
        /// Use like this:
        ///
        /// ```
        /// {{#ifeq language 'foo'}}
        /// Content shown if the variable equals 'foo'
        /// {{/else}}
        /// Optional content shown if the comparison is not true.
        /// {{/ifeq}}
        /// ```
        /// </summary>
        /// <param name="output">The TextWriter to write the output to.</param>
        /// <param name="options">The HelperOptions to use.</param>
        /// <param name="context">The context, where parameters can have been defined.</param>
        /// <param name="arguments">The arguments, as provided in the Handlebars source file.</param>
        /// <exception cref="HandlebarsException">If the number of arguments does not equal two.</exception>
        private static void IfEqHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2)
            {
                throw new HandlebarsException("{{ifeq}} helper must have exactly two arguments");
            }

            var left = arguments[0] as string;
            var right = arguments[1] as string;

            if (left == right)
            {
                options.Template(output, null);
            }
            else
            {
                options.Inverse(output, null);
            }
        }
    }
}
