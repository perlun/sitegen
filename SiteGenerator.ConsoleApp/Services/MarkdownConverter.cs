using Markdig;

namespace SiteGenerator.ConsoleApp.Services
{
    public static class MarkdownConverter
    {
        /// <summary>
        /// Converts the given Markdown text to HTML format.
        ///
        /// This method enables a number of Markdig extensions.
        /// </summary>
        /// <param name="markdown">The Markdown content</param>
        /// <returns>An HTML representation of the given content.</returns>
        public static string ToHtml(string markdown)
        {
            MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
                //.UseAdvancedExtensions()
                .UseSoftlineBreakAsHardlineBreak()
                .Build();

            return Markdown.ToHtml(markdown, pipeline);
        }
    }
}
