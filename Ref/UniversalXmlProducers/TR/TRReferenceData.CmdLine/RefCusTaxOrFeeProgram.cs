using CargoWise.RefDbRepo.TRReferenceData.Business.RefCusTaxOrFeeParser;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
	public static class RefCusTaxOrFeeProgram
	{
		public static void Run()
		{
			var dataFileName = SystemIOHelper.GetResFilePath("TR Stamp Duty.xlsx");
			var outputFileName = SystemIOHelper.GetOutputFilePath("RefCusTaxOrFee_TR.xml");
			Run(new DateTimeProvider(), dataFileName, outputFileName);
		}

		public static void Run(IDateTimeProvider dateTimeProvider, string dataFileName, string outputFileName)
		{
			var parser = new RefCusTaxOrFeeParser(dateTimeProvider, dataFileName);
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
