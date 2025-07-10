using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	[ModuleID(ModuleId.EDIMessageDeliveryContext)]
	public class EDIMessageDeliveryContextSelectorCollection : ActiveBusinessObjectCollection<EDIMessageDeliveryContextSelector>, IEDIMessageDeliveryContextSelectorCollection
	{
		public EDIMessageDeliveryContextSelectorCollection(BusinessObjectFactory factory, ZString processType)
			: base(factory, new ZQuery(EDIMessageDeliveryContextSelectorSchema.ECS_ProcessType, processType))
		{
		}

		public EDIMessageDeliveryContextSelectorCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
