namespace SiteGenerator.ConsoleApp
{
    public static class UrlUtils
    {
        // Note: the algorithm below is obviously utterly inefficient in that it performs n passes to perform the
        // slugification, when a single pass doing it all-at-once would be tons more efficient, both in terms of
        // runtime speed and GC pressure. This is only suited for running on short strings, where those factors are
        // practically irrelevant.
        public static string Slugify(string s) => s
            .ToLower()

            // The umlaut (åäö -> aao) conversion part of this is arguably very Swedish/Finnish-specific. Could we make it
            // more generic somehow?
            .Replace('å', 'a')
            .Replace('ä', 'a')
            .Replace('ö', 'o')

            // Existing dashes can be removed...
            .Replace("-", "")
            .Replace("&mdash;", "")

            // ...but then again, spaces are converted to dashes, since they work better in URLs.
            .Replace(' ', '-')

            // Multiple spaces (dashes) get converted to single space. This would obviously have to run multiple times
            // to handle scenarios with _more_ than two spaces, but for our very simplistic existing use cases, this
            // is again sufficient.
            .Replace("--", "-")

            // We consider certain characters "forbidden" or unsuitable from being used in URLs; we simply strip them
            // out when generating the slugs.
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace("\"", "");
    }
}
