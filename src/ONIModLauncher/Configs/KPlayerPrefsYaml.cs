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
		private const string PlayShortOnLaunchKey = @"PlayShortOnLaunch";
		private const string SaveFilenameKey = @"SaveFilenameKey/";

		private const string ResolutionWidthKey = @"ResolutionWidth";
		private const string ResolutionHeightKey = @"ResolutionHeight";
		private const string RefreshRateKey = @"RefreshRate";
		private const string FullScreenKey = @"FullScreen";
		private const string Expansion1EnabledKey = @"EXPANSION1_ID.ENABLED";

		private const string UIScalePrefKey = @"UIScalePref";

		private string filePath;

		private bool configured = false;

		[YamlMember]
		public Dictionary<string, string> strings
		{ get; set; } = new Dictionary<string, string>();

		[YamlMember]
		public Dictionary<string, int> ints
		{ get; set; } = new Dictionary<string, int>();

		[YamlMember]
		public Dictionary<string, float> floats
		{ get; set; } = new Dictionary<string, float>();

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

		[YamlIgnore]
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
			if (GamePaths.HasDLC1)
			{
				strings[PlayShortOnLaunchKey] = @"intro\Spaced_Out_Intro";
			}
			SaveFile = "";
			ResolutionWidth = 1920;
			ResolutionHeight = 1080;
			RefreshRate = 60;
			FullScreen = true;
			DLC1Enabled = false;

			configured = true;
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

		private static readonly UTF8Encoding encoding = new UTF8Encoding(false);

		public void Save()
		{
			if (!configured) return;
			if (filePath == null) return;

			var serializer = new SerializerBuilder().Build();
			string yaml = serializer.Serialize(this);

			File.WriteAllText(filePath, yaml, encoding);
		}
	}
}
