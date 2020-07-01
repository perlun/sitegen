using System;

namespace SiteGenerator.ConsoleApp
{
    internal class ConfigurationException : Exception
    {
        public ConfigurationException(string message)
            : base(message)
        {
        }
    }

    internal class PostFormatException : Exception
    {
        public PostFormatException(string message)
            : base(message)
        {
        }
    }
}
