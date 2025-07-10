using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Moq;

namespace Enterprise.TransportBookings.ServiceTasks.Test
{
	internal class TestDtbDeliveryManager : IDtbDeliveryManager
	{
		public TestDtbDeliveryManager(BusinessObjectFactory factory, IDtbBookingParent parent, DtbBookingDirection direction, ZBool combineContainers, ILogger logger, IAutomatedDtbBookingCreationErrorManager errorManager)
		{
			Factory = factory;
			Parent = parent;
			Direction = direction;
			CombineContainers = combineContainers;
			if (logger is LoggerWithPrefix loggerWithPrefix)
			{
				Logger = loggerWithPrefix.ServiceTasklogger;
				BranchCode = loggerWithPrefix.Prefix;
			}
			else
			{
				Logger = logger;
			}
			ErrorManager = errorManager;
		}

		public IEnumerable<IDtbBooking> DeliverTransportBooking()
		{
			if (DeliverSetsCancellation && TokenSource != null)
			{
				TokenSource.Cancel();
			}

			if (DeliverThrowsException)
			{
				throw new Exception("Test DeliverTransportBooking Exception.");
			}

			AddDeliverErrorIfSet();

			ParentDeliveryManagerFactory.AddCall();

			var bookingsCreated = new List<IDtbBooking>();
			if (!DeliverAddsErrorToErrorManager)
			{
				for (var i = 0; i < ManagerCreatedBookings; i++)
				{
					bookingsCreated.Add(new Mock<IDtbBooking>().Object);
				}
			}

			return bookingsCreated;
		}

		public bool DeliverThrowsException { get; set; }

		public bool DeliverSetsCancellation { get; set; }

		public int ManagerCreatedBookings { get; set; } = 1;
		public DtbBookingCreationErrorType? DeliverError { get; set; }
		public bool DeliverAddsErrorToErrorManager
		{
			get
			{
				return DeliverError != null;
			}
		}

		void AddDeliverErrorIfSet()
		{
			if (DeliverError.HasValue)
			{
				ErrorManager?.AddError(DeliverError.Value, "Test Deliver Transport Booking error.");
			}
		}

		public CancellationTokenSource TokenSource { get; set; }

		public TestDtbDeliveryManagerFactory ParentDeliveryManagerFactory { get; set; }
		public BusinessObjectFactory Factory { get; }
		public IDtbBookingParent Parent { get; }
		public BusinessObject ParentBO => (BusinessObject)Parent;
		public string ParentTableCode
		{
			get
			{
				return ParentBO.TablePrefix;
			}
		}
		public DtbBookingDirection Direction { get; }
		public ZBool CombineContainers { get; }
		public ILogger Logger { get; }
		public ZString BranchCode { get; }
		public IAutomatedDtbBookingCreationErrorManager ErrorManager { get; }
	}
}
