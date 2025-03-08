using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ONIModLauncher.Configs
{
	public class ModInfoYaml
	{
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitDefaults)]
		public string supportedContent = null;

		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitEmptyCollections)]
		public string[] requiredDlcIds = null;

		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitEmptyCollections)]
		public string[] forbiddenDlcIds = null;

		[YamlMember]
		public int minimumSupportedBuild;

		[YamlMember]
		public string version;

		public ModInfoYaml()
		{ }

		public static ModInfoYaml Load(string file)
		{
			if (!File.Exists(file)) return null;

			string yaml = File.ReadAllText(file);
			var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
			ModInfoYaml modInfo = deserializer.Deserialize<ModInfoYaml>(yaml);

			return modInfo;
		}
	}
}
