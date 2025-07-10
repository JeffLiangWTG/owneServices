using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class RepresentativeProvider : IRepresentative
{
	public RepresentativeProvider(OrgHeader orgHeader)
	{
		this.orgHeader = orgHeader;
	}
	readonly OrgHeader orgHeader;

	public static RepresentativeProvider NewOrNull(OrgHeader orgHeader) => orgHeader != null ? new RepresentativeProvider(orgHeader) : null;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => EuEoriResolver.GetRegNoWithCountryCode(orgHeader));
	CachedValue<string> identificationNumber;

	public string Status => "2";

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson,
		() => ContactPersonProvider.NewOrNull(GlbStaff.CurrentUser));
	CachedValue<IContactPerson> contactPerson;
}
