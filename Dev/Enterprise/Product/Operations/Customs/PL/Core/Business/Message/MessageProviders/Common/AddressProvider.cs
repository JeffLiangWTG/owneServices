using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.PL.Business;

public class AddressProvider(IDocAddress docAddress) : IAddress
{
	readonly IDocAddress docAddress = Argument.NotNull(docAddress, nameof(docAddress));

	public string PostCode => CachedValueHelper.GetValue(ref postCode, () => MessageProviderHelper.ReturnNullIfEmpty(docAddress.E2_Postcode));
	CachedValue<string> postCode;

	public string CountryCode => CachedValueHelper.GetValue(ref countryCode, () => MessageProviderHelper.ReturnNullIfEmpty(docAddress.E2_RN_NKCountryCode));
	CachedValue<string> countryCode;

	public string City => CachedValueHelper.GetValue(ref city, () => MessageProviderHelper.ReturnNullIfEmpty(docAddress.E2_City));
	CachedValue<string> city;

	public string StreetAndNumber => CachedValueHelper.GetValue(ref streetAndNumber, GetStreetAndNumberCore);
	CachedValue<string> streetAndNumber;

	protected virtual string GetStreetAndNumberCore() => MessageProviderHelper.ReturnNullIfEmpty(ZString.Join(new(" "), [docAddress.E2_Address1.Trim(), docAddress.E2_Address2.Trim()]).Trim());
}
