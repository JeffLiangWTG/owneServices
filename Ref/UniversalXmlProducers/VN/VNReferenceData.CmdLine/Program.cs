using System;
using System.Globalization;
using CargoWise.RefDbRepo.VNReferenceData.Business;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.VNReferenceData.CmdLine
{
	public static class Program
	{
		static int Main(string[] args)
		{
			if (args.Length == 0)
			{
				const string errorMessage = """
				                            You need to specify which function to run.
				                            Please refer to Constants.ProgramFunctions class for all supported functions
				                            """;
				throw new ArgumentException(errorMessage);
			}

			var functionToRun = args[0].ToUpper(CultureInfo.InvariantCulture);
			Console.WriteLine($"Running function {functionToRun}");

			switch (functionToRun)
			{
				case Constants.ProgramFunctions.CustomsOfficeCode:
					new CustomsOfficeParser().DownloadAndExportToXmlFile().GetAwaiter().GetResult();
					break;
				default:
					throw new ArgumentException($"Unknown function: {functionToRun}. Please refer to Constants.ProgramFunctions class for all supported functions");
			}

			return (int)ProducerStatus.Success;
		}
	}
}
