using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ONIModManager.Core.Configs
{
	public class KPlayerPrefsYaml
	{
		private const string PlayShortOnLaunchKey = @"PlayShortOnLaunch";
		private const string SaveFilenameKey = @"SaveFilenameKey/";
		private const string ResolutionWidthKey = @"ResolutionWidth";
		private const string ResolutionHeightKey = @"ResolutionHeight";
		private const string RefreshRateKey = @"RefreshRate";
		private const string FullScreenKey = @"FullScreen";
		private const string Expansion1EnabledKey = @"EXPANSION1_ID.ENABLED";
		private const string UIScalePrefKey = @"UIScalePref";
		private const string DisableAutoModSafeModeKey = @"DisableAutoModSafeMode";
		
		public static KPlayerPrefsYaml Deserialize(string filePath)
		{
			var serializer = new DeserializerBuilder()
				.Build();
			string yaml = File.ReadAllText(filePath, Encoding.UTF8);
			return serializer.Deserialize<KPlayerPrefsYaml>(yaml);
		}
		
		public static void Serialize(string filePath, KPlayerPrefsYaml instance)
		{
			var serializer = new SerializerBuilder()
				.Build();
			string yaml = serializer.Serialize(instance);
			File.WriteAllText(filePath, yaml, Encoding.UTF8);
		}
		
		[YamlMember(Alias = "strings")]
		public Dictionary<string, string> Strings
		{ get; set; } = new Dictionary<string, string>();

		[YamlMember(Alias = "ints")]
		public Dictionary<string, int> Ints
		{ get; set; } = new Dictionary<string, int>();

		[YamlMember(Alias = "floats")]
		public Dictionary<string, float> Floats
		{ get; set; } = new Dictionary<string, float>();
		
		[YamlIgnore]
		public string SaveFile
		{
			get => Strings.GetValueOrDefault(SaveFilenameKey, "");
			set => Strings[SaveFilenameKey] = value;
		}

		[YamlIgnore]
		public int ResolutionWidth
		{
			get => Ints.GetValueOrDefault(ResolutionWidthKey);
			set => Ints[ResolutionWidthKey] = value;
		}

		[YamlIgnore]
		public int ResolutionHeight
		{
			get => Ints.GetValueOrDefault(ResolutionHeightKey);
			set => Ints[ResolutionHeightKey] = value;
		}

		[YamlIgnore]
		public int RefreshRate
		{
			get => Ints.GetValueOrDefault(RefreshRateKey);
			set => Ints[RefreshRateKey] = value;
		}

		[YamlIgnore]
		public bool FullScreen
		{
			get => Ints.GetValueOrDefault(FullScreenKey) == 1;
			set => Ints[FullScreenKey] = value ? 1 : 0;
		}

		[YamlIgnore]
		public bool SpacedOutEnabled
		{
			get => Ints.GetValueOrDefault(Expansion1EnabledKey) == 1;
			set => Ints[Expansion1EnabledKey] = value ? 1 : 0;
		}

		[YamlIgnore]
		public float UIScale
		{
			get => Floats.GetValueOrDefault(UIScalePrefKey);
			set => Floats[UIScalePrefKey] = value;
		}
		
		[YamlIgnore]
		public bool DisableSafeMode
		{
			get => Ints.GetValueOrDefault(DisableAutoModSafeModeKey) == 1;
			set => Ints[DisableAutoModSafeModeKey] = value ? 1 : 0;
		}
	}
}
