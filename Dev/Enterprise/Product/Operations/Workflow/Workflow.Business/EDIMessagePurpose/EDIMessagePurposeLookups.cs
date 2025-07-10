//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIMessagePurposeLookups
//
//    This class should be used for overriding collections in AutoEDIMessagePurposeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Workflow.Business
{
	public class EDIMessagePurposeLookups : AutoEDIMessagePurposeLookups
	{
		public EDIMessagePurposeLookups(AutoEDIMessagePurpose parent) : base(parent)
		{
		}

		public EDIMessageContentFilterCollection Filters => Factory.GetCachedValue("EDIMessagePurposeLookups.Filters", () => new EDIMessageContentFilterCollection(Factory));
	}
}
