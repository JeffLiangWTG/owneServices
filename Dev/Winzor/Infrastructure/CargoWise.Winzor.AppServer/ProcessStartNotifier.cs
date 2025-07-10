using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CargoWise.Winzor.AppServer
{
	public interface IProcessStartNotifier
	{
		void ProcessStarted();
	}

	public class ProcessStartNotifier : IProcessStartNotifier
	{
		readonly IConfiguration config;
		readonly IServer server;
		readonly ILogger<ProcessStartNotifier> logger;

		public const string OutputDirectoryConfigKey = "BlazorAppProcInfoDirectory";
		public const string EventNameConfigKey = "SignalEventWhenStarted";

		// Keeps the stream alive so that the file handle doesn't get disposed which would cause the file to get deleted as it is opened with FileOptions.DeleteOnClose
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Required as per the above comment")]
		static FileStream processFileStream;

		public ProcessStartNotifier(IConfiguration config, IServer server, ILogger<ProcessStartNotifier> logger)
		{
			this.config = config;
			this.server = server;
			this.logger = logger;
		}

		public void ProcessStarted()
		{
			var blazorAppProcInfoDirectory = config.GetValue<string>(OutputDirectoryConfigKey);

			if (!string.IsNullOrEmpty(blazorAppProcInfoDirectory))
			{
#pragma warning disable CA1305 // Specify IFormatProvider
				var path = Path.Combine(blazorAppProcInfoDirectory, Environment.ProcessId + ".json");
				logger.LogInformation($"Writing process start notifier to {path}");
#pragma warning restore CA1305 // Specify IFormatProvider
				var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.DeleteOnClose);
				var address = server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault().Replace("[::]", "localhost", StringComparison.OrdinalIgnoreCase);
				var contents = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { UniqueId = Guid.NewGuid(), Address = address }));
				stream.Write(contents);
				stream.Flush();
				processFileStream = stream;
			}

			var eventName = config.GetValue<string>(EventNameConfigKey);
			if (!string.IsNullOrEmpty(eventName))
			{
				using var ewh = EventWaitHandle.OpenExisting(eventName);
				ewh?.Set();
			}
		}
	}
}
