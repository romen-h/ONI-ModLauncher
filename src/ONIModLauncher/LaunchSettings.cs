using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ONIModLauncher.Configs;

namespace ONIModLauncher
{
	public class LaunchSettings : INotifyPropertyChanged
	{
		public static readonly LaunchSettings Instance = new LaunchSettings();

		public bool HasDLC1 => GamePaths.HasDLC1;

		public KPlayerPrefsYaml PlayerPrefs
		{ get; private set; }

		public DebugSettingsYaml Debug
		{ get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void InvokePropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private LaunchSettings()
		{
			PlayerPrefs = KPlayerPrefsYaml.Load(GamePaths.PlayerPrefsFile);
			Debug = DebugSettingsYaml.Load(GamePaths.DebugSettingsFile);
		}
	}
}
