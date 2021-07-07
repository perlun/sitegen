using JetBrains.Annotations;

namespace Sitegen.Models.Config
{
    /// <summary>
    /// Top-level class used when deserializing `config.yaml` files.
    /// </summary>
    [UsedImplicitly]
    public class TopLevelConfig
    {
        public Config Config { get; set; }
        public Site Site { get; set; }
    }
}
