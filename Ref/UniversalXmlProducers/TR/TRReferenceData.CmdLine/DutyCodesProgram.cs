using CargoWise.RefDbRepo.TRReferenceData.Business.DutyCodesParser;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
	public static class DutyCodesProgram
	{
		public static void Run()
		{
			var dataFileName = SystemIOHelper.GetResFilePath("DutyCodes.xlsx");
			var outputFileName = SystemIOHelper.GetOutputFilePath("DutyCodes_TR.xml");
			Run(new DateTimeProvider(), dataFileName, outputFileName);
		}

		public static void Run(IDateTimeProvider dateTimeProvider, string dataFileName, string outputFileName)
		{
			var parser = new DutyCodesParser(dateTimeProvider, dataFileName);
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

