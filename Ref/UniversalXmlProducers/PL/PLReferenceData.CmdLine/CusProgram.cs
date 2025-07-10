using CargoWise.RefDbRepo.PLReferenceData.Business.Cus;

namespace CargoWise.RefDbRepo.PLReferenceData.CmdLine;

sealed class CusProgram
{
	public static bool Run() => CusUniversalReferenceDataXmlGenerator.GenerateCusUniversalReferenceData();
}
