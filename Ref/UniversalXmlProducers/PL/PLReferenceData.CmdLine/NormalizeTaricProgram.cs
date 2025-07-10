using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.BaseFile;
using CargoWise.RefDbRepo.PLReferenceData.Services;

namespace CargoWise.RefDbRepo.PLReferenceData.CmdLine;

sealed class NormalizeTaricProgram
{
	public static bool Run() => TaricBase.Process(TaricBase.ProcessingMode.Normalize, baseTaricFilePath);

	static string baseTaricFilePath =>
		Directory.GetFiles(ApplicationConfig.Instance.DownloadsPLPath)
		.Single(v => v.Contains("base"));
}
