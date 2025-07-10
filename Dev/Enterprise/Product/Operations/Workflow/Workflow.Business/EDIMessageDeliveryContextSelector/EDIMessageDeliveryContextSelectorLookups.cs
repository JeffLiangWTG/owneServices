//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIMessageDeliveryContextSelectorLookups
//
//    This class should be used for overriding collections in AutoEDIMessageDeliveryContextSelectorLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageDeliveryContextSelectorLookups : AutoEDIMessageDeliveryContextSelectorLookups
	{
		public EDIMessageDeliveryContextSelectorLookups(AutoEDIMessageDeliveryContextSelector parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ProcessTypes => Factory.GetCachedValue<WorkflowDescriptorList>();
	}
}
