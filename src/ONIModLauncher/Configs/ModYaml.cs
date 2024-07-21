using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace ONIModLauncher.Configs
{
    public class ModYaml
    {
        [YamlMember]
        public string title;

        [YamlMember]
        public string description;

        [YamlMember]
        public string staticID;
    }
}
