using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CarrierProvider : ICarrier
{
	public CarrierProvider(JobDocAddress header)
	{
		DocAddress = Argument.NotNull(header, nameof(header));
		OrgAddress = DocAddress.Address;
		OrgHeader = OrgAddress?.Header;
	}

	protected readonly OrgHeader OrgHeader;
	protected readonly OrgAddress OrgAddress;
	protected readonly JobDocAddress DocAddress;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => EuEoriResolver.GetRegNoWithCountryCode(OrgHeader));
	CachedValue<string> identificationNumber;

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson, () => ContactPersonProvider.NewOrNull(OrgHeader));
	CachedValue<IContactPerson> contactPerson;
}
