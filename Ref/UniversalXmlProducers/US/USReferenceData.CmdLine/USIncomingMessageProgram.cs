using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	public static class USIncomingMessageProgram
	{
		public static void USIncomingMessageRequest(string[] args)
		{
			var legalRequestCode = new[]
			{
				Constants.USIncomingMessageRequest.F101,
				Constants.USIncomingMessageRequest.F104,
				Constants.USIncomingMessageRequest.F111
			};
			if (args.Length <= 1 || string.IsNullOrEmpty(args[1]) || !legalRequestCode.Contains(args[1]))
			{
				Console.WriteLine("Invalid second argument. It should be " + string.Join(" or ", legalRequestCode) + ".");
				return;
			}

			var identifier = args[1];
			using (var adapter = CreateNewAdapter())
			{
				var sendResult = USIncomingMessageQuery.SendMessages(adapter, identifier);
				Console.WriteLine($"Send {identifier} query message {sendResult}.");
			}
		}

		public static void USIncomingMessageDownload(string outputPath)
		{
			using (var adapter = CreateNewAdapter())
			{
				var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.Instance.ScheduleKCsvFileName);
				var sendResult = new USIncomingMessageDownloader(adapter, outputPath, filePath).Run();
				Console.WriteLine($"US incoming message download {sendResult}.");
			}
		}

		public static eHubAdapter CreateNewAdapter()
		{
			return new eHubAdapter(ApplicationConfig.Instance.USIncomingMessageEHubServerAddress, ApplicationConfig.Instance.USIncomingMessageEHubClientID, ApplicationConfig.Instance.USIncomingMessageEHubClientPassword);
		}
	}
}
