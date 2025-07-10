using System;
using System.IO;
using CargoWise.RefDbRepo.INReferenceData.Business;

namespace CargoWise.RefDbRepo.INReferenceData.CmdLine
{
	public static class DBKTariffProgram
	{
		public static void Run(string[] args)
		{
			var inputPath = string.Empty;
			var startDateString = string.Empty;
			if (args.Length > 1)
			{
				inputPath = args[1];
			}
			else
			{
				Console.WriteLine(@"Please enter the instruction in the following format:
DBKTARIFF ""PDF File Path""");
				return;
			}

			if (!File.Exists(inputPath))
			{
				Console.WriteLine("The file does not exist.");
				return;
			}
			if (!Path.GetExtension(inputPath).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("The file is not a PDF.");
				return;
			}

			new DBKTariffXmlProducer(DataSource, inputPath).ProduceXml();

			Console.WriteLine("Processing completed.");
		}

		const string DataSource = "IN CUS Drawback Schedule";
	}
}
