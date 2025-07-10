using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveProcessTasks : WhsDocketProcessTasks, Enterprise.Integration.Warehouse.IWhsReceiveProcessTask
	{
		#region Constructors

		public WhsReceiveProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Parent

		protected override Type ParentType
		{
			get { return typeof(WhsReceive); }
		}

		public new WhsReceive Parent
		{
			get { return (WhsReceive)base.Parent; }
		}

		#endregion
	}
}
