using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ONIModLauncher.Configs
{
	public class DebugSettingsYaml : ConfigBase
	{
		[YamlIgnore]
		private string filePath;

		[YamlMember]
		public bool debugEnable;

		[YamlMember]
		public bool developerDebugEnable;

		[YamlMember]
		public bool autoResumeGame;

		[YamlIgnore]
		public bool EnableDebug
		{
			get => debugEnable;
			set
			{
				if (debugEnable != value)
				{
					debugEnable = value;
					Save();
					InvokePropertyChanged(nameof(EnableDebug));
				}
			}
		}

		[YamlIgnore]
		public bool AutoResumeLastSave
		{
			get => autoResumeGame;
			set
			{
				if (autoResumeGame != value)
				{
					autoResumeGame = value;
					Save();
					InvokePropertyChanged(nameof(AutoResumeLastSave));
				}
			}
		}

		public DebugSettingsYaml()
		{
			debugEnable = false;
			developerDebugEnable = false;
			autoResumeGame = false;
		}

		public static DebugSettingsYaml Load(string file)
		{
			if (!File.Exists(file)) return new DebugSettingsYaml() { filePath = file };

			string yaml = File.ReadAllText(file);

			var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
			var s = deserializer.Deserialize<DebugSettingsYaml>(yaml);
			s.filePath = file;

			return s;
		}

		public void Save()
		{
			if (filePath == null) return;

			var serializer = new SerializerBuilder().Build();
			string yaml = serializer.Serialize(this);

			File.WriteAllText(filePath, yaml);
		}
	}
}
