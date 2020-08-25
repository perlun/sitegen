namespace SiteGenerator.ConsoleApp
{
    public class UrlUtils
    {
        public static string Slugify(string s) => s
            .ToLower()

            // The umlaut (åäö -> aao) conversion part of this is arguably very Swedish/Finnish-specific. Could we make it
            // more generic somehow?
            .Replace('å', 'a')
            .Replace('ä', 'a')
            .Replace('ö', 'o')

            // Dashes work better than spaces in URLs.
            .Replace(' ', '-')

            // We consider certain characters "forbidden" or unsuitable from being used in URLs; we simply strip them
            // out when generating the slugs.
            .Replace("!", "")
            .Replace("?", "");
    }
}
