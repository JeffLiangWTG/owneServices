using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingBuildConsolHelperTest : BuildConsolHelperTest
	{
		public void TestAttachNewConsolToBookedShipmentWithFormConsol_BookedShippingLineIsRemoved()
		{
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_IsShippingLine = true;
			consignee.OH_IsShippingProvider = true;
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignee.MainAddress.OA_Address1 = "Address 1";
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_IsShippingLine = true;
			consignor.OH_IsShippingProvider = true;
			consignor.OH_FullName = "Consignor";
			consignor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignor.MainAddress.OA_Address1 = "Address 1";
			QuotedBooking bok = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
			bok.Mode = Constants.TransportModes.Air;
			bok.Booking.JS_PackingMode = Constants.ContainerModes.Loose;
			bok.Destination = "MAAGA";
			bok.Booking.JS_RL_NKDestination = "MAAGA";
			bok.OH_Carrier = carrier.PK;
			bok.Booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			bok.Booking.JS_CFSReference = "Carrier Ref.";
			bok.Booking.ConsignorPK = consignor.PK;
			bok.Booking.ConsigneePK = consignee.PK;
			Factory.Save();
			var helper = new BuildConsolHelper();
			var shipment = Factory.Load<ForwardingShipment>(bok.Booking.PK);
			helper.TurnBookingIntoShipment(shipment, null, bok.PK);
			AssertEquals("Precondition: Expecting bookings' carrier to be carrier", carrier.MainAddress.PK, shipment.JS_OA_BookedShippingLineAddress);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			var consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.ParentShipment = shipment;
			consolCollection.AddRelationshipToNewObject(consol);
			Factory.Save();
			AssertEquals("Expecting bookings' carrier to be carrier", carrier.MainAddress.PK, shipment.JS_OA_BookedShippingLineAddress);
		}

		public void TestOOQConsolidation_WhenConsolidateOOQWithCarrier_ThenCarrierShouldBePreservedAndCopiedToConsol()
		{
			var carrier = CreateCarrier();

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "MAAGA";
			quote.CurrentOneOffQuote.TT_OH_Carrier = carrier.PK;

			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.ConvertQuoteToQuotedBooking();

			Factory.Save();

			var newConsol = ConsolidateConsolFromBooking(quotedBooking);

			AssertEquals("Carrier should be copied to the new consol", carrier.MainAddress.PK, newConsol.JK_OA_ShippingLineAddress);
			AssertEquals("Carrier should be preserved on the booking", carrier.MainAddress.PK, quotedBooking.Booking.JS_OA_BookedShippingLineAddress);
		}

		public void TestQBConsolidation_WhenConsolidateQBWithCarrier_ThenCarrierShouldBePreservedAndCopiedToConsol()
			=> AssertCarrierAfterConsolidatingBooking(QuoteBookingType.QuickBooking);

		public void TestBWQConsolidation_WhenConsolidateBWQWithCarrier_ThenCarrierShouldBePreservedAndCopiedToConsol()
			=> AssertCarrierAfterConsolidatingBooking(QuoteBookingType.BookingWithQuote);

		public void TestMakeConsolFromBooking_MAWB()
		{
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_FullName = "ShippingLine";
			shippingLine.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shippingLine.MainAddress.OA_Address1 = "Address 1";
			Factory.Save();
			JobSailing exportSailing = sailingsHelper.SydLaxFlightLeg;
			exportSailing.Voyage.JV_OH_Line = shippingLine.PK;
			exportSailing.JX_IsPublished = true;
			JobMawb jobMawb = Factory.New<JobMawb>();
			jobMawb.JM_Airline3DigitPrefix = "176";
			jobMawb.JM_MAWB = "10000001";
			jobMawb.JM_ServiceLevel = "STD";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			jobMawb.JM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_IsShippingLine = true;
			consignee.OH_IsShippingProvider = true;
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignee.MainAddress.OA_Address1 = "Address 1";
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_IsShippingLine = true;
			consignor.OH_IsShippingProvider = true;
			consignor.OH_FullName = "Consignor";
			consignor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignor.MainAddress.OA_Address1 = "Address 1";
			QuotedBooking bok = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
			((ISailingChooserParent)bok).SailingJX = exportSailing.PK;
			bok.TransportMode = Constants.TransportModes.Air;
			bok.ContainerMode = Constants.ContainerModes.Loose;
			bok.Destination = "MAAGA";
			bok.Booking.JS_RL_NKDestination = "MAAGA";
			bok.OH_Carrier = carrier.PK;
			bok.Booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			bok.Booking.JS_CFSReference = "Carrier Ref.";
			bok.Booking.ConsignorPK = consignor.PK;
			bok.Booking.ConsigneePK = consignee.PK;
			((ISailingChooserParent)bok).IsDirect = true;
			((ISailingChooserParent)bok).AWBServiceLevel = "STD";
			((ISailingChooserParent)bok).MawbNumber = "176";
			((ISailingChooserParent)bok).IsNeutralMaster = true;
			Factory.Save();
			AssertEquals("17610000001", ((ISailingChooserParent)bok).MawbNumber);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol newConsol = newFactory.New<ForwardingConsol>();
			BuildConsolHelper helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, bok.Booking.PK);
			CommonShipment loadedShipment = LoadShipmentInConsolsFactory(newConsol, bok.Booking.PK);
			Assert("Expecting JS_IsBooking to be true.", loadedShipment.JS_IsBooking);
			Assert("Expecting JS_IsForwardRegistered to be true.", loadedShipment.JS_IsForwardRegistered);
			AssertEquals("Expecting new consol to have one shipment.", 1, newConsol.Shipments.Count);
			AssertEquals("Expecting consol to be sea", Constants.TransportModes.Air, newConsol.JK_TransportMode);
			AssertEquals("Expecting consol to be fcl", Enterprise.Core.Constants.ContainerModes.Loose, newConsol.JK_ConsolMode);
			AssertEquals("Expecting consols' carrier to be carrier.", carrier.PK, newConsol.ShippingLinePK);
			AssertEquals("Expecting consol's booking ref to be carrier ref.", "Carrier Ref.", newConsol.JK_BookingReference);
			AssertEquals("Expecting consol's SendingForwarder to be Consignor", ZGuid.Empty, newConsol.SendingForwarderPK);
			AssertEquals("Expecting consol's ReceivingForwarder to be Consignee", ZGuid.Empty, newConsol.ReceivingForwarderPK);
			AssertEquals("Expecting consol's AWBServiceLevel to be AWBServiceLevel", "STD", newConsol.JK_AWBServiceLevel);
			AssertEquals("Expecting consol's IsNeutralMaster to be IsNeutralMaster", true, newConsol.JK_IsNeutralMaster);
			AssertEquals("Expecting consol's MasterBillNum to be MasterBillNum", "17610000001", newConsol.JK_MasterBillNum);
		}

		public void TestMakingAConsolFromABookingShouldNotChangeMawbs()
		{
			ZDateTime now = ZDateTime.Today;
			JobSailing sailing1 = CreateExportSailing(now.AddHours(6), "ZN4321");
			JobSailing sailing2 = CreateExportSailing(now.AddHours(6), "CX4321");
			JobSailing sailing3 = CreateExportSailing(now.AddHours(6), "QF1234");
			JobMawb mawb1 = CreateMawb("ZN");
			JobMawb mawb2 = CreateMawb("CX");
			JobMawb mawb3 = CreateMawb("QF");
			ZString houseBill = mawb2.JM_Airline3DigitPrefix + mawb2.JM_MAWB;
			Factory.Save();
			QuotedBooking booking = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);
			((ISailingChooserParent)booking).SailingJX = sailing2.PK;
			booking.Mode = Core.Constants.TransportModes.Air;
			((ISailingChooserParent)booking).IsDirect = true;
			((ISailingChooserParent)booking).IsNeutralMaster = true;
			Factory.Save();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			new BuildConsolHelper().MakeConsolFromBookingOrStandaloneShipment(consol, booking.Booking.PK);
			Factory.Save();
			AssertEquals("Should not have been shafted", houseBill, consol.JK_MasterBillNum);
			AssertEquals("Should be reset", ZString.Empty, consol.Shipments[0].JS_HouseBill);
		}

		void AssertCarrierAfterConsolidatingBooking(QuoteBookingType type)
		{
			var carrier = CreateCarrier();
			var consignee = CreateConsignee();
			var consignor = CreateConsignor();

			var booking = QuotedBooking.New(type, Factory);
			booking.TransportMode = Constants.TransportModes.Air;
			booking.ContainerMode = Constants.ContainerModes.Loose;
			booking.Destination = "MAAGA";
			booking.OH_Carrier = carrier.PK;
			booking.Booking.JS_RL_NKDestination = "MAAGA";
			booking.Booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			booking.Booking.ConsignorPK = consignor.PK;
			booking.Booking.ConsigneePK = consignee.PK;

			Factory.Save();

			var newConsol = ConsolidateConsolFromBooking(booking);

			AssertEquals("Carrier should be copied to the new consol", carrier.MainAddress.PK, newConsol.JK_OA_ShippingLineAddress);
			AssertEquals("Carrier should be preserved on the booking", carrier.MainAddress.PK, booking.Booking.JS_OA_BookedShippingLineAddress);
		}

		#region CO2e

		public void TestTurnBookingIntoShipment_DoesNotPopulateShipmentCO2e()
		{
			// Arrange
			var quotedBooking = CreateQuotedBookingWithCO2e(2m, CO2eStatusList.Codes.Current);
			var shipment = quotedBooking.Booking;
			Factory.Save();

			// Act
			new BuildConsolHelper().TurnBookingIntoShipment(shipment, null, quotedBooking.PK);

			// Assert
			CombineAssertions("Shipment", () =>
			{
				AssertEquals("CO2e status should be NON when TurnBookingIntoShipment is called", CO2eStatusList.Codes.NotCalculated, shipment.GetCO2eStatus());
				AssertEquals("CO2e status should be 0 when TurnBookingIntoShipment is called", 0m, shipment.GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty when TurnBookingIntoShipment is called", ZString.Empty, shipment.TotalCO2eForBinding);
			});
		}

		public void TestTurnBookingIntoShipment_DoesNotPopulateShipmentCO2e_WithVia()
		{
			// Arrange
			var quotedBooking = CreateQuotedBookingWithCO2e(2m, CO2eStatusList.Codes.Current, via: "USLAX");
			var shipment = quotedBooking.Booking;
			Factory.Save();

			// Act
			new BuildConsolHelper().TurnBookingIntoShipment(shipment, null, quotedBooking.PK);

			// Assert
			CombineAssertions("Shipment", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, shipment.GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, shipment.GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, shipment.TotalCO2eForBinding);
			});
		}

		public void TestMakeConsolFromBooking_DoesNotPopulateShipmentConsolCO2e_NoSailing()
		{
			// Arrange
			var quotedBooking = CreateQuotedBookingWithCO2e(2m, CO2eStatusList.Codes.Current);
			Factory.Save();

			// Act
			var consol = new BusinessObjectFactory().New<ForwardingConsol>();
			new BuildConsolHelper().MakeConsolFromBookingOrStandaloneShipment(consol, quotedBooking.Booking.PK, quotedBooking.PK);

			// Assert
			var shipment = LoadShipmentInConsolsFactory(consol, quotedBooking.Booking.PK);
			CombineAssertions("Shipment", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, ((ICO2eProvider)shipment).GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, ((ICO2eProvider)shipment).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, ((ForwardingShipment)shipment).TotalCO2eForBinding);
			});

			CombineAssertions("Consol", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, consol.GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, consol.GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, consol.TotalCO2eForBinding);
			});
		}

		public void TestMakeConsolFromBooking_DoesNotPopulateShipmentConsolCO2e_SailingLinked()
		{
			// Arrange
			var quotedBooking = CreateQuotedBookingWithCO2e(2m, CO2eStatusList.Codes.Current, withSailing: true);
			Factory.Save();

			// Act
			var consol = new BusinessObjectFactory().New<ForwardingConsol>();
			new BuildConsolHelper().MakeConsolFromBookingOrStandaloneShipment(consol, quotedBooking.Booking.PK, quotedBooking.PK);

			// Assert
			var shipment = LoadShipmentInConsolsFactory(consol, quotedBooking.Booking.PK);
			CombineAssertions("Shipment", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, ((ICO2eProvider)shipment).GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, ((ICO2eProvider)shipment).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, ((ForwardingShipment)shipment).TotalCO2eForBinding);
			});

			CombineAssertions("Consol", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, consol.GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, consol.GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, consol.TotalCO2eForBinding);
			});

			CombineAssertions("Transport Leg", () =>
			{
				AssertEquals("CO2e status should be the same", CO2eStatusList.Codes.Current, consol.Transports[0].GetCO2eStatus());
				AssertEquals("CO2e value should be the same", 10m, consol.Transports[0].GetTotalCO2e());
				AssertEquals("CO2e value should be the same", 10m, consol.Transports[0].GetCO2ePerTonneInKg());
			});
		}

		public void TestMakeConsolFromBooking_DoesNotPopulateShipmentConsolCO2e_WithVia()
		{
			// Arrange
			var quotedBooking = CreateQuotedBookingWithCO2e(2m, CO2eStatusList.Codes.Current, via: "USLAX", withSailing: true);
			Factory.Save();

			// Act
			var consol = new BusinessObjectFactory().New<ForwardingConsol>();
			new BuildConsolHelper().MakeConsolFromBookingOrStandaloneShipment(consol, quotedBooking.Booking.PK, quotedBooking.PK);

			// Assert
			var shipment = LoadShipmentInConsolsFactory(consol, quotedBooking.Booking.PK);
			CombineAssertions("Shipment", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, ((ICO2eProvider)shipment).GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, ((ICO2eProvider)shipment).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, ((ForwardingShipment)shipment).TotalCO2eForBinding);
			});

			CombineAssertions("Consol", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, consol.GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, consol.GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, consol.TotalCO2eForBinding);
			});

			CombineAssertions("Transport Leg", () =>
			{
				AssertEquals("CO2e status should be the same", CO2eStatusList.Codes.Current, consol.Transports[0].GetCO2eStatus());
				AssertEquals("CO2e value should be the same", 10m, consol.Transports[0].GetCO2ePerTonneInKg());
			});
		}

		public void TestAddBookingsToConsol_DoesNotPopulateShipmentCO2e_NoSailing()
		{
			// Arrange
			var quotedBooking = CreateQuotedBookingWithCO2e(2m, CO2eStatusList.Codes.Current);
			Factory.Save();

			// Act
			var consol = CreateExistingConsol(5, CO2eStatusList.Codes.Current);
			new BuildConsolHelper().AddBookingsToConsol(consol, new[] { quotedBooking.Booking.PK }, quotedBookingPK: quotedBooking.PK);

			// Assert
			var shipment = LoadShipmentInConsolsFactory(consol, quotedBooking.Booking.PK);
			CombineAssertions("Shipment", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, ((ICO2eProvider)shipment).GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, ((ICO2eProvider)shipment).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, ((ForwardingShipment)shipment).TotalCO2eForBinding);
			});

			CombineAssertions("Consol", () =>
			{
				AssertEquals("CO2e status should be set to NCU", CO2eStatusList.Codes.NotCurrent, ((ICO2eProvider)consol).GetCO2eStatus());
				AssertEquals("CO2e value should be the same", 5m, ((ICO2eProvider)consol).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be the same", "5", ((ForwardingConsol)consol).TotalCO2eForBinding);
			});
		}

		public void TestAddBookingsToConsol_DoesNotPopulateShipmentCO2e_SailingLinked()
		{
			// Arrange
			var quotedBooking = CreateQuotedBookingWithCO2e(2m, CO2eStatusList.Codes.Current, withSailing: true);
			Factory.Save();

			// Act
			var consol = CreateExistingConsol(5, CO2eStatusList.Codes.Current);
			new BuildConsolHelper().AddBookingsToConsol(consol, new[] { quotedBooking.Booking.PK }, quotedBookingPK: quotedBooking.PK);

			// Assert
			var shipment = LoadShipmentInConsolsFactory(consol, quotedBooking.Booking.PK);
			CombineAssertions("Shipment", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, ((ICO2eProvider)shipment).GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, ((ICO2eProvider)shipment).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, ((ForwardingShipment)shipment).TotalCO2eForBinding);
			});

			CombineAssertions("Consol", () =>
			{
				AssertEquals("CO2e status should be NCU", CO2eStatusList.Codes.NotCurrent, ((ICO2eProvider)consol).GetCO2eStatus());
				AssertEquals("CO2e value should be the same", 5m, ((ICO2eProvider)consol).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be the same", "5", ((ForwardingConsol)consol).TotalCO2eForBinding);
			});
		}

		public void TestAddBookingsToConsol_DoesNotPopulateShipmentCO2e_WithVia()
		{
			// Arrange
			var quotedBooking = CreateQuotedBookingWithCO2e(2m, CO2eStatusList.Codes.Current, via: "USLAX", withSailing: true);
			Factory.Save();

			// Act
			var consol = CreateExistingConsol(5, CO2eStatusList.Codes.NotCurrent);
			new BuildConsolHelper().AddBookingsToConsol(consol, new[] { quotedBooking.Booking.PK }, quotedBookingPK: quotedBooking.PK);

			// Assert
			var shipment = LoadShipmentInConsolsFactory(consol, quotedBooking.Booking.PK);
			CombineAssertions("Shipment", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, ((ICO2eProvider)shipment).GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, ((ICO2eProvider)shipment).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, ((ForwardingShipment)shipment).TotalCO2eForBinding);
			});

			CombineAssertions("Consol", () =>
			{
				AssertEquals("CO2e status should be NCU", CO2eStatusList.Codes.NotCurrent, ((ICO2eProvider)consol).GetCO2eStatus());
				AssertEquals("CO2e value should be the same", 5m, ((ICO2eProvider)consol).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be the same", "5", ((ForwardingConsol)consol).TotalCO2eForBinding);
			});
		}

		public void TestAddBookingsToConsol_DoesNotChangeConsolCO2eStatusToNCU_IfConsolStatusIsNON()
		{
			// Arrange
			var quotedBooking = CreateQuotedBookingWithCO2e(2m, CO2eStatusList.Codes.NotCalculated, withSailing: true);
			Factory.Save();

			// Act
			var consol = GetExportConsol(typeof(ForwardingConsol));
			new BuildConsolHelper().AddBookingsToConsol(consol, new[] { quotedBooking.Booking.PK }, quotedBookingPK: quotedBooking.PK);

			// Assert
			var shipment = LoadShipmentInConsolsFactory(consol, quotedBooking.Booking.PK);
			CombineAssertions("Shipment", () =>
			{
				AssertEquals("CO2e status should be NON", CO2eStatusList.Codes.NotCalculated, ((ICO2eProvider)shipment).GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, ((ICO2eProvider)shipment).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, ((ForwardingShipment)shipment).TotalCO2eForBinding);
			});

			CombineAssertions("Consol", () =>
			{
				AssertEquals("CO2e status should be the NON if booking is NON", CO2eStatusList.Codes.NotCalculated, ((ICO2eProvider)consol).GetCO2eStatus());
				AssertEquals("CO2e value should be 0", 0m, ((ICO2eProvider)consol).GetCO2ePerTonneInKg());
				AssertEquals("Total CO2e should be empty", ZString.Empty, ((ForwardingConsol)consol).TotalCO2eForBinding);
			});
		}

		QuotedBooking CreateQuotedBookingWithCO2e(decimal value, string status, string via = "", bool withSailing = false)
		{
			var quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			if (!string.IsNullOrEmpty(via))
			{
				quotedBooking.Via = via;
			}
			if (withSailing)
			{
				ExportSailing.SetCO2ePerTonneInKg(10m);
				ExportSailing.SetTotalCO2e(10m);
				ExportSailing.SetCO2eStatus(CO2eStatusList.Codes.Current);
				((ISailingChooserParent)quotedBooking).SailingJX = ExportSailing.PK;
			}
			quotedBooking.SetCO2ePerTonneInKg(value);
			quotedBooking.SetTotalCO2e(value);
			quotedBooking.SetCO2eStatus(status);
			return quotedBooking;
		}

		CommonConsol CreateExistingConsol(decimal value, string status)
		{
			var consol = GetExportConsol(typeof(ForwardingConsol));
			((ICO2eProvider)consol).SetCO2ePerTonneInKg(value);
			((ICO2eProvider)consol).SetTotalCO2e(value);
			((ICO2eProvider)consol).SetCO2eStatus(status);
			return consol;
		}

		#endregion

		#region Implementation

		JobMawb CreateMawb(ZString airlineCode)
		{
			ZQuery airlineFilter = new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, airlineCode);
			RefAirline airline = Factory.LoadTop1<RefAirline>(airlineFilter);
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
			mawb.JM_MAWB = "55555" + airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			return mawb;
		}

		JobSailing CreateExportSailing(ZDateTime flightDate, ZString flightNo)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = flightNo;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = flightDate;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = flightDate.AddHours(18);
			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;
			return sailing;
		}

		OrgHeader CreateCarrier()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";

			return carrier;
		}

		OrgHeader CreateConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_IsShippingLine = true;
			consignee.OH_IsShippingProvider = true;
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignee.MainAddress.OA_Address1 = "Address 1";

			return consignee;
		}

		OrgHeader CreateConsignor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_IsShippingLine = true;
			consignor.OH_IsShippingProvider = true;
			consignor.OH_FullName = "Consignor";
			consignor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consignor.MainAddress.OA_Address1 = "Address 1";

			return consignor;
		}

		public ForwardingConsol ConsolidateConsolFromBooking(QuotedBooking booking)
		{
			var newFactory = new BusinessObjectFactory();
			var newConsol = newFactory.New<ForwardingConsol>();
			var helper = new BuildConsolHelper();
			helper.MakeConsolFromBookingOrStandaloneShipment(newConsol, booking.Booking.PK);

			return newConsol;
		}

		#endregion
	}
}
