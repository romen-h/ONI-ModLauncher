using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONIModLauncher
{
	public static class ShellHelper
	{
		public static void OpenFolder(string path)
		{
			try
			{
				Process.Start("explorer.exe", $"\"{path}\"");
			}
			catch
			{ }
		}

		public static void OpenTextFile(string path)
		{
			try
			{
				var startInfo = new ProcessStartInfo()
				{
					FileName = path,
					UseShellExecute = true,
					CreateNoWindow = true
				};
				Process.Start(startInfo);
			}
			catch (Exception ex)
			{ }
		}

		public static void OpenURL(string url)
		{
			try
			{
				var startInfo = new ProcessStartInfo()
				{
					FileName = "cmd.exe",
					Arguments = $"/c start {url}",
					CreateNoWindow = true
				};
				Process.Start(startInfo);
			}
			catch (Exception ex)
			{ }
		}
	}
}
