using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class ActiveBorderTransportMeansProvider(CusExitReport exitReport) : IActiveBorderTransportMeans
{
	readonly CusExitReport cusExitReport = Argument.NotNull(exitReport, nameof(exitReport));

	public string TypeOfIdentification => cusExitReport.CER_TransportType;

	public string IdentificationNumber => IdentificationNumberAllowedByR0049E ? cusExitReport.CER_TransportID : null;

	bool IdentificationNumberAllowedByR0049E => !ExitControlConstants.Rules.R0049ETransportTypeFirstNumbers.Contains(cusExitReport.CER_TransportType.SubstringSafe(0, 1));

	public string Nationality => NationalityAllowedByR0050E ? cusExitReport.CER_RN_NKTransportNationality : null;

	bool NationalityAllowedByR0050E => !ExitControlConstants.Rules.R0050ETransportTypeFirstNumbers.Contains(cusExitReport.CER_TransportType.SubstringSafe(0, 1));
}
