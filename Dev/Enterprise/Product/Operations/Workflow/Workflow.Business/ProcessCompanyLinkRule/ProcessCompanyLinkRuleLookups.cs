//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessCompanyLinkRuleLookups
//
//    This class should be used for overriding collections in AutoProcessCompanyLinkRuleLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class ProcessCompanyLinkRuleLookups : AutoProcessCompanyLinkRuleLookups
	{
		public ProcessCompanyLinkRuleLookups(AutoProcessCompanyLinkRule parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Types => Factory.GetCachedValue<WorkflowDescriptorList>();

		public GlbStaffCollection Staffs => Factory.GetCachedValue("PCRLookups.Staffs", () => new GlbStaffCollection(Factory));
	}
}
