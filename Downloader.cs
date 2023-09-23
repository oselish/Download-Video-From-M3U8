using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;


namespace DownloadFromM3U8
{
	public delegate void DownloadFinished();

	internal class Downloader
	{
		public string url { get; private set; }
		public string ffmpegPath { get; private set; }
		public string downloadPath { get; private set; }

		public string m3u8 {  get; private set; }

		public DownloadFinished downloadFinished;


		public Downloader(string url)
		{
			this.url = url;
			WebRequest webRequest = HttpWebRequest.Create(url);
			webRequest.Method = "HEAD";
			webRequest.GetResponse();

			ffmpegPath = Path.Combine(Environment.CurrentDirectory, "ffmpeg-master-latest-win64-gpl\\bin\\ffmpeg.exe");

			if (!File.Exists(ffmpegPath))
			{
				throw new Exception("ffmpeg.exe is not found");
			}

			m3u8 = GetM3U8FromHtml(url);
			if (m3u8 == null)
			{
				throw new Exception("Link to the .m3u8-playlist is not found on this page");
			}
		}


		/// <summary>
		/// Start downloading video to the folder
		/// </summary>
		/// <param name="downloadPath"></param>
		public async void StartDownloading(string downloadPath)
		{
			string arguments = $"-i \"{m3u8}\" -c copy -bsf:a aac_adtstoasc \"{downloadPath}\"";
			string program = $"\"{ffmpegPath}\"";

			var ffmpegProcessInfo = new ProcessStartInfo()
			{
				FileName = program,
				Arguments = arguments,
				UseShellExecute = false,
				CreateNoWindow = true,
			};

			Process ffmpegProcess = new Process()
			{
				StartInfo = ffmpegProcessInfo,
				EnableRaisingEvents = true
			};

			await Task.Run(() =>
			{
				ffmpegProcess.Start();
				ffmpegProcess.WaitForExit();
			});
			downloadFinished.Invoke();
		}


		/// <summary>
		/// Getting link to the .m3u8 file from HTML page
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		private string GetM3U8FromHtml(string url)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			req.Method = "GET";
			req.CookieContainer = new CookieContainer();
			req.AllowAutoRedirect = true;

			string m3u8Dirty = "";

			HttpWebResponse Response = (HttpWebResponse)req.GetResponse();
			StreamReader sr = new StreamReader(Response.GetResponseStream());
			string htmlcode = sr.ReadToEnd();

			foreach (var line in htmlcode.Split('\n'))
				if (line.Contains("m3u8"))
				{
					m3u8Dirty = line;
					break;
				}

			if (m3u8Dirty == null)
				return null;

			string m3u8Clean = "";

			bool startParse = false;
			

			for (int i = 0; i < m3u8Dirty.Length; i++)
			{
				if (m3u8Dirty.Substring(i, 5) == "https") 
					startParse = true;
				if (startParse)
				{
					if (m3u8Dirty.Substring(i, 5) == ".m3u8")
					{
						m3u8Clean += ".m3u8";
						break;
					}
					m3u8Clean += m3u8Dirty[i];
				}
			}

			return m3u8Clean;
		}
	}
}
