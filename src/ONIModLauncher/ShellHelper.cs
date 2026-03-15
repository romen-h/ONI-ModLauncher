using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

		public static readonly char[] InvalidFileChars = new char[] { '"', '?', '<', '>', ':', '*', '?' };

		public static string ToFileSafeString(this string str)
		{
			foreach (var c in Path.GetInvalidPathChars())
			{
				str = str.Replace(c.ToString(), "");
			}
			foreach (var c in InvalidFileChars)
			{
				str = str.Replace(c.ToString(), "");
			}
			return str;
		}

		public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
		{
			// Get information about the source directory
			var dir = new DirectoryInfo(sourceDir);

			// Check if the source directory exists
			if (!dir.Exists)
				throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

			// Cache directories before we start copying
			DirectoryInfo[] dirs = dir.GetDirectories();

			// Create the destination directory
			Directory.CreateDirectory(destinationDir);

			// Get the files in the source directory and copy to the destination directory
			foreach (FileInfo file in dir.GetFiles())
			{
				string targetFilePath = Path.Combine(destinationDir, file.Name);
				file.CopyTo(targetFilePath);
			}

			// If recursive and copying subdirectories, recursively call this method
			if (recursive)
			{
				foreach (DirectoryInfo subDir in dirs)
				{
					string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
					CopyDirectory(subDir.FullName, newDestinationDir, true);
				}
			}
		}
		
		public static void ClearDirectory(string dir)
		{
			foreach (string file in Directory.GetFiles(dir))
			{
				File.Delete(file);
			}

			foreach (string folder in Directory.GetDirectories(dir))
			{
				Directory.Delete(folder, true);
			}
		}
		
		public static void ClearDirectoryExceptExtensions(string dir, params string[] extensionsToKeep)
		{
			foreach (string file in Directory.GetFiles(dir))
			{
				string extension = Path.GetExtension(file);
				if (extensionsToKeep != null && extensionsToKeep.Length > 0 && extensionsToKeep.Contains(extension)) continue;
				
				File.Delete(file);
			}
			
			foreach (string folder in Directory.GetDirectories(dir))
			{
				ClearDirectoryExceptExtensions(folder, extensionsToKeep);
				if (Directory.GetFiles(folder).Length == 0)
				{
					Directory.Delete(folder);
				}
			}
		}
	}
}
