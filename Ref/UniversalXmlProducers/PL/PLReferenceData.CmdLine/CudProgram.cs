using CargoWise.RefDbRepo.PLReferenceData.Business.Cud;

namespace CargoWise.RefDbRepo.PLReferenceData.CmdLine;

sealed class CudProgram
{
	public static bool Run() => CudUniversalReferenceDataXmlGenerator.GenerateCudUniversalReferenceData();
}
