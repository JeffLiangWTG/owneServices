//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM Autovw_List_ContainerAvailabilityLookups
//
//    This class should be used for overriding collections in Autovw_List_ContainerAvailabilityLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.WebCFS.Business
{
	public class vw_List_ContainerAvailabilityLookups : Autovw_List_ContainerAvailabilityLookups
	{
		public vw_List_ContainerAvailabilityLookups(Autovw_List_ContainerAvailability parent) : base(parent)
		{
		}
	}
}
