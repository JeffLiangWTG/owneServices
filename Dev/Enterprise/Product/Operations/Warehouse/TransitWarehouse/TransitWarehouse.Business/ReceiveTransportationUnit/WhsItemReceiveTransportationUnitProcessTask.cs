using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveTransportationUnitProcessTask : ProcessTask
	{
		public WhsItemReceiveTransportationUnitProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ParentType

		protected override Type ParentType
		{
			get { return typeof(WhsItemReceiveTransportationUnit); }
		}

		#endregion

		#region ParentControllerID

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.WhsItemReceiveTransportationUnit; }
		}

		#endregion
	}
}
