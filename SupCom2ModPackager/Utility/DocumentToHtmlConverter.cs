using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtfPipe;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace SupCom2ModPackager.Utility
{
    public class DocumentToHtmlConverter
    {
        private readonly Dictionary<string, Func<string, string>> converters;
        private readonly IDeserializer yamlDeserializer;

        public DocumentToHtmlConverter()
        {
            converters = new Dictionary<string, Func<string, string>>
                {
                    { ".rtf", rtf => Rtf.ToHtml(rtf) },
                    { ".html", html => html },
                    { ".htm", htm => htm },
                    { ".txt", text => "<pre>" + text + "</pre>" },
                    { ".yml", ConvertYamlToHtml }
                };
            yamlDeserializer = new DeserializerBuilder()
                //.WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                .Build();
        }

        public bool TryConvert(string extension, Func<string> getString, out string converted)
        {
            if (converters.TryGetValue(extension, out var converter))
            {
                converted = converter(getString());
                return true;
            }

            converted = string.Empty;
            return false;
        }


        private string ConvertYamlToHtml(string yaml)
        {
            var lines = yaml
                .Split("\r\n")
                .Select(line =>
                {
                    var parts = line.Replace(" ", "&nbsp;").Split(':', 2);
                    return parts.Length == 2 ? $"<b>{parts[0]}</b>: {parts[1]}" : parts[0]
                    ;
                })
                .Select(line => line + "<br/>")
                .ToList();
            var output = string.Join("\r\n", lines);
            return $"<p style=\"font-family: Consolas;\">{output}</p>";
            //var profile = yamlDeserializer.Deserialize<Profile>(yaml);
            //var sb = new StringBuilder();
            //return sb.ToString();
        }

    }

    public class Profile
    {
        public string Author { get; set; } = string.Empty;
        public List<ProfileDependency> Dependencies { get; set; } = [];
        public string Description { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<string> EditableFiles { get; set; } = new List<string>();
        public string FileHash { get; set; } = string.Empty;
        public string FQPN { get; set; } = string.Empty;
        public List<ProfileDependency> Incompatibilities { get; set; } = [];
        public List<ProfileSource> InstallationInstructions { get; set; } = [];
        public List<ProfileDependency> LegacySupported { get; set; } = [];
        public List<ProfileDependency> LegacySupportedDependents { get; set; } = [];
        public string RTFDocumentation { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string _DocumentationSource { get; set; } = string.Empty;
    }

    public class ProfileDependency
    {
        public string FileHash { get; set; } = string.Empty;
        public string FQPN { get; set; } = string.Empty;
        public string _Version { get; set; } = string.Empty;
    }

    public class ProfileSource
    {
        public string FileName { get; set; } = string.Empty;
        public string _Source { get; set; } = string.Empty;
    }
}
