using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsLoadProcessTask : ProcessTask
	{
		public WhsLoadProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ParentControllerID

		public override ControllerID ParentControllerID => ControllerIDs.WhsLoad;

		#endregion

		#region ParentType

		protected override Type ParentType => typeof(WhsLoad);

		#endregion
	}
}
