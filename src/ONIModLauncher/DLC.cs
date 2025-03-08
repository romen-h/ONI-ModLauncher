using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONIModLauncher
{
	public static class DLC
	{
		[Obsolete]
		public static readonly string Vanilla = "VANILLA_ID";

		public static readonly string SpacedOut = "EXPANSION1_ID";

		public static readonly string FrostyPlanetPack = "DLC2_ID";

		public static readonly string BionicBoosterPack = "DLC3_ID";
	}

	public enum Compatibility
	{
		Unknown,
		Incompatible,
		Compatible,
		Required
	}
}
