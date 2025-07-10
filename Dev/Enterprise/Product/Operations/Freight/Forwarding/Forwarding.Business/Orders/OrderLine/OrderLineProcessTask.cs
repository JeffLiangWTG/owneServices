using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLineProcessTask : ProcessTask, Integration.Forwarding.IOrderLineProcessTask
	{
		public OrderLineProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Parent

		public override ZArchitecture.Modules.ControllerID ParentControllerID
			=> Enterprise.ZArchitecture.Modules.ControllerIDs.OrderLine;

		protected override Type ParentType => typeof(OrderLine);

		public new OrderLine Parent
		{
			get { return (OrderLine)base.Parent; }
		}

		#endregion
	}
}
