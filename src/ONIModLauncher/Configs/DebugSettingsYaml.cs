using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YamlDotNet.Serialization;

namespace ONIModLauncher.Configs
{
	public class DebugSettingsYaml : YamlConfig<DebugSettingsYaml>
	{
		[YamlMember(Alias = "debugEnable")]
		public bool debugEnable = false;

		[YamlMember(Alias = "developerDebugEnable")]
		public bool developerDebugEnable = false;

		[YamlMember(Alias = "autoResumeGame")]
		public bool autoResumeGame = false;

		[YamlIgnore]
		public bool EnableDebug
		{
			get => debugEnable;
			set
			{
				if (debugEnable != value)
				{
					debugEnable = value;
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
					InvokePropertyChanged(nameof(AutoResumeLastSave));
				}
			}
		}
	}
}
