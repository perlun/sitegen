using System;
using System.IO;
using HandlebarsDotNet;

namespace SiteGenerator.ConsoleApp
{
    public static class Program
    {
        // TODO: Make configurable. Ideally, support something like gcc's -I parameter which can be provided multiple
        // TODO: times. Perhaps the easiest way is to just make this a setting in config.toml? Yes, that'll
        // TODO: probably be the case.
        private const string SourceDir = "src";

        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Syntax: sitegen <src-file> <target-file>");
                Environment.Exit(1);
            }

            string source = File.ReadAllText(args[0]);

            Handlebars.RegisterHelper("include", IncludeHelper);

            var template = Handlebars.Compile(source);

            var data = new {
                title = "My new post",
                body = "This is my first post!",
                now = DateTime.Now,
                site = new {
                    Title = "halleluja.nu"
                }
            };

            var result = template(data);

            File.WriteAllText(args[1], result);
        }

        private static void IncludeHelper(TextWriter writer, dynamic context, object[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new HandlebarsException("{{include}} helper must have exactly one argument");
            }

            if (!(parameters[0] is string fileName))
            {
                throw new HandlebarsException("{{include}} expected string parameter, not " + parameters[0].GetType().Name);
            }

            string templateSource = File.ReadAllText(Path.Join(SourceDir, fileName));

            var template = Handlebars.Compile(templateSource);
            string result = template(context);

            writer.WriteSafeString(result);
        }
    }
}
