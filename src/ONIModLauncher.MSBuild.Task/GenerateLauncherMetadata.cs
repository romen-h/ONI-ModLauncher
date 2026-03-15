using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Build.Framework;
using ONIModLauncher.Common.Configs;

namespace ONIModLauncher.MSBuild.Task
{
	public class GenerateLauncherMetadata : Microsoft.Build.Utilities.Task
	{
		[Required]
		public string TargetDir
		{ get; set; }

		[Required]
		public string ModStaticId
		{ get; set; }

		[Required]
		public string Version
		{ get; set; }
		
		public long? SteamWorkshopId
		{ get; set; }
		
		public string? Title
		{ get; set; } = null;

		public string? Authors
		{ get; set; } = null;
		
		public string[]? AuthorUrls
		{ get; set; } = null;
		
		public string[]? Dependencies
		{ get; set; } = null;
		
		public string? Category
		{ get; set; } = null;
		
		public string? Keywords
		{ get; set; } = null;
		
		public string? UpdateIndexUrl
		{ get; set; } = null;
		
		public string? UpdateNestedPath
		{ get; set; } = null;
		
		public string[]? UpdatePreservedFiles
		{ get; set; } = null;
		
		public override bool Execute()
		{
			try
			{
				Log.LogMessage(MessageImportance.Low, "Building LauncherMetadata.json...");
				LauncherMetadataJson metadata = new LauncherMetadataJson()
				{
					ModStaticId = ModStaticId,
					SteamWorkshopId = SteamWorkshopId,
					Version = System.Version.Parse(Version),
					Title = Title
				};
				
				if (Authors != null)
				{
					Log.LogMessage(MessageImportance.Low, "Initializing author info...");

					string[] authors = Authors.Split(',').Select(s => s.Trim()).ToArray();
					
					metadata.Authors = new AuthorInfo[authors.Length];
					for (int i=0; i<authors.Length; i++)
					{
						metadata.Authors[i] = new AuthorInfo()
						{
							Name = authors[i]
						};
						
						if (AuthorUrls != null)
						{
							if (AuthorUrls.Length != authors.Length) throw new ArgumentException("AuthorUrls array must be the same size as Authors array.");
							metadata.Authors[i].Url = AuthorUrls[i];
						}
					}
				}
				
				if (Dependencies != null && Dependencies.Length > 0)
				{
					Log.LogMessage(MessageImportance.Low, "Initializing dependency info...");
					metadata.Dependencies = Dependencies;
				}
				
				if (Category != null || Keywords != null)
				{
					Log.LogMessage(MessageImportance.Low, "Initializing sorting info...");
					metadata.Sorting = new SortInfo()
					{
						Category = Category,
						Keywords = Keywords?.Split(' ')
					};
				}
				
				if (UpdateIndexUrl != null)
				{
					Log.LogMessage(MessageImportance.Low, "Initializing update info...");
					metadata.Updates = new UpdateInfo()
					{
						UpdateIndexUrl = UpdateIndexUrl,
						NestedPath = UpdateNestedPath,
						PreservedFiles = UpdatePreservedFiles
					};
				}

				Log.LogMessage(MessageImportance.Low, "Serializing LauncherMetadata...");
				string json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions()
				{
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
				});

				string filePath = Path.Combine(TargetDir, "LauncherMetadata.json");

				Log.LogMessage(MessageImportance.Low, "Writing LauncherMetadata.json...");
				File.WriteAllText(filePath, json, Encoding.UTF8);

				return true;
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex, true);
				return false;
			}
		}
	}
}
