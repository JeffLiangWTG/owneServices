//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVesselScheduleLookups
//
//    This class should be used for overriding collections in AutoJobVesselScheduleLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class JobVesselScheduleLookups : AutoJobVesselScheduleLookups
	{
		public JobVesselScheduleLookups(AutoJobVesselSchedule parent) : base(parent)
		{
		}
	}
}
