using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWorkOrderProcessTasks : WhsDocketProcessTasks
	{
		public WhsWorkOrderProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ControllerID ParentControllerID => ControllerIDs.WhsWorkOrder;

		protected override Type ParentType => typeof(WhsWorkOrder);

		public new WhsWorkOrder Parent => (WhsWorkOrder)base.Parent;
	}
}
