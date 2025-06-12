using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using FluentFTP;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hawking.CSI.Plugins.FTP
{
	public class FtpClient : IDisposable
	{
		Func<FluentFTP.IFtpClient> ftpClientFactory;
        private readonly ILogger logger;

        //XmlDocument configXml;
        FluentFTP.IFtpClient ftpClient;
		// string server;
		// int port;
		// string user;
		// string password;
		// string mode;
		// int timeout;
		// bool useNLST;
		// bool moveWorkingDirectory;
		// string ftpsMode;
		// bool validateServerCert;
		// string thumbprintClientCert;
		// bool dataEncryption;
		// bool disposed;
		FtpOptions options;

		public FtpClient() : this(() => new FluentFTP.FtpClient(), NullLoggerFactory.Instance.CreateLogger<FtpClient>()) { }

		internal FtpClient(Func<FluentFTP.IFtpClient> ftpClientFactory, ILogger<FtpClient> logger)
		{
			this.ftpClientFactory = ftpClientFactory;
            this.logger = logger;
        }

		public async Task OpenAsync(FtpOptions options, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			this.options = options;
			ftpClient = ftpClientFactory();
			ftpClient.Host = options.Server;
			ftpClient.Port = options.Port;
			ftpClient.Credentials = new NetworkCredential(options.User, options.Password);
			ftpClient.ConnectTimeout = options.Timeout;
			ftpClient.ReadTimeout = options.Timeout;
			ftpClient.ControlDialogTrace = new FtpTraceLogger { Log = logger };
			ftpClient.EnableThreadSafeDataConnections = false;
			switch (options.Mode)
			{
				case FtpMode.Passive:
					ftpClient.DataConnectionType = FtpDataConnectionType.AutoPassive;
					break;
				case FtpMode.Active:
					ftpClient.DataConnectionType = FtpDataConnectionType.AutoActive;
					break;
				default:
					ftpClient.DataConnectionType = (FtpDataConnectionType)Enum.Parse(typeof(FtpDataConnectionType), options.Mode.ToString());
					break;
			}
			ftpClient.EncryptionMode = (FtpEncryptionMode)Enum.Parse(typeof(FtpEncryptionMode), options.FtpsMode.ToString());

			cancellationToken.ThrowIfCancellationRequested();

			logger.LogDebug("Connecting to server.");
			await Task.Factory.StartNew(async () =>
			{
				try
				{
					var connectCancelTokenSource = new CancellationTokenSource();
					connectCancelTokenSource.CancelAfter(options.Timeout);
					await Task.Factory.StartNew(ftpClient.Connect, connectCancelTokenSource.Token);
				}
				catch (OperationCanceledException)
				{
					throw new MessagingException("Timeout connecting to server.");
				}
			}, cancellationToken);
			logger.LogDebug("Successfully connected to server.");
		}

		public async Task CloseAsync(CancellationToken cancellationToken)
		{
			await Task.Factory.StartNew(ftpClient.Disconnect, cancellationToken);
		}

		public async Task<List<FtpFileInfo>> ListFilesAsync(string folder, string fileMask, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var list = new List<FtpFileInfo>();
			var mask = new Regex(ConvertFileMaskToRegexPattern(fileMask), RegexOptions.IgnoreCase);

			var listOptions = new FtpListOption();
			if (options.UseNLST)
				listOptions |= FtpListOption.ForceNameList | FtpListOption.Modify | FtpListOption.Size;

			try
			{
				string startFolder = null;
				if (options.MoveWorkingDirectory && !string.IsNullOrWhiteSpace(folder))
				{
					startFolder = ftpClient.GetWorkingDirectory().Replace('\\', '/');
					ftpClient.SetWorkingDirectory(folder);
					folder = String.Empty;
					listOptions |= FtpListOption.NoPath;
				}

				cancellationToken.ThrowIfCancellationRequested();

				await Task.Factory.StartNew(() =>
				{
					list = ftpClient.GetListing(folder, listOptions)
						.Where(f => f.Type == FtpFileSystemObjectType.File && mask.IsMatch(f.Name))
						.Select(f => new FtpFileInfo { Folder = startFolder ?? folder, Name = f.Name, Size = f.Size, Timestamp = f.Modified }).ToList();
				}, cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();

				if (options.MoveWorkingDirectory && !string.IsNullOrWhiteSpace(startFolder))
					await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(startFolder), cancellationToken);
				return list;
			}
			catch (FtpException ex)
			{
				throw new MessagingException("FtpClient error. See inner exception for details.", ex);
			}
		}

		public async Task<Stream> GetFileAsync(string path, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				string startFolder = null;
				if (options.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
				{
					var pathValues = await SetWorkingDirectoryAsync(path, cancellationToken);
					path = pathValues.ServerPath;
					startFolder = pathValues.StartFolder;
				}

				cancellationToken.ThrowIfCancellationRequested();
				var result = new MemoryStream();
				using (var file = await Task.Factory.StartNew(() => ftpClient.OpenRead(path), cancellationToken))
				{
					var buffer = new byte[4096];
					int count;
					while ((count = await file.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
						await result.WriteAsync(buffer, 0, count, cancellationToken);
					file.Close();
				}
				result.Position = 0;

				if (options.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(startFolder))
					await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(startFolder), cancellationToken);
				return result;
			}
			catch (FtpException ex)
			{
				throw new MessagingException("FtpClient error. See inner exception for details.", ex);
			}
		}

		public async Task PutFileAsync(string path, Stream source, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				string startFolder = null;
				if (options.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
				{
					var pathValues = await SetWorkingDirectoryAsync(path, cancellationToken);
					path = pathValues.ServerPath;
					startFolder = pathValues.StartFolder;
				}

				cancellationToken.ThrowIfCancellationRequested();
				using (var target = await Task.Factory.StartNew(() => ftpClient.OpenWrite(path), cancellationToken))
				{
					var buffer = new byte[4096];
					int count;
					while ((count = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
					{
						cancellationToken.ThrowIfCancellationRequested();
						await target.WriteAsync(buffer, 0, count, cancellationToken);
					}
				}

				cancellationToken.ThrowIfCancellationRequested();
				if (options.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(startFolder))
					await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(startFolder), cancellationToken);
			}
			catch (FtpException ex)
			{
				throw new MessagingException("FtpClient error. See inner exception for details.", ex);
			}
		}

		public async Task RenameFileAsync(string path, string dest, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				string startFolder = null;
				if (options.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
				{
					dest = GetRenameRelativePath(path, dest);
					var pathValues = await SetWorkingDirectoryAsync(path, cancellationToken);
					path = pathValues.ServerPath;
					startFolder = pathValues.StartFolder;
				}
				cancellationToken.ThrowIfCancellationRequested();

				try
				{
					if (await Task.Factory.StartNew(() => ftpClient.GetFileSize(dest), cancellationToken) >= 0)
						await Task.Factory.StartNew(() => ftpClient.DeleteFile(dest), cancellationToken);
				}
				catch (Exception) { }

				cancellationToken.ThrowIfCancellationRequested();
				await Task.Factory.StartNew(() => ftpClient.Rename(path, dest), cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();
				if (options.MoveWorkingDirectory && !string.IsNullOrWhiteSpace(startFolder))
					await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(startFolder), cancellationToken);
			}
			catch (FtpException ex)
			{
				throw new MessagingException("FtpClient error. See inner exception for details.", ex);
			}
		}

		public async Task DeleteFileAsync(string path, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				string startFolder = null;
				if (options.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
				{
					var pathValues = await SetWorkingDirectoryAsync(path, cancellationToken);
					path = pathValues.ServerPath;
					startFolder = pathValues.StartFolder;
				}
				cancellationToken.ThrowIfCancellationRequested();

				try
				{
					await Task.Factory.StartNew(() => ftpClient.DeleteFile(path), cancellationToken);
				}
				catch (FtpCommandException) { }

				cancellationToken.ThrowIfCancellationRequested();
				if (options.MoveWorkingDirectory && !string.IsNullOrWhiteSpace(startFolder))
					await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(startFolder), cancellationToken);
			}
			catch (FtpException ex)
			{
				throw new MessagingException("FtpClient error. See inner exception for details.", ex);
			}
		}

		public async Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				return await Task.Factory.StartNew(() => ftpClient.FileExists(path), cancellationToken);
			}
			catch (FtpException ex)
			{
				throw new MessagingException("FtpClient error. See inner exception for details.", ex);
			}
		}

		async Task<PathValues> SetWorkingDirectoryAsync(string path, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (string.IsNullOrWhiteSpace(path)) return new PathValues { ServerPath = path };

			path = path.Trim().Replace('\\', '/');
			if (!path.Contains('/')) return new PathValues { ServerPath = path };

			var fileName = Path.GetFileName(path);
			var pathDir = (path == "/" + fileName) ? "/" : path.Remove(path.Length - fileName.Length - 1);
			path = fileName;

			var startFolder = await Task.Factory.StartNew(() => ftpClient.GetWorkingDirectory(), cancellationToken);

			if (pathDir == startFolder)
				return new PathValues { ServerPath = path };
			else
			{
				cancellationToken.ThrowIfCancellationRequested();
				await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(pathDir), cancellationToken);
				return new PathValues { ServerPath = path, StartFolder = startFolder };
			}
		}

		static string GetRenameRelativePath(string source, string dest)
		{
			var sourceParts = source.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			sourceParts.RemoveAt(sourceParts.Count - 1);
			var destParts = dest.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			var resultParts = new List<string>();

			for (int i = 0; i < sourceParts.Count && i < destParts.Count; i++)
			{
				if (sourceParts[i] == destParts[i])
				{
					sourceParts.RemoveAt(i);
					destParts.RemoveAt(i);
					i--;
				}
				else
					break;
			}

			for (int i = 0; i < sourceParts.Count; i++)
			{
				resultParts.Add("..");
			}
			resultParts.AddRange(destParts);

			return string.Join("/", resultParts);
		}

		private static string ConvertFileMaskToRegexPattern(string filemask)
		{
			if (String.IsNullOrWhiteSpace(filemask)) filemask = "*";
			return "^" + filemask.Replace(".", "\\.").Replace("*", ".*") + "$";
		}

		struct PathValues
		{
			public string ServerPath { get; set; }
			public string StartFolder { get; set; }
		}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
        			if (ftpClient != null)
        			{
        				try
        				{
        					if (ftpClient.IsConnected)
        						ftpClient.Disconnect();
        					ftpClient.Dispose();
        				}
        				catch (Exception) { }
        			}
                }

				ftpClient = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
