#nullable enable

using System;
using Sitegen.Models;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Sitegen.YamlDotNet
{
    /// <summary>
    /// <see cref="INodeDeserializer"/> which supports emitting `OnDeserialized` events.
    /// </summary>
    public class EventSupportingNodeDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer nodeDeserializer;

        public EventSupportingNodeDeserializer(INodeDeserializer nodeDeserializer)
        {
            this.nodeDeserializer = nodeDeserializer;
        }

        public bool Deserialize(IParser parser, Type expectedType,
            Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (!nodeDeserializer.Deserialize(parser, expectedType, nestedObjectDeserializer, out value) ||
                value == null)
            {
                return false;
            }

            if (value is IDeserialized deserializedObject)
            {
                deserializedObject.OnDeserialized();
            }

            return true;
        }
    }
}
