//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobContainerPackPivotLookups
//
//    This class should be used for overriding collections in AutoJobContainerPackPivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Business
{
	public class JobContainerPackPivotLookups : AutoJobContainerPackPivotLookups
	{
		public JobContainerPackPivotLookups(AutoJobContainerPackPivot parent) : base(parent)
		{
		}
	}
}
