using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESConsignmentConsigneeConsignorProvider : IConsigneeConsignor
{
	protected AESConsignmentConsigneeConsignorProvider(JobDocAddress docAddress)
	{
		this.docAddress = docAddress;
		orgAddress = docAddress.Address;
	}

	protected readonly JobDocAddress docAddress;
	readonly OrgAddress orgAddress;

	public static AESConsignmentConsigneeConsignorProvider NewOrNull(JobDocAddress docAddress) => docAddress?.Address?.Header is not null
		? new AESConsignmentConsigneeConsignorProvider(docAddress)
		: null;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => EuEoriResolver.GetRegNoWithCountryCode(orgAddress));
	CachedValue<string> identificationNumber;

	public string Name => CachedValueHelper.GetValue(ref name, () => AesRuleHelper.ApplyC0050Rule(IdentificationNumber, GetNameCore));
	protected virtual string GetNameCore() => docAddress.CompanyName;
	CachedValue<string> name;

	public IAddress Address => CachedValueHelper.GetValue(ref address, () => AesRuleHelper.ApplyC0050Rule(IdentificationNumber, GetAddressCore));
	protected virtual IAddress GetAddressCore() => new AddressProvider(docAddress);
	CachedValue<IAddress> address;
}
