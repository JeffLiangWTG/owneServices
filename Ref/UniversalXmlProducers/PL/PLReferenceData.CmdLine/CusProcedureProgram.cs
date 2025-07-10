using CargoWise.RefDbRepo.PLReferenceData.Business.CusProcedure;

namespace CargoWise.RefDbRepo.PLReferenceData.CmdLine;

sealed class CusProcedureProgram
{
	public static bool Run() => CusProcedureUniversalReferenceDataXmlGenerator.GenerateCusProcedureUniversalReferenceData();
}
