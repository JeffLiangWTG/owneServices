using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Workflow.Business
{
	[ModuleID(ModuleId.EDIMessageContentFilter)]
	public class EDIMessageContentFilterCollection : ActiveBusinessObjectCollection<EDIMessageContentFilter>
	{
		public EDIMessageContentFilterCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
