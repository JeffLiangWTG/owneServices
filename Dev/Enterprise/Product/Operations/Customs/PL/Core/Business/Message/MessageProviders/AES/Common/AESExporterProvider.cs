using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESExporterProvider : IExporter
{
	protected AESExporterProvider(OrgAddress orgAddress)
	{
		this.orgAddress = orgAddress;
		orgHeader = orgAddress.Header;
	}

	protected readonly OrgAddress orgAddress;
	readonly OrgHeader orgHeader;

	public static AESExporterProvider NewOrNull(OrgAddress orgAddress) => orgAddress?.Header is not null
		? new AESExporterProvider(orgAddress)
		: null;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => EuEoriResolver.GetRegNoWithCountryCode(orgHeader));
	CachedValue<string> identificationNumber;

	public IPersonIdentificationNumbers IdentificationNumbers => CachedValueHelper.GetValue(ref identificationNumbers, () => new AESPersonIdentificationNumbersProvider(orgHeader.CustomsCodes));
	CachedValue<IPersonIdentificationNumbers> identificationNumbers;

	public string Name => CachedValueHelper.GetValue(ref name, () => AesRuleHelper.ApplyC0050Rule(IdentificationNumber, GetNameCore));
	protected virtual string GetNameCore() => orgAddress.CompanyName;
	CachedValue<string> name;

	public IAddress Address => CachedValueHelper.GetValue(ref address, () => AesRuleHelper.ApplyC0050Rule(IdentificationNumber, GetAddressCore));
	protected virtual IAddress GetAddressCore() => new AddressProvider(orgAddress);
	CachedValue<IAddress> address;
}
