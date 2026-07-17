using JetBrains.Annotations;

namespace ONIModManager.Core.Types
{
	[PublicAPI]
	public enum ModDistributionPlatform
	{
		Local = 0,
		Steam = 1,
		Epic = 2,
		Rail = 3,
		Dev = 4
	}
}
