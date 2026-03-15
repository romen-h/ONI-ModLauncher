using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ONIModLauncher.Common.Configs
{
	/// <summary>
	/// Mod metadata used by ONI Mod Launcher
	/// </summary>
	public class LauncherMetadataJson
	{
		/// <summary>
		/// The static ID of the mod.
		/// </summary>
		/// <remarks>Required. This is used to uniquely identify this mod against all other mods.</remarks>
		[JsonRequired]
		public string ModStaticId
		{ get; set; }

		/// <summary>
		/// The Steam Workshop ID of the mod.
		/// </summary>
		/// <remarks>Optional. If set, the launcher will allow a locally installed copy of this mod to be converted back to its Steam version.</remarks>
		public long? SteamWorkshopId
		{ get; set; }

		/// <summary>
		/// The version number of the currently installed mod.
		/// </summary>
		/// <remarks>Required. This is used for detecting updates and workshop update errors.</remarks>
		[JsonRequired]
		public Version Version
		{ get; set; }

		/// <summary>
		/// The title of the mod to display in the launcher.
		/// </summary>
		/// <remarks>Optional. If not set, the launcher will use the title from 'mod.yaml'.</remarks>
		public string? Title
		{ get; set; }

		/// <summary>
		/// The authors that created the mod.
		/// </summary>
		/// <remarks>Optional. If set, the launcher will display a list of authors below the mod title.</remarks>
		public AuthorInfo[]? Authors
		{ get; set; }
		
		/// <summary>
		/// A list of mod static IDs that this mod depends on.
		/// </summary>
		/// <remarks>Optional. If set, the launcher will automatically disable this mod if a dependency mod is disabled.</remarks>
		public string[]? Dependencies
		{ get; set; }
		
		/// <summary>
		/// Information about how to search and sort this mod.
		/// </summary>
		/// <remarks>Optional. If missing, the launcher will assign a default category for the mod and only use the title for searching.</remarks>
		public SortInfo? Sorting
		{ get; set; }

		/// <summary>
		/// Information about how to update this mod.
		/// </summary>
		/// <remarks>Optional. If missing, the launcher will not allow updating this mod.</remarks>
		public UpdateInfo? Updates
		{ get; set; }

		/// <summary>
		/// A list of config files for this mod.
		/// The paths must be relative to the mod DLL.
		/// </summary>
		/// <remarks>Optional. If missing, the launcher will display no GUI for config files.</remarks>
		public Dictionary<string, string>? Configs
		{ get; set; }

		/// <summary>
		/// A list of any arbitrary strings that can be associated with this mod.
		/// Currently unused by Mod Launcher.
		/// </summary>
		/// <remarks>Optional. If missing, the launcher will only consider the mod title when searching.</remarks>
		public string[]? Tags
		{ get; set; }
	}
	
	/// <summary>
	/// Author information used by ONI Mod Launcher.
	/// </summary>
	public class AuthorInfo
	{
		/// <summary>
		/// The name of the author.
		/// </summary>
		/// <remarks>Required. This field is expected to exist if you have nested anything under the Authors field in the metadata.</remarks>
		[JsonRequired]
		public string Name
		{ get; set; }

		/// <summary>
		/// A general purpose url for the author.
		/// </summary>
		/// <remarks>Optional. If set, the launcher will make the author name clickable and open this url in the browser.</remarks>
		public string? Url
		{ get; set; }
		
		/// <summary>
		/// A url for the author's steam workshop page.
		/// </summary>
		/// <remarks>Optional.</remarks>
		public string? SteamWorkshopUrl
		{ get; set; }

		/// <summary>
		/// A url for the author's code repository.
		/// </summary>
		/// <remarks>Optional.</remarks>
		public string? RepoUrl
		{ get; set; }

		/// <summary>
		/// A URL that leads to this author's community (discord, etc.).
		/// </summary>
		/// <remarks>Optional.</remarks>
		public string? CommunityUrl
		{ get; set; }
	}

	/// <summary>
	/// Sorting and searching information used by ONI Mod Launcher.
	/// </summary>
	public class SortInfo
	{
		/// <summary>
		/// The category of the mod.
		/// </summary>
		/// <remarks>Optional.</remarks>
		public string? Category
		{ get; set; }
		
		/// <summary>
		/// ONI Mod Launcher will check these keywords when searching mods.
		/// </summary>
		/// <remarks>Optional.</remarks>
		public string[]? Keywords
		{ get; set; }
	}

	/// <summary>
	/// Mod update information used by ONI Mod Launcher.
	/// </summary>
	public class UpdateInfo
	{
		/// <summary>
		/// A URL that leads to an index file on the internet that announces new versions of mods to download.
		/// </summary>
		/// <remarks>Required</remarks>
		[JsonRequired]
		public string UpdateIndexUrl
		{ get; set; }

		/// <summary>
		/// The location of the mod files relative to the zip root.
		/// </summary>
		/// <remarks>Optional</remarks>
		public string? NestedPath
		{ get; set; }

		/// <summary>
		/// A list of relative file paths for files to back up before updating and restore after updating.
		/// </summary>
		/// <remarks>Optional</remarks>
		public string[]? PreservedFiles
		{ get; set; }
	}
}
