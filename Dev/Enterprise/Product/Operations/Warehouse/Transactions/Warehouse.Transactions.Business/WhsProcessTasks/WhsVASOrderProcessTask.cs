using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderProcessTask : ProcessTask
	{
		public WhsVASOrderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ParentControllerID

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.WhsVASOrder; }
		}

		#endregion

		#region ParentType

		protected override Type ParentType
		{
			get { return typeof(WhsVASOrder); }
		}

		#endregion
	}
}
