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
    public class ModYaml : YamlConfig<ModYaml>
    {
        [YamlMember]
        public string title;

        [YamlMember]
        public string description;

        [YamlMember]
        public string staticID;
	}
}
