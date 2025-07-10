using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderProcessTasks : WhsDocketProcessTasks, Enterprise.Integration.Warehouse.IWhsOrderProcessTask
	{
		#region Constructors

		public WhsOrderProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Parent

		protected override Type ParentType
		{
			get { return typeof(WhsOrder); }
		}

		public new WhsOrder Parent
		{
			get { return (WhsOrder)base.Parent; }
		}

		#endregion
	}
}
