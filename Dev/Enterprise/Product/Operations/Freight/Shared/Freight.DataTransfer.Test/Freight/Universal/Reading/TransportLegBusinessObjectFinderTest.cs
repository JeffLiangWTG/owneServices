using System;
using CargoWise.Application;
using Enterprise.Freight.Business;
using Enterprise.Integration.Schedule;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(TransportLegBusinessObjectFinder))]
	sealed class TransportLegBusinessObjectFinderTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new TransportLegBusinessObjectFinder(null, null));

			var shipment = Factory.New<CommonShipment>();

			AssertExceptionThrown(typeof(ArgumentNullException), () => new TransportLegBusinessObjectFinder(null, shipment));

			var dataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.PortOfLoading = new UNLOCO { Code = "NZAKL" };

			AssertExceptionThrown(typeof(ArgumentNullException), () => new TransportLegBusinessObjectFinder(dataObject, null));

			var transportLeg1 = shipment.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NZAKL";

			var transportLeg2 = shipment.Transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "NZAKL";
			transportLeg2.JW_RL_NKDiscPort = "USSFO";

			var transportLeg3 = shipment.Transports.AddNew();
			transportLeg3.JW_RL_NKLoadPort = "USSFO";
			transportLeg3.JW_RL_NKLoadPort = "USMIA";

			var finder = new TransportLegBusinessObjectFinder(dataObject, shipment);
			AssertEquals(null, finder.Find(new[] { transportLeg1, transportLeg2, transportLeg3 }));

			dataObject.PortOfDischarge = new UNLOCO { Code = "USSFO" };

			finder = new TransportLegBusinessObjectFinder(dataObject, shipment);
			AssertEquals(transportLeg2, finder.Find(new[] { transportLeg1, transportLeg2, transportLeg3 }));

			var bookingConsolidation = Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>()) as ITransportParentCommon;
			AssertExceptionThrown(typeof(ArgumentNullException), () => new TransportLegBusinessObjectFinder(null, bookingConsolidation));
			var collection = ObjectFactory.Get<ITransportCollection>("ITransportCollection", bookingConsolidation) as TransportCollection;
			collection.Load();

			var consolLeg1 = collection.AddNew();
			consolLeg1.JW_RL_NKLoadPort = "AUSYD";
			consolLeg1.JW_RL_NKDiscPort = "NZAKL";

			var consolLeg2 = collection.AddNew();
			consolLeg2.JW_RL_NKLoadPort = "NZAKL";
			consolLeg2.JW_RL_NKDiscPort = "USSFO";

			var consolLeg3 = shipment.Transports.AddNew();
			consolLeg3.JW_RL_NKLoadPort = "USSFO";
			consolLeg3.JW_RL_NKLoadPort = "USMIA";

			var tbfinder = new TransportLegBusinessObjectFinder(dataObject, bookingConsolidation);
			dataObject.PortOfDischarge = null;
			AssertEquals(null, tbfinder.Find(new[] { consolLeg1, consolLeg2, consolLeg3 }));

			dataObject.PortOfDischarge = new UNLOCO { Code = "USSFO" };

			finder = new TransportLegBusinessObjectFinder(dataObject, shipment);
			AssertEquals(consolLeg2, finder.Find(new[] { consolLeg1, consolLeg2, consolLeg3 }));
		}
	}
}
