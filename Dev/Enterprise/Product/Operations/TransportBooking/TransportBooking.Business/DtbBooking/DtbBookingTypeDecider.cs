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
	public class DtbBookingTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(DtbBooking);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var jobType = (string)row[DtbBookingSchema.Constants.KM_JobType];
			switch (jobType)
			{
				case TransportConsolidationJobTypes.Codes.Consignment:
					{
						return ObjectFactory.GetType<IDtbBookingConsignment>();
					}

				case TransportConsolidationJobTypes.Codes.Booking:
				case TransportConsolidationJobTypes.Codes.HighVolumeLowValue:
				case TransportConsolidationJobTypes.Codes.QuotedBooking:
					{
						return ObjectFactory.GetType<IDtbBooking>();
					}
			}

			return typeof(Common.AutoDtbBooking);
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<IDtbBooking>();
		}
	}
}
