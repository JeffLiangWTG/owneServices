using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterSpecLookups : ZLookups
	{
		public EDIMessageContentFilterSpecLookups(EDIMessageContentFilterSpec parent)
			: base(parent)
		{
			Parent = parent;
		}

		public new EDIMessageContentFilterSpec Parent { get; }

		public CodeDescriptionPairList Schemas => Factory.GetCachedValue<EDIMessageContentFilterTypes>();
	}
}
