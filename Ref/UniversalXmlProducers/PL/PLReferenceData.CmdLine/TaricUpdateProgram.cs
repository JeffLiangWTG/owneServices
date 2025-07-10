using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.BaseFile;
using System.IO;

namespace CargoWise.RefDbRepo.PLReferenceData.CmdLine;

sealed class TaricUpdateProgram
{
	public static bool Run()
	{
		var normalizedTaricFilePath = Taric4Constants.NORMALIZED_BASE_FILE_FULL_PATH;

		var processingMode = File.Exists(normalizedTaricFilePath)
			? TaricBase.ProcessingMode.GetUpdates
			: TaricBase.ProcessingMode.Download;

		if (!TaricBase.Process(processingMode, normalizedTaricFilePath))
		{
			return false;
		}

		var isztarHistoryResponse = Business.Helpers.XmlParser.DeserializeFromFile<IsztarHistoryResponse>(normalizedTaricFilePath);

		return TariffProgram.Run(isztarHistoryResponse)
			& QuotaProgram.Run(isztarHistoryResponse);
	}
}
