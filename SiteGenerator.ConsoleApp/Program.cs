using System;
using System.IO;
using SiteGenerator.ConsoleApp.Models.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SiteGenerator.ConsoleApp
{
    public class Program
    {
        private HandlebarsConverter handlebarsConverter;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Syntax: sitegen --post <src-file> | <src-file> <target-file>");
                Environment.Exit(1);
            }

            TopLevelConfig config = ReadConfig();

            string sourcePath = args[0];
            string targetPath = args[1];

            var program = new Program(config);
            program.ConvertHandlebarsFile(sourcePath, targetPath);
        }

        private Program(TopLevelConfig topLevelConfig)
        {
            handlebarsConverter = new HandlebarsConverter(topLevelConfig);
        }

        private static TopLevelConfig ReadConfig()
        {
            var input = new StringReader(File.ReadAllText("config.yaml"));

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var config = deserializer.Deserialize<TopLevelConfig>(input);

            ValidateConfig(config);
            return config;
        }

        private static void ValidateConfig(TopLevelConfig config)
        {
            // Set default values for config settings which have not been provided
            config.Config ??= new Config();
            config.Config.SourceDir ??= "src";
        }

        /// <summary>
        /// Converts a Handlebars (.hbs) file to HTML (.html) format.
        /// </summary>
        /// <param name="sourcePath">the path to the source .hbs file</param>
        /// <param name="targetPath">the path to the target .html file</param>
        private void ConvertHandlebarsFile(string sourcePath, string targetPath)
        {
            string source = File.ReadAllText(sourcePath);
            string result = handlebarsConverter.Convert(source);

            File.WriteAllText(targetPath, result);
        }
    }
}
