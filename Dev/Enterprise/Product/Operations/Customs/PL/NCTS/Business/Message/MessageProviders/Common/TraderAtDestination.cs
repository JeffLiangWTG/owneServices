using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class TraderAtDestinationProvider(JobDocAddress jobDocAddress) : ITraderAtDestination
{
	public string IdentificationNumber => MessageProviderHelper.ReturnNullIfEmpty(OrgAddress?.GetEORI(countryOfIssuance: Core.Constants.CountryCodes.Poland, ignoreCountryOfIssuanceIfNotMatched: true) ?? ZString.Empty);

	public string CommunicationLanguageAtDestination => MessageProviderHelper.ReturnNullIfEmpty(OrgAddress?.Language.Left(2) ?? ZString.Empty);

	OrgAddress OrgAddress => jobDocAddress?.Address;
}
