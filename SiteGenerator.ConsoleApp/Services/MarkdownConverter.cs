using Markdig;
using SiteGenerator.ConsoleApp.Models;

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
        /// <param name="softLineBreaks">The LineBreaks setting.</param>
        /// <returns>An HTML representation of the given content.</returns>
        public static string ToHtml(string markdown, LineBreaks softLineBreaks)
        {
            MarkdownPipelineBuilder pipelineBuilder = new MarkdownPipelineBuilder().UseAdvancedExtensions();

            if (softLineBreaks == LineBreaks.Hard)
            {
                pipelineBuilder.UseSoftlineBreakAsHardlineBreak();
            }

            MarkdownPipeline pipeline = pipelineBuilder.Build();

            return Markdown.ToHtml(markdown, pipeline);
        }
    }
}
