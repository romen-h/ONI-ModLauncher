using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ONIModManager.Core.Configs
{
	[YamlSerializable]
    public class ModYaml
    {
	    public static ModYaml Deserialize(string filePath)
	    {
		    return new DeserializerBuilder()
			    .WithNamingConvention(CamelCaseNamingConvention.Instance)
			    .Build()
			    .Deserialize<ModYaml>(File.ReadAllText(filePath, Encoding.UTF8));
	    }
	    
        [YamlMember(Alias = "title")]
        public string? Title
        { get; private set; }

        [YamlMember(Alias = "description")]
        public string? Description
        { get; private set; }

        [YamlMember( Alias = "staticID")]
        public string? StaticID
        { get; private set; }
	}
}
