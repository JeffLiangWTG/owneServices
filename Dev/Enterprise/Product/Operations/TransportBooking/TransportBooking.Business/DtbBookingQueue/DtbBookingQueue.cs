using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingQueue : AutoDtbBookingQueue
	{
		public DtbBookingQueue(BusinessObjectFactory factory, DataRow dataRow)
			: base(factory, dataRow)
		{
		}

		public short Iteration
		{
			get { return Convert.ToInt16(KMQ_Iteration); }
			set { KMQ_Iteration = Convert.ToByte(value); }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			KMQ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			KMQ_ParentID = Guid.Empty;
		}
#endif
	}
}
