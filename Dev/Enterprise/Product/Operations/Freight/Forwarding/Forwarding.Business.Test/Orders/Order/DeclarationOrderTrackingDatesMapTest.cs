using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(DeclarationOrderTrackingDatesMap))]
	sealed class DeclarationOrderTrackingDatesMapTest : OrderTrackingDatesMapTest<DeclarationOrderTrackingDatesMap>
	{
		public void TestDepartureActualDateFromTransport()
		{
			var map = GetNewMapWithTransport();

			AssertEquals("DepartureActualDate", actualDepartureDateTransport, map.DepartureActualDate);
		}

		public void TestArrivalActualDateFromTransport()
		{
			var map = GetNewMapWithTransport();

			AssertEquals("ArrivalActualDate", actualArrivalDateTransport, map.ArrivalActualDate);
		}

		public override void TestDepartureActualDate()
		{
			var map = GetNewMap();

			AssertEquals("DepartureActualDate", ZDateTime.Empty, map.DepartureActualDate);
		}

		public override void TestArrivalActualDate()
		{
			var map = GetNewMap();

			AssertEquals("ArrivalActualDate", ZDateTime.Empty, map.ArrivalActualDate);
		}

		public override void TestCargoAvailableActualDate()
		{
			var map = GetNewMap();

			AssertEquals("CargoAvailableActualDate", ZDateTime.Empty, map.CargoAvailableActualDate);
		}

		public override void TestDeliveryCartageAdvisedActualDate()
		{
			var map = GetNewMap();

			AssertEquals("DeliveryCartageAdvisedActualDate", deliveryCartageAdvised, map.DeliveryCartageAdvisedActualDate);
		}

		public override void TestDeliveryCartageCompleteFinalizedActualDate()
		{
			var map = GetNewMap();

			AssertEquals("DeliveryCartageCompleteFinalizedActualDate", deliveryCartageCompleted, map.DeliveryCartageCompleteFinalizedActualDate);
		}

		public override void TestDepartureScheduledDate()
		{
			var map = GetNewMap();

			AssertEquals("DepartureScheduledDate", estimatedDepartureDate, map.DepartureScheduledDate);
		}

		public override void TestArrivalScheduledDate()
		{
			var map = GetNewMap();

			AssertEquals("ArrivalScheduledDate", estimatedArrivalDate, map.ArrivalScheduledDate);
		}

		public override void TestDeliveryCartageCompleteFinalizedScheduledDate()
		{
			var map = GetNewMap();

			AssertEquals("DeliveryCartageCompleteFinalizedScheduledDate", estimatedCartageDelivery, map.DeliveryCartageCompleteFinalizedScheduledDate);
		}

		readonly ZDateTime estimatedDepartureDate = new ZDateTime(2012, 12, 31);
		readonly ZDateTime actualDepartureDate = new ZDateTime(2013, 1, 1);
		readonly ZDateTime actualDepartureDateTransport = new ZDateTime(2013, 1, 3);

		readonly ZDateTime estimatedArrivalDate = new ZDateTime(2012, 1, 9);
		readonly ZDateTime actualArrivalDate = new ZDateTime(2013, 1, 10);
		readonly ZDateTime actualArrivalDateTransport = new ZDateTime(2013, 1, 12);

		readonly ZDateTime estimatedCartageDelivery = new ZDateTime(2013, 1, 11);
		readonly ZDateTime deliveryCartageAdvised = new ZDateTime(2013, 1, 12);
		readonly ZDateTime deliveryCartageCompleted = new ZDateTime(2013, 1, 13);

		DeclarationOrderTrackingDatesMap GetNewMap()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();

			declaration[JobDeclarationSchema.Constants.JE_RL_NKOrigin] = "USSFO";
			declaration[JobDeclarationSchema.Constants.JE_RL_NKFinalDestination] = "AUSYD";
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";

			declaration[JobDeclarationSchema.Constants.JE_DateAtOrigin] = estimatedDepartureDate;
			declaration[JobDeclarationSchema.Constants.JE_ExportDate] = actualDepartureDate;

			declaration[JobDeclarationSchema.Constants.JE_DateOfArrival] = actualArrivalDate;
			declaration[JobDeclarationSchema.Constants.JE_DateAtFinalDestination] = estimatedArrivalDate;

			var doscAndCartageParent = (IShipmentWithDocsAndCartage)declaration;

			doscAndCartageParent.DocsAndCartage.JP_EstimatedDelivery = estimatedCartageDelivery;
			doscAndCartageParent.DocsAndCartage.JP_DeliveryCartageAdvised = deliveryCartageAdvised;
			doscAndCartageParent.DocsAndCartage.JP_DeliveryCartageCompleted = deliveryCartageCompleted;

			return new DeclarationOrderTrackingDatesMap(declaration);
		}

		DeclarationOrderTrackingDatesMap GetNewMapWithTransport()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_ExportDate] = actualDepartureDate;
			declaration[JobDeclarationSchema.Constants.JE_DateOfArrival] = actualArrivalDate;

			var departureTransport = ((IRoutingSupport)declaration).Transports.AddNew();
			departureTransport.JW_RL_NKLoadPort = "AUSYD";
			departureTransport.JW_RL_NKDiscPort = "NZAKL";
			departureTransport.JW_ATD = actualDepartureDateTransport;

			var arrivalTransport = ((IRoutingSupport)declaration).Transports.AddNew();
			arrivalTransport.JW_RL_NKLoadPort = "NZAKL";
			arrivalTransport.JW_RL_NKDiscPort = "USSFO";
			arrivalTransport.JW_ATA = actualArrivalDateTransport;

			return new DeclarationOrderTrackingDatesMap(declaration);
		}
	}
}
