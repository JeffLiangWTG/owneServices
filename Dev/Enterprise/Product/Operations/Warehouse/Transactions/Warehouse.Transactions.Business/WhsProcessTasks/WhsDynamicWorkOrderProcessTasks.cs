using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderProcessTasks : WhsDocketProcessTasks
	{
		public WhsDynamicWorkOrderProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ControllerID ParentControllerID => ControllerIDs.WhsDynamicWorkOrder;

		protected override Type ParentType => typeof(WhsDynamicWorkOrder);

		public new WhsDynamicWorkOrder Parent => (WhsDynamicWorkOrder)base.Parent;
	}
}
