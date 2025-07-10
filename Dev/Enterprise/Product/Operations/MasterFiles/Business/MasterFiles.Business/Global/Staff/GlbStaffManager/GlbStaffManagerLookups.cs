//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbStaffManagerLookups
//
//    This class should be used for overriding collections in AutoGlbStaffManagerLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffManagerLookups : AutoGlbStaffManagerLookups
	{
		public GlbStaffManagerLookups(AutoGlbStaffManager parent) : base(parent)
		{
		}

		public ICodeDescriptionBoolList StaffReportingRoles
		{
			get
			{
				return Factory.GetCachedValue("StaffReportingRoles", delegate
				{
					var list = new StaffReportingRoleCollection();
					list.AddRange(SystemDataRegistry.Instance.StaffReportingRoles.Value.OfType<StaffReportingRole>().Where(x => x.Bool));
					return list;
				});
			}
		}
	}
}
