using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentProcessTasks : WhsDocketProcessTasks, Enterprise.Integration.Warehouse.IWhsAdjustmentProcessTask
	{
		#region Constructors

		public WhsAdjustmentProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Parent

		public override ZArchitecture.Modules.ControllerID ParentControllerID
		{
			get
			{
				return Enterprise.ZArchitecture.Modules.ControllerIDs.WhsAdjustment;
			}
		}

		protected override Type ParentType
		{
			get { return typeof(WhsAdjustment); }
		}

		public new WhsAdjustment Parent
		{
			get { return (WhsAdjustment)base.Parent; }
		}

		#endregion
	}
}
