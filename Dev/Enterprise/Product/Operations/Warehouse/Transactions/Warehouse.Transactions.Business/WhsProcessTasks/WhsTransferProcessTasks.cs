using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTransferProcessTasks : WhsDocketProcessTasks
	{
		#region Constructors

		public WhsTransferProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.WhsTransfer; }
		}

		protected override Type ParentType
		{
			get { return typeof(WhsTransfer); }
		}

		public new WhsTransfer Parent
		{
			get { return (WhsTransfer)base.Parent; }
		}

		#endregion
	}
}
