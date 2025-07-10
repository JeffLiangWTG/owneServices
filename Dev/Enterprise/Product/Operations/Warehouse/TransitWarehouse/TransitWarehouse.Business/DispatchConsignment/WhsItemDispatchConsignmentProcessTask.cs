using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchConsignmentProcessTask : ProcessTask
	{
		public WhsItemDispatchConsignmentProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ParentType

		protected override Type ParentType
		{
			get { return typeof(WhsItemDispatchConsignment); }
		}

		#endregion

		#region ParentControllerID

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.WhsTransitDispatchConsignment; }
		}

		#endregion
	}
}
