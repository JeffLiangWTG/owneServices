//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConsolDGRestrictionsLookups
//
//    This class should be used for overriding collections in AutoJobConsolDGRestrictionsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobConsolDGRestrictionsLookups : AutoJobConsolDGRestrictionsLookups
	{
		public JobConsolDGRestrictionsLookups(AutoJobConsolDGRestrictions parent) : base(parent)
		{
		}
	}
}
