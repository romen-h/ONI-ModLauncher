using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ONIModLauncher
{
	public class SteamWorkshopApi
	{
		private const string WORKSHOP_URL_GetPublishedFieldetails = "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/";
		
		private HttpClient _httpClient = new HttpClient();
		
		public async Task<(bool,long,string)> CheckForUpdate(ulong workshopId, long lastUpdateTime)
		{
			JObject reqJson = new JObject();
			reqJson["publishedfielids[0]"] = workshopId;
			reqJson["itemcount"] = 1;
			string reqBody = reqJson.ToString();
			
			HttpContent content = new StringContent(reqBody, Encoding.UTF8);
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			
			var res = await _httpClient.PostAsync(WORKSHOP_URL_GetPublishedFieldetails, content);
			string resBody = await res.Content.ReadAsStringAsync();
			JObject resJson = JObject.Parse(resBody);
			
			if (!resJson.ContainsKey("time_updated")) throw new Exception("Response missing time_updated field.");

			long timeUpdated = resJson.Value<long>("time_updated");
			
			bool updated = timeUpdated > lastUpdateTime;
			string downloadURL = null;
			
			if (updated)
			{
				if (!resJson.ContainsKey("file_url")) throw new Exception("Response missing file_url field.");
				
				downloadURL = resJson.Value<string>("file_url");
			}
			
			return (updated, timeUpdated, downloadURL);
		}
		
		public async Task DownloadFile(string url, string destination)
		{
			var ds = await _httpClient.GetStreamAsync(url);

			await using FileStream fs = new FileStream(destination, FileMode.Create, FileAccess.Write);
			await ds.CopyToAsync(fs);
		}
		
		public async Task Unzip(string zipFile, string destFolder)
		{
			throw new NotImplementedException();
		}
		
		public async Task TryUpdateMod(ONIMod mod)
		{
			if (mod.IsSteam)
			{
				if (!mod.SteamWorkshopId.HasValue) throw new Exception("Mod workshop id is null.");
				
				var updateInfo = await CheckForUpdate(mod.SteamWorkshopId.Value, mod.SteamUpdateTimestamp);
				if (!updateInfo.Item1) return; // No update to get
				
				string downloadFileName = $"{mod.SteamWorkshopId}.zip";
				string downloadFilePath = Path.Combine(GamePaths.WorkshopDownloadsFolder, downloadFileName);
				await DownloadFile(updateInfo.Item3, downloadFilePath);
				
				await Unzip(downloadFilePath, mod.Folder);
				
				mod.SteamUpdateTimestamp = updateInfo.Item2;
				ModManager.Instance.SaveModList();
			}
			else if (mod.IsLocal)
			{
				// Check if converted from workshop to local. Not supported yet.
				// Otherwise, check author's download index. Not supported yet.
			}
		}
	}
}
