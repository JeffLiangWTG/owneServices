using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class ProcessTaskIterationLinkLookups : AutoProcessTaskIterationLinkLookups
	{
		public ProcessTaskIterationLinkLookups(AutoProcessTaskIterationLink parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList LinkTypes
		{
			get { return Factory.GetCachedValue<IterationLinkTypeList>(); }
		}
	}
}
