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
	public class KPlayerPrefsYaml : ConfigBase
	{
		private const string SaveFilenameKey = @"SaveFilenameKey/";

		private const string ResolutionWidthKey = @"ResolutionWidth";
		private const string ResolutionHeightKey = @"ResolutionHeight";
		private const string RefreshRateKey = @"RefreshRate";
		private const string FullScreenKey = @"FullScreen";
		private const string Expansion1EnabledKey = @"EXPANSION1_ID.ENABLED";

		private const string UIScalePrefKey = @"UIScalePref";

		private string filePath;

		[YamlMember(Alias = "strings")]
		public Dictionary<string, string> strings = new Dictionary<string, string>();

		[YamlMember(Alias = "ints")]
		public Dictionary<string, int> ints = new Dictionary<string, int>();

		[YamlMember(Alias = "floats")]
		public Dictionary<string, float> floats = new Dictionary<string, float>();

		[YamlIgnore]
		public string SaveFile
		{
			get => strings.ContainsKey(SaveFilenameKey) ? strings[SaveFilenameKey] : "";
			set
			{
				strings[SaveFilenameKey] = value;
				Save();
				InvokePropertyChanged(nameof(SaveFile));
			}
		}

		[YamlIgnore]
		public int ResolutionWidth
		{
			get => ints.ContainsKey(ResolutionWidthKey) ? ints[ResolutionWidthKey] : 1920;
			set
			{
				ints[ResolutionWidthKey] = value;
				Save();
				InvokePropertyChanged(nameof(ResolutionWidth));
			}
		}

		[YamlIgnore]
		public int ResolutionHeight
		{
			get => ints.ContainsKey(ResolutionHeightKey) ? ints[ResolutionHeightKey] : 1080;
			set
			{
				ints[ResolutionHeightKey] = value;
				Save();
				InvokePropertyChanged(nameof(ResolutionHeight));
			}
		}

		[YamlIgnore]
		public int RefreshRate
		{
			get => ints.ContainsKey(RefreshRateKey) ? ints[RefreshRateKey] : 60;
			set
			{
				ints[RefreshRateKey] = value;
				Save();
				InvokePropertyChanged(nameof(RefreshRateKey));
			}
		}

		[YamlIgnore]
		public bool FullScreen
		{
			get => ints.ContainsKey(FullScreenKey) && ints[FullScreenKey] == 1;
			set
			{
				ints[FullScreenKey] = value ? 1 : 0;
				Save();
				InvokePropertyChanged(nameof(FullScreen));
			}
		}

		[YamlIgnore]
		public bool DLC1Enabled
		{
			get => ints.ContainsKey(Expansion1EnabledKey) && ints[Expansion1EnabledKey] == 1;
			set
			{
				ints[Expansion1EnabledKey] = value ? 1 : 0;
				Save();
				InvokePropertyChanged(nameof(DLC1Enabled));
			}
		}

		public float UIScale
		{
			get => floats.ContainsKey(UIScalePrefKey) ? floats[UIScalePrefKey] : float.NaN;
			set
			{
				floats[UIScalePrefKey] = value;
				Save();
				InvokePropertyChanged(nameof(UIScale));
			}
		}

		public KPlayerPrefsYaml()
		{
			SaveFile = "";
			ResolutionWidth = 1920;
			ResolutionHeight = 1080;
			RefreshRate = 60;
			FullScreen = true;
			DLC1Enabled = false;
		}

		public static KPlayerPrefsYaml Load(string file)
		{
			if (!File.Exists(file)) return new KPlayerPrefsYaml()
			{
				filePath = file
			};

			string yaml = File.ReadAllText(file);

			var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
			var s = deserializer.Deserialize<KPlayerPrefsYaml>(yaml);
			s.filePath = file;

			return s;
		}

		public void Save()
		{
			if (filePath == null) return;

			var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
			string yaml = serializer.Serialize(this);

			File.WriteAllText(filePath, yaml, Encoding.UTF8);
		}
	}
}
