//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSeaManSlotOrgLookups
//
//    This class should be used for overriding collections in AutoCusSeaManSlotOrgLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusSeaManSlotOrgLookups : AutoCusSeaManSlotOrgLookups
	{
		public CusSeaManSlotOrgLookups(AutoCusSeaManSlotOrg parent) : base(parent)
		{
		}

		#region Properties

		public override OrgHeaderCollection SlotCharterers
		{
			get { return new ShippingProviderCollection(Factory); }
		}

		#endregion
	}
}
