using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class JobVoyageReferencesTest : TestCaseWithFactory
	{
		public void TestPopulateFromContext_Sea()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobVoyageReferences((IXmlEventValueObjectContextValueList)null));

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			context.Setup(m => m.TransportMode).Returns("SEA");
			context.Setup(m => m.VesselName).Returns("TAIKO");
			context.Setup(m => m.FlightNumber).Returns("A001");
			context.Setup(m => m.VoyageNumber).Returns("A001");
			context.Setup(m => m.FlightDate).Returns(new ZDateTime(2012, 1, 10, 16, 50, 0));
			context.Setup(m => m.IsCharter).Returns(true);
			context.Setup(m => m.LloydsNumber).Returns("1234567");
			context.Setup(m => m.VesselCallSign).Returns("SEATAIKO01");
			context.Setup(m => m.CarrierCode).Returns("TAIK");

			var references = new JobVoyageReferences(context.Object);

			AssertEquals("SEA", references.TransportMode);
			AssertEquals("TAIKO", references.VesselName);
			AssertEquals("A001", references.VoyageFlight);
			AssertEquals(ZGuid.Empty, references.CarrierPK);
			AssertEquals(new ZDateTime(2012, 1, 10, 16, 50, 0), references.FlightDate);
			AssertEquals(true, references.IsCharter);
			AssertEquals("1234567", references.LloydsNumber);
			AssertEquals("SEATAIKO01", references.VesselCallSign);
			AssertEquals("TAIK", references.CarrierSCACCode);
		}

		public void TestPopulateFromContext_Air()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobVoyageReferences((IXmlEventValueObjectContextValueList)null));

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			context.Setup(m => m.TransportMode).Returns("AIR");
			context.Setup(m => m.VesselName).Returns("ZEBRA");
			context.Setup(m => m.FlightNumber).Returns("QN001");
			context.Setup(m => m.FlightDate).Returns(new ZDateTime(2012, 1, 10, 16, 50, 0));
			context.Setup(m => m.IsCharter).Returns(true);
			context.Setup(m => m.LloydsNumber).Returns("7654321");
			context.Setup(m => m.VesselCallSign).Returns("AIRZEBRA01");
			context.Setup(m => m.CarrierCode).Returns("ZEBR");

			var references = new JobVoyageReferences(context.Object);

			AssertEquals("AIR", references.TransportMode);
			AssertEquals("ZEBRA", references.VesselName);
			AssertEquals("QN001", references.VoyageFlight);
			AssertEquals(ZGuid.Empty, references.CarrierPK);
			AssertEquals(new ZDateTime(2012, 1, 10, 16, 50, 0), references.FlightDate);
			AssertEquals(true, references.IsCharter);
			AssertEquals(ZString.Empty, references.LloydsNumber);
			AssertEquals(ZString.Empty, references.VesselCallSign);
			AssertEquals(ZString.Empty, references.CarrierSCACCode);
		}

		public void TestPopulateFromContext_Road()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobVoyageReferences((IXmlEventValueObjectContextValueList)null));

			var context = new Mock<IXmlEventValueObjectContextValueList>();
			context.Setup(m => m.TransportMode).Returns("ROA");
			context.Setup(m => m.VesselName).Returns("HORSE");
			context.Setup(m => m.FlightNumber).Returns("R001");
			context.Setup(m => m.VoyageNumber).Returns("R001");
			context.Setup(m => m.FlightDate).Returns(new ZDateTime(2012, 1, 10, 16, 50, 0));
			context.Setup(m => m.IsCharter).Returns(true);
			context.Setup(m => m.LloydsNumber).Returns("0101010");
			context.Setup(m => m.VesselCallSign).Returns("ROAHORSE01");
			context.Setup(m => m.CarrierCode).Returns("HORSE");

			var references = new JobVoyageReferences(context.Object);

			AssertEquals("ROA", references.TransportMode);
			AssertEquals("HORSE", references.VesselName);
			AssertEquals("R001", references.VoyageFlight);
			AssertEquals(ZGuid.Empty, references.CarrierPK);
			AssertEquals(new ZDateTime(2012, 1, 10, 16, 50, 0), references.FlightDate);
			AssertEquals(true, references.IsCharter);
			AssertEquals(ZString.Empty, references.LloydsNumber);
			AssertEquals(ZString.Empty, references.VesselCallSign);
			AssertEquals(ZString.Empty, references.CarrierSCACCode);
		}

		public void TestPopulateFromDataObject_Sea()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobVoyageReferences((Shipment)null));

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("SEA", (ICodeDescriptionPairList)null);
			shipment.VesselName = "VESSEL1";
			shipment.VoyageFlightNo = "144H";
			shipment.LloydsIMO = "LLOY";

			var transport = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			transport.EstimatedDeparture = ZDate.Today;
			transport.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transport.Carrier.OrganizationCode = "AAABBB";
			transport.VesselLloydsIMO = "LLOY1";

			transport.Carrier.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transport.Carrier.RegistrationNumberCollection.Add(new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
			{
				CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.UnitedStates },
				Type = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.CarrierCode },
				Value = "CCDD"
			});

			var transport2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			transport2.EstimatedDeparture = ZDate.Today.AddDays(1);
			transport2.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transport2.Carrier.OrganizationCode = "XXXYYY";
			transport2.VesselLloydsIMO = "LLOY2";

			transport2.Carrier.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transport2.Carrier.RegistrationNumberCollection.Add(new UniversalDataBuss.DataObjects.Universal.RegistrationNumber()
			{
				CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.UnitedStates },
				Type = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.CarrierCode },
				Value = "EEFF"
			});

			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			shipment.TransportLegCollection.Add(transport);
			shipment.TransportLegCollection.Add(transport2);

			var references = new JobVoyageReferences(shipment);

			AssertEquals("SEA", references.TransportMode);
			AssertEquals("VESSEL1", references.VesselName);
			AssertEquals("144H", references.VoyageFlight);
			AssertEquals(ZDate.Today, references.FlightDate);
			AssertEquals("CCDD", references.CarrierSCACCode);
			AssertEquals("LLOY", references.LloydsNumber);
		}

		public void TestPopulateFromDataObject_Air()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobVoyageReferences((Shipment)null));

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("AIR", (ICodeDescriptionPairList)null);
			shipment.VesselName = "X1";
			shipment.VoyageFlightNo = "QF52";
			shipment.LloydsIMO = "LLOY";

			var transport = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			transport.EstimatedDeparture = ZDate.Today;
			transport.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transport.Carrier.OrganizationCode = "AAABBB";
			transport.VesselLloydsIMO = "LLOY1";

			var transport2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			transport2.EstimatedDeparture = ZDate.Today.AddDays(1);
			transport2.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transport2.Carrier.OrganizationCode = "XXXYYY";
			transport2.VesselLloydsIMO = "LLOY2";

			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			shipment.TransportLegCollection.Add(transport);
			shipment.TransportLegCollection.Add(transport2);

			var references = new JobVoyageReferences(shipment);

			AssertEquals("AIR", references.TransportMode);
			AssertEquals("X1", references.VesselName);
			AssertEquals("QF52", references.VoyageFlight);
			AssertEquals(ZDate.Today, references.FlightDate);
			AssertEquals("", references.CarrierSCACCode);
			AssertEquals("", references.LloydsNumber);
		}

		[ExpectNoExceptions]
		public void TestPopulateFromDataObject_WhenTransportModeIsNull()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertNull(shipment.TransportMode);

			var references = new JobVoyageReferences(shipment);
			AssertEquals(ZString.Empty, references.TransportMode);
		}

		public void TestPopulateFromDataObject_Road()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobVoyageReferences((Shipment)null));

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("ROA", (ICodeDescriptionPairList)null);
			shipment.VesselName = "V1";
			shipment.VoyageFlightNo = "AAA";
			shipment.LloydsIMO = "LLOY";

			var transport = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			transport.EstimatedDeparture = ZDate.Today;
			transport.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transport.Carrier.OrganizationCode = "AAABBB";
			transport.VesselLloydsIMO = "LLOY1";

			var transport2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			transport2.EstimatedDeparture = ZDate.Today.AddDays(1);
			transport2.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transport2.Carrier.OrganizationCode = "XXXYYY";
			transport2.VesselLloydsIMO = "LLOY2";

			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			shipment.TransportLegCollection.Add(transport);
			shipment.TransportLegCollection.Add(transport2);

			var references = new JobVoyageReferences(shipment);

			AssertEquals("ROA", references.TransportMode);
			AssertEquals("V1", references.VesselName);
			AssertEquals("AAA", references.VoyageFlight);
			AssertEquals(ZDate.Today, references.FlightDate);
			AssertEquals("", references.CarrierSCACCode);
			AssertEquals("", references.LloydsNumber);
		}

		public void TestPopulateFromVoyage_Sea()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobVoyageReferences((JobVoyage)null));

			var vessel = RefVessel.LookupVesselByName("TAIKO", Factory).First();

			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "A001");
			var org = Factory.New<OrgHeader>();
			voyage.JV_OH_Line = org.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.Vessel.RV_RadioCallSign = "SEATAIKO01";
			org.CustomsCodes.AddNew("CCC", "TAIK", "US");

			var references = new JobVoyageReferences(voyage);

			AssertEquals("SEA", references.TransportMode);
			AssertEquals("TAIKO", references.VesselName);
			AssertEquals("A001", references.VoyageFlight);
			AssertEquals(voyage.JV_OH_Line, references.CarrierPK);
			AssertEquals(ZDateTime.Empty, references.FlightDate);
			AssertEquals(false, references.IsCharter);
			AssertEquals("8204975", references.LloydsNumber);
			AssertEquals("SEATAIKO01", references.VesselCallSign);
			AssertEquals("TAIK", references.CarrierSCACCode);
		}

		public void TestPopulateFromVoyage_Air()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobVoyageReferences((JobVoyage)null));

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "ZEBRA";

			var voyage = UniversalTestHelper.CreateAirVoyage(Factory, "QN001");
			var org = Factory.New<OrgHeader>();
			voyage.JV_OH_Line = org.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			org.CustomsCodes.AddNew("CCC", "ZEBR");

			var references = new JobVoyageReferences(voyage);

			AssertEquals("AIR", references.TransportMode);
			AssertEquals("ZEBRA", references.VesselName);
			AssertEquals("QN001", references.VoyageFlight);
			AssertEquals(voyage.JV_OH_Line, references.CarrierPK);
			AssertEquals(ZDateTime.Empty, references.FlightDate);
			AssertEquals(false, references.IsCharter);
			AssertEquals(ZString.Empty, references.LloydsNumber);
			AssertEquals(ZString.Empty, references.VesselCallSign);
			AssertEquals(ZString.Empty, references.CarrierSCACCode);
		}

		public void TestPopulateFromVoyage_Road()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobVoyageReferences((JobVoyage)null));

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "HORSE";

			var voyage = UniversalTestHelper.CreateRoadVoyage(Factory, "R001");
			var org = Factory.New<OrgHeader>();
			voyage.JV_OH_Line = org.PK;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			org.CustomsCodes.AddNew("CCC", "HORS");

			var references = new JobVoyageReferences(voyage);

			AssertEquals("ROA", references.TransportMode);
			AssertEquals("HORSE", references.VesselName);
			AssertEquals("R001", references.VoyageFlight);
			AssertEquals(voyage.JV_OH_Line, references.CarrierPK);
			AssertEquals(ZDateTime.Empty, references.FlightDate);
			AssertEquals(false, references.IsCharter);
			AssertEquals(ZString.Empty, references.LloydsNumber);
			AssertEquals(ZString.Empty, references.VesselCallSign);
			AssertEquals(ZString.Empty, references.CarrierSCACCode);
		}
	}
}
