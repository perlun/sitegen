using System;
using System.IO;
using HandlebarsDotNet;
using Nett;

namespace SiteGenerator.ConsoleApp
{
    public class Config
    {
        [TomlMember(Key = "source_dir")]
        public string SourceDir { get; set; }
    }

    public class Site
    {
        [TomlMember(Key = "title")]
        public string Title { get; set; }
    }

    public class TopLevelConfig
    {
        [TomlMember(Key = "config")]
        public Config Config { get; set; }

        [TomlMember(Key = "site")]
        public Site Site { get; set; }
    }

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

            var config = Toml.ReadFile<TopLevelConfig>("config.toml");

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

            var template = Handlebars.Compile(source);

            var data = new
            {
                now = DateTime.Now,
                site = TopLevelConfig.Site
            };

            var result = template(data);

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
    }
}
