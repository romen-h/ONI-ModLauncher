using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONIModLauncher
{
	public static class ModUpdateUrls
	{
		public static string PeterHan_ModUpdater = "https://github.com/peterhaneve/ONIMods/releases/download/ModUpdaterLatest/ModUpdateDate.zip";
		
		public static string GetUrl(string modId)
		{
			switch (modId)
			{
				case "PeterHan.ModUpdateDate": return PeterHan_ModUpdater;
				default: return null;
			}
		}
	}
}
