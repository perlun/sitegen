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
}
