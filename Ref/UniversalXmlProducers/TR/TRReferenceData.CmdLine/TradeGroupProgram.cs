using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
	public static class TradeGroupProgram
	{
		public static void Run()
		{
			var dataFileName = SystemIOHelper.GetResFilePath("TR Trade Group Country Codes with Expiry Dates.xlsx");
			var outputFileName = SystemIOHelper.GetOutputFilePath("RefCusTradeGroupZZ_TR.xml");
			Run(new DateTimeProvider(), dataFileName, outputFileName);
		}

		public static void Run(IDateTimeProvider dateTimeProvider, string dataFileName, string outputFileName)
		{
			var parser = new TradeGroupParser(dateTimeProvider, dataFileName);
			try
			{
				parser.GenerateUXML(outputFileName);
			}
			finally
			{
				Program.PrintErrorMessage(parser.ErrorMessage);
			}
		}
	}
}
