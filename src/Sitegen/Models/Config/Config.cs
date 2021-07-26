using System.IO;

namespace Sitegen.Models.Config
{
    public class Config : IDeserialized
    {
        public string SourceDir { get; set; }
        public string LayoutsDir { get; set; }
        public string OutputDir { get; set; }
        public string PostsDir { get; set; }

        // Enabling "soft line breaks as hard" is currently the default. Can be opted out by individual blog posts
        // as needed.
        public LineBreaks LineBreaks { get; set; } = Models.LineBreaks.Hard;

        public bool MultipleLanguages { get; set; }

        public void OnDeserialized()
        {
            SourceDir ??= "src";
            LayoutsDir ??= Path.Join(SourceDir, "_layouts");
            OutputDir ??= "out";
            PostsDir ??= "src/_posts";
        }
    }
}
