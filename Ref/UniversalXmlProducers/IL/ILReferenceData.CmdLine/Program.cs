using System;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.ILReferenceData.CmdLine
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml(string[] cmdLineArguments)
		{
			MainAsync(cmdLineArguments).GetAwaiter().GetResult();
		}

		static async Task MainAsync(string[] cmdLineArguments)
		{
			if (cmdLineArguments.Length == 0)
			{
				throw new ArgumentException("No command line argument found.");
			}
			var logger = new Logger();

			var functionToRun = cmdLineArguments[0].ToUpper(CultureInfo.InvariantCulture);
			switch (functionToRun)
			{
				case Constants.ProgramFunctions.CustomsTableUpdate:
					await CustomsTableUpdateRequestSender.Run(logger, cmdLineArguments);
					break;

				case Constants.ProgramFunctions.CustomsExchangeRateUpdate:
					await CustomsExchangeRateUpdateRequestSender.Run(logger, cmdLineArguments);
					break;

				case Constants.ProgramFunctions.CustomsTariffUpdate:
					CustomsTariffUpdater.Run(logger, cmdLineArguments);
					break;

				case Constants.ProgramFunctions.Receive:
					new BaseReceiving(logger).Receive();
					break;

				default:
					throw new ArgumentException("Invalid Function To Run.");
			}
		}
	}
}
