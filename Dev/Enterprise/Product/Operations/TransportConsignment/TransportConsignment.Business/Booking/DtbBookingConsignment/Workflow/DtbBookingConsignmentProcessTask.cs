using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentProcessTask : ProcessTask, IDtbBookingConsignmentProcessTask
	{
		public DtbBookingConsignmentProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(DtbBookingConsignment); }
		}

		public new DtbBookingConsignment Parent
		{
			get { return (DtbBookingConsignment)base.Parent; }
		}
	}
}
