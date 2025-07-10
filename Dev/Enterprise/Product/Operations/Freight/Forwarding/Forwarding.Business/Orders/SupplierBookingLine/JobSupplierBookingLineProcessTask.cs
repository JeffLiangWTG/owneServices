using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobSupplierBookingLineProcessTask(BusinessObjectFactory factory, DataRow row)
		: ProcessTask(factory, row)
	{
		#region Parent

		protected override Type ParentType => typeof(JobSupplierBookingLine);

		public new JobSupplierBookingLine Parent => (JobSupplierBookingLine)base.Parent;

		#endregion
	}
}
