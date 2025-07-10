using System.Globalization;
using System;
using CargoWise.RefDbRepo.XmlProducer.Common;
using CargoWise.RefDbRepo.AsycudaReferenceData.Services;
using CargoWise.RefDbRepo.AsycudaReferenceData.Business;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.CmdLine;

class Program
{
	static async Task<int> Main(string[] args)
	{
		await ProduceXml(args);
		return (int)ProducerStatus.Success;
	}

	static async Task ProduceXml(string[] args)
	{
		if (args.Length != 0)
		{
			var functionToRun = args[0].ToUpper(CultureInfo.InvariantCulture);
			switch (functionToRun)
			{
				case Constants.ProgramFunctions.NCExchangeRates:
					await new NCExchangeRateProcessor(ApplicationConfig.OutputPath).Run();
					break;
				default:
					throw new ArgumentException($"Invalid argument entered: {functionToRun}");
			}
		}
		else
		{
			throw new ArgumentException("No arguments entered.");
		}
	}
}
