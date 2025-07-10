using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.BaseFile;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.BaseFile.TaricBase;

namespace CargoWise.RefDbRepo.PLReferenceData.CmdLine;

sealed class Taric4BaseProgram
{
	public static bool Run() => TaricBase.Process(ProcessingMode.Download, Taric4Constants.NORMALIZED_BASE_FILE_FULL_PATH);
}
