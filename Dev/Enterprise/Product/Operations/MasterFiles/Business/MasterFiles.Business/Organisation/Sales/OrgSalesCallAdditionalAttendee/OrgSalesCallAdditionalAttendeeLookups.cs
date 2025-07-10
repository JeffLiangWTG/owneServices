//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSalesCallAdditionalAttendeeLookups
//
//    This class should be used for overriding collections in AutoOrgSalesCallAdditionalAttendeeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallAdditionalAttendeeLookups : AutoOrgSalesCallAdditionalAttendeeLookups
	{
		public OrgSalesCallAdditionalAttendeeLookups(AutoOrgSalesCallAdditionalAttendee parent) : base(parent)
		{
		}

		OrgContactCollection fContacts;
		public OrgContactCollection Contacts
		{
			get
			{
				if (fContacts == null)
				{
					fContacts = new OrgContactCollection(Factory);
					var call = ((OrgSalesCallAdditionalAttendee)Parent).Parent;
					var org = call != null ? call.Header : null;
					if (org != null)
					{
						fContacts.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation", "Property", org.PK));
					}
				}
				return fContacts;
			}
		}

		GlbStaffCollection fStaff;
		public GlbStaffCollection Staff
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = new GlbStaffCollection(Factory);
				}
				return fStaff;
			}
		}
	}
}
