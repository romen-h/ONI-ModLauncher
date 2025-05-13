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
		public HashSet<string> ToToggleMods
		{ get; private set; } = new HashSet<string>();

		[JsonProperty]
		public HashSet<string> BrokenMods
		{ get; private set; } = new HashSet<string>();

		public static string ModUniqueIdentifier(ModConfigItem mod) => $"{mod.label.distribution_platform.ToString()}.{mod.label.id}";

		public bool HasPending(string uid) => ToToggleMods.Contains(uid);

		public void ApplyPendingToggleChanges(ModConfigJson modConfig, out bool hadToggles)
		{
			hadToggles = false;
			var currentlyActiveDlc = Launcher.Instance.PlayerPrefs.SpacedOutEnabled ? DLC.SpacedOut : "";
			foreach (var mod in modConfig.mods)
			{
				var uniqueModId = ModUniqueIdentifier(mod);
				if (ToToggleMods.Contains(uniqueModId))
				{
					hadToggles = true;
					if (mod.enabledForDlc.Contains(currentlyActiveDlc))
						mod.enabledForDlc.Remove(currentlyActiveDlc);
					else
						mod.enabledForDlc.Add(currentlyActiveDlc);
				}
			}
			ToToggleMods.Clear();
			InvokePropertyChanged(nameof(ToToggleMods));
		}
		public void AddPending(string id)
		{
			ToToggleMods.Add(id);
			InvokePropertyChanged(nameof(ToToggleMods));
		}
		public void RemovePending(string id)
		{
			ToToggleMods.Remove(id);
			InvokePropertyChanged(nameof(ToToggleMods));
		}

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
		public Dictionary<string, int> CustomModSorting
		{ get; private set; } = new Dictionary<string, int>();

		public LauncherSettingsJson()
		{

		}
	}
}
