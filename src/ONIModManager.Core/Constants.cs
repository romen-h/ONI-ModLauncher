using System.Collections.Generic;

namespace ONIModManager.Core
{
	public static class Constants
	{
		public static class Dlc
		{
			public const string Vanilla = "";
			public const string SpacedOut = "EXPANSION1_ID";
			public const string FrostyPlanetPack = "DLC2_ID";
			public const string BionicBoosterPack = "DLC3_ID";
			public const string PrehistoricPlanetPack = "DLC4_ID";
			
			public static readonly IReadOnlyList<string> ToggleableDlcIds = [
				Vanilla,
				SpacedOut
			];
		}
	}
}
