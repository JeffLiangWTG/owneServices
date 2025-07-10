using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Testing.GUI
{
	internal class AgencyBookingIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestLinerAgencyBooking_WithMultipleContainers_PercentageCalculatorWithMultipleApplyTo_SameCharges()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("CAF");

			var bookingParty = Helper.NewOrgHeader(1);

			var tariff = Factory.New<CompanyTariff>();
			var rateEntry20GP = tariff.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.SCO, RateMode.SEA, "AUSYD", "USLAX", "FRT", 20m, "CN", container: "20GP");
			var rateLine11 = rateEntry20GP.AddUnitCharge("BAF", 21m, "CN");
			var rateLine12 = rateEntry20GP.AddRateLine("CAF", PercentageCalculator.Code);
			rateLine12.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			rateLine12.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["BAF"].PK;
			rateLine12.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 20m;

			var rateEntry40GP = tariff.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.SCO, RateMode.SEA, "AUSYD", "USLAX", "FRT", 40m, "CN", container: "40GP");
			var rateLine21 = rateEntry40GP.AddUnitCharge("BAF", 41m, "CN");
			var rateLine22 = rateEntry40GP.AddRateLine("CAF", PercentageCalculator.Code);
			rateLine22.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			rateLine22.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["BAF"].PK;
			rateLine22.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = 20m;

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = TransportModes.Sea;
			voyage.JV_VoyageFlight = "123456";
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			voyage.GenerateSailings();

			var agencyBooking = Factory.NewWithValidTestData<AgencyBooking>();
			agencyBooking.JS_UniqueConsignRef = "BOOKING00001";
			agencyBooking.JS_RL_NKOrigin = "AUSYD";
			agencyBooking.JS_RL_NKDestination = "USLAX";
			agencyBooking.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty.PK;
			agencyBooking.JS_OH_DeliveryAgent = Helper.NewOrgHeader().PK;
			agencyBooking.Sailings.Add(voyage.Sailings[0]);

			var container20GP = agencyBooking.ShippingContainers.AddNew();
			container20GP.JC_ContainerCount = 1;
			container20GP.JC_RC = GP20.PK;
			container20GP.JC_ContainerMode = ContainerModes.FCL;

			var container40GP = agencyBooking.ShippingContainers.AddNew();
			container40GP.JC_ContainerCount = 1;
			container40GP.JC_RC = GP40.PK;
			container40GP.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			AutorateAndAssert
			(
				expected: new[]
				{
					new AssertionCharge { JR_OSSellAmt = 20m, RevenueCalculationDescription = "FRT: 1 20GP Container(s) @ USD 20.00/Container" },
					new AssertionCharge { JR_OSSellAmt = 21m, RevenueCalculationDescription = "BAF: 1 20GP Container(s) @ USD 21.00/Container" },
					new AssertionCharge { JR_OSSellAmt = 8.2m, RevenueCalculationDescription = "CAF: 20.00% of (USD 41.00 (FRT + BAF FRT 20.00 + BAF 21.00))" },
					new AssertionCharge { JR_OSSellAmt = 40m, RevenueCalculationDescription = "FRT: 1 40GP Container(s) @ USD 40.00/Container" },
					new AssertionCharge { JR_OSSellAmt = 41m, RevenueCalculationDescription = "BAF: 1 40GP Container(s) @ USD 41.00/Container" },
					new AssertionCharge { JR_OSSellAmt = 16.2m, RevenueCalculationDescription = "CAF: 20.00% of (USD 81.00 (FRT + BAF FRT 40.00 + BAF 41.00))" },
				},
				agencyBooking,
				Consignor,
				autorateCosts: false
			);
		}

		#region NonContainerizedMode in Shipping AutoRates BreakBulk Containers

		public void TestNonContainerizedModeAutoRatesBreakBulkContainers()
		{
			var chargeCode = Helper.ChargeCodes.New("AGBFRT", "Agency Booking Freight", UnitCalculator.Code);
			Helper.ChargeCodes.NewChargeTypeOverride(chargeCode, JobInvoicingConsumerTypes.AgencyBookingCode, invoiceType: AgencyInvoiceTypesList.Codes.LocalCollect);

			ClientRate rate = Helper.NewClientRate(Consignor);
			RateEntry entry = rate.AddRateEntry("SNC", "LCL", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 10;
			rateLine1.TL_RX_NKCurrency = "AUD";

			var shipment = Factory.NewWithValidTestData<AgencyBooking>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "BBK";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualVolume = 15;
			shipment.JS_ActualWeight = 3000;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = "AGBFRT: Base Rate AUD 10.00"
				}
			};

			AutorateAndAssert(expected, shipment, Consignor);
		}

		#endregion

		public void TestAutoRatingAgencyBooking_ChargesAreNotFilteredDueToPaymentTerm()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OBILL");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DPCH");

			var bookingParty = Helper.NewOrgHeader();
			var principal = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(bookingParty);
			var rateEntryContainerized = rate.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "AUSYD", "NZAKL", ZString.Empty, "20GP");
			rateEntryContainerized.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntryContainerized.Currency);
			var rateLineContainerized = rateEntryContainerized.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			rateLineContainerized.GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var rateEntryOrigin = rate.AddRateEntry(RatingConstants.RateCategory.SOR, RateMode.FCL, "AUSYD", "NZAKL", ZString.Empty, "20GP");
			rateEntryOrigin.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntryOrigin.Currency);
			var rateLineOrigin = rateEntryOrigin.AddRateLine("OBILL", FlatCalculator.Code, QuantityUnit.CN);
			rateLineOrigin.GetCalculator<FlatCalculator>().BaseRate = 50m;

			var rateEntryDestination = rate.AddRateEntry(RatingConstants.RateCategory.SDE, RateMode.FCL, "AUSYD", "NZAKL", ZString.Empty, "20GP");
			rateEntryDestination.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntryDestination.Currency);
			var rateLineDestination = rateEntryDestination.AddRateLine("DPCH", UnitCalculator.Code, QuantityUnit.CN);
			rateLineDestination.GetCalculator<UnitCalculator>().PerUnit = 500m;

			Factory.Save();

			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "NZAKL";
			booking.JS_TransportMode = TransportModes.Sea;
			booking.JS_PackingMode = ContainerModes.FCL;
			booking.JS_INCO = "PPD";
			booking.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty.PK;
			booking.JS_OH_DeliveryAgent = principal.PK;

			var container = booking.ShippingContainers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			container.JC_ContainerCount = 2;

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = TransportModes.Sea;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			booking.Sailings.Add(sailing);

			Factory.Save();

			var expectedCharges = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 2000m
						},
					new AssertionCharge
						{
							ChargeCode = "OBILL",
							JR_OSSellAmt = 50m
						},
					new AssertionCharge
						{
							ChargeCode = "DPCH",
							JR_OSSellAmt = 1000m
						}
				};

			AutorateAndAssert(expectedCharges, booking, bookingParty);
		}

		#region SeveralCostsApplyForOneCostBasedCalculator

		public void TestSeveralCostsApplyForOneCostBasedCalculator()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var departureCTO = Helper.CreateCreditor("DEPCTO");
			var principle = Helper.CreateCreditor("PRINCI");
			var arrivalCTO = Helper.CreateCreditor("ARVCTO");
			var client = Helper.NewOrgHeader(1);
			client.OH_IsDebtor = false;

			var costing1 = Helper.NewCosting(arrivalCTO);
			var costEntry1 = costing1.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "CNSHA", "AUFRE", "STD", "40GP");
			costEntry1.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costEntry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 2000;

			var costing2 = Helper.NewCosting(principle);
			var costEntry2 = costing2.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "CNSHA", "AUFRE", "STD", "40GP");
			costEntry2.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costEntry2.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 2100;

			var costing3 = Helper.NewCosting(departureCTO);
			var costEntry3 = costing3.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "CNSHA", "AUFRE", "STD", "40GP");
			costEntry3.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costEntry3.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 2200m;

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "CNSHA", "AUFRE", "STD", "40GP");
			tariffEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			tariffEntry.RateLines[0].TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			tariffEntry.RateLines[0].GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 15m;
			tariffEntry.RateLines[0].GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 15m;

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = TransportModes.Sea;
			voyage.JV_VoyageFlight = "123456";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "CNSHA";
			origin.JA_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUFRE";
			destination.JB_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;

			voyage.GenerateSailings();

			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_UniqueConsignRef = "BOOKING00001";
			booking.JS_RL_NKOrigin = "CNSHA";
			booking.JS_RL_NKDestination = "AUFRE";
			booking.BookingPartyDocumentaryAddress.OrganisationPK = client.PK;
			booking.JS_OH_DeliveryAgent = Helper.NewOrgHeader().PK;
			booking.Sailings.Add(voyage.Sailings[0]);

			var container1 = booking.ShippingContainers.AddNew();
			container1.JC_ContainerCount = 2;
			container1.JC_RC = GP40.PK;
			container1.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 4600,
							JR_OSCostAmt = 4000m,
							RevenueCalculationDescription = @"FRT: 2 40GP Container(s) @ AUD 2300.00/Container",
							CostCalculationDescription = "FRT: 2 40GP Container(s) @ AUD 2000.00/Container"
						},
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt = 5060,
							JR_OSCostAmt = 4400m,
							RevenueCalculationDescription = "FRT: 2 40GP Container(s) @ AUD 2530.00/Container",
							CostCalculationDescription = "FRT: 2 40GP Container(s) @ AUD 2200.00/Container"
						},
				};

			AutorateAndAssert(expected, booking, client);
		}

		#endregion

		#region SvcProviderAndPrincipalInShipping

		public void TestSvcProviderAndPrincipalInShipping()
		{
			var chargeCode = Helper.ChargeCodes.New("AGBFRT", "Agency Booking Freight", UnitCalculator.Code);
			Helper.ChargeCodes.NewChargeTypeOverride(chargeCode, JobInvoicingConsumerTypes.AgencyBookingCode);

			var cto1 = Helper.NewOrgHeader();
			var cto2 = Helper.NewOrgHeader();
			var principal = Helper.NewOrgHeader();
			var bookingParty = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(bookingParty);
			var entry1 = rate.AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "", "20GP");
			var entry2 = rate.AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "", "20GP");
			var entry3 = rate.AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "", "20GP");

			AddParityExchangeRate(entry1.Currency);

			entry1.TI_OH_TransportProvider = principal.PK;
			entry2.TI_OH_TransportProvider = principal.PK;
			entry3.TI_OH_TransportProvider = principal.PK;

			entry2.TI_OH_Supplier = cto2.PK;
			entry3.TI_OH_Supplier = cto1.PK;

			entry1.RateLines.RemoveAndDeleteAll();
			entry2.RateLines.RemoveAndDeleteAll();
			entry3.RateLines.RemoveAndDeleteAll();

			var line1 = entry1.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			var line2 = entry2.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			var line3 = entry3.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);

			line1.GetCalculator<UnitCalculator>().PerUnit = 1000;
			line2.GetCalculator<UnitCalculator>().PerUnit = 1100;
			line3.GetCalculator<UnitCalculator>().PerUnit = 1200;

			Factory.Save();

			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USLAX";
			booking.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty.PK;
			booking.JS_OH_DeliveryAgent = principal.PK;

			var container = booking.ShippingContainers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			container.JC_ContainerCount = 2;

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = TransportModes.Sea;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_OA_ArrivalCTOAddress = cto1.MainAddress.PK;

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			booking.Sailings.Add(sailing);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 2400m,
					RevenueCalculationDescription = "AGBFRT: 2 20GP Container(s) @ USD 1200.00/Container"
				}
			};

			AutorateAndAssert(expected, booking, bookingParty);

			entry3.TI_OH_TransportProvider = ZGuid.Empty;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 2000m,
					RevenueCalculationDescription = "AGBFRT: 2 20GP Container(s) @ USD 1000.00/Container"
				}
			};

			AutorateAndAssert(expected, booking, bookingParty);
		}

		#endregion

		#region PaymentTerms

		public void TestPaymentTerms_AgencyInvoiceTypes_Export_Collect()
		{
			AssertPaymentTermsSetAgencyInvoiceTypes(true, true);
		}

		public void TestPaymentTerms_AgencyInvoiceTypes_Export_Prepaid()
		{
			AssertPaymentTermsSetAgencyInvoiceTypes(false, true);
		}

		public void TestPaymentTerms_AgencyInvoiceTypes_Import_Collect()
		{
			AssertPaymentTermsSetAgencyInvoiceTypes(true, false);
		}

		public void TestPaymentTerms_AgencyInvoiceTypes_Import_Prepaid()
		{
			AssertPaymentTermsSetAgencyInvoiceTypes(false, false);
		}

		void AssertPaymentTermsSetAgencyInvoiceTypes(bool isCollect, bool isExport, bool isLocalCurrencyExpected = true)
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OSEC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DSEC");
			var origin = isExport ? "AUBTB" : "CNSHA";
			var destination = isExport ? "CNSHA" : "AUBTB";

			var bookingParty = Helper.NewOrgHeader(1);
			bookingParty.OH_IsDebtor = true;

			var clientRate = Helper.NewClientRate(bookingParty);
			var frtEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SCO, RateMode.SEA, origin, destination, "FRT", 10m);
			var orgEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SOR, RateMode.FCL, origin, destination, "OSEC", 15m);
			var dstEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.SDE, RateMode.FCL, origin, destination, "DSEC", 20m);
			AddParityExchangeRate(frtEntry.Currency);
			AddParityExchangeRate(orgEntry.Currency);
			AddParityExchangeRate(dstEntry.Currency);

			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_TransportMode = TransportModes.Sea;
			booking.JS_PackingMode = ContainerModes.FCL;
			booking.JS_RL_NKOrigin = origin;
			booking.JS_RL_NKDestination = destination;
			booking.JS_GoodsDescription = "a description";
			booking.JS_INCO = isCollect ? DomesticPaymentTerms.Collect : DomesticPaymentTerms.Prepaid;
			booking.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty.PK;

			var container = booking.BookedContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Save();

			string frtInvoiceTerm, orgInvoiceType, dstInvoiceType = "";

			if (isLocalCurrencyExpected)
			{
				frtInvoiceTerm = isCollect ? AgencyInvoiceTypesList.Codes.LocalCollect : AgencyInvoiceTypesList.Codes.LocalPrePaid;
				orgInvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid;
				dstInvoiceType = AgencyInvoiceTypesList.Codes.LocalCollect;
			}
			else
			{
				bookingParty.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();

				var invoiceGroup = bookingParty.CompanyData.InvoiceRollupOrGroups.AddNew();
				invoiceGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
				invoiceGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
				invoiceGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
				invoiceGroup.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.ForeignCurrencyInvoiceAndFinal;

				frtEntry.RateLines[0].TL_RX_NKCurrency = CurrencyCodes.China;
				orgEntry.RateLines[0].TL_RX_NKCurrency = CurrencyCodes.China;
				dstEntry.RateLines[0].TL_RX_NKCurrency = CurrencyCodes.China;

				Factory.Save();

				frtInvoiceTerm = isCollect ? AgencyInvoiceTypesList.Codes.ForeignCollect : AgencyInvoiceTypesList.Codes.ForeignPrePaid;
				orgInvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid;
				dstInvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			}

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 10m, JR_InvoiceType = frtInvoiceTerm },
				new AssertionCharge { ChargeCode = "OSEC", JR_OSSellAmt = 15m, JR_InvoiceType = orgInvoiceType },
				new AssertionCharge { ChargeCode = "DSEC", JR_OSSellAmt = 20m, JR_InvoiceType = dstInvoiceType },
			};

			AutorateAndAssert($"{isExport}-{isCollect}", expectedCharges, booking, bookingParty);
		}

		#endregion
	}
}
