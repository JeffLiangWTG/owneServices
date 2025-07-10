//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobCartageRunSheetLookups
//
//    This class should be used for overriding collections in AutoJobCartageRunSheetLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public class LocalCartageJobOrgLookups : AutoLocalCartageJobOrgLookups
	{
		public LocalCartageJobOrgLookups(AutoLocalCartageJobOrg parent)
			: base(parent)
		{
		}

		public LocalCartageJobOrgTypeList OrgTypeList
		{
			get { return LocalCartageJobOrgTypeList.Instance; }
		}
	}
}
