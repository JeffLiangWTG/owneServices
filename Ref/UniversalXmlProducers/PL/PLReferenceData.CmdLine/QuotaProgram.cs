using CargoWise.RefDbRepo.PLReferenceData.Business.Quota;

namespace CargoWise.RefDbRepo.PLReferenceData.CmdLine;

sealed class QuotaProgram
{
	public static bool Run(IsztarHistoryResponse isztarHistoryResponse = null)
		=> QuotaUniversalReferenceDataXmlGenerator.GenerateQuotaUniversalReferenceData(isztarHistoryResponse);
}
