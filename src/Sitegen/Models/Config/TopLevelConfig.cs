namespace Sitegen.Models.Config
{
    /// <summary>
    /// Top-level class used when deserializing `config.yaml` files.
    /// </summary>
    public class TopLevelConfig
    {
        public Config Config { get; set; } = new();
        public Site Site { get; set; } = new();
    }
}
