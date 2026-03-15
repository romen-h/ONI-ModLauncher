using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YamlDotNet.Serialization;

namespace ONIModLauncher.Configs
{
	public class ModInfoYaml : YamlConfig<ModInfoYaml>
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
	}
}
