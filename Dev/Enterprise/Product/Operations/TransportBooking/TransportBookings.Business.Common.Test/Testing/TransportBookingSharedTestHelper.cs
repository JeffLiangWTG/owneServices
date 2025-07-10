using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Shared.Testing
{
	public class TransportBookingSharedTestHelper
	{
		public TransportBookingSharedTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public IDtbBookingConsolidation CreateConsolidation()
		{
			return (IDtbBookingConsolidation)Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
		}

		public IDtbBookingConsolidation CreateConsolidation(IDtbBookingParent bookingParent)
		{
			var result = CreateConsolidation();
			var bo = (BusinessObject)result;

			using (bo.SuspendSettingHasChanges())
			{
				result.KB_ParentTableCode = bookingParent.TablePrefix;
				result.KB_ParentID = bookingParent.PK;
			}

			return result;
		}

		public IDtbBookingConsolidation CreateConsolidationMultiJob()
		{
			var bizO = (BusinessObject)Factory.New<IDtbBookingConsolidation>();
			using (bizO.SuspendSettingHasChanges())
			{
				bizO[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			}
			return (IDtbBookingConsolidation)bizO;
		}

		public IDtbBooking CreateBooking()
		{
			var consolidation = CreateConsolidation();
			return CreateBooking(consolidation);
		}

		public IDtbBooking CreateBooking(IDtbBookingConsolidation consolidation)
		{
			var bo = Factory.New(ObjectFactory.GetType<IDtbBooking>());
			var result = (IDtbBooking)bo;

			using (bo.SuspendSettingHasChanges())
			{
				if (consolidation.KB_JobType == TransportConsolidationJobTypes.Codes.BookingTransportConsolidation)
				{
					result.KM_KB_BookingConsolidationMultiJob = consolidation.PK;
				}
				else
				{
					result.KM_KB_Booking = consolidation.PK;
				}
			}

			return result;
		}

		public IDisposable LastDocumentOptionsForTestStartRecording()
		{
			var provider = ObjectFactory.Get<ITransportBookingDocumentOptionsProvider>();
			return provider.LastDocumentOptionsForTestStartRecording();
		}

		public ITransportBookingDocumentOptions LastDocumentOptionsForTest()
		{
			var provider = ObjectFactory.Get<ITransportBookingDocumentOptionsProvider>();
			return provider.LastDocumentOptionsForTest();
		}
	}
}
