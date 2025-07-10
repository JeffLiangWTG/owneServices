//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSalesCallAdditionalAttendeeValidation
//
//    This class should be used for overriding validation in AutoOrgSalesCallAdditionalAttendeeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallAdditionalAttendeeValidation : AutoOrgSalesCallAdditionalAttendeeValidation
	{
		public OrgSalesCallAdditionalAttendeeValidation(AutoOrgSalesCallAdditionalAttendee parent)
			: base(parent)
		{
		}

		new OrgSalesCallAdditionalAttendee Parent
		{
			get { return (OrgSalesCallAdditionalAttendee)base.Parent; }
		}

		protected override void CheckO6_AttendeeIDIsValidZGuid()
		{
			if (!Parent.O6_AttendeeTableCode.IsEmpty)
			{
				base.CheckO6_AttendeeIDIsValidZGuid();
			}
		}

		protected override void CheckO6_AttendeeID()
		{
			base.CheckO6_AttendeeID();
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.O6_AttendeeIDInfo);
			if (!Parent.O6_AttendeeTableCode.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.O6_AttendeeIDInfo);
			}
			if (Parent.O6_AttendeeTableCode == GlbStaffSchema.Constants.Prefix)
			{
				GlbStaff staff = Parent.Factory.Load<GlbStaff>(Parent.O6_AttendeeID);
				if (staff != null && !staff.GS_IsActive)
				{
					if (!Parent.IsInDatabase || Parent.HasChanges)
					{
						Parent.O6_AttendeeIDInfo.AddError(Res.GetString("80e499d3-8ffc-423c-a920-170bfabac4cf", "This Staff is inactive - it may not be used."));
					}
					else
					{
						Parent.O6_AttendeeIDInfo.AddWarning(Res.GetString("2695fcf4-a797-4cac-badb-abe491b26118", "This Staff is inactive."));
					}
				}
			}
		}

		protected override void CheckO6_AttendeeName()
		{
			base.CheckO6_AttendeeName();
			if (Parent.O6_AttendeeTableCode.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.O6_AttendeeNameInfo);
			}
		}
	}
}
