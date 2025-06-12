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
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;
using Common.Logging;
using FluentFTP;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Ftp
{
	public class FtpTransferrer : ITransferrer
	{
		Func<IFtpClient> ftpClientFactory;
		XmlDocument configXml;
		IFtpClient ftpClient;
		string server;
		int port;
		string user;
		string password;
		string mode;
		int timeout;
		bool useNLST;
		bool moveWorkingDirectory;
		string ftpsMode;
		bool validateServerCert;
		string thumbprintClientCert;
		bool dataEncryption;
		bool disposed;

		public FtpTransferrer() : this(() => new FtpClient()) { }

		internal FtpTransferrer(Func<IFtpClient> ftpClientFactory)
		{
			this.ftpClientFactory = ftpClientFactory;
		}

		public async Task OpenAsync(XmlDocument configXml, ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();

			this.configXml = configXml;
			server = TransferrerHelpers.GetValue<string>(configXml, "/Config/Server");
			port = TransferrerHelpers.GetValueOrDefault(configXml, "/Config/Port", 21);
			user = TransferrerHelpers.GetValue<string>(configXml, "/Config/User");
			password = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/Password");
			mode = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/Mode", "Passive");
			timeout = TransferrerHelpers.GetValueOrDefault(configXml, "/Config/Timeout", 90000);
			useNLST = TransferrerHelpers.GetValueOrDefault(configXml, "/Config/UseNLST", false);
			moveWorkingDirectory = TransferrerHelpers.GetValueOrDefault(configXml, "/Config/MoveWorkingDirectory", false);
			ftpsMode = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/FtpsMode", "None");
			validateServerCert = TransferrerHelpers.GetValueOrDefault(configXml, "/Config/ValidateServerCert", false);
			thumbprintClientCert = TransferrerHelpers.GetValueOrDefault<string>(configXml, "/Config/ThumbprintClientCert");
			dataEncryption = TransferrerHelpers.GetValueOrDefault(configXml, "/Config/DataEncryption", false);

			ftpClient = ftpClientFactory();
			ftpClient.Host = server;
			ftpClient.Port = port;
			ftpClient.Credentials = new NetworkCredential(user, password);
			ftpClient.ConnectTimeout = timeout;
			ftpClient.ReadTimeout = timeout;
			ftpClient.ControlDialogTrace = new TransferrerTraceLogger { Log = log };
			ftpClient.EnableThreadSafeDataConnections = false;
			switch (mode)
			{
				case "Passive":
					ftpClient.DataConnectionType = FtpDataConnectionType.AutoPassive;
					break;
				case "Active":
					ftpClient.DataConnectionType = FtpDataConnectionType.AutoActive;
					break;
				default:
					ftpClient.DataConnectionType = (FtpDataConnectionType)Enum.Parse(typeof(FtpDataConnectionType), mode);
					break;
			}
			ftpClient.EncryptionMode = (FtpEncryptionMode)Enum.Parse(typeof(FtpEncryptionMode), ftpsMode);

			cancelToken.ThrowIfCancellationRequested();

			log.Debug("Connecting to server.");
			await Task.Factory.StartNew(async () =>
			{
				try
				{
					var connectCancelTokenSource = new CancellationTokenSource();
					connectCancelTokenSource.CancelAfter(timeout);
					await Task.Factory.StartNew(ftpClient.Connect, connectCancelTokenSource.Token);
				}
				catch (OperationCanceledException)
				{
					throw new TransferrerException("Timeout connecting to server.");
				}
			}, cancelToken);
			log.Debug("Successfully connected to server.");
		}

		public async Task CloseAsync(ILog log, CancellationToken cancelToken)
		{
			await Task.Factory.StartNew(ftpClient.Disconnect, cancelToken);
		}

		public async Task<List<TransferrerFileInfo>> ListFilesAsync(string folder, string fileMask, ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();

			var list = new List<TransferrerFileInfo>();
			var mask = new Regex(TransferrerHelpers.ConvertFileMaskToRegexPattern(fileMask), RegexOptions.IgnoreCase);

			var options = new FtpListOption();
			if (useNLST)
				options |= FtpListOption.ForceNameList | FtpListOption.Modify | FtpListOption.Size;

			try
			{
				string startFolder = null;
				if (moveWorkingDirectory && !string.IsNullOrWhiteSpace(folder))
				{
					startFolder = ftpClient.GetWorkingDirectory().Replace('\\', '/');
					ftpClient.SetWorkingDirectory(folder);
					folder = String.Empty;
					options |= FtpListOption.NoPath;
				}

				cancelToken.ThrowIfCancellationRequested();

				await Task.Factory.StartNew(() =>
				{
					list = ftpClient.GetListing(folder, options)
						.Where(f => f.Type == FtpFileSystemObjectType.File && mask.IsMatch(f.Name))
						.Select(f => new TransferrerFileInfo { Folder = startFolder ?? folder, Name = f.Name, Size = f.Size, Timestamp = f.Modified }).ToList();
				}, cancelToken);

				cancelToken.ThrowIfCancellationRequested();

				if (moveWorkingDirectory && !string.IsNullOrWhiteSpace(startFolder))
					await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(startFolder), cancelToken);
				return list;
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public async Task<Stream> GetFileAsync(string path, ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();

			try
			{
				string startFolder = null;
				if (moveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
				{
					var pathValues = await SetWorkingDirectoryAsync(path, cancelToken);
					path = pathValues.ServerPath;
					startFolder = pathValues.StartFolder;
				}

				cancelToken.ThrowIfCancellationRequested();
				var result = new VirtualStream();
				using (var file = await Task.Factory.StartNew(() => ftpClient.OpenRead(path), cancelToken))
				{
					var buffer = new byte[4096];
					int count;
					while ((count = await file.ReadAsync(buffer, 0, buffer.Length, cancelToken)) > 0)
						await result.WriteAsync(buffer, 0, count, cancelToken);
					file.Close();
				}
				result.Position = 0;

				if (moveWorkingDirectory && !String.IsNullOrWhiteSpace(startFolder))
					await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(startFolder), cancelToken);
				return result;
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public async Task PutFileAsync(string path, Stream source, ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();

			try
			{
				string startFolder = null;
				if (moveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
				{
					var pathValues = await SetWorkingDirectoryAsync(path, cancelToken);
					path = pathValues.ServerPath;
					startFolder = pathValues.StartFolder;
				}

				cancelToken.ThrowIfCancellationRequested();
				using (var target = await Task.Factory.StartNew(() => ftpClient.OpenWrite(path), cancelToken))
				{
					var buffer = new byte[4096];
					int count;
					while ((count = await source.ReadAsync(buffer, 0, buffer.Length, cancelToken)) > 0)
					{
						cancelToken.ThrowIfCancellationRequested();
						await target.WriteAsync(buffer, 0, count, cancelToken);
					}
				}

				cancelToken.ThrowIfCancellationRequested();
				if (moveWorkingDirectory && !String.IsNullOrWhiteSpace(startFolder))
					await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(startFolder), cancelToken);
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public async Task RenameFileAsync(string path, string dest, ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();

			try
			{
				string startFolder = null;
				if (moveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
				{
					dest = GetRenameRelativePath(path, dest);
					var pathValues = await SetWorkingDirectoryAsync(path, cancelToken);
					path = pathValues.ServerPath;
					startFolder = pathValues.StartFolder;
				}
				cancelToken.ThrowIfCancellationRequested();

				try
				{
					if (await Task.Factory.StartNew(() => ftpClient.GetFileSize(dest), cancelToken) >= 0)
						await Task.Factory.StartNew(() => ftpClient.DeleteFile(dest), cancelToken);
				}
				catch (Exception) { }

				cancelToken.ThrowIfCancellationRequested();
				await Task.Factory.StartNew(() => ftpClient.Rename(path, dest), cancelToken);

				cancelToken.ThrowIfCancellationRequested();
				if (moveWorkingDirectory && !string.IsNullOrWhiteSpace(startFolder))
					await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(startFolder), cancelToken);
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public async Task DeleteFileAsync(string path, ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();

			try
			{
				string startFolder = null;
				if (moveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
				{
					var pathValues = await SetWorkingDirectoryAsync(path, cancelToken);
					path = pathValues.ServerPath;
					startFolder = pathValues.StartFolder;
				}
				cancelToken.ThrowIfCancellationRequested();

				try
				{
					await Task.Factory.StartNew(() => ftpClient.DeleteFile(path), cancelToken);
				}
				catch (FtpCommandException) { }

				cancelToken.ThrowIfCancellationRequested();
				if (moveWorkingDirectory && !string.IsNullOrWhiteSpace(startFolder))
					await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(startFolder), cancelToken);
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public async Task<bool> FileExistsAsync(string path, ILog log, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();

			try
			{
				return await Task.Factory.StartNew(() => ftpClient.FileExists(path), cancelToken);
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		async Task<PathValues> SetWorkingDirectoryAsync(string path, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();

			if (string.IsNullOrWhiteSpace(path)) return new PathValues { ServerPath = path };

			path = path.Trim().Replace('\\', '/');
			if (!path.Contains('/')) return new PathValues { ServerPath = path };

			var fileName = Path.GetFileName(path);
			var pathDir = (path == "/" + fileName) ? "/" : path.Remove(path.Length - fileName.Length - 1);
			path = fileName;

			var startFolder = await Task.Factory.StartNew(() => ftpClient.GetWorkingDirectory(), cancelToken);

			if (pathDir == startFolder)
				return new PathValues { ServerPath = path };
			else
			{
				cancelToken.ThrowIfCancellationRequested();
				await Task.Factory.StartNew(() => ftpClient.SetWorkingDirectory(pathDir), cancelToken);
				return new PathValues { ServerPath = path, StartFolder = startFolder };
			}
		}

		struct PathValues
		{
			public string ServerPath { get; set; }
			public string StartFolder { get; set; }
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

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
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
					ftpClient = null;
					disposed = true;
				}
			}
		}
	}
}
