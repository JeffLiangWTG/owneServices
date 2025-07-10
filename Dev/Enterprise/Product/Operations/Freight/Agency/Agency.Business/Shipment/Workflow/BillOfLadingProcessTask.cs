using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingProcessTask : AgencyShipmentProcessTask, Integration.Agency.IBillOfLadingProcessTask
	{
		public BillOfLadingProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			AgencyBooking.CheckBillOfLadingIsNotUsedForCurrentBooking(Factory, new ZGuid(row[ProcessTasksSchema.P9_ParentID.Name]));
		}

		public override ZGuid P9_ParentID
		{
			get { return base.P9_ParentID; }
			set
			{
				base.P9_ParentID = value;
				AgencyBooking.CheckBillOfLadingIsNotUsedForCurrentBooking(Factory, value);
			}
		}

		public override ControllerID ParentControllerID
		{
			get { return ZArchitecture.Modules.ControllerIDs.AgencyBillOfLading; }
		}

		protected override Type ParentType
		{
			get { return typeof(BillOfLading); }
		}

		public new BillOfLading Parent
		{
			get { return (BillOfLading)base.Parent; }
		}
	}
}
