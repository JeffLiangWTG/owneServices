using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class SupplierBookingProcessTask : ProcessTask
	{
		public SupplierBookingProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Parent

		public override ZArchitecture.Modules.ControllerID ParentControllerID => Enterprise.ZArchitecture.Modules.ControllerIDs.SupplierBooking;

		protected override Type ParentType => typeof(JobSupplierBooking);

		public new JobSupplierBooking Parent
		{
			get { return (JobSupplierBooking)base.Parent; }
		}

		#endregion
	}
}
