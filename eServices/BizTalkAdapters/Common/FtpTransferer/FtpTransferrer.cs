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
using Common.Logging;
using FluentFTP;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	class FtpTransferrer : FtpTransferrerAbstract
	{
		IFtpClient ftpClient;
		string startFolder;

		internal FtpTransferrer() : this(new FtpClient()) { }

		internal FtpTransferrer(IFtpClient client) : base()
		{
			this.ftpClient = client;
		}
		 
		FtpTransferrerProperties ftpProperties = new FtpTransferrerProperties();

		#region Interface Implementations

		public override void ReadLocationConfiguration(XmlDocument configDOM)
		{
			this.ftpProperties.ReadLocationConfiguration(configDOM);
		}

		public override void Open()
		{
			try
			{
				this.ftpClient.Host = this.Server;
				this.ftpClient.Port = this.Port;
				this.ftpClient.Credentials = new NetworkCredential(this.UserName, this.Password);
				this.ftpClient.ConnectTimeout = this.Timeout;
				this.ftpClient.ReadTimeout = this.Timeout;
				this.ftpClient.DataConnectionConnectTimeout = this.Timeout;
				this.ftpClient.DataConnectionReadTimeout = this.Timeout;
				if (this.Logger != null && this.Logger.IsTraceEnabled)
					this.ftpClient.ControlDialogTrace = new TransferrerTraceLogger { Logger = this.Logger };
				this.ftpClient.EnableThreadSafeDataConnections = false;
				switch (this.ftpProperties.Mode)
				{
					case "Passive":
						ftpClient.DataConnectionType = FtpDataConnectionType.AutoPassive;
						break;
					case "Active":
						ftpClient.DataConnectionType = FtpDataConnectionType.AutoActive;
						break;
					default:
						ftpClient.DataConnectionType = (FtpDataConnectionType)Enum.Parse(typeof(FtpDataConnectionType), this.ftpProperties.Mode);
						break;
				}
				this.ftpClient.EncryptionMode = (FtpEncryptionMode)Enum.Parse(typeof(FtpEncryptionMode), this.ftpProperties.FtpsMode);
				if (!this.ftpProperties.ValidateServerCert)
					this.ftpClient.ValidateCertificate += new FtpSslValidation((FtpClient control, FtpSslValidationEventArgs e) => e.Accept = true);
				if (!String.IsNullOrWhiteSpace(this.ftpProperties.ThumbprintClientCert))
				{
					var store = new X509Store(StoreLocation.CurrentUser);
					var certs = store.Certificates.Find(X509FindType.FindByThumbprint, Regex.Replace(this.ftpProperties.ThumbprintClientCert, "[^0-9a-fA-F]", ""), true);
					this.ftpClient.ClientCertificates.AddRange(certs);
				}
				this.ftpClient.DataConnectionEncryption = this.ftpProperties.DataEncryption;
				this.ftpClient.Connect();
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public override void Close()
		{
			try
			{
				this.ftpClient.Disconnect();
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public override Stream GetFile(string path)
		{
			try
			{
				string localStartFolder = null;
				if (this.ftpProperties.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
					localStartFolder = SetWorkingDirectory(ref path);
				var result = new VirtualStream();
				using (var file = (FtpDataStream)ftpClient.OpenRead(path))
				{
					var buffer = new byte[4096];
					int bytesread = 0;
					while ((bytesread = file.Read(buffer, 0, buffer.Length)) > 0)
					{
						if (this.aborting)
							return null;
						result.Write(buffer, 0, bytesread);
					}
					file.Close();
				}
				result.Position = 0;
				if (this.ftpProperties.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(localStartFolder))
					ftpClient.SetWorkingDirectory(localStartFolder);
				return result;
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public override void PutFile(string path, Stream source)
		{
			try
			{
				string localStartFolder = null;
				if (this.ftpProperties.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
					localStartFolder = SetWorkingDirectory(ref path);
				using (var target = (FtpDataStream)ftpClient.OpenWrite(path))
				{
					var buffer = new byte[4096];
					int bytesread = 0;
					while ((bytesread = source.Read(buffer, 0, buffer.Length)) > 0)
					{
						if (this.aborting)
							return;
						target.Write(buffer, 0, bytesread);
					}
					target.Close();
				}
				if (this.ftpProperties.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(localStartFolder))
					ftpClient.SetWorkingDirectory(localStartFolder);
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public override void RenameFile(string path, string dest)
		{
			try
			{
				string localStartFolder = null;
				if (this.ftpProperties.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
				{
					dest = GetRenameRelativePath(path, dest);
					localStartFolder = SetWorkingDirectory(ref path);
				}
				try
				{
					if (ftpClient.GetFileSize(dest) >= 0)
						ftpClient.DeleteFile(dest);
				}
				catch (Exception) { }
				ftpClient.Rename(path, dest);
				if (this.ftpProperties.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(localStartFolder))
					ftpClient.SetWorkingDirectory(localStartFolder);
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public override void DeleteFile(string path)
		{
			try
			{
				string localStartFolder = null;
				if (this.ftpProperties.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(Path.GetDirectoryName(path)))
					localStartFolder = SetWorkingDirectory(ref path);
				try
				{
					ftpClient.DeleteFile(path);
				}
				catch (FtpCommandException) { }
				if (this.ftpProperties.MoveWorkingDirectory && !String.IsNullOrWhiteSpace(localStartFolder))
					ftpClient.SetWorkingDirectory(localStartFolder);
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		public override bool FileExists(string path)
		{
			try
			{
				return ftpClient.FileExists(path);
			}
			catch (FtpException ex)
			{
				throw new TransferrerException(ex);
			}
		}

		#endregion

		#region Abstract Implementation

		internal override IEnumerable<TransferrerFileInfo> GetListing(string folder)
		{
			var options = new FtpListOption();
			if (this.ftpProperties.UseNLST)
				options |= FtpListOption.ForceNameList | FtpListOption.Modify | FtpListOption.Size;
			if (this.ftpProperties.MoveWorkingDirectory && !string.IsNullOrWhiteSpace(startFolder))
				options |= FtpListOption.NoPath;
			return ftpClient.GetListing(folder, options)
				.Where(f => f.Type == FtpFileSystemObjectType.File)
				.Select(f => new TransferrerFileInfo { Name = f.Name, Size = f.Size, Timestamp = f.Modified });
		}

		protected override void PreMoveWorkingDirectory(ref string folder)
		{
			startFolder = "";
			if (this.ftpProperties.MoveWorkingDirectory && !string.IsNullOrWhiteSpace(folder))
			{
				startFolder = ftpClient.GetWorkingDirectory().Replace('\\', '/');
				this.ftpClient.SetWorkingDirectory(folder);
				folder = String.Empty;
			}
		}

		protected override void PostMoveWorkingDirectory()
		{
			if (this.ftpProperties.MoveWorkingDirectory && !string.IsNullOrWhiteSpace(startFolder))
			{
				this.ftpClient.SetWorkingDirectory(startFolder);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (this)
				{
					if (!disposed)
					{
						if (this.Logger != null)
							TransferrerHelpers.Log(this, this.Logger, LogLevel.Debug, "Disposing transferrer. Hash Code = {0}", this.GetHashCode());
						if (this.ftpClient != null)
						{
							if (this.Logger != null)
								TransferrerHelpers.Log(this, this.Logger, LogLevel.Debug, "Disposing FTP client.", this.GetHashCode());
							if (this.ftpClient != null)
							{
								this.aborting = true;
								Task.Factory.StartNew(c =>
								{
									try
									{
										var client = (IFtpClient)c;
										client.Disconnect();
										client.Dispose();
									}
									catch (Exception) { }
								}, this.ftpClient, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Current).Wait(TransferrerProperties.TERMINATE_WAIT_LIMIT);
							}
							if (this.Logger != null)
								TransferrerHelpers.Log(this, this.Logger, LogLevel.Debug, "FTP client disposed.", this.GetHashCode());
						}
						if (this.Logger != null)
							TransferrerHelpers.Log(this, this.Logger, LogLevel.Debug, "Transferrer disposed. Hash Code = {0}", this.GetHashCode());
					}
					this.ftpClient = null;
					this.disposed = true;
				}
			}
		}

		#endregion

		string SetWorkingDirectory(ref string path)
		{
			if (String.IsNullOrWhiteSpace(path))
				return null;

			path = path.Trim().Replace('\\', '/');
			if (!path.Contains('/'))
				return null;

			string fileName = Path.GetFileName(path);
			string pathDir = (path == "/" + fileName) ? "/" : path.Remove(path.Length - fileName.Length - 1);
			path = fileName;

			string localStartFolder = ftpClient.GetWorkingDirectory();
			if (pathDir == localStartFolder)
				return null;
			else
			{
				this.ftpClient.SetWorkingDirectory(pathDir);
				return localStartFolder;
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

			return String.Join("/", resultParts);
		}
	}
}
