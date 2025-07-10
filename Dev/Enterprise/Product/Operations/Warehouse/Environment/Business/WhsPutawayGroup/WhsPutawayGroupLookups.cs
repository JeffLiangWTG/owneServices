//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPutawayGroupLookups
//
//    This class should be used for overriding validation in AutoWhsPutawayGroupLookups.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPutawayGroupLookups : AutoWhsPutawayGroupLookups
	{
		public WhsPutawayGroupLookups(AutoWhsPutawayGroup parent)
			: base(parent)
		{
		}
	}
}
