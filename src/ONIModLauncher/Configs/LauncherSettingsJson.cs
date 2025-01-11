using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ONIModLauncher.Configs
{
	[JsonObject(MemberSerialization.OptIn)]
	public class LauncherSettingsJson : ConfigBase
	{
		[JsonProperty]
		public HashSet<string> BrokenMods
		{ get; private set; } = new HashSet<string>();

		public void AddBrokenMod(string id)
		{
			BrokenMods.Add(id);
			InvokePropertyChanged(nameof(BrokenMods));
		}

		public void RemoveBrokenMod(string id)
		{
			BrokenMods.Remove(id);
			InvokePropertyChanged(nameof(BrokenMods));
		}

		[JsonProperty]
		public HashSet<string> KeepEnabled
		{ get; private set; } = new HashSet<string>();

		public void AddKeepEnabled(string id)
		{
			KeepEnabled.Add(id);
			InvokePropertyChanged(nameof(KeepEnabled));
		}

		public void RemoveKeepEnabled(string id)
		{
			KeepEnabled.Remove(id);
			InvokePropertyChanged(nameof(KeepEnabled));
		}

		[JsonProperty]
		public Dictionary<string,int> CustomModSorting
		{ get; private set; } = new Dictionary<string, int>();

		public LauncherSettingsJson()
		{

		}
	}
}
