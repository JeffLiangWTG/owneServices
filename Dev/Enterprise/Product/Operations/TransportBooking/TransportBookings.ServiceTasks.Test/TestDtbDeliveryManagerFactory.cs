using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.ServiceTasks.Test
{
	class TestDtbDeliveryManagerFactory : IDtbDeliveryManagerFactory
	{
		public IDtbDeliveryManager CreateManager(BusinessObjectFactory factory, IDtbBookingParent parent, DtbBookingDirection direction, ZBool combineContainers, ILogger logger, IAutomatedDtbBookingCreationErrorManager errorManager)
		{
			var manager = new TestDtbDeliveryManager(factory, parent, direction, combineContainers, logger, errorManager);
			manager.ParentDeliveryManagerFactory = this;
			manager.ManagerCreatedBookings = ManagerCreatedBookings;

			managersCreated++;
			if (managersCreated == ManagerCreatedToThrowException)
			{
				manager.DeliverThrowsException = true;
			}
			if (managersCreated == ManagerCreatedToSetCancellation)
			{
				manager.DeliverSetsCancellation = true;
				manager.TokenSource = TokenSource;
			}
			if (ManagerDeliverErrorType != null)
			{
				manager.DeliverError = ManagerDeliverErrorType;
			}
			return manager;
		}

		int managersCreated;
		public int ManagerCreatedToThrowException { get; set; }

		public int ManagerCreatedToSetCancellation { get; set; }

		public int ManagerCreatedBookings { get; set; } = 1;

		public CancellationTokenSource TokenSource { get; set; }

		public DtbBookingCreationErrorType? ManagerDeliverErrorType { get; set; }

		public void AddCall()
		{
			successfulCallsToDeliverTransportBooking.Add(new TestDtbDeliveryManagerDeliverTransportBookingCall());
		}

		public void ClearCalls()
		{
			successfulCallsToDeliverTransportBooking.Clear();
		}

		public IEnumerable<TestDtbDeliveryManagerDeliverTransportBookingCall> SuccessfulCallsToDeliverTransportBooking
		{
			get
			{
				return successfulCallsToDeliverTransportBooking.ToArray();
			}
		}
		readonly List<TestDtbDeliveryManagerDeliverTransportBookingCall> successfulCallsToDeliverTransportBooking = new List<TestDtbDeliveryManagerDeliverTransportBookingCall>();
	}
}
