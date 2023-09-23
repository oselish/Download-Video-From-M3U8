using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DownloadFromM3U8
{
	public partial class MainWindow : Window
	{
		Downloader downloader;
		SaveFileDialog saveFileDialog;

		delegate void DownloadFinished();

		public MainWindow()
		{
			InitializeComponent();

			saveFileDialog = new SaveFileDialog();
			saveFileDialog.Title = "Save file";
			saveFileDialog.DefaultExt = ".mp4";
			saveFileDialog.Filter = $"Video in format MP4 | *{saveFileDialog.DefaultExt}";

			OpenFileButton.IsEnabled = false;
			OpenFolderButton.IsEnabled = false;

			string path = "";

			DownloadButton.Click += (s, e) =>
			{
				try
				{
					saveFileDialog.FileName = "output";
					downloader = new Downloader(UrlTextBox.Text);
					
					var showDialogResult = saveFileDialog.ShowDialog();
					
					downloader.downloadFinished += Downloaded;

					if (saveFileDialog.OverwritePrompt)
						File.Delete(saveFileDialog.FileName);

					path = Directory.GetParent(saveFileDialog.FileName).FullName;
					VideoSavePath.Text = $"Downloading {saveFileDialog.FileName}";

					if (showDialogResult == false)
						throw new Exception("Choose the folder for file");

					downloader.StartDownloading(saveFileDialog.FileName);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			};
			OpenFileButton.Click += (s, e) => Process.Start(saveFileDialog.FileName);
			OpenFolderButton.Click += (s, e) => Process.Start("explorer.exe", path);
		}

		public void Downloaded()
		{
			OpenFolderButton.IsEnabled = true;
			OpenFileButton.IsEnabled = true;
			var fileName = Path.GetFileName(saveFileDialog.FileName);
			VideoSavePath.Text = $"The file \"{fileName}\" was succesfully downloaded";
			MessageBox.Show($"The file \"{saveFileDialog.FileName}\" was succesfully downloaded", "Success", MessageBoxButton.OK, MessageBoxImage.Asterisk);
		}
	}
}
