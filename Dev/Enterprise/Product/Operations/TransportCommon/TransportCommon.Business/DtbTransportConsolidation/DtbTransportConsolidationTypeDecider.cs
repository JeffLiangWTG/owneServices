using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	[ImmutableObject(true)]
	public class DtbTransportConsolidationTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(DtbTransportConsolidation);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			switch ((string)row[DtbBookingConsolidationSchema.KB_JobType.Name])
			{
				case TransportConsolidationJobTypes.Codes.Consignment:
					return ObjectFactory.GetType<IDtbConsignmentConsolidation>();

				case TransportConsolidationJobTypes.Codes.Booking:
				case TransportConsolidationJobTypes.Codes.BookingTransportConsolidation:
				case TransportConsolidationJobTypes.Codes.HighVolumeLowValue:
				case TransportConsolidationJobTypes.Codes.QuotedBooking:
					return ObjectFactory.GetType<IDtbBookingConsolidation>();

				default:
					return typeof(DtbTransportConsolidation);
			}
		}

		public override Type GetTypeForNew()
		{
			throw new NotSupportedException("Abstract type.");
		}
	}
}
