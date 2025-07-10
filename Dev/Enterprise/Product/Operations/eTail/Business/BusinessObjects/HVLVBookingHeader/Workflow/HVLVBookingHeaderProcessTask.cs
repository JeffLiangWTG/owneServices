using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Business
{
	public class HVLVBookingHeaderProcessTask : ProcessTask, IHVLVBookingHeaderProcessTask
	{
		public HVLVBookingHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override Type ParentType
		{
			get { return typeof(HVLVBookingHeader); }
		}

		public new HVLVBookingHeader Parent
		{
			get { return (HVLVBookingHeader)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.JobShipment; }
		}
	}
}
