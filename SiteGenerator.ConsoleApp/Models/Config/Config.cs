namespace SiteGenerator.ConsoleApp.Models.Config
{
    public class Config
    {
        public string SourceDir { get; set; }
        public string LayoutsDir { get; set; }
        public string OutputDir { get; set; }
        public string PostsDir { get; set; }
        public LineBreaks? LineBreaks { get; set; }
        public bool MultipleLanguages { get; set; }
    }
}
