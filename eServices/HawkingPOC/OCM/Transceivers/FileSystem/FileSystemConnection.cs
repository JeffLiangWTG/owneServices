using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace OcmPoc.Transceivers.FileSystem
{
	class FileSystemConnection : ISendingConnection, IReceivingConnection
	{
		readonly DirectoryInfo sendDirectory;
		readonly DirectoryInfo receiveDirectory;

		public FileSystemConnection(FileSystemConfiguration config)
		{
			this.sendDirectory = config.SendDirectoryInfo;
			this.receiveDirectory = config.ReceiveDirectoryInfo;
			PollInterval = config.PollInterval;
		}

		public TimeSpan PollInterval { get; }

		public async Task SendAsync(string name, Stream content)
		{
			var fullName = Path.Combine(sendDirectory.FullName, name);
			using (var fileStream = new FileStream(fullName, FileMode.CreateNew, FileAccess.Write))
			{
				await content.CopyToAsync(fileStream);
				await fileStream.FlushAsync();
			}
		}

		public Task<IEnumerable<string>> ListAsync()
		{
			return Task.FromResult(receiveDirectory.EnumerateFiles().Select(fi => fi.Name));
		}

		public Task<Stream> ReceiveAsync(string name)
		{
			var fullName = Path.Combine(receiveDirectory.FullName, name);
			Stream stream = File.Open(fullName, FileMode.Open, FileAccess.Read, FileShare.None);
			return Task.FromResult(stream);
		}

		public Task AcknowledgeAsync(string name)
		{
			var fullName = Path.Combine(receiveDirectory.FullName, name);
			File.Delete(fullName);

			return Task.CompletedTask;
		}

		public Task ConnectAsync()
		{
			return Task.CompletedTask;
		}

		public Task DisconnectAsync()
		{
			return Task.CompletedTask;
		}
	}
}
