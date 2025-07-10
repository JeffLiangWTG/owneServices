using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(OrgSalesCall), "AdditionalAttendeesOther")]
	public class OrgSalesCallAdditionalAttendee : AutoOrgSalesCallAdditionalAttendee
	{
		public OrgSalesCallAdditionalAttendee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public OrgSalesCall Parent
		{
			get { return Factory.Load<OrgSalesCall>(O6_OQ); }
		}

		#region Properties

		public bool IsPrimaryContact
		{
			get { return O6_AttendeeID.IsValid && Parent.OQ_OC == O6_AttendeeID; }
		}

		#region O6_AttendeeID

		[List("Lookups.Contacts")]
		public override ZGuid O6_AttendeeID
		{
			get { return base.O6_AttendeeID; }
			set { base.O6_AttendeeID = value; }
		}

		protected bool O6_AttendeeID_ReadOnly
		{
			get { return IsPrimaryContact; }
		}

		#endregion

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return base.CanDelete && (!IsPrimaryContact || ParentHasDuplicateAttendee(this)); }
		}

		bool ParentHasDuplicateAttendee(OrgSalesCallAdditionalAttendee attendee)
		{
			return Parent.AdditionalAttendeesContact.Cast<OrgSalesCallAdditionalAttendee>().Count(x => x.O6_AttendeeID == attendee.O6_AttendeeID) > 1;
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsPrimaryContact)
				{
					return ResString.GetMultilingualString("89a5c924-5b14-41af-ac28-89cefd67b234", "Can not remove Primary Contact from list of attendees");
				}
				else
				{
					return base.ReasonForNotAbleToDelete;
				}
			}
		}

		#endregion

		public ZString Name
		{
			get
			{
				if (O6_AttendeeTableCode == OrgContactSchema.Constants.Prefix)
				{
					OrgContact contact = Factory.Load<OrgContact>(O6_AttendeeID);
					if (contact != null)
					{
						return contact.OC_ContactName;
					}
				}
				else if (O6_AttendeeTableCode == GlbStaffSchema.Constants.Prefix)
				{
					GlbStaff staff = Factory.Load<GlbStaff>(O6_AttendeeID);
					if (staff != null)
					{
						return staff.GS_FullName;
					}
				}
				else
				{
					return O6_AttendeeName;
				}

				return ZString.Empty;
			}
		}

		public ZString Email
		{
			get
			{
				if (O6_AttendeeTableCode == OrgContactSchema.Constants.Prefix)
				{
					OrgContact contact = Factory.Load<OrgContact>(O6_AttendeeID);
					if (contact != null)
					{
						return contact.OC_Email;
					}
				}
				else if (O6_AttendeeTableCode == GlbStaffSchema.Constants.Prefix)
				{
					GlbStaff staff = Factory.Load<GlbStaff>(O6_AttendeeID);
					if (staff != null)
					{
						return staff.GS_EmailAddress;
					}
				}

				return ZString.Empty;
			}
		}
	}
}
