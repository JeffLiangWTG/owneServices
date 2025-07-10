using CargoWise.RefDbRepo.USReferenceData.Business;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	class UnitedNationsStandardProductAndServiceCodesProgram
	{
		public static void UnitedNationsStandardProductAndServiceCodesMain(string outputPath)
		{
			Program.PrintLogMessage(new UnitedNationsStandardProductAndServiceCodesParser(outputPath).ConvertCodeListToXML());
		}
	}
}
