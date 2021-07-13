using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Sitegen.YamlDotNet
{
    public static class YamlDotNetExtensions
    {
        public static DeserializerBuilder WithSerializationEventSupport(this DeserializerBuilder builder)
        {
            return builder
                .WithNodeDeserializer(inner => new EventSupportingNodeDeserializer(inner),
                    s => s.InsteadOf<ObjectNodeDeserializer>());
        }
    }
}
