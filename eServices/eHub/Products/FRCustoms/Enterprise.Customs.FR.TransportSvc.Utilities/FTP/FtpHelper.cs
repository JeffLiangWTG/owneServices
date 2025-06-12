using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Renci.SshNet;

namespace Enterprise.Customs.FR.TransportSvc.Utilities
{
	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
	[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "The object is supposed to be disposed by a caller")]
	public class FtpHelper : IDisposable
	{
		public FtpHelper()
		{
		}

		public void Dispose()
		{
			if (sftp != null)
			{
				sftp.Dispose();
			}
		}

		public SftpClient Sftp
		{
			get
			{
				if (sftp == null)
				{
					var connInfo = GetFTPConnexionInfo();
					sftp = new SftpClient(connInfo);
				}

				return sftp;
			}
		}
		SftpClient sftp;

		#region Methods

		public static bool CheckFTPSettings(Logger logger)
		{
			if (logger != null)
			{
				FtpHost = ConfigurationManager.AppSettings["FTP_Host"];
				if (string.IsNullOrEmpty(FtpHost))
				{
					logger.AddNotification("FTP host is not valid. Check the app settings.", Notification.MessageType.Error, Notification.Events.FtpParamsError, verboseModeOnly: false);
					return false;
				}

				FtpUser = ConfigurationManager.AppSettings["FTP_User"];
				if (string.IsNullOrEmpty(FtpUser))
				{
					logger.AddNotification("FTP username is not valid. Check the app settings.", Notification.MessageType.Error, Notification.Events.FtpParamsError, verboseModeOnly: false);
					return false;
				}

				var privateKeyDirectory = ConfigurationManager.AppSettings["FTP_PrivateKeyFileDirectory"];
				if (string.IsNullOrEmpty(privateKeyDirectory))
				{
					logger.AddNotification("FTP private key is not valid. Check the app settings.", Notification.MessageType.Error, Notification.Events.FtpParamsError, verboseModeOnly: false);
					return false;
				}

				FtpPrivateKeyPath = Path.Combine(privateKeyDirectory, FtpUser);
				if (!File.Exists(FtpPrivateKeyPath))
				{
					logger.AddNotification("FTP private key file not found: " + FtpPrivateKeyPath + ". Check the app settings.", Notification.MessageType.Error, Notification.Events.FtpParamsError, verboseModeOnly: false);
					return false;
				}

				FtpHost = ConfigurationManager.AppSettings["FTP_Host"];
				if (string.IsNullOrEmpty(FtpHost))
				{
					logger.AddNotification("FTP host is not valid. Check the app settings.", Notification.MessageType.Error, Notification.Events.FtpParamsError, verboseModeOnly: false);
					return false;
				}
			}

			return true;
		}

		static ConnectionInfo GetFTPConnexionInfo()
		{
			var privateKeyAuthenticationMethod = new PrivateKeyAuthenticationMethod(FtpUser, new PrivateKeyFile(FtpPrivateKeyPath));
			var connInfo = new ConnectionInfo(FtpHost, FtpPort, FtpUser, 0, null, 0, null, null, privateKeyAuthenticationMethod);
			return connInfo;
		}

		public void UploadStreamToFTP(MemoryStream memoryStream, string ftpPath, string fileName)
		{
			try
			{
				if (string.IsNullOrEmpty(FtpDirectory))
				{
					sftp.ChangeDirectory("/" + ftpPath);
				}
				else
				{
					sftp.ChangeDirectory("/" + FtpDirectory);
				}

				var iAsync = sftp.BeginUploadFile(memoryStream, fileName + ".TMP");

				while (!iAsync.IsCompleted)
				{
					System.Threading.Thread.Sleep(30);
				}

				sftp.EndUploadFile(iAsync);

				var uploadedFileName = "/" + ftpPath + "/" + fileName + ".TMP";
				sftp.RenameFile(uploadedFileName, uploadedFileName.Replace(".TMP", ".xml"));
			}
			catch (Exception)
			{
				throw;
			}
		}

		public bool UploadFileToFTP(string content, string filePath, string fileName, string ftpPath)
		{
			using (var memoryStream = ToolBox.GenerateStreamFromString(content))
			{
				memoryStream.Seek(0, SeekOrigin.Begin);
				File.WriteAllBytes(filePath, memoryStream.ToArray());
				UploadStreamToFTP(memoryStream, ftpPath, fileName);
				return true;
			}
		}

		public static MemoryStream DownloadFromFTP(SftpClient sftp, string filePath)
		{
			var memoryStream = new MemoryStream();

			if (sftp != null)
			{
				try
				{
					var iAsync = sftp.BeginDownloadFile(filePath, memoryStream);
					sftp.EndDownloadFile(iAsync);
					while (!iAsync.IsCompleted)
					{
						System.Threading.Thread.Sleep(50);
					}
				}
				catch (Exception)
				{
					return null;
				}
			}

			return memoryStream;
		}

		/// <summary>
		/// This method is not recursive.
		/// </summary>
		public int GetFTPDirectoryContent(string ftpPath, string outputDirectory)
		{
			var fileCount = 0;
			try
			{
				sftp.ChangeDirectory("/" + ftpPath);
				var files = sftp.ListDirectory(".");
				foreach (var entry in files.OrderBy(m => m.LastWriteTime))
				{
					if (entry.Name.StartsWith(".", StringComparison.InvariantCulture) || entry.IsDirectory || !entry.Name.ToUpper(CultureInfo.CurrentCulture).EndsWith(".XML", StringComparison.InvariantCulture))
					{
						continue;
					}

					var memoryStream = DownloadFromFTP(sftp, entry.FullName);

					if (memoryStream != null && memoryStream.Length > 0)
					{
						File.WriteAllBytes(Path.Combine(outputDirectory, entry.Name), memoryStream.ToArray());
						fileCount += 1;
						memoryStream.Close();
					}

					entry.Delete();
				}
			}
			catch
			{
				throw;
			}
			return fileCount;
		}

		public void MoveFtpFile(Logger logger, string sourcePath, string targetPath)
		{
			try
			{
				sftp.RenameFile(sourcePath, targetPath);
			}
			catch
			{
				sftp.DeleteFile(sourcePath);
			}

			if (logger != null)
			{
				logger.AddNotification("Remote file moved to " + targetPath, Notification.MessageType.Information, Notification.Events.FileMoved, verboseModeOnly: false);
			}
		}

		public void FtpConnect(Logger logger)
		{
			if (logger != null)
			{
				logger.AddNotification("Connecting to SFTP...", Notification.MessageType.Information, Notification.Events.ProcessInProgress, verboseModeOnly: true);
			}
			Sftp.Connect();

			if (logger != null)
			{
				logger.AddNotification("...connected.", Notification.MessageType.Information, Notification.Events.ProcessInProgress, verboseModeOnly: true);
			}
		}

		public void DisconnectAndWait(Logger logger)
		{
			Sftp.Disconnect();
			while (Sftp.IsConnected)
			{
				System.Threading.Thread.Sleep(50);
			}

			if (logger != null)
			{
				logger.AddNotification("Disconnected from SFTP.", Notification.MessageType.Information, Notification.Events.ProcessInProgress, verboseModeOnly: true);
			}
		}

		public static string GetFtpPathForUpload(bool isUCC6, bool isTest)
		{
			var sftpAdditionalSubDirectory = isUCC6 ? (isTest ? "/UCC6" : "/UCC6_PROD") : string.Empty;
			var ftpPath = FtpReceptionDirectory + sftpAdditionalSubDirectory;
			return ftpPath;
		}

		#endregion

		public static string FtpHost = ApplicationConfig.Instance.FTP_Host;
		public static int FtpPort = ApplicationConfig.Instance.FTP_Port == 0 ? 22 : ApplicationConfig.Instance.FTP_Port;
		public static string FtpUser = ApplicationConfig.Instance.FTP_User;
		public static string FtpPrivateKeyPath = Path.Combine(ApplicationConfig.Instance.FTP_PrivateKeyFileDirectory, ConfigurationManager.AppSettings["FTP_User"]);
		public static string FtpDirectory = string.Empty;
		public static string FtpSendDirectory = ApplicationConfig.Instance.FTPSendDirectory;
		public static string FtpReceptionDirectory = ApplicationConfig.Instance.FTPReceptionDirectory;

		public MemoryStream MessageStream { get; }
		public string LogInformation { get; set; } = string.Empty;
		public string FTPFileName { get; set; } = string.Empty;
		public bool IsFtpTest { get; set; }
	}
}
