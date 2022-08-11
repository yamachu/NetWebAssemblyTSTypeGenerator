using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetWebAssemblyTSTypeGenerator
{
    internal class TypeScriptDefinitionGenerator
    {
        private JsonSerializerOptions SerializerOptions { get; init; }
        private static TypeScriptDefinitionGenerator _instance;

        public static TypeScriptDefinitionGenerator GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TypeScriptDefinitionGenerator();
            }
            return _instance;
        }

        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, SerializerOptions);
        }

        private TypeScriptDefinitionGenerator()
        {
            SerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new DynamicDictConverter(),
                    new ArgumentsJsonConverter()
                }
            };
        }
        
    }

    // inspired: https://github.com/joseftw/JOS.STJ.DictionaryStringObjectJsonConverter
    internal class DynamicDictConverter : JsonConverter<Dictionary<string, dynamic>>
    {
        public override Dictionary<string, dynamic> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<string, dynamic> argumentValues,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var key in argumentValues.Keys)
            {
                HandleValue(writer, key, argumentValues[key], options);
            }

            writer.WriteEndObject();
        }

        private static void HandleValue(Utf8JsonWriter writer, string key, dynamic value, JsonSerializerOptions options)
        {
            if (key != null)
            {
                writer.WritePropertyName(key);
            }

            switch (value)
            {
                case string stringValue:
                    writer.WriteStringValue(stringValue);
                    break;
                case Dictionary<string, dynamic> dictionaryValue:
                    writer.WriteStartObject();
                    foreach (var i in dictionaryValue)
                    {
                        HandleValue(writer, i.Key, i.Value, options);
                    }
                    writer.WriteEndObject();
                    break;
                case IEnumerable<Argument> argumentsValue:
                    JsonSerializer.Serialize(writer, argumentsValue, options);
                    break;
                default:
                    break;
            }
        }
    }

    internal class ArgumentsJsonConverter : JsonConverter<IEnumerable<Argument>>
    {
        public override IEnumerable<Argument> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            IEnumerable<Argument> argumentValues,
            JsonSerializerOptions options)
        {
            writer.WriteRawValue($"""({string.Join(", ", argumentValues.Select(a => a.Name + ": any /* TODO */"))}) => any /* TODO */""", true);
        }
    }
}