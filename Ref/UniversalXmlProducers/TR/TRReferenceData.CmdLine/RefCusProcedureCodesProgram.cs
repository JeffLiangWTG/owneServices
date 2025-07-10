using CargoWise.RefDbRepo.TRReferenceData.Business.RefCusProcedureCodesParser;
using CargoWise.RefDbRepo.TRReferenceData.Services;

namespace CargoWise.RefDbRepo.TRReferenceData.CmdLine
{
	public static class RefCusProcedureCodesProgram
	{
		public static void Run()
		{
			var dataFileName = SystemIOHelper.GetResFilePath("TR - RefCusProcedureCodes.xlsx");
			var outputFileName = SystemIOHelper.GetOutputFilePath("RefCusProcedureCodes_TR.xml");
			Run(dataFileName, outputFileName);
		}

		public static void Run( string dataFileName, string outputFileName)
		{
			var parser = new RefCusProcedureCodesParser( dataFileName);
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
