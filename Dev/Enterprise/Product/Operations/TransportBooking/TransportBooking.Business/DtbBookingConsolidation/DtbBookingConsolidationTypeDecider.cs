using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	[ImmutableObject(true)]
	public class DtbBookingConsolidationTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(DtbBookingConsolidation);
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
					return typeof(Common.AutoDtbBookingConsolidation);
			}
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<IDtbBookingConsolidation>();
		}
	}
}
