using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyShipmentReferencesTest : TestCaseWithFactory
	{
		public void TestPopulateFromContext()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AgencyShipmentReferences((IXmlEventValueObjectContextValueList)null));
			var context = new Mock<IXmlEventValueObjectContextValueList>();
			context.Setup(ctx => ctx.CarriersBookingReference).Returns("BKG001");
			context.Setup(ctx => ctx.MBOLNumber).Returns("HBL001");
			context.Setup(ctx => ctx.LloydsNumber).Returns("12345");
			context.Setup(ctx => ctx.VesselName).Returns("TAIKO");
			context.Setup(ctx => ctx.VoyageNumber).Returns("001");
			context.Setup(ctx => ctx.HBOLOriginUNLOCO).Returns("AUSYD");
			context.Setup(ctx => ctx.HBOLDestinationUNLOCO).Returns("NZAKL");
			context.Setup(ctx => ctx.LegOriginUNLOCO).Returns("AUMEL");
			context.Setup(ctx => ctx.LegDestinationUNLOCO).Returns("NZCHC");
			var references = new AgencyShipmentReferences(context.Object);
			AssertEquals("BKG001", references.CarriersBookingReference);
			AssertEquals("HBL001", references.OceanBillNumber);
			AssertEquals(1, references.SailingReferences.Count());
			AssertEquals("12345", references.SailingReferences.ElementAt(0).LloydsNumber);
			AssertEquals("TAIKO", references.SailingReferences.ElementAt(0).VesselName);
			AssertEquals("001", references.SailingReferences.ElementAt(0).VoyageNumber);
			AssertEquals("AUMEL", references.SailingReferences.ElementAt(0).PortOfLoading);
			AssertEquals("NZCHC", references.SailingReferences.ElementAt(0).PortOfDischarge);
		}

		public void TestPopulateFromUniversalShipment()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AgencyShipmentReferences(null, ZGuid.Empty, ZString.Empty));

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var references = new AgencyShipmentReferences(dataObject, ZGuid.Empty, ZString.Empty);

			AssertEquals(ZString.Empty, references.AgentsReference);
			AssertEquals(ZString.Empty, references.CarriersBookingReference);
			AssertEquals(ZString.Empty, references.OceanBillNumber);
			AssertEquals(0, references.SailingReferences.Count());

			dataObject.AgentsReference = "AGT001";
			dataObject.BookingConfirmationReference = "BKG001";
			dataObject.WayBillNumber = "HBL001";
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "CTN00001" } });
			dataObject.DataContext = new DataContext { RecipientRoleCollection = new List<RecipientRole> { new RecipientRole { Code = RecipientRoleType.CAR, ServiceCode = ServiceCodeType.VGM } } };
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { new TransportLeg { VesselLloydsIMO = "8204975", //VesselName = "TAIKO"
 VoyageFlightNo = "001", PortOfLoading = new UNLOCO { Code = "AUSYD" }, PortOfDischarge = new UNLOCO { Code = "AUMEL" }, TransportMode = TransportMode.Sea }, new TransportLeg { VesselName = "POTIOMKIN", VoyageFlightNo = "002", PortOfLoading = new UNLOCO { Code = "AUMEL" }, PortOfDischarge = new UNLOCO { Code = "NZAKL" }, TransportMode = TransportMode.Sea }, new TransportLeg { VesselName = "POTIOMKIN", VoyageFlightNo = "002", PortOfLoading = new UNLOCO { Code = "AUMEL" }, PortOfDischarge = new UNLOCO { Code = "NZAKL" }, TransportMode = TransportMode.Sea }, new TransportLeg { VesselName = "AEROFLOT", VoyageFlightNo = "QF001", PortOfLoading = new UNLOCO { Code = "NZAKL" }, PortOfDischarge = new UNLOCO { Code = "SGSIN" }, TransportMode = TransportMode.Air } });

			references = new AgencyShipmentReferences(dataObject, ZGuid.Empty, ZString.Empty);
			AssertEquals("AGT001", references.AgentsReference);
			AssertEquals("BKG001", references.CarriersBookingReference);
			AssertEquals("HBL001", references.OceanBillNumber);
			AssertEquals("CTN00001", references.ContainerNumber);
			AssertEquals(false, references.IsTargettedToBothAgentModules);
			AssertEquals(true, references.IsVGM);
			AssertEquals(true, references.IsCarrierVGM);
			AssertContainsExactElementsInAnyOrder(new[] { "TAIKO|001|AUSYD|AUMEL", "POTIOMKIN|002|AUMEL|NZAKL" }, references.SailingReferences.Select(Format));

			dataObject.DataContext = new DataContext { RecipientRoleCollection = new List<RecipientRole> { new RecipientRole { Code = RecipientRoleType.CAR } }, DataTargetCollection = new List<DataTarget> { new DataTarget { Type = "BillOfLading" }, new DataTarget { Type = "AgencyBooking" } } };
			references = new AgencyShipmentReferences(dataObject, ZGuid.Empty, ZString.Empty);
			AssertEquals(true, references.IsTargettedToBothAgentModules);
			AssertEquals(false, references.IsVGM);
			AssertEquals(false, references.IsCarrierVGM);

			dataObject.DataContext = new DataContext { RecipientRoleCollection = new List<RecipientRole> { new RecipientRole { ServiceCode = ServiceCodeType.VGM } }, DataTargetCollection = new List<DataTarget> { new DataTarget { Type = "BillOfLading" }, new DataTarget { Type = "AgencyBooking" } } };
			references = new AgencyShipmentReferences(dataObject, ZGuid.Empty, ZString.Empty);
			AssertEquals(true, references.IsVGM);
			AssertEquals(false, references.IsCarrierVGM);
		}

		public void TestPopulateFromAgencyShipment()
		{
			AssertPopulateFromAgencyShipment<AgencyBooking>();
			AssertPopulateFromAgencyShipment<BillOfLading>();
		}

		void AssertPopulateFromAgencyShipment<T>()
			where T : AgencyShipment
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AgencyShipmentReferences(default(T)));
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY pty ltd";
			var agencyShipment = Factory.New<T>();
			agencyShipment.JS_BookingReference = "AGT001";
			agencyShipment.JS_CFSReference = "BKG001";
			agencyShipment.JS_HouseBill = "HBL001";
			agencyShipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			var references = new AgencyShipmentReferences(agencyShipment);
			AssertEquals("HBL001", references.OceanBillNumber);
			AssertEquals(0, references.SailingReferences.Count());
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "NZAKL", "USS ESSES", "001");
			agencyShipment.JS_JX = sailing.PK;
			references = new AgencyShipmentReferences(agencyShipment);
			AssertEquals("AGT001", references.AgentsReference);
			AssertEquals("BKG001", references.CarriersBookingReference);
			AssertEquals("HBL001", references.OceanBillNumber);
			AssertEquals(bookingParty.PK, references.BookingPartyPK);
			AssertEquals("", references.BookingPartyName);
			AssertContainsExactElementsInAnyOrder(new[] { "USS ESSES|001|AUSYD|NZAKL" }, references.SailingReferences.Select(Format));
		}

		#region Implementation
		string Format(SailingReference sailingReference)
		{
			return string.Format("{0}|{1}|{2}|{3}", sailingReference.VesselName, sailingReference.VoyageNumber, sailingReference.PortOfLoading, sailingReference.PortOfDischarge);
		}
		#endregion
	}
}
