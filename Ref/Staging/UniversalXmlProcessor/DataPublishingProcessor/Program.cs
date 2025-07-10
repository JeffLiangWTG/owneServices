using System;
using CargoWise.eHub.Adapter;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor
{
	public static class Program
	{
		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				var safeConnString = ApplicationConfig.SafeConnectionString;
				var commandTimeout = ApplicationConfig.CommandTimeoutInSeconds;
				var appArg = args[0];
				using (var conn = new SqlConnection(safeConnString))
				{
					conn.Open();
					var publisher = new DataPublisher(conn);
					switch (appArg)
					{
						case ApplicationConfig.ProgramArgs.AUTO:
							publisher.PublishData(commandTimeout);
							break;
						case ApplicationConfig.ProgramArgs.PUSH:
							var eHubAdapter = new eHubAdapter(ApplicationConfig.EHubGatewayServerAddress,
										  ApplicationConfig.EHubGatewayClientId,
										  ApplicationConfig.EHubGatewayClientPassword);

							var messageFactory = new MessageFactory(eHubAdapter);

							publisher.PublishData(true, messageFactory, commandTimeout);
							break;
						default:
							Console.Error.WriteLine("Incorrect argument specified.");
							break;
					}
				}
			}
			else
			{
				Console.Error.WriteLine("No arguments specified.");
			}
		}
	}
}
