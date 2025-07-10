using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class PartyWithStaffWrapper : PartyWrapper
{
	PartyWithStaffWrapper(OrgHeader party, GlbStaff staff) : base(party)
	{
		this.staff = Argument.NotNull(staff, nameof(staff));
	}
	readonly GlbStaff staff;

	protected override IContact ContactCore => new ContactWrapper(staff);

	public static PartyWithStaffWrapper New(OrgHeader party, GlbStaff staff) =>
		party == null || staff == null ? null : new PartyWithStaffWrapper(party, staff);
}
