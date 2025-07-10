using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSShipmentProcessTask : ProcessTask
	{
		public CFSShipmentProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(CFSShipment); }
		}

		public new CFSShipment Parent
		{
			get { return (CFSShipment)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.ShipmentReceival; }
		}
	}
}
