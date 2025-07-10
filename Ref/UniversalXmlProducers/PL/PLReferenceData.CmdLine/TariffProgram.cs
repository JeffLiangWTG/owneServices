
namespace CargoWise.RefDbRepo.PLReferenceData.CmdLine;

sealed class TariffProgram
{
	public static bool Run(IsztarHistoryResponse isztarHistoryResponse = null)
		=> Business.Tariff.TariffUniversalReferenceDataXmlGenerator.GenerateTariffUniversalReferenceData(isztarHistoryResponse);
}
