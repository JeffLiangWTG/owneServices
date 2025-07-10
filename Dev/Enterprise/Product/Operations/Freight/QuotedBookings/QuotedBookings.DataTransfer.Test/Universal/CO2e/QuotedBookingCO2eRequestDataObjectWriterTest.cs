using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	public sealed class QuotedBookingCO2eRequestDataObjectWriterTest : BaseFreightTest
	{
		#region OneOffQuote

		public void TestPopulateBusinessObject_OOQ()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var quotedBookingBO = CO2eTestHelper.CreateQuotedBooking(Factory, RateMode.LSE, ZString.Empty, consignee, consignee, consignor, null, HomePort, OverseasPort, 10m, 1m, QuotedBookingState.QuoteOnly);

			quotedBookingBO.TransportMode = TransportModes.Sea;
			quotedBookingBO.Mode = RateMode.LCL;
			quotedBookingBO.Weight = 10m;
			quotedBookingBO.WeightUnit = Weight.Kilograms;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER 1";
			var cusCode = carrier.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = "US";
			cusCode.OK_CustomsRegNo = "1111";
			quotedBookingBO.OH_Carrier = carrier.PK;

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBookingBO)));
			var ooqData = writer.GetDataObject(quotedBookingBO);

			AssertNotNull("ooqData", ooqData);
			AssertEquals("ooqData.TransportLegCollection.Count", 0, ooqData.TransportLegCollection.Count);
			AssertEquals("ooqData.PortOfLoading", HomePort, ooqData.PortOfLoading.Code);
			AssertEquals("ooqData.PortOfDischarge", OverseasPort, ooqData.PortOfDischarge.Code);
			AssertEquals("ooqData.TotalWeight", 10m, ooqData.TotalWeight);
			AssertEquals("ooqData.TotalWeightUnit", Weight.Kilograms, ooqData.TotalWeightUnit?.Code);
			AssertEquals("ooqData.TransportMode", TransportModes.Sea, ooqData.TransportMode.Code);
			AssertEquals("ooqData.ContainerMode", RateMode.LCL, ooqData.ContainerMode.Code);
			AssertEquals("ooqData.OrganizationAddressCollection[0].AddressType", "ShippingLineAddress", ooqData.OrganizationAddressCollection[0].AddressType);
			AssertEquals("ooqData.OrganizationAddressCollection[0].RegistrationNumber", "1111", ooqData.OrganizationAddressCollection[0].RegistrationNumberCollection[0].Value);

			quotedBookingBO.Quote.CurrentOneOffQuote.TT_TransportMode = TransportModes.Sea;
			quotedBookingBO.Quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = AlternateHomePort;
			quotedBookingBO.Quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = OverseasPort2;
			quotedBookingBO.Quote.CurrentOneOffQuote.TT_ActualWeight = 2;
			quotedBookingBO.Quote.CurrentOneOffQuote.TT_UnitOfWeight = Weight.Tonnes;
			ooqData = writer.GetDataObject(quotedBookingBO);

			AssertNotNull("ooqData", ooqData);
			AssertEquals("ooqData.TransportLegCollection.Count", 0, ooqData.TransportLegCollection.Count);
			AssertEquals("ooqData.PortOfLoading", OverseasPort2, ooqData.PortOfLoading.Code);
			AssertEquals("ooqData.PortOfDischarge", AlternateHomePort, ooqData.PortOfDischarge.Code);
			AssertEquals("ooqData.TotalWeight", 2m, ooqData.TotalWeight);
			AssertEquals("ooqData.TotalWeightUnit", Weight.Tonnes, ooqData.TotalWeightUnit?.Code);
			AssertEquals("ooqData.TransportMode", TransportModes.Sea, ooqData.TransportMode.Code);
		}

		public void TestPopulateOneOffQuoteTransportMode()
		{
			SetupAndAssertTransportMode(AlternateHomePort, OverseasPort3, "LSE", TransportModes.Air);
			SetupAndAssertTransportMode(AlternateHomePort, OverseasPort3, TransportModes.Sea, TransportModes.Sea);
			SetupAndAssertTransportMode(AlternateHomePort, OverseasPort3, TransportModes.Road, TransportModes.Road);
			SetupAndAssertTransportMode(AlternateHomePort, OverseasPort3, TransportModes.Rail, TransportModes.Rail);
			SetupAndAssertTransportMode(AlternateHomePort, OverseasPort3, TransportModes.Courier, TransportModes.Air);
			SetupAndAssertTransportMode(AlternateHomePort, AlternateHomePort2, TransportModes.Courier, TransportModes.Road);
		}

		void SetupAndAssertTransportMode(string origin, string destination, string transportMode, string expectedTransportMode)
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var quotedBookingBO = CO2eTestHelper.CreateQuotedBooking(Factory, transportMode, ZString.Empty, consignee, consignee, consignor, null, HomePort, OverseasPort, 10m, 1m, QuotedBookingState.QuoteOnly);

			quotedBookingBO.Origin = origin;
			quotedBookingBO.Destination = destination;

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBookingBO)));
			var ooqDataWithLegs = writer.GetDataObject(quotedBookingBO);
			AssertNotNull("ooqDataWithLegs", ooqDataWithLegs);
			AssertEquals("One Off Quote TransportMode", expectedTransportMode, ooqDataWithLegs.TransportMode.Code);
		}

		public void TestPopulateVirtualLegs_OOQ()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var quotedBookingBO = CO2eTestHelper.CreateQuotedBooking(Factory, RateMode.LSE, ZString.Empty, consignee, consignee, consignor, null, HomePort, OverseasPort, 10m, 1m, QuotedBookingState.QuoteOnly);

			quotedBookingBO.Via = OverseasPort2;

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBookingBO)));
			var transportModeConverter = new TransportModeConverter();
			var ooqDataWithLegs = writer.GetDataObject(quotedBookingBO);
			AssertNotNull("ooqDataWithLegs", ooqDataWithLegs);
			AssertEquals("ooqDataWithLegs.TransportLegCollection.Count", 2, ooqDataWithLegs.TransportLegCollection.Count);

			AssertEquals("ooqDataWithLegs.TransportLegCollection[0].TransportMode", transportModeConverter.ToEnumValue(TransportModes.Air), ooqDataWithLegs.TransportLegCollection[0].TransportMode);
			AssertEquals("ooqDataWithLegs.TransportLegCollection[0].PortOfLoading.Code", HomePort.Right(3), ooqDataWithLegs.TransportLegCollection[0].PortOfLoading.Code);
			AssertEquals("ooqDataWithLegs.TransportLegCollection[0].PortOfDischarge.Code", OverseasPort2.Right(3), ooqDataWithLegs.TransportLegCollection[0].PortOfDischarge.Code);

			AssertEquals("ooqDataWithLegs.TransportLegCollection[1].TransportMode", transportModeConverter.ToEnumValue(TransportModes.Air), ooqDataWithLegs.TransportLegCollection[1].TransportMode);
			AssertEquals("ooqDataWithLegs.TransportLegCollection[1].PortOfLoading.Code", OverseasPort2.Right(3), ooqDataWithLegs.TransportLegCollection[1].PortOfLoading.Code);
			AssertEquals("ooqDataWithLegs.TransportLegCollection[1].PortOfDischarge.Code", OverseasPort.Right(3), ooqDataWithLegs.TransportLegCollection[1].PortOfDischarge.Code);
		}

		public void TestPopulateTEU_OOQ()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var quotedBookingBO = CO2eTestHelper.CreateQuotedBooking(Factory, RateMode.SEA, ZString.Empty, consignee, consignee, consignor, null, "AUSYD", "CNSHA", 2m, 1m, QuotedBookingState.QuoteOnly);

			quotedBookingBO.TransportMode = TransportModes.Sea;
			quotedBookingBO.ContainerMode = RateMode.FCL;
			quotedBookingBO.WeightUnit = Weight.Tonnes;

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBookingBO)));
			Shipment ooqData;

			#region Empty Containers

			AssertEmptyTEU();

			#endregion

			#region With Containers

			quotedBookingBO.Quote.CurrentOneOffQuote.Containers.RemoveAll();
			var container1 = quotedBookingBO.Quote.CurrentOneOffQuote.Containers.AddNew();
			container1.TC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("20GP", "22G0", 1m, 2280m);
			container1.TC_RC = refContainer1.PK;

			var container2 = quotedBookingBO.Quote.CurrentOneOffQuote.Containers.AddNew();
			container2.TC_ContainerCount = 1;
			var refContainer2 = NewRefContainer("40REHC", "45R0", 2.3m, 4420m);
			container2.TC_RC = refContainer2.PK;

			AssertTEU();

			#endregion

			#region Mode

			quotedBookingBO.Mode = RateMode.FRO;
			AssertTEU();
			quotedBookingBO.ContainerMode = RateMode.LCL;
			AssertEmptyTEU();
			quotedBookingBO.TransportMode = TransportModes.Air;
			quotedBookingBO.ContainerMode = RateMode.LSE;
			AssertEmptyTEU();

			#endregion

			void AssertEmptyTEU()
			{
				ooqData = writer.GetDataObject(quotedBookingBO);

				// Assert
				AssertNotNull("ooqData", ooqData);
				AssertNull("ooqData.TEU", ooqData.TEU);
			}

			void AssertTEU()
			{
				ooqData = writer.GetDataObject(quotedBookingBO);

				// Assert
				AssertNotNull("ooqData", ooqData);
				AssertNotNull("ooqData.TEU", ooqData.TEU);
				AssertEquals("ooqData.TEU.NumberOfTEU", 1 * 2 + 2.3m * 1m, ooqData.TEU.NumberOfTEU);
				AssertEquals("ooqData.TEU.TonnesPerTEU", Utilities.Round(2 / (1m * 2m + 2.3m * 1m), 6), ooqData.TEU.TonnesPerTEU);
				AssertEquals("ooqData.TEU.ContainerEmptyWeightPerTEU", 2088.372093m, ooqData.TEU.ContainerEmptyWeightPerTEU);
			}
		}

		public void TestPopulateRequiresTemperatureControl_OOQ()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var quotedBookingBO = CO2eTestHelper.CreateQuotedBooking(Factory, RateMode.LSE, ZString.Empty, consignee, consignee, consignor, null, HomePort, OverseasPort, 10m, 1m, QuotedBookingState.QuoteOnly);

			quotedBookingBO.TransportMode = TransportModes.Air;
			quotedBookingBO.Weight = 10m;
			quotedBookingBO.WeightUnit = Weight.Kilograms;

			var container1 = quotedBookingBO.Quote.CurrentOneOffQuote.Containers.AddNew();
			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_Code = "20RE";
			refContainer1.RC_ContainerType = ContainerTypes.DryStorage;
			container1.TC_RC = refContainer1.PK;

			var container2 = quotedBookingBO.Quote.CurrentOneOffQuote.Containers.AddNew();
			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_Code = "40FR";
			refContainer2.RC_ContainerType = ContainerTypes.DryStorage;
			container2.TC_RC = refContainer2.PK;

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBookingBO)));
			var ooqData = writer.GetDataObject(quotedBookingBO);
			AssertEquals("ooqData.RequiresTemperatureControl is null because there is no refrigerated container", null, ooqData.RequiresTemperatureControl);

			refContainer1.RC_ContainerType = ContainerTypes.Refrigerated;
			ooqData = writer.GetDataObject(quotedBookingBO);
			AssertEquals("ooqData.RequiresTemperatureControl", true, ooqData.RequiresTemperatureControl);
		}

		public void TestRemoveUnusedFields_OOQ()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var quotedBookingBO = CO2eTestHelper.CreateQuotedBooking(Factory, RateMode.LSE, ZString.Empty, consignee, consignee, consignor, null, HomePort, OverseasPort, 10m, 1m, QuotedBookingState.QuoteOnly);

			quotedBookingBO.TransportMode = TransportModes.Air;
			quotedBookingBO.Weight = 10m;
			quotedBookingBO.WeightUnit = Weight.Kilograms;

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBookingBO)));
			var ooqData = writer.GetDataObject(quotedBookingBO);

			AssertEquals("CustomizedFieldCollection should be null for CO2e Calculation", null, ooqData.CustomizedFieldCollection);
			AssertEquals("MilestoneCollection should be null for CO2e Calculation", null, ooqData.MilestoneCollection);
			AssertEquals("ExceptionCollection should be null for CO2e Calculation", null, ooqData.ExceptionCollection);
			AssertEquals("JobCosting should be null for CO2e Calculation", null, ooqData.JobCosting);
			AssertEquals("ConsolCosts should be null for CO2e Calculation", null, ooqData.ConsolCosts);
		}

		#endregion

		#region Booking

		public void TestPopulateBusinessObject_Booking()
		{
			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER 1";
			var cusCode = carrier.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = "US";
			cusCode.OK_CustomsRegNo = "1111";
			quotedBooking.OH_Carrier = carrier.PK;

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBooking)));
			var bookingData = writer.GetDataObject(quotedBooking);

			AssertNotNull("bookingData", bookingData);
			AssertEquals("bookingData.TransportLegCollection.Count", 0, bookingData.TransportLegCollection.Count);
			AssertEquals("bookingData.PortOfLoading", "AUSYD", bookingData.PortOfLoading.Code);
			AssertEquals("bookingData.PortOfDischarge", "VNVNH", bookingData.PortOfDischarge.Code);
			AssertEquals("bookingData.TotalWeight", 1m, bookingData.TotalWeight);
			AssertEquals("bookingData.TotalWeightUnit", "T", bookingData.TotalWeightUnit.Code);
			AssertEquals("bookingData.TransportMode", "SEA", bookingData.TransportMode.Code);
			AssertEquals("bookingData.ContainerMode", "FCL", bookingData.ContainerMode.Code);
			AssertEquals("bookingData.OrganizationAddressCollection[0].AddressType", "ShippingLineAddress", bookingData.OrganizationAddressCollection[0].AddressType);
			AssertEquals("bookingData.OrganizationAddressCollection[0].RegistrationNumber", "1111", bookingData.OrganizationAddressCollection[0].RegistrationNumberCollection[0].Value);
		}

		public void TestPopulateVirtualLegs_Booking()
		{
			void AssertTransportLegs(string message, TransportLeg[] transportLegDOs, (string Mode, string Load, string Discharge)[] expected)
			{
				var transportModeConverter = new TransportModeConverter();
				CombineAssertions(message, () =>
				{
					AssertEquals(expected.Length, transportLegDOs.Length);
					for (var i = 0; i < transportLegDOs.Length; i++)
					{
						AssertEquals($"bookingData.TransportLegCollection[{i}].TransportMode", transportModeConverter.ToEnumValue(expected[i].Mode), transportLegDOs[i].TransportMode);
						AssertEquals($"bookingData.TransportLegCollection[{i}].PortOfLoading.Code", expected[i].Load, transportLegDOs[i].PortOfLoading.Code);
						AssertEquals($"bookingData.TransportLegCollection[{i}].PortOfDischarge.Code", expected[i].Discharge, transportLegDOs[i].PortOfDischarge.Code);
					}
				});
			}

			#region Booking with Load, Discharge and Sailing

			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory, loadPort: "AUMEL", dischargePort: "VNSGN", via: "MYKUL", sailingLoad: "AUBNE", sailingDischarge: "SGSIN");
			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBooking)));
			var bookingData = writer.GetDataObject(quotedBooking);

			AssertNotNull("bookingData", bookingData);
			AssertTransportLegs("Booking with Load, Discharge and Sailing",
				bookingData.TransportLegCollection.ToArray(),
				new[]
				{
					("SEA", "AUSYD", "AUMEL"),
					("SEA", "AUMEL", "AUBNE"),
					("SEA", "AUBNE", "SGSIN"),
					("SEA", "SGSIN", "VNSGN"),
					("SEA", "VNSGN", "VNVNH")
				});

			#endregion

			#region Booking with Load, Discharge and Via

			quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory, loadPort: "AUMEL", dischargePort: "VNSGN", via: "SGSIN");
			writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBooking)));
			bookingData = writer.GetDataObject(quotedBooking);

			AssertNotNull("bookingData", bookingData);
			AssertTransportLegs("Booking with Load, Discharge and Via",
				bookingData.TransportLegCollection.ToArray(),
				new[]
				{
					("SEA", "AUSYD", "AUMEL"),
					("SEA", "AUMEL", "SGSIN"),
					("SEA", "SGSIN", "VNSGN"),
					("SEA", "VNSGN", "VNVNH")
				});

			#endregion

			#region Booking with Load, Discharge

			quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory, loadPort: "AUMEL", dischargePort: "VNSGN");
			writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBooking)));
			bookingData = writer.GetDataObject(quotedBooking);

			AssertNotNull("bookingData", bookingData);
			AssertTransportLegs("Booking with Load, Discharge",
				bookingData.TransportLegCollection.ToArray(),
				new[]
				{
					("SEA", "AUSYD", "AUMEL"),
					("SEA", "AUMEL", "VNSGN"),
					("SEA", "VNSGN", "VNVNH")
				});

			#endregion
		}

		public void TestPopulateIATAForAir()
		{
			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory, via: "SGSIN");
			quotedBooking.Mode = "LSE";
			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBooking)));
			var bookingData = writer.GetDataObject(quotedBooking);

			AssertEquals("bookingData.PortOfLoading", "SYD", bookingData.PortOfLoading.Code);
			AssertEquals("bookingData.PortOfDischarge", "VII", bookingData.PortOfDischarge.Code);
			AssertEquals("bookingData.TransportLegCollection.Count", 2, bookingData.TransportLegCollection.Count);
			AssertEquals("bookingData.TransportLegCollection[0].PortOfLoading.Code", "SYD", bookingData.TransportLegCollection[0].PortOfLoading.Code);
			AssertEquals("bookingData.TransportLegCollection[0].PortOfDischarge.Code", "SIN", bookingData.TransportLegCollection[0].PortOfDischarge.Code);
			AssertEquals("bookingData.TransportLegCollection[1].PortOfLoading.Code", "SIN", bookingData.TransportLegCollection[1].PortOfLoading.Code);
			AssertEquals("bookingData.TransportLegCollection[1].PortOfDischarge.Code", "VII", bookingData.TransportLegCollection[1].PortOfDischarge.Code);

			var query = new ZQuery(RefUNLOCOSchema.RL_IATA, SQLComparisonOperator.Equal, ZString.Empty);
			query.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, "AU");
			var unlocoWithoutIATA = Factory.LoadTop1<RefUNLOCO>(query);
			quotedBooking.Origin = unlocoWithoutIATA.RL_Code;

			writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBooking)));
			bookingData = writer.GetDataObject(quotedBooking);
			AssertEquals("bookingData.PortOfLoading fallback to UNLOCO when IATA is not configured", unlocoWithoutIATA.RL_Code, bookingData.PortOfLoading.Code);
		}

		public void TestPopulateJobLegDates_Booking()
		{
			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory, origin: "AUSYD", destination: "CNSHA", sailingLoad: "AUSYD", sailingDischarge: "CNSHA");
			quotedBooking.ETD = new ZDateTime(2022, 1, 1);
			quotedBooking.ETA = new ZDateTime(2022, 1, 10);

			var sailing = quotedBooking.Booking.Sailing;
			sailing.Origin.JA_E_DEP = new ZDateTime(2022, 1, 1);
			sailing.Destination.JB_E_ARV = new ZDateTime(2022, 1, 10);

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBooking)));
			var bookingData = writer.GetDataObject(quotedBooking);

			AssertNotNull("bookingData", bookingData);
			AssertContents(bookingData.DateCollection[0], DateType.Arrival, new ZDateTime(2022, 1, 10), ZBool.True);
			AssertContents(bookingData.DateCollection[1], DateType.Departure, new ZDateTime(2022, 1, 1), ZBool.True);
			AssertEquals("bookingData.TransportLegCollection.Count", 1, bookingData.TransportLegCollection.Count);
			AssertEquals("bookingData.TransportLegCollection[0].EstimatedArrival", new ZDateTime(2022, 1, 10), bookingData.TransportLegCollection[0].EstimatedArrival);
			AssertEquals("bookingData.TransportLegCollection[0].EstimatedDeparture", new ZDateTime(2022, 1, 1), bookingData.TransportLegCollection[0].EstimatedDeparture);
		}

		public void TestPopulateVoyageFlightNoFromCarrierIfEmpty()
		{
			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory, origin: "AUSYD", destination: "CNSHA", sailingLoad: "AUSYD", sailingDischarge: "CNSHA");

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER 1";

			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "AZ";
			var miscServ = carrier.MiscServ;
			miscServ.OM_RM_Airline = airline.PK;
			quotedBooking.Booking.Sailing.Voyage.JV_OH_Line = carrier.PK;
			quotedBooking.Booking.Sailing.Voyage.JV_AirSeaRoad = TransportModes.Air;

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBooking)));
			var bookingData = writer.GetDataObject(quotedBooking);

			AssertNotNull("bookingData", bookingData);
			AssertEquals("bookingData.TransportLegCollection.Count", 1, bookingData.TransportLegCollection.Count);
			AssertEquals("bookingData.TransportLegCollection[0].VoyageFlightNo", "AZ", bookingData.TransportLegCollection[0].VoyageFlightNo);
		}

		void AssertContents(Date dateDataObject, DateType type, ZDateTime dateTime, ZBool isEstimate)
		{
			AssertNotNull("Precondition: dateDataObject", dateDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("dateDataObject.Type", type, dateDataObject.Type);
				AssertEquals("dateDataObject.Value", dateTime, dateDataObject.Value);
				AssertEquals("dateDataObject.IsEstimate", isEstimate, dateDataObject.IsEstimate);
			});
		}

		public void TestPopulateTEU_Booking()
		{
			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory, origin: "AUSYD", destination: "CNSHA", sailingLoad: "AUSYD", sailingDischarge: "CNSHA");

			quotedBooking.Weight = 2m;
			quotedBooking.WeightUnit = Weight.Tonnes;

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBooking)));
			Shipment bookingData;

			#region Empty Containers

			AssertEmptyTEU();

			#endregion

			#region With Containers

			quotedBooking.QuotedBookingContainers.RemoveAll();
			var container1 = quotedBooking.QuotedBookingContainers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer("20GP", "22G0", 1m, 2280m);
			container1.JC_RC = refContainer1.PK;
			container1.JC_DunnageWeight = 100m;

			var container2 = quotedBooking.QuotedBookingContainers.AddNew();
			container2.JC_ContainerCount = 1;
			var refContainer2 = NewRefContainer("40REHC", "45R0", 2.3m, 4420m);
			container2.JC_RC = refContainer2.PK;
			container2.JC_DunnageWeight = 100m;

			AssertTEU();
			AssertEquals("bookingData.TEU.ContainerEmptyWeightPerTEU", 2134.883721m, bookingData.TEU.ContainerEmptyWeightPerTEU);
			AssertEquals("bookingData.TEU.ContainerEmptyWeightPerTEUUnit.Code", Core.Constants.Weight.Kilograms, bookingData.TEU.ContainerEmptyWeightPerTEUUnit.Code);

			#endregion

			#region Mode

			quotedBooking.Mode = RateMode.FRO;
			AssertTEU();
			quotedBooking.Mode = RateMode.LCL;
			AssertEmptyTEU();
			quotedBooking.Mode = RateMode.LSE;
			AssertEmptyTEU();
			quotedBooking.Mode = RateMode.ULD;
			AssertEmptyTEU();

			#endregion

			void AssertEmptyTEU()
			{
				bookingData = writer.GetDataObject(quotedBooking);

				// Assert
				AssertNotNull("bookingData", bookingData);
				AssertNull("bookingData.TEU", bookingData.TEU);
			}

			void AssertTEU()
			{
				bookingData = writer.GetDataObject(quotedBooking);

				// Assert
				AssertNotNull("bookingData", bookingData);
				AssertNotNull("bookingData.TEU", bookingData.TEU);
				AssertEquals("bookingData.TEU.NumberOfTEU", 1 * 2 + 2.3m * 1m, bookingData.TEU.NumberOfTEU);
				AssertEquals("bookingData.TEU.TonnesPerTEU", 0.465116m, bookingData.TEU.TonnesPerTEU);
			}
		}

		RefContainer NewRefContainer(ZString code, ZString isoType, decimal teu, decimal tareWeight)
		{
			var refContainer = RefContainer.New(Factory);
			refContainer.RC_Code = code;
			refContainer.RC_ISOType = isoType;
			refContainer.RC_TEU = teu;
			refContainer.RC_TareWeight = tareWeight;

			return refContainer;
		}

		public void TestPopulateRequiresTemperatureControl()
		{
			var quotedBooking = CO2eTestHelper.CreateQuotedBooking(Factory, loadPort: "AUMEL", dischargePort: "VNSGN", via: "MYKUL", sailingLoad: "AUBNE", sailingDischarge: "SGSIN");
			quotedBooking.Mode = RateMode.LSE;
			quotedBooking.Weight = 10m;
			quotedBooking.WeightUnit = Weight.Kilograms;

			var packline1 = quotedBooking.Booking.OuterPackLines.AddNew();
			packline1.JL_RequiredTemperatureMinimum = 5m;
			packline1.JL_RequiredTemperatureMaximum = 6m;

			var writer = new QuotedBookingCO2eRequestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, quotedBooking)));
			var bookingData = writer.GetDataObject(quotedBooking);
			AssertEquals("bookingData.RequiresTemperatureControl", true, bookingData.RequiresTemperatureControl);

			packline1.JL_RequiredTemperatureMinimum = 0m;
			packline1.JL_RequiredTemperatureMaximum = 0m;
			bookingData = writer.GetDataObject(quotedBooking);
			AssertEquals("bookingData.RequiresTemperatureControl is null because Min Temp/Max Temp is not entered ", null, bookingData.RequiresTemperatureControl);

			packline1.JL_RequiredTemperatureMaximum = 3m;
			bookingData = writer.GetDataObject(quotedBooking);
			AssertEquals("bookingData.RequiresTemperatureControl", true, bookingData.RequiresTemperatureControl);
			packline1.JL_RequiredTemperatureMaximum = 0m;
			bookingData = writer.GetDataObject(quotedBooking);
			AssertEquals("bookingData.RequiresTemperatureControl is null because Min Temp/Max Temp is not entered ", null, bookingData.RequiresTemperatureControl);

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_Code = "20RE";
			refContainer1.RC_ContainerType = ContainerTypes.Refrigerated;
			container.JC_RC = refContainer1.PK;
			bookingData = writer.GetDataObject(quotedBooking);
			AssertEquals("bookingData.RequiresTemperatureControl", true, bookingData.RequiresTemperatureControl);

			refContainer1.RC_ContainerType = ContainerTypes.OpenTop;
			bookingData = writer.GetDataObject(quotedBooking);
			AssertEquals("bookingData.RequiresTemperatureControl is null because there is no refrigerated container", null, bookingData.RequiresTemperatureControl);
		}

		#endregion
	}
}
