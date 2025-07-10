using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESDeclarantProvider : IAESDeclarant
{
	protected AESDeclarantProvider(OrgAddress orgAddress, JobDeclaration jobDeclaration)
	{
		this.jobDeclaration = Argument.NotNull(jobDeclaration, $"{nameof(jobDeclaration)}");
		this.orgAddress = orgAddress;
		orgHeader = orgAddress.Header;
	}

	protected readonly OrgAddress orgAddress;
	protected readonly OrgHeader orgHeader;
	protected readonly JobDeclaration jobDeclaration;

	public static AESDeclarantProvider NewOrNull(OrgAddress orgAddress, JobDeclaration jobDeclaration) => orgAddress?.Header is not null
		? new AESDeclarantProvider(orgAddress, jobDeclaration)
		: null;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => EuEoriResolver.GetRegNoWithCountryCode(orgHeader));
	CachedValue<string> identificationNumber;

	public string Name => CachedValueHelper.GetValue(ref name, () => AesRuleHelper.ApplyC0050Rule(IdentificationNumber, GetNameCore));
	CachedValue<string> name;
	protected virtual string GetNameCore() => orgAddress.CompanyName;

	public IAddress Address => CachedValueHelper.GetValue(ref address, () => AesRuleHelper.ApplyC0050Rule(IdentificationNumber, GetAddressCore));
	CachedValue<IAddress> address;
	protected virtual IAddress GetAddressCore() => new AddressProvider(orgAddress);

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson,
		() => ContactPersonProvider.NewOrNull(jobDeclaration.CusAgent ?? GlbStaff.CurrentUser));
	CachedValue<IContactPerson> contactPerson;
}
