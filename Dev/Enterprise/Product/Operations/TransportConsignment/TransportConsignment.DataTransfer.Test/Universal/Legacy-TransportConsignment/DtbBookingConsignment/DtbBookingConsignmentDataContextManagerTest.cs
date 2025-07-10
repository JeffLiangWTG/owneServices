using System;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.DataTransfer.Universal.Testing;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	[TestedType(typeof(DtbBookingConsignmentDataContextManager))]
	class DtbBookingConsignmentDataContextManagerTest : DtbTransportDataContextManagerTest<DtbBookingConsignment, DtbBookingConsignmentDataContextManager>
	{
		#region Context
		#region TestDataContextType
		protected override DataContextType ExpectedDataContextType
		{
			get
			{
				return DataContextType.TransportConsignment;
			}
		}

		#endregion
		#endregion
		#region Shipments
		#region TestShipmentDataObjectWriter
		protected override Type ExpectedShipmentDataObjectWriterType
		{
			get
			{
				return typeof(DtbBookingConsignmentDataObjectWriter);
			}
		}

		#endregion
		#endregion
		#region Events
		#region TestEventContextValues
		public void TestEventContextValues()
		{
			var consignment = Helper.CreateBookingConsignment();
			var manager = consignment.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertEquals("", manager.EventContextValues.ToStringContents(o => o.Key + " - " + o.Value));
		}

		#endregion
		#region TestEventParentFinder
		public void TestEventParentFinder()
		{
			IEventDataContextManager manager = new DtbBookingConsignmentDataContextManager();
			AssertNull(manager.GetLogParentsForEvent(new UniversalEvent(), Factory.BOFactory, new TestErrorLogger()));
		}

		#endregion
		#endregion
		#region Implementation
		protected override DtbTransport GetTransport()
		{
			return Helper.CreateBookingConsignment();
		}

		protected override DtbTransportConsolidation GetNewConsolidation()
		{
			return Helper.CreateConsolidation();
		}

		new TransportBookingConsignmentTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory.BOFactory));
			}
		}

		TransportBookingConsignmentTestHelper helper;
		#endregion
	}
}
