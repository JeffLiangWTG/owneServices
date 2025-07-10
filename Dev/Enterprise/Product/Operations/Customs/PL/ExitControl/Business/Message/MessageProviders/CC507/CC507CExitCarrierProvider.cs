using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CC507CExitCarrierProvider : ICC507CExitCarrier
{
	readonly OrgAddress orgAddress;
	readonly OrgHeader orgHeader;

	CC507CExitCarrierProvider(OrgAddress orgAddress)
	{
		this.orgAddress = Argument.NotNull(orgAddress, nameof(orgAddress));
		orgHeader = orgAddress.Header;
	}

	public static CC507CExitCarrierProvider NewOrNull(OrgAddress orgAddress) => orgAddress is not null
		? new CC507CExitCarrierProvider(orgAddress)
		: null;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber,
		() => EuEoriResolver.GetRegNoWithCountryCode(orgAddress));
	CachedValue<string> identificationNumber;

	public string Name => CachedValueHelper.GetValue(ref name, GetName);
	CachedValue<string> name;

	public IPersonIdentificationNumbers IdentificationDataPL => CachedValueHelper.GetValue(ref identificationDataPL,
		() => string.IsNullOrEmpty(IdentificationNumber)
			? new AESPersonIdentificationNumbersProvider(orgHeader.CustomsCodes)
			: null);
	CachedValue<IPersonIdentificationNumbers> identificationDataPL;

	public IAddress Address => CachedValueHelper.GetValue(ref address,
		() => string.IsNullOrEmpty(IdentificationNumber)
			? new AddressProvider(orgAddress)
			: null);
	CachedValue<IAddress> address;

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson,
		() => ContactPersonProvider.NewOrNull(GlbStaff.CurrentUser));
	CachedValue<IContactPerson> contactPerson;

	string GetName() => orgAddress.OA_CompanyNameOverride.IsEmpty ? orgAddress.Header.OH_FullName : orgAddress.OA_CompanyNameOverride;
}
