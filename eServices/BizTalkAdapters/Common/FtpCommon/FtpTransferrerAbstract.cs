using Common.Logging;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using WTG.ErrorReporting;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	abstract class FtpTransferrerAbstract : ITransferrer
	{
		public string Server { get; set; }
		public int Port { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public int Timeout { get; set; }
		public ILog Logger { get; set; }
		public string ConfigDom { get; set; }
		public CancellationToken CancelToken { get; set; }
		public bool HighPriority { get; set; }
		protected bool disposed;
		protected bool aborting;

		protected FtpTransferrerAbstract()
		{
			this.CancelToken = CancellationToken.None;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~FtpTransferrerAbstract()
		{
			Dispose(false);
		}

		internal abstract IEnumerable<TransferrerFileInfo> GetListing(string folder);
		protected abstract void Dispose(bool disposing);

		#region Interface Implementations

		public abstract void ReadLocationConfiguration(XmlDocument configDOM);
		public abstract void Open();
		public abstract void Close();
		public abstract Stream GetFile(string path);
		public abstract void PutFile(string path, Stream source);
		public abstract void RenameFile(string path, string dest);
		public abstract void DeleteFile(string path);
		public abstract bool FileExists(string path);

		public virtual IEnumerable<TransferrerFileInfo> ListFiles(TransferrerProperties.Receive.Location location, TransferrerProperties.Receive properties, CancellationTokenSource tokenSource)
		{
			try
			{
				string folder = location.Folder;
				string fileMask = location.FileMask;
				Regex localFileMatch = null;
				Stopwatch stopwatch = new Stopwatch();

				stopwatch.Start();

				if (string.IsNullOrWhiteSpace(folder)) folder = string.Empty;
				if (string.IsNullOrWhiteSpace(fileMask)) fileMask = "*";

				PreMoveWorkingDirectory(ref folder);

				IEnumerable<TransferrerFileInfo> files;
				if (properties.ServerSideFiltering)
				{
					var pathWithFileMask = string.Format("{0}{1}{2}", folder,
						folder.EndsWith("/") || folder.EndsWith("\\") ? "" : "/", fileMask);
					TransferrerHelpers.Log(this, Logger, LogLevel.Debug, "Server-side filtering: {0}", pathWithFileMask);
					files = GetListing(pathWithFileMask);
				}
				else
				{
					localFileMatch = new Regex("^" + fileMask.Replace(".", "\\.").Replace("*", ".*") + "$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
					TransferrerHelpers.Log(this, Logger, LogLevel.Debug, "Client-side filtering: Folder='{0}', Regex='{1}'", folder, localFileMatch.ToString());
					files = GetListing(folder);
				}

				if (tokenSource.IsCancellationRequested)
					return null;

				var output = FilterAndSort(location, properties, tokenSource, folder, files, localFileMatch);

				// If FilterAndSort above got cancelled, still restore the working directory
				PostMoveWorkingDirectory();

				if (tokenSource.IsCancellationRequested)
					return null;

				stopwatch.Stop();
				if (stopwatch.Elapsed.TotalSeconds > properties.ReceiveProcessingTimeWarning)
				{
					var message = string.Format("FTP Processing took {0} seconds, which is longer than the warn limit of {1}s", stopwatch.Elapsed.TotalSeconds, properties.ReceiveProcessingTimeWarning);
					TransferrerHelpers.Log(this, Logger, LogLevel.Info, message);
					ReportToIssueManager("Slow FTP receive processing", message, properties, location, new Exception(message), tokenSource);
				}

				return output;
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		protected virtual void PreMoveWorkingDirectory(ref string folder)
		{
			// Do Nothing
		}

		protected virtual void PostMoveWorkingDirectory()
		{
			// Do Nothing
		}

		#endregion

		internal virtual IEnumerable<TransferrerFileInfo> FilterAndSort(TransferrerProperties.Receive.Location location, TransferrerProperties.Receive properties, CancellationTokenSource tokenSource, string folder, IEnumerable<TransferrerFileInfo> files, Regex fileMatch)
		{
			var serverListCount = 0;

			files = files.Where(f =>
			{
				serverListCount++;

				var isMatched = fileMatch?.IsMatch(f.Name) ?? true;
				if (isMatched && f.Size == 0)
				{
					HandleEmptyFiles(properties, folder, f.Name);
					return false;
				}

				return isMatched;
			});

			if (!string.IsNullOrWhiteSpace(properties.FlagFile))
			{
				TransferrerHelpers.Log(this, Logger, LogLevel.Debug, "Checking for flag files.");
				files = files.Where(item =>
				{
					string flagName = FileDownloadJob.ReplaceFileNamePlaceholders(properties.FlagFile, item.Name);
					var result = flagName != item.Name &&
						   this.FileExists(Path.Combine(location.Folder, flagName).Replace('\\', '/'));
					if (result)
						TransferrerHelpers.Log(this, Logger, LogLevel.Debug, "Flag file found for file: {0}.", item.Name);
					return result;
				});
			}

			if (properties.SortOrder == "Timestamp")
			{
				TransferrerHelpers.Log(this, Logger, LogLevel.Debug, "Sorting file list into timestamp order.");
				files = files.OrderBy(x => x.Timestamp);
			}
			else if (properties.SortOrder == "Name")
			{
				TransferrerHelpers.Log(this, Logger, LogLevel.Debug, "Sorting file list into name order.");
				files = files.OrderBy(x => x.Name);
			}

			if (tokenSource.IsCancellationRequested)
			{
				TransferrerHelpers.Log(this, Logger, LogLevel.Debug, "Cancellation was requested.");
				return null;
			}

			var output = files.ToArray();

			TransferrerHelpers.Log(this, Logger, LogLevel.Info, "The number of file(s) will be downloaded/The number of file(s) listed on server: {0}/{1}.", output.Length, serverListCount);

			if (serverListCount - output.Length > properties.DownloadExcludedFilesLimit)
			{
				var message = string.Format("Excluded files limit exceeded. {0} out of {1} listed files were excluded", (serverListCount - output.Length), serverListCount);
				TransferrerHelpers.Log(this, Logger, LogLevel.Info, message);
				ReportToIssueManager("DownloadExcludedFilesLimit exceeded", message, properties, location, new Exception(message), tokenSource);
			}

			return output;
		}

		internal virtual void ReportToIssueManager(string subject, string message, TransferrerProperties.Receive properties, TransferrerProperties.Receive.Location location, Exception ex, CancellationTokenSource tokenSource)
		{

			var token = tokenSource?.Token ?? CancellationToken.None;
			TransferrerHelpers.ReportToIssueManager(this, this.Logger, subject, message, properties, location.Uri, ex, token);
		}

		void HandleEmptyFiles(TransferrerProperties.Receive properties, string folder, string name)
		{
			if (properties.EmptyFileOption == "Discard")
			{
				string filePath = Path.Combine(folder, name).Replace('\\', '/');
				TransferrerHelpers.Log(this, Logger, LogLevel.Debug, "Deleting zero-byte file.");
				DeleteFile(filePath);
				TransferrerHelpers.Log(this, Logger, LogLevel.Info, "Deleted zero-byte file.");
			}
			else
			{
				TransferrerHelpers.Log(this, Logger, LogLevel.Debug, "Ignoring zero-byte file.");
			}
		}

		public override int GetHashCode()
		{
			return hash;
		}
		readonly int hash = (int)(TransferrerHelpers.Rng.NextDouble() * int.MaxValue);
	}
}