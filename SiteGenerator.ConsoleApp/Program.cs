using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using HandlebarsDotNet;
using HandlebarsDotNet.Compiler;
using Markdig;
using SiteGenerator.ConsoleApp.Models.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SiteGenerator.ConsoleApp
{
    public class Program
    {
        private string SourcePath { get; }
        private string TargetPath { get; }
        private TopLevelConfig TopLevelConfig { get; }
        private Config Config => TopLevelConfig.Config;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Syntax: sitegen <src-file> <target-file>");
                Environment.Exit(1);
            }

            var input = new StringReader(File.ReadAllText("config.yaml"));

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var config = deserializer.Deserialize<TopLevelConfig>(input);

            ValidateConfig(config);

            new Program(args[0], args[1], config)
                .Run();
        }

        private static void ValidateConfig(TopLevelConfig config)
        {
            // Set default values for config settings which have not been provided
            config.Config ??= new Config();
            config.Config.SourceDir ??= "src";
        }

        private Program(string sourcePath, string targetPath, TopLevelConfig topLevelConfig)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
            TopLevelConfig = topLevelConfig;
        }

        private void Run()
        {
            string source = File.ReadAllText(SourcePath);

            Handlebars.RegisterHelper("include", IncludeHelper);
            Handlebars.RegisterHelper("markdown", MarkdownHelper);
            Handlebars.RegisterHelper("set", SetHelper);

            var template = Handlebars.Compile(source);

            var data = new ExpandoObject() as IDictionary<string, object>;

            data.Add("now", DateTime.Now);
            data.Add("site", TopLevelConfig.Site);

            string result = template(data);

            File.WriteAllText(TargetPath, result);
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

            var template = Handlebars.Compile(templateSource);
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
