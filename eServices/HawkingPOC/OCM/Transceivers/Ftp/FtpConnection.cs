using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentFTP;

namespace OcmPoc.Transceivers.Ftp
{
	class FtpConnection : ISendingConnection, IReceivingConnection
	{
		readonly IFtpClient ftpClient;
		readonly string sendDirectory;
		readonly string receiveDirectory;
		
		public FtpConnection(IFtpClient ftpClient, FtpConfiguration config)
		{
			this.ftpClient = ftpClient;
			ftpClient.Encoding = Encoding.UTF8;
			ftpClient.UploadDataType = FtpDataType.Binary;
			ftpClient.DownloadDataType = FtpDataType.Binary;

			sendDirectory = config.SendDirectory;
			receiveDirectory = config.ReceiveDirectory;
			PollInterval = config.PollInterval;

			Console.WriteLine($"FTP Connection created.  Send = {sendDirectory}, Receive = {receiveDirectory}");
		}

		public TimeSpan PollInterval { get; }

		public async Task SendAsync(string name, Stream content)
		{
			CheckConnection();

			var fullName = sendDirectory.GetFtpPath(name);

			Console.WriteLine($"Sending {fullName}");

			await ftpClient.UploadAsync(content, fullName);
		}

		public async Task<IEnumerable<string>> ListAsync()
		{
			CheckConnection();

			var fullNames = await ftpClient.GetNameListingAsync(receiveDirectory.GetFtpPath());

			return fullNames.Select(fullName => fullName.GetFtpFileName());
		}

		public async Task<Stream> ReceiveAsync(string name)
		{
			CheckConnection();

			var fullName = receiveDirectory.GetFtpPath(name);
			Console.WriteLine($"Downloading {fullName}");

			var bytes = await ftpClient.DownloadAsync(fullName);
//			bytes = Encoding.UTF8.GetBytes(Encoding.Unicode.GetString(bytes).Trim());

			return new MemoryStream(bytes);
		}

		public async Task AcknowledgeAsync(string name)
		{
			CheckConnection();

			var fullName = receiveDirectory.GetFtpPath(name);
			await ftpClient.DeleteFileAsync(fullName);
		}

		public async Task ConnectAsync()
		{
			if (ftpClient.IsConnected) { return; }

			await ftpClient.ConnectAsync();
		}

		public async Task DisconnectAsync()
		{
			await ftpClient.DisconnectAsync();
		}

		void CheckConnection([CallerMemberName] string method = null)
		{
			if (!ftpClient.IsConnected)
			{
				throw new Exception($"Call {nameof(ConnectAsync)} before {method}");
			}
		}
	}
}
