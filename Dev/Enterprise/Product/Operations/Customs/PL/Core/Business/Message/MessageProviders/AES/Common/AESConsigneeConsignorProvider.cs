using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESConsigneeConsignorProvider : IConsigneeConsignor
{
	protected AESConsigneeConsignorProvider(OrgAddress orgAddress)
	{
		this.orgAddress = orgAddress;
	}

	protected readonly OrgAddress orgAddress;

	public static AESConsigneeConsignorProvider NewOrNull(OrgAddress orgAddress) => orgAddress?.Header is not null
		? new AESConsigneeConsignorProvider(orgAddress)
		: null;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => EuEoriResolver.GetRegNoWithCountryCode(orgAddress));
	CachedValue<string> identificationNumber;

	public string Name => CachedValueHelper.GetValue(ref name, () => AesRuleHelper.ApplyC0050Rule(IdentificationNumber, GetNameCore));
	protected virtual string GetNameCore() => orgAddress.CompanyName;
	CachedValue<string> name;

	public IAddress Address => CachedValueHelper.GetValue(ref address, () => AesRuleHelper.ApplyC0050Rule(IdentificationNumber, GetAddressCore));
	protected virtual IAddress GetAddressCore() => new AddressProvider(orgAddress);
	CachedValue<IAddress> address;
}
