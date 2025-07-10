using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static Enterprise.Core.Constants;
using static Enterprise.Rating.Business.FreightInclusiveCalculator;
using Charge = WiseRates.Api.Model.Charge;
using ChargeType = WiseRates.Api.Model.ChargeType;
using It = Moq.It;
using Rate = WiseRates.Api.Model.Rate;
using RatesSearchRequest = WiseRates.Api.Model.RatesSearchRequest;
using RatesSearchResponse = WiseRates.Api.Model.RatesSearchResponse;
using RefCarrier = WiseRates.Api.Model.RefCarrier;
using RefChargeCode = WiseRates.Api.Model.RefChargeCode;
using RefContainer = WiseRates.Api.Model.RefContainer;
using ReferenceNumbersCodes = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes;

namespace Enterprise.RatingTests.WiseRates
{
	public class WiseRatesIntegrationBusinessTests : BaseRatingIntegrationTest
	{
		[GuiTest]
		public void TestSearchRate_WithoutRateSelector_Finds_CW1RateWithBogus_TM_Text_AutoratingHalts()
		{
			var org1 = Helper.NewOrgHeader("ORG1");

			var costing = Helper.NewCosting(org1);
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AU");
			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AUSYD");
			var rateEntry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AUEC");

			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry3.RateLines.RemoveAndDeleteAll();

			rateEntry1.TI_OH_TransportProvider = org1.PK;

			var rateLine1 = rateEntry1.AddRateLine("BAF", MinimumCalculator.Code);
			var rateLine2 = rateEntry2.AddRateLine("CAF", MinimumCalculator.Code);
			var rateLine3 = rateEntry3.AddRateLine("WAR", MinimumCalculator.Code);

			// The Minimum calculator has String1 bound to TM_Text. It also controls the
			// minimum type. Whether Minimum-for-job or minimum-per-chargecode
			var rateLine1Calculator = rateLine1.GetCalculator<MinimumCalculator>();
			rateLine1Calculator.String1 = "???";
			rateLine1Calculator.MinimumValue = 100;

			var rateLine2Calculator = rateLine2.GetCalculator<MinimumCalculator>();
			rateLine2Calculator.MinimumValue = 200;
			rateLine2Calculator.IsChargeCodeMinimum = true;

			var rateLine3Calculator = rateLine3.GetCalculator<MinimumCalculator>();
			rateLine3Calculator.MinimumValue = 300;
			rateLine3Calculator.IsChargeCodeMinimum = true;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.CreditorPK = org1.PK;
			consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = org1.MainAddress.PK;
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_AWBServiceLevel = "STD";

			Factory.Save();

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutoCostAndAssert
				(
						"Expect a popup warning",
						null,
						Array.Empty<AssertionCost>(),
						consol,
						autorateRevenue: false,
						autorateCosts: true,
						expectedErrors: new[] { @"Error Autorating has encountered an error:
Cannot complete Auto-Rating as this Job has been matched to an invalid Rate Line. Please either correct the 'Apply to' field or delete the 'BAF' Rate Line that uses a 'MIN' Calculator with currently an Apply To of '???' on Costing ORG1." }
				);
			}
		}

		public void TestMappings_DifferentRateLinesForEachCarrierSpecific()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var costing = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "Boaty1", localClient.OH_Code, "");
			costing.Charges = new[]
			{
				new Charge
				{
					ChargeCode = "FRT",
					FlatRate = 10,
					Currency = "AUD",
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "FIRST_SPECFLTC",	// Not a schema column name
						Group = "FRT"
					}
				},
				new Charge
				{
					ChargeCode = "FRT",
					FlatRate = 10,
					Currency = "AUD",
					CarrierChargeCodeInfo = new CarrierSpecificChargeCode
					{
						Code = "SEC_SPECFLTC",	// Not a schema column name
						Group = "FRT"
					}
				},
			}.ToList();

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 20,
				},
			};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings, carrierCode: "Boaty1"));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AutorateAndAssertRatesService("Should provide costs from Rates Service", expected, shipment, testContext);
			}
		}

		[TestDate(2016, 08, 05)]
		public void TestUnresolvableCalculator()
		{
			var client = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var costing = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "Boaty", client.OH_Code, "");
			costing.Charges.Add(new Charge { ChargeCode = "CAF", Currency = "AUD" });
			costing.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 5));

			Factory.Save();

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings, carrierCode: "Boaty"));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .05;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AutorateAndAssertRatesService("Should ignore faulty wise rates. Faulty RateLine invalidates entie RateEntry", null, shipment, testContext, expectNoErrorsLogged: false);
				var testLogger = testContext.TestLogger;
				var actualErrors = string.Join("\r\n", testLogger.Errors);
				AssertContains("Error:Rates Service: Could not create calculator from:", actualErrors);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestConflictingRates()
		{
			Helper.ChargeCodes.New("TSTFUM", "Fumigation Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);

			var costing = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", "", "Boaty", NewClient.OH_Code, "");
			// Conflicting RateLines
			costing.Charges.Add(CreatePerUnitCharge("TSTFUM", QuantityUnit.SV, "USD", 10));
			costing.Charges.Add(CreatePerUnitCharge("TSTFUM", QuantityUnit.SV, "AUD", 20));

			var shipment = CreateForwardingShipment(Factory, TransportModes.Sea, ContainerModes.FCL, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", weight: 100m, volume: 10m);
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.Transports[0].CreditorPK = TransportProvider1.PK;

			var fumigationService = shipment.DocsAndCartage.Services.AddNew();
			fumigationService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 1;
			fumigationService.ES_Duration = new TimeSpan(2, 30, 0);
			fumigationService.ES_Completed = ZDateTime.Today;

			Factory.Save();

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings, carrierCode: "Boaty"));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AutorateAndAssertRatesService
				(
					"GIVEN conflicting RateLines WHEN autorate WiseRates THEN should not throw exception",
					expected: new[]
					{
						new AssertionCharge { ChargeCode = "TSTFUM", JR_OSCostAmt = 0.00m, },
					},
					shipment,
					testContext
				);
			}
		}

		[TestDate(2016, 08, 05)]
		public void TestPercentageCalculator()
		{
			var client = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var costing = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "Boaty", client.OH_Code, "");
			costing.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 5));
			costing.Charges.Add(CreatePercentageCharge("CAF", "AUD", 10, CalculatorConstants.Text.FreightCharges));
			costing.Charges.Add(CreatePerUnitCharge("BAF", "KG", "AUD", 1));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 500,
						},
						new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSCostAmt = 100,
						},
						new AssertionCharge
						{
							ChargeCode = "CAF",
							JR_OSCostAmt = 50,
						},
				};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings, carrierCode: "Boaty"));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AutorateAndAssertRatesService("Should provide costs from Rates Service", expected, shipment, testContext);
			}
		}

		[TestDate(2016, 08, 05)]
		public void TestFlatPlusPerUnitCalculator()
		{
			var client = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var costing = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "Boaty", client.OH_Code, "");
			costing.Charges.Add(CreateFlatOrPerUnitCharge("ODOC", "KG", "AUD", 200, 15));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSCostAmt = 1700m,
						},
				};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings, carrierCode: "Boaty"));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AutorateAndAssertRatesService("Should provide costs from Rates Service", expected, shipment, testContext);
			}
		}

		[TestDate(2016, 08, 05)]
		public void TestFlatCalculator()
		{
			var client = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var costing = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "Boaty", client.OH_Code, "");
			costing.Charges.Add(CreateFlatCharge("FRT", "AUD", 200));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 200.00m,
						},
				};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings, carrierCode: "Boaty"));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AutorateAndAssertRatesService("Should provide costs from Rates Service", expected, shipment, testContext);
			}
		}

		[TestDate(2016, 08, 05)]
		public void TestMinimumCalculator()
		{
			var client = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var costing = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "Boaty", client.OH_Code, "");
			costing.Charges.Add(CreateMinCharge("BAF", "AUD", string.Empty, 300));
			//			costing.Charges.Add(CreateMinCharge("BAF", "AUD", 30));		//Todo uncomment and fix. Also we should support job level minimum calculators.
			costing.Charges.Add(CreateFlatCharge("FRT", "AUD", 200));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 200.00m,
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 300.00m,
				},
			};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings, carrierCode: "Boaty"));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AutorateAndAssertRatesService("Should provide costs from Rates Service", expected, shipment, testContext);
			}
		}

		[TestDate(2016, 08, 05)]
		public void TestMinimumOrPerUnitCalculator()
		{
			var client = NewClient;

			var costing = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", client.OH_Code, "");
			costing.Charges.Add(CreateMinimumOrPerUnitCharge("FRT", "KG", "AUD", 300, 5));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = EmiratesAirlines.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 500m,
						},
				};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateAIRRatesSearchResponse(costings, costings));

			AutorateAndAssertRatesService("Should provide costs from Rates Service", expected, shipment, testContext);
		}

		[TestDate(2016, 08, 05)]
		public void TestPerUnitCalculator()
		{
			var localClient = NewClient;

			var costing = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", localClient.OH_Code, "");
			costing.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 8));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = EmiratesAirlines.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 800,
				},
			};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateAIRRatesSearchResponse(costings, costings));

			AutorateAndAssertRatesService("Should provide costs from Rates Service", expected, shipment, testContext);
		}

		[TestDate(2016, 08, 05)]
		public void TestRatesServiceCombinedCalculatorNonInclusiveBreaks()
		{
			var localClient = NewClient;

			const string origin = "AUSYD";
			const string destination = "USLAX";
			const string carrierCode = "Emirates";
			const string unit = "KG";
			const string chargeCode = "FRT";
			const string currency = "AUD";

			var costing = CreateTestRate(TransportModes.Air, "LCL", origin, destination, "", "", "", carrierCode, localClient.OH_Code, "");

			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 0, 1));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 45, 2));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 100, 3));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 150, 4));
			Factory.Save();
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Rates Service Breaks, greater than", 303, 101, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Rates Service Breaks, greater than or equal to", 90, 45, EmiratesAirlines, costing);

			costing.Charges.Clear();
		}

		[TestDate(2016, 08, 05)]
		public void TestRatesServiceCombinedCalculatorInclusiveBreaks()
		{
			var localClient = NewClient;

			const string origin = "AUSYD";
			const string destination = "USLAX";
			const string carrierCode = "Emirates";
			const string unit = "KG";
			const string chargeCode = "FRT";
			const string currency = "AUD";

			var costing = CreateTestRate(TransportModes.Air, "LCL", origin, destination, "", "", "", carrierCode, localClient.OH_Code, "");

			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 0, 1));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 45, 2));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 100, 3));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 150, 4));
			Factory.Save();
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Rates Service Inclusive Breaks, greater than", 198, 99, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Rates Service Inclusive Breaks, greater than NOT equal to", 45, 45, EmiratesAirlines, costing);
		}

		[TestDate(2016, 08, 05)]
		public void TestRatesServiceCombinedCalculatorMinMax()
		{
			var localClient = NewClient;

			const string origin = "AUSYD";
			const string destination = "USLAX";
			const string carrierCode = "Emirates";
			const string unit = "KG";
			const string chargeCode = "FRT";
			const string currency = "AUD";

			var costing = CreateTestRate(TransportModes.Air, "LCL", origin, destination, "", "", "", carrierCode, localClient.OH_Code, "");

			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 0, 1, 100, 500));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 40, 2, 100, 500));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 100, 8, 100, 500));
			Factory.Save();
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Min", 100, 40, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Max", 500, 100, EmiratesAirlines, costing);
		}

		[TestDate(2016, 08, 05)]
		public void TestRatesServiceCombinedCalculatorMinMaxInclusiveBreaks()
		{
			var localClient = NewClient;

			const string origin = "AUSYD";
			const string destination = "USLAX";
			const string carrierCode = "Emirates";
			const string unit = "KG";
			const string chargeCode = "FRT";
			const string currency = "AUD";

			var costing = CreateTestRate(TransportModes.Air, "LCL", origin, destination, "", "", "", carrierCode, localClient.OH_Code, "");

			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 0, 1, minRate: 100, maxRate: 500));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 41, 4, minRate: 100, maxRate: 500));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 100, 8, minRate: 100, maxRate: 500));
			Factory.Save();
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Min", 100, 40, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Max", 500, 500, EmiratesAirlines, costing);
		}

		[TestDate(2016, 08, 05)]
		public void TestRatesServiceCombinedCalculatorFlatRate()
		{
			var localClient = NewClient;

			const string origin = "AUSYD";
			const string destination = "USLAX";
			const string carrierCode = "Emirates";
			const string unit = "KG";
			const string chargeCode = "FRT";
			const string currency = "AUD";

			var costing = CreateTestRate(TransportModes.Air, "LCL", origin, destination, "", "", "", carrierCode, localClient.OH_Code, "");

			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 0, 5, flatRate: 5));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 40, 10, flatRate: 10));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 100, 15, flatRate: 15));
			Factory.Save();
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Min", 155, 30, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Min", 410, 40, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Max", 1515, 100, EmiratesAirlines, costing);
		}

		[TestDate(2016, 08, 05)]
		public void TestRatesServiceCombinedCalculatorAllAttributes()
		{
			var localClient = NewClient;

			const string origin = "AUSYD";
			const string destination = "USLAX";
			const string carrierCode = "Emirates";
			const string unit = "KG";
			const string chargeCode = "FRT";
			const string currency = "AUD";

			var costing = CreateTestRate(TransportModes.Air, "LCL", origin, destination, "", "", "", carrierCode, localClient.OH_Code, "");

			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 0, 3, minRate: 100, maxRate: 1000, flatRate: 5));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 40, 4, minRate: 100, maxRate: 1000, flatRate: 10));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 100, 5, minRate: 100, maxRate: 1000));
			Factory.Save();
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Min", 100, 30, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Break plus flat amount", 110, 35, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Break plus flat amount", 170, 40, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Break", 500, 100, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Max", 1000, 300, EmiratesAirlines, costing);
		}

		[TestDate(2016, 08, 05)]
		public void TestRatesServiceCombinedCalculatorUnorderedCharges()
		{
			var localClient = NewClient;

			const string origin = "AUSYD";
			const string destination = "USLAX";
			const string carrierCode = "Emirates";
			const string unit = "KG";
			const string chargeCode = "FRT";
			const string currency = "AUD";

			var costing = CreateTestRate(TransportModes.Air, "LCL", origin, destination, "", "", "", carrierCode, localClient.OH_Code, "");

			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 40, 4, minRate: 100, maxRate: 1000, flatRate: 10));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 0, 3, minRate: 100, maxRate: 1000, flatRate: 5));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 100, 5, minRate: 100, maxRate: 1000));
			Factory.Save();

			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Min", 100, 30, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Break plus flat amount", 110, 35, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Break plus flat amount", 170, 40, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Break", 500, 100, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Max", 1000, 300, EmiratesAirlines, costing);

			costing.Charges.Clear();
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 100, 8, minRate: 100, maxRate: 500));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 41, 4, minRate: 100, maxRate: 500));
			costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 0, 1, minRate: 100, maxRate: 500));
			Factory.Save();
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Min", 100, 40, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Max", 500, 500, EmiratesAirlines, costing);
			AutorateAndAssertRatesServiceFor("Should provide costs from Rates Service using Rates Service Breaks, greater than", 396, 99, EmiratesAirlines, costing);
		}

		[TestDate(2016, 08, 05)]
		public void TestRatesServiceCombinedCalculatorResolver()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			const string origin = "AUSYD";
			const string destination = "USLAX";
			const string carrierCode = "Boaty";
			const string unit = "KG";
			const string chargeCode = "FRT";
			const string currency = "AUD";
			const string transport = TransportModes.Sea;

			var costing = CreateTestRate(TransportModes.Sea, "FCL", origin, destination, "", "", "", carrierCode, localClient.OH_Code, "");

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = transport;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 50;
			shipment.JS_UnitOfWeight = unit;
			// Set volume so that when converted to weight it becomes same as
			// shipment actual weight. It is converted to weight because
			// it's a SEA LCL
			shipment.JS_ActualVolume = .05;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = transport;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode,
					JR_OSCostAmt = 200,
				},
			};

			var expected2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode,
					JR_OSCostAmt = 100,
				},
			};

			var expected4 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode,
					JR_OSCostAmt = 3300,
				},
			};

			var costings = new[] { costing };
			Func<MockRatesServiceContext> context = () => new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings, carrierCode: carrierCode));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, "<=", 40, 3, flatRate: 5));
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 40, 4, flatRate: 10));
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 100, 5));
				Factory.Save();
				AutorateAndAssertRatesService("Invalid break charges should not resolve", null, shipment, context(), null, null, false);

				costing.Charges.Clear();
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 0, 3, minRate: 0, maxRate: 1000));
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 40, 4, minRate: 0, maxRate: 1000));
				Factory.Save();
				AutorateAndAssertRatesService("Should resolve", expected, shipment, context(), null, null, false);

				costing.Charges.Clear();
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 0, 3, minRate: 100, maxRate: 10));
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 40, 4, minRate: 100, maxRate: 10));
				Factory.Save();
				AutorateAndAssertRatesService("Max smaller than min then max = min", expected2, shipment, context(), null, null, false);

				costing.Charges.Clear();
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 40, null));
				Factory.Save();
				AutorateAndAssertRatesService("Flat rate/Min/Max/Or per unit rate are not specified, should not resolve", null, shipment, context(), null, null, false);

				costing.Charges.Clear();
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 0, 66));
				Factory.Save();
				AutorateAndAssertRatesService("Only one brake item with 0 break. The combined calculator should not resolve but per unit calculator should pick it up.", expected4, shipment, context(), null, null, false);

				costing.Charges.Clear();
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, "<", 40, 3));
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 40, 4));
				Factory.Save();
				AutorateAndAssertRatesService("Invalid Break charges should not resolve", null, shipment, context(), null, null, false);

				costing.Charges.Clear();
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 0, 1));
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 45, 2));
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", null, 3));
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">=", 150, 4));
				Factory.Save();
				AutorateAndAssertRatesService("Null charge breaks should not resolve", null, shipment, context(), null, null, false);

				costing.Charges.Clear();
				costing.Charges.Add(CreateSlidingChargeWithBreak(chargeCode, unit, currency, ">", 0, 5, minRate: 100));

				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = chargeCode,
						JR_OSCostAmt = 250
					}
				};

				AutorateAndAssertRatesService("Null charge breaks should not resolve", expected, shipment, context(), null, null, false);
			}
		}

		public void TestRatesServiceCombinedCalculator_ContainerPivotBreak_OverridesBreakValue_SEA()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");
			Factory.Save();

			var costing = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", cont20GP.RC_Code, "Emirates", localClient.OH_Code, "");
			costing.Charges.Add(CreateSlidingChargeWithBreak("FRT", "KG", "USD", ">=", 0, 20, flatRate: 200));
			costing.Charges.Add(CreateSlidingChargeWithBreak("FRT", "KG", "USD", ">=", 45, 10, flatRate: 100));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 50m;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_ActualWeight = 50m;
			packline.JL_ActualWeightUQ = QuantityUnit.KG;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "KIKI4100011";
			container.JC_RC = cont20GP.PK;
			container.JC_ContainerCount = 1;
			container.JC_ContainerMode = ContainerModes.FCL;
			container.PackLines.Add(packline);

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 600m,
					CostCalculationDescription = "FRT: Base Rate USD 100.00 + 50 Kilogram(s) @ USD 10.00/KG"
				}
			};

			AutorateAndAssertRatesService("Before setting pivot break", expected, shipment, testContext);

			container.JC_PivotBreak = 60m;//setting pivot break 60

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1200m,
					CostCalculationDescription = "FRT: Base Rate USD 200.00 + 50 Kilogram(s) @ USD 20.00/KG"
				}
			};

			AutorateAndAssertRatesService("After setting pivot break to 60", expected, shipment, testContext);
		}

		public void TestRatesServiceCombinedCalculator_ContainerPivotBreak_OverridesBreakValue_AIR()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			var localClient = NewClient;

			var refContainer = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			refContainer.RC_TareWeight = 0m;
			Factory.Save();

			var costing = CreateTestRate(TransportModes.Air, ContainerModes.FCL, "AUSYD", "USLAX", "", "", refContainer.RC_Code, "Emirates", localClient.OH_Code, "");
			costing.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", EquipmentUnit = "CN", Unit = "CN", PerUnitRate = 200m });
			costing.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", EquipmentUnit = "CN", Unit = "KG", PerUnitRate = 10m, Break = 45, BreakOperator = ">=", BreakUnit = "KG", ActualPercentage = 100 });

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100, .1m);
			shipment.JS_PackingMode = ContainerModes.ULD;
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", EmiratesAirlines, shipment, PaymentType.Prepaid);
			consol.CreditorPK = EmiratesAirlines.PK;
			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-1);
			consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(1);
			consol.JK_ConsolMode = ContainerModes.ULD;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_ActualWeight = 50m;
			packline.JL_ActualWeightUQ = QuantityUnit.KG;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "KIKI4100011";
			container.JC_RC = refContainer.PK;
			container.JC_ContainerCount = 1;
			container.JC_ContainerMode = ContainerModes.ULD;
			container.PackLines.Add(packline);

			var registryValue = new RatesServiceRegistrySettingsCollection
			{
				new RatesServiceRegistrySettings
				{
					TransportMode = TransportModes.Air,
					ContainerMode = ContainerModes.ULD,
					IsSubscriptionEnabled = true
				}
			};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateAIRRatesSearchResponse(costings, costings), useCW1RatesProvider: true);

			var expected = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_LocalCostAmount = 250m,
					CostCalculationDescription = "FRT: 1 LD-6 Container(s) @ AUD 200.00/Container + 5 Kilogram(s) @ AUD 10.00/KG"
				}
			};

			AutoCostAndAssertRatesService("Before setting pivot break", expected, consol, testContext, ratesServiceRegistrySettings: registryValue);

			container.JC_PivotBreak = 40m;//setting pivot break 40

			expected = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_LocalCostAmount = 300m,
					CostCalculationDescription = "FRT: 1 LD-6 Container(s) @ AUD 200.00/Container + 10 Kilogram(s) @ AUD 10.00/KG"
				}
			};

			AutoCostAndAssertRatesService("After setting pivot break to 40", expected, consol, testContext, ratesServiceRegistrySettings: registryValue);
		}

		public void TestCombinedCalculator_RateOverridesContainerPayloadWeight_UseWeightFromRateToCalculateNumberOfOccupiedContainers()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			var refContainer = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "LD-6");
			refContainer.RC_GrossWeight = 500m;
			refContainer.RC_TareWeight = 50m;
			refContainer.RC_CubicCapacity = 2;

			Factory.Save();

			var costing = CreateTestRate(TransportModes.Air, ContainerModes.FCL, "AUSYD", "USLAX", "", "", refContainer.RC_Code, "Emirates", null, null);
			costing.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", EquipmentUnit = "CN", Unit = "CN", PerUnitRate = 200m });

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 50, 0m);
			shipment.JS_PackingMode = ContainerModes.ULD;
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", EmiratesAirlines, shipment, PaymentType.Prepaid);
			consol.CreditorPK = EmiratesAirlines.PK;
			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-1);
			consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(1);
			consol.JK_ConsolMode = ContainerModes.ULD;

			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_ActualWeight = 3000;
			packline.JL_ActualWeightUQ = QuantityUnit.KG;
			packline.JL_ActualVolume = 3;
			packline.JL_ActualVolumeUQ = QuantityUnit.M3;

			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			container.JC_ContainerCount = 1;
			container.JC_ContainerMode = ContainerModes.ULD;
			container.JC_RH_NKContainerCommodityCode = "HAZ";
			container.PackLines.Add(packline);

			var registryValue = new RatesServiceRegistrySettingsCollection
			{
				new RatesServiceRegistrySettings
				{
					TransportMode = TransportModes.Air,
					ContainerMode = ContainerModes.ULD,
					IsSubscriptionEnabled = true
				}
			};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateAIRRatesSearchResponse(costings, costings), useCW1RatesProvider: true);

			// Calculate by weight
			costing.Container.PayloadWeight = 2000;
			costing.Container.PayloadVolume = 3;
			var expected = new[]
			{
				new AssertionCost
				{
					//CG Rate
					ChargeCode = "FRT",
					E6_LocalCostAmount = 400m,
					CostCalculationDescription = "FRT: 2 LD-6 Container(s) @ AUD 200.00/Container"
				}
			};

			AutoCostAndAssertRatesService("Must be 2 containers since max payload is 2000 KG and goods are 3000 KG", expected, consol, testContext, ratesServiceRegistrySettings: registryValue);

			// Calculate by Volume
			costing.Container.PayloadWeight = 4000;
			costing.Container.PayloadVolume = 1;
			expected = new[]
			{
				new AssertionCost
				{
					//CG Rate
					ChargeCode = "FRT",
					E6_LocalCostAmount = 600m,
					CostCalculationDescription = "FRT: 3 LD-6 Container(s) @ AUD 200.00/Container"
				}
			};

			AutoCostAndAssertRatesService("Must be 3 containers since max payload is 1 M3 and goods have 3 M3", expected, consol, testContext, ratesServiceRegistrySettings: registryValue);
		}

		public void Test_CMB_CalculatorHasMinChargeable_UseMinChargeableIfApplicable()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			var container1 = Helper.Containers["LD-3"];
			container1.RC_ISOType = "LD-3";
			var container2 = Helper.Containers["LD-6"];
			container2.RC_ISOType = "LD-6";

			Factory.Save();

			var commodity = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, "GEN");
			commodity.RH_UniversalCommodityGroup = "GENL";

			var cost1 = CreateRate(container: "LD-3");
			cost1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "USD", Unit = "KG", MinChargeable = 1000, BreakOperator = ">=", Break = 1, PerUnitRate = 2m });
			cost1.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "USD", Unit = "KG", MinChargeable = 1000, BreakOperator = ">=", Break = 500, PerUnitRate = 1m });
			cost1.Charges.Add(new Charge { ChargeCode = "BAF", Currency = "USD", Unit = "KG", MinChargeable = 200, BreakOperator = ">=", Break = 1, PerUnitRate = 2m });
			cost1.Charges.Add(new Charge { ChargeCode = "BAF", Currency = "USD", Unit = "KG", MinChargeable = 200, BreakOperator = ">=", Break = 500, PerUnitRate = 1m });

			var cost2 = CreateRate(container: "LD-6");
			cost2.Charges.Add(new Charge { ChargeCode = "WAR", Currency = "USD", Unit = "KG", MinChargeable = 1000, BreakOperator = ">=", Break = 1, PerUnitRate = 2m });
			cost2.Charges.Add(new Charge { ChargeCode = "WAR", Currency = "USD", Unit = "KG", MinChargeable = 1000, BreakOperator = ">=", Break = 500, PerUnitRate = 1m });
			cost2.Charges.Add(new Charge { ChargeCode = "CAF", Currency = "USD", Unit = "KG", MinRate = 100, BreakOperator = ">=", Break = 1, PerUnitRate = 2m });
			cost2.Charges.Add(new Charge { ChargeCode = "CAF", Currency = "USD", Unit = "KG", MinRate = 100, BreakOperator = ">=", Break = 500, PerUnitRate = 1m });

			var shipment = CreateShipment(transportMode: "AIR", containerMode: "ULD");
			var consol = CreateConsol(transportMode: "AIR", containerMode: "ULD");
			consol.JK_OA_CreditorAddress = EmiratesAirlines.MainAddress.PK;
			consol.Shipments.Add(shipment);
			consol.AddContainer("LD-3", packLines: new[] { shipment.AddPackLine(weight: 300) });
			consol.AddContainer("LD-6", pivotBreak: 300m, packLines: new[] { shipment.AddPackLine(weight: 200) });

			Factory.Save();

			var costings = new[] { cost1, cost2 };
			var testContext = new MockRatesServiceContext(CreateAIRRatesSearchResponse(costings, costings), useCW1RatesProvider: false);

			var registryValue = new RatesServiceRegistrySettingsCollection
			{
				new RatesServiceRegistrySettings
				{
					TransportMode = TransportModes.Air,
					ContainerMode = ContainerModes.ULD,
					IsSubscriptionEnabled = true
				}
			};

			var expected = new[]
			{
				new AssertionCost
				{
					// Rate has min chargeable 1000 KG which is greater than shipment weight (300 KG)
					ChargeCode = "FRT",
					E6_OSCostAmount = 1000m,
					CostCalculationDescription = "FRT: Min 1000 Kilogram(s) @ USD 1.00/KG"
				},
				new AssertionCost
				{
					// Shipment weight (300 KG) is greater than rate min chargeable (200 KG)
					ChargeCode = "BAF",
					E6_OSCostAmount = 600m,
					CostCalculationDescription = "BAF: 300 Kilogram(s) @ USD 2.00/KG"
				},
				new AssertionCost
				{
					// Pivot weight on the container (300 KG) is greater than shipment weight (200 KG).
					// And pivot weight on the container overrides min chargeable on the rate (1000 KG)
					ChargeCode = "WAR",
					E6_OSCostAmount = 600m,
					CostCalculationDescription = "WAR: 300 Kilogram(s) @ USD 2.00/KG"
				},
				new AssertionCost
				{
					// Shipment weight (200 KG) is used as the rate has no min chargeable.
					// We don't fallback to container pivot break (300 KG) as it only overrides rate min chargeable weight if it exists,
					// if it doesn't - there is nothing to override and thus it is ignored.
					ChargeCode = "CAF",
					E6_OSCostAmount = 400m,
					CostCalculationDescription = "CAF: 200 Kilogram(s) @ USD 2.00/KG"
				},
			};

			AutoCostAndAssertRatesService("Should match", expected, consol, testContext, ratesServiceRegistrySettings: registryValue);
		}

		[TestDate(2016, 08, 05)]
		public void TestCarrierSpecificCostingWithInclusiveOverridesGlobalCostingWithNonInclusive()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var frtCharge = Helper.ChargeCodes["FRT"];
			var bafCharge = Helper.ChargeCodes["BAF"];

			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var globalCostingWithNonInclusiveBAF = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", cont20GP.RC_Code, "Emirates", localClient.OH_Code, "");
			globalCostingWithNonInclusiveBAF.Charges.Add(CreatePerUnitCharge(bafCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 9m));

			var carrierSpecificCosting = Helper.NewCosting(carrier);
			var costEntryWithInclusive = carrierSpecificCosting.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "AUSYD", "USLAX", "", cont20GP.RC_Code);
			costEntryWithInclusive.RateLines.RemoveAndDeleteAll();
			costEntryWithInclusive.AddRateLine(frtCharge.AC_Code, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia).GetCalculator<UnitCalculator>().PerUnit = 8m;
			var bafRateLine = costEntryWithInclusive.AddRateLine(bafCharge.AC_Code, Code, currencyCode: CurrencyCodes.Australia);
			bafRateLine.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.Included;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packline = shipment.OuterPackLines.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "KIKI4100011";
			container.JC_RC = cont20GP.PK;
			container.JC_ContainerCount = 1;
			container.JC_ContainerMode = ContainerModes.FCL;
			container.PackLines.Add(packline);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 8m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 8.00/Container

International Freight

Inclusive charges:
BAF - Bunker Adjustment Factor (Included)"
				}
			};

			var costings = new[] { globalCostingWithNonInclusiveBAF };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);

			AutorateAndAssertRatesService("Should discard rates from global costing if the carrier specific cost has included charges with same code", expected, shipment, testContext);
		}

		[TestDate(2016, 08, 05)]
		public void TestCarrierSpecificCostingWithNonInclusiveIsNotOverridenByGlobalCostingWithInclusive()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var frtCharge = Helper.ChargeCodes["FRT"];
			var bafCharge = Helper.ChargeCodes["BAF"];

			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var rate = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", cont20GP.RC_Code, "Emirates", localClient.OH_Code, "");
			rate.Charges = new List<Charge>(new[]
			{
				new Charge
				{
					ChargeCode = "FRT",
					Unit = "CN",
					Currency = "AUD",
					PerUnitRate = 8m
				},
				new Charge
				{
					ChargeCode = "BAF",
					FreightInclusiveCarriageCharge = "FRT",
					ChargeType = ChargeType.Included,
					Currency = "AUD"
				}
			});

			var carrierSpecificCosting = Helper.NewCosting(carrier);
			var costEntryWithNonInclusive = carrierSpecificCosting.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "AUSYD", "USLAX", "", cont20GP.RC_Code);
			costEntryWithNonInclusive.RateLines.RemoveAndDeleteAll();
			costEntryWithNonInclusive.AddRateLine(bafCharge.AC_Code, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia).GetCalculator<UnitCalculator>().PerUnit = 20;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packline = shipment.OuterPackLines.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "KIKI4100011";
			container.JC_RC = cont20GP.PK;
			container.JC_ContainerCount = 1;
			container.JC_ContainerMode = ContainerModes.FCL;
			container.PackLines.Add(packline);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 8m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 8.00/Container

International Freight"
				},
				new AssertionCharge
				{
					ChargeCode = bafCharge.AC_Code,
					JR_OSCostAmt = 20m,
					CostCalculationDescription = @"BAF: 1 20GP Container(s) @ AUD 20.00/Container"
				}
			};

			var rates = new[] { rate };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(rates, rates), useCW1RatesProvider: true);

			AutorateAndAssertRatesService("Should discard rates from costing if there are included charges with same code", expected, shipment, testContext);
		}

		[TestDate(2016, 08, 05)]
		public void TestLocationSpecificCostingWithInclusiveOverridesGenericCostingWithNonInclusive()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var frtCharge = Helper.ChargeCodes["FRT"];
			var bafCharge = Helper.ChargeCodes["BAF"];

			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var globalCostingWithNonInclusiveBAF = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AU", "US", "", "", cont20GP.RC_Code, "Emirates", localClient.OH_Code, "");
			globalCostingWithNonInclusiveBAF.Charges.Add(CreatePerUnitCharge(bafCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 9m));

			var locationSpecificCosting = Helper.NewCosting(carrier);
			var locationSpecificEntry = locationSpecificCosting.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "AUSYD", "USLAX", "", cont20GP.RC_Code);
			locationSpecificEntry.RateLines.RemoveAndDeleteAll();
			locationSpecificEntry.AddRateLine(frtCharge.AC_Code, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia).GetCalculator<UnitCalculator>().PerUnit = 8m;
			var locationSpecificRateLine = locationSpecificEntry.AddRateLine(bafCharge.AC_Code, Code, currencyCode: CurrencyCodes.Australia);
			locationSpecificRateLine.GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.Included;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packline = shipment.OuterPackLines.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "KIKI4100011";
			container.JC_RC = cont20GP.PK;
			container.JC_ContainerCount = 1;
			container.JC_ContainerMode = ContainerModes.FCL;
			container.PackLines.Add(packline);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 8m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 8.00/Container

International Freight

Inclusive charges:
BAF - Bunker Adjustment Factor (Included)"
				},
			};

			var costings = new[] { globalCostingWithNonInclusiveBAF };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);

			AutorateAndAssertRatesService("Should discard rates from global costing if there are included charges with same code into carrier specific costing", expected, shipment, testContext);
		}

		[TestDate(2016, 08, 05)]
		public void TestLocationSpecificCostingWithNonInclusiveOverridesGenericCostingWithInclusive()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var frtCharge = Helper.ChargeCodes["FRT"];
			var bafCharge = Helper.ChargeCodes["BAF"];

			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var globalCostingWithInclusiveBAF = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AU", "US", "", "", cont20GP.RC_Code, "Emirates", localClient.OH_Code, "");
			globalCostingWithInclusiveBAF.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 8m));
			globalCostingWithInclusiveBAF.Charges.Add(CreateInclusiveCharge(bafCharge.AC_Code, CurrencyCodes.Australia, frtCharge.AC_Code));

			var locationSpecificCosting = Helper.NewCosting(carrier);
			var locationSpecificEntry = locationSpecificCosting.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "AUSYD", "USLAX", "", cont20GP.RC_Code);
			locationSpecificEntry.RateLines.RemoveAndDeleteAll();
			locationSpecificEntry.AddRateLine(bafCharge.AC_Code, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia).GetCalculator<UnitCalculator>().PerUnit = 20;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packline = shipment.OuterPackLines.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "KIKI4100011";
			container.JC_RC = cont20GP.PK;
			container.JC_ContainerCount = 1;
			container.JC_ContainerMode = ContainerModes.FCL;
			container.PackLines.Add(packline);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 8m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 8.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = bafCharge.AC_Code,
					JR_OSCostAmt = 20m,
					CostCalculationDescription = @"BAF: 1 20GP Container(s) @ AUD 20.00/Container"
				}
			};

			var costings = new[] { globalCostingWithInclusiveBAF };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);

			AutorateAndAssertRatesService("Should discard rates from costing if there are included charges with same code", expected, shipment, testContext);
		}

		[TestDate(2016, 08, 05)]
		public void TestGlobalCW1CostIsPreferredToRatesServiceCost()
		{
			var chargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBCHRG");

			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsCreditor = true;

			var ratesServiceCosting = CreateTestRate("SEA", "FCL", "AUSYD", "USLAX", "", "", "", "Boaty", NewClient.OH_Code, "");
			ratesServiceCosting.Charges.Add(CreateFlatCharge(chargeCode.AC_Code, CurrencyCodes.Australia, 150));

			var globalCosting = Helper.NewGlobalCosting(carrier);
			var globalEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX");
			globalEntry.RateLines.RemoveAndDeleteAll();
			globalEntry.AddRateLine(chargeCode, FlatCalculator.Code, currencyCode: CurrencyCodes.Australia).GetCalculator<FlatCalculator>().BaseRate = 200;

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, ZGuid.Empty, "AUSYD", "USLAX", 100);
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", carrier, shipment);
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.CreditorPK = carrier.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 200m,
					CostCalculationDescription = "GLBCHRG: Base Rate AUD 200.00"
				}
			};

			var rates = new[] { ratesServiceCosting };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(rates, rates, carrierCode: "Boaty"), useCW1RatesProvider: true);

			var expectedLog = @"Information: RateLine Found GLBCHRG-FLT-Wise Costing TRASPROV1
Information: RateLine Found GLBCHRG-FLT-Global Costing TRASPROV1
Information: RateLine Filtered GLBCHRG-FLT-Wise Costing TRASPROV1	reason:	failed similarity check";

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AutorateAndAssertRatesService("Should provide costs from Rates Service", expected, shipment, testContext);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Filter out Rates Service", expectedLog);
			}
		}

		[TestDate(2016, 08, 05)]
		public void TestIncludedChargesAffectRateFiltering()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var frtCharge = Helper.ChargeCodes["FRT"];
			var bafCharge = Helper.ChargeCodes["BAF"];
			var cafCharge = Helper.ChargeCodes["CAF"];
			var warCharge = Helper.ChargeCodes["WAR"];
			var odocCharge = Helper.ChargeCodes["ODOC"];

			//Charges
			// Rates Service:
			// 20GP - FRT inclusive BAF and ODOC
			// 40GP - FRT inclusive CAF
			// 40GP - FRT inclusive WAR (AU - US) - should be both filtered out
			//
			// CW1 Rates:
			// 20GP - BAF
			// 40GP - BAF

			//Expected outcome:
			//FRT Charge combined for 20GP and 40GP, listing inclusive ODOC charge. Inclusive BAF charge should be discarded.
			//BAF Charge for 20GP and 40GP coming from CW1 Cost

			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var cont40GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var wiseCost20GP = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", cont20GP.RC_Code, "Emirates", localClient.OH_Code, "");
			wiseCost20GP.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 8m));
			wiseCost20GP.Charges.Add(CreateInclusiveCharge(bafCharge.AC_Code, CurrencyCodes.Australia, frtCharge.AC_Code));
			wiseCost20GP.Charges.Add(CreateInclusiveCharge(odocCharge.AC_Code, CurrencyCodes.Australia, frtCharge.AC_Code));

			var wiseCost40GP = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", cont40GP.RC_Code, "Emirates", localClient.OH_Code, "");
			wiseCost40GP.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 10m));
			wiseCost40GP.Charges.Add(CreateInclusiveCharge(cafCharge.AC_Code, CurrencyCodes.Australia, frtCharge.AC_Code));

			var lessSpecificWiseCost40GP = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AU", "US", "", "", cont40GP.RC_Code, "Emirates", localClient.OH_Code, "");
			lessSpecificWiseCost40GP.Charges.Add(CreateFlatCharge(frtCharge.AC_Code, CurrencyCodes.Australia, 10m));
			lessSpecificWiseCost40GP.Charges.Add(CreateInclusiveCharge(warCharge.AC_Code, CurrencyCodes.Australia, frtCharge.AC_Code));

			var costing = Helper.NewCosting(carrier);
			var costEntry20GP = costing.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "AUSYD", "USLAX", "", cont20GP.RC_Code);
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine(bafCharge, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia).GetCalculator<UnitCalculator>().PerUnit = 20;

			var costEntry40GP = costing.AddRateEntry(ContainerModes.FCL, TransportModes.Sea, "AUSYD", "USLAX", "", cont40GP.RC_Code);
			costEntry40GP.RateLines.RemoveAndDeleteAll();
			costEntry40GP.AddRateLine(bafCharge, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia).GetCalculator<UnitCalculator>().PerUnit = 25;

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "KIKI4100011";
			container1.JC_RC = cont20GP.PK;
			container1.JC_ContainerCount = 1;
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.PackLines.Add(packline1);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "KIKI4100022";
			container2.JC_RC = cont40GP.PK;
			container2.JC_ContainerCount = 1;
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.PackLines.Add(packline2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 8m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 8.00/Container

International Freight

Inclusive charges:
ODOC - Origin Documentation Fee (Included)"
				},
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 10m,
					CostCalculationDescription = @"FRT: 1 40GP Container(s) @ AUD 10.00/Container

International Freight

Inclusive charges:
CAF - Currency Adjustment Factor (Included)
WAR - War Risk Surcharge (Included)"
				},
				new AssertionCharge
				{
					ChargeCode = bafCharge.AC_Code,
					JR_OSCostAmt = 20m,
					CostCalculationDescription = "BAF: 1 20GP Container(s) @ AUD 20.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = bafCharge.AC_Code,
					JR_OSCostAmt = 25m,
					CostCalculationDescription = "BAF: 1 40GP Container(s) @ AUD 25.00/Container"
				}
			};

			var costings = new[] { wiseCost20GP, wiseCost40GP, lessSpecificWiseCost40GP };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);

			AutorateAndAssertRatesService("Should discard rates from costing if there are included charges with same code", expected, shipment, testContext);
		}

		[TestDate(2021, 08, 13)]
		public void TestFilteredInclusiveChargeShouldNotIncludedInCalculationDescription()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var frtCharge = Helper.ChargeCodes["FRT"];
			var bafCharge = Helper.ChargeCodes["BAF"];
			var cafCharge = Helper.ChargeCodes["CAF"];
			var odocCharge = Helper.ChargeCodes["ODOC"];
			var warCharge = Helper.ChargeCodes["WAR"];

			bafCharge.AC_ChargeGroup = "DST";

			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var cont40GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var wiseCost20GP = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", cont20GP.RC_Code, "Emirates", localClient.OH_Code, "");
			wiseCost20GP.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 8m));
			wiseCost20GP.Charges.Add(CreateInclusiveCharge(bafCharge.AC_Code, CurrencyCodes.Australia, frtCharge.AC_Code));
			wiseCost20GP.Charges.Add(CreateInclusiveCharge(odocCharge.AC_Code, CurrencyCodes.Australia, frtCharge.AC_Code));

			var wiseCost40GP = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", cont40GP.RC_Code, "Emirates", localClient.OH_Code, "");
			wiseCost40GP.Charges.Add(CreatePerUnitCharge(warCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 10m));
			wiseCost40GP.Charges.Add(CreateInclusiveCharge(cafCharge.AC_Code, CurrencyCodes.Australia, warCharge.AC_Code));

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "KIKI4100011";
			container1.JC_RC = cont20GP.PK;
			container1.JC_ContainerCount = 1;
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.PackLines.Add(packline1);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "KIKI4100022";
			container2.JC_RC = cont40GP.PK;
			container2.JC_ContainerCount = 1;
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.PackLines.Add(packline2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 8m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 8.00/Container

International Freight

Inclusive charges:
	ODOC - Origin Documentation Fee (Included)"
				},
				new AssertionCharge
				{
					ChargeCode = warCharge.AC_Code,
					JR_OSCostAmt = 10m,
					CostCalculationDescription = @"WAR: 1 40GP Container(s) @ AUD 10.00/Container

War Risk Surcharge

Inclusive charges:
CAF - Currency Adjustment Factor (Included)"
				}
			};

			var costings = new[] { wiseCost20GP, wiseCost40GP };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);

			var expectedLog = @"Information: RateLine Found BAF-FRT-20GP-Wise Costing TRASPROV1
Information: RateLine Found FRT-UNT-CN-20GP-Wise Costing TRASPROV1
Information: RateLine Found ODOC-FRT-20GP-Wise Costing TRASPROV1
Information: RateLine Found CAF-FRT-40GP-Wise Costing TRASPROV1
Information: RateLine Found WAR-UNT-CN-40GP-Wise Costing TRASPROV1
Information: RateLine Filtered BAF-FRT-20GP-Wise Costing TRASPROV1	reason:	DST charge group is not applicable for AUSYD-USLAX Export CCX";

			AutorateAndAssertRatesService("Should not have included charge in description if we filter it", expected, shipment, testContext);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Filter out Rates Service", expectedLog);

			var shipmentJob = shipment.Job as Job;
			var frtChargeFromShipment = shipmentJob.Charges.Cast<Accounting.Business.JobInvoicing.Charge>().FirstOrDefault(x => x.ChargeCode.AC_Code == "FRT");
			var costCalculationDescription = frtChargeFromShipment.CostCalculationDescription.ToAscii();
			AssertContains("Should contain included charge", "ODOC - Origin Documentation Fee (Included)", costCalculationDescription);
			AssertNotContains("Should not contain filtered included charge", "BAF - Bunker Adjustment Factor (Included)", costCalculationDescription);
		}

		[TestDate(2016, 08, 05)]
		public void TestCompanyTariffIsCorrectlyApplied()
		{
			var localClient = NewClient;
			var companyTariff = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			companyTariff.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 15m));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = EmiratesAirlines.PK;
			consol.JK_OA_ShippingLineAddress = EmiratesAirlines.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 1500m,
						},
				};

			var rates = new[] { companyTariff };
			var testContext = new MockRatesServiceContext(CreateAIRRatesSearchResponse(rates, rates));

			AutorateAndAssertRatesService("Should apply Company Tariff", expected, shipment, testContext);
		}

		[TestDate(2017, 03, 14)]
		public void TestWarehouseJobsDontUseRatesService()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 2;
			data.Part1.OP_Width = 2;
			data.Part1.OP_Depth = 2;
			data.Part1.OP_MeasureUQ = Length.Metres;
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = Weight.Kilograms;

			var costingWR = CreateTestRate("WHS", "ALL", "", "", "", "", "", "", data.Whs1.WarehouseAddress.Header.OH_Code, "");
			costingWR.Charges.Add(CreateFlatCharge("WCS", "AUD", 200));

			var warehouseHandlingFactor = new ChargeableFactor(
				new ConversionFactor(3000m, Volume.CubicCentimeters, Weight.Kilograms),
				new ConversionFactor(97m, Volume.CubicInches, Weight.Pounds));

			WarehouseDataRegistry.Instance.WarehouseChargeableFactorStorage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, warehouseHandlingFactor);

			new WhsTestHelperFunctions(Factory).CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", now.ToOffset().AddDays(-4), data.Part1, 1m);

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = now.AddDays(-6);
			invoice.ET_StorageToDate = now;

			Factory.Save();

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Throws(new InvalidOperationException());

			var testContext = new MockRatesServiceContext(clientMock.Object);

			AutorateAndAssertRatesService("Should not use Rates Service", Array.Empty<AssertionCharge>(), invoice, testContext);
		}

		[TestDate(2017, 03, 14)]
		[ExpectNoExceptions]
		public void TestLinerAgencyJobsDontUseRatesService()
		{
			var client = Helper.NewOrgHeader();

			var costEntry = Helper.NewCosting(null).AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "AU", "US", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costRateLine = costEntry.AddRateLine("OSEC", FlatCalculator.Code);
			costRateLine.GetCalculator<FlatCalculator>().BaseRate = 50m;
			AddParityExchangeRate(costRateLine.Currency);

			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_TransportMode = TransportModes.Sea;
			billOfLading.JS_PackingMode = ContainerModes.FCL;
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "USLAX";
			billOfLading.JS_INCO = PaymentType.Prepaid;
			billOfLading.ConsigneeDocumentaryAddress.OrganisationPK = client.MainAddress.PK;

			var container = billOfLading.RealContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Save();

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(m => m.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Throws(new InvalidOperationException());

			var testContext = new MockRatesServiceContext(clientMock.Object);

			AutorateAndAssertRatesService("Should not use Rates Service", Array.Empty<AssertionCharge>(), billOfLading, testContext);
		}

		[TestDate(2017, 03, 14)]
		[ExpectNoExceptions]
		public void TestCustomsJobsDontUseRatesService()
		{
			GlbBranch.CurrentBranch.Company.SetCountry("CA");

			var client = Helper.NewOrgHeader();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Customs.Common.CA.CAJobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = "VAR";
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "LVSIDA";
			invoice1.JZ_OH_Buyer = client.PK;
			invoice1.InvoiceLines.AddNew();
			invoice1.JZ_IncoTerm = "FOB";

			Factory.Save();

			declaration.ResumeApportionment();
			declaration.DoMerge();

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(m => m.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Throws(new InvalidOperationException());

			var testContext = new MockRatesServiceContext(clientMock.Object);

			AutorateAndAssertRatesService("Should not use Rates Service", Array.Empty<AssertionCharge>(), declaration, testContext);
		}

		[TestDate(2017, 08, 22)]
		public void TestQAWEventLoggedOnOneOffQuote()
		{
			var frtCharge = Helper.ChargeCodes["FRT"];
			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var costing = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", cont20GP.RC_Code, "Emirates", NewClient.OH_Code, "");
			costing.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 800m));

			var quote = CreateQuotedBooking(TransportModes.Sea, "FCL", string.Empty, Consignor, Consignor, NewClient, TransportProvider1, "AUSYD", "USLAX", 450m, 1m, QuotedBookingState.QuoteOnly);

			var container = quote.Quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_RC = Factory.LoadTop1<MasterFiles.Business.RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			container.TC_ContainerCount = 1;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 800,
				},
			};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings));

			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				AutorateAndAssertRatesService("Should autorate quote", expected, quote, testContext);

				var logParent = (IStmALogParent)quote.Quote;
				var eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.QuoteAutoratedWithWiseRatesCode);
				var events = logParent.Logs.Find(eventFilter);

				AssertEquals("Should have added 1 QAW event", 1, events.Length);

				AutorateAndAssertRatesService("Should autorate quote again", expected, quote, testContext);

				events = logParent.Logs.Find(eventFilter);

				AssertEquals("Should not add more QAW events", 1, events.Length);

				quote.ConvertQuoteToQuotedBooking();
				Factory.Save();
			}
		}

		[TestDate(2017, 08, 22)]
		public void TestBAWEventLoggedOnQuotedBooking()
		{
			var frtCharge = Helper.ChargeCodes["FRT"];
			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var costing = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", cont20GP.RC_Code, "Emirates", NewClient.OH_Code, "");
			costing.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 800m));

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", string.Empty, NewClient, Consignor, Consignee, TransportProvider1, "AUSYD", "USLAX", 450m, 1m, QuotedBookingState.BookingOnly);
			quotedBooking.Booking.JS_E_DEP = ZDateTime.Today;
			quotedBooking.Booking.JS_E_ARV = ZDateTime.Today;

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<MasterFiles.Business.RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 800,
				},
			};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings));

			AutorateAndAssertRatesService("Should autorate booking", expected, quotedBooking, testContext);

			var logParent = (IStmALogParent)quotedBooking.Booking;
			var eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.BookingAutoratedWithRatesServiceCode);
			var events = logParent.Logs.Find(eventFilter);

			AssertEquals("Should have added 1 BAW event", 1, events.Length);
			Factory.Save();

			AutorateAndAssertRatesService("Should autorate booking again", expected, quotedBooking, testContext);

			events = logParent.Logs.Find(eventFilter);

			AssertEquals("Should not have added more BAW events", 1, events.Length);
		}

		[TestDate(2017, 08, 22)]
		public void TestBAWEventNotLoggedOnQuotedBookingIfQuoteHasQAWEvent()
		{
			var frtCharge = Helper.ChargeCodes["FRT"];
			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var costing = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", cont20GP.RC_Code, "Emirates", NewClient.OH_Code, "");
			costing.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 800m));

			var quotedBooking = CreateQuotedBooking(TransportModes.Sea, "FCL", string.Empty, NewClient, Consignor, Consignee, TransportProvider1, "AUSYD", "USLAX", 450m, 1m);
			quotedBooking.Booking.JS_E_DEP = ZDateTime.Today;
			quotedBooking.Booking.JS_E_ARV = ZDateTime.Today;

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<MasterFiles.Business.RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 800,
				},
			};

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings));

			AutorateAndAssertRatesService("Should autorate booking", expected, quotedBooking, testContext);

			var logParent = (IStmALogParent)quotedBooking.Booking;
			var eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.BookingAutoratedWithRatesServiceCode);
			var events = logParent.Logs.Find(eventFilter);

			AssertEquals("Should not have added a BAW event to Booking", 0, events.Length);

			logParent = quotedBooking.Quote;
			eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.QuoteAutoratedWithWiseRatesCode);
			events = logParent.Logs.Find(eventFilter);

			AssertEquals("Should have added 1 QAW event to Quote", 1, events.Length);
		}

		public void TestAirSearch_CallsRateServiceOnce()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;

			var mockFactory = new Mock<IWiseRatesClientFactory>();
			var mockClient = new Mock<IWiseRatesClient>();

			mockFactory
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((mockClient.Object, string.Empty));

			mockClient
				.Setup(f => f.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			ObjectFactory.Substitute(mockFactory.Object);

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var context = new RatingContext();
				var testAutoRater = new FreightAutoRater(context);

				var proxy = new AutoRatingProxy(consol.RatingAdapter);
				testAutoRater.AutoRate(new RatingCriteria(proxy, context.Factory), CostSell.Cost, false);
			}

			mockClient
				.Verify(f => f.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

			Assert(true);
		}

		public void TestBeingMultipleChargeRecordsWithSameCodeInOneRate()
		{
			var localClient = NewClient; // Do not remove. AutorateAndAssertRatesService expects NewClient as Local Client

			var rate = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT1 = CreatePerUnitCharge("FRT", "KG", "AUD", 15m);
			rate.Charges.Add(chargeFRT1);
			var chargeFRT2 = CreatePerUnitCharge("FRT", "KG", "AUD", 27m);
			chargeFRT2.RateLineID = 2;
			rate.Charges.Add(chargeFRT2);
			rate.Charges.Add(CreatePerUnitCharge("BAF", "KG", "AUD", 10m));

			var rates = new[] { rate };
			var testContext = new MockRatesServiceContext(CreateAIRRatesSearchResponse(rates, rates));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = EmiratesAirlines.PK;
			consol.JK_OA_ShippingLineAddress = EmiratesAirlines.MainAddress.PK;

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSCostAmt = 4200m,
						},
					new AssertionCharge
						{
							ChargeCode = "BAF",
							JR_OSCostAmt = 1000m,
						},
				};

			AutorateAndAssertRatesService("Should apply Company Tariff", expected, shipment, testContext);
		}

		#region Commodity Group

		public void TestCommodityGroup_GivenCommodityGroupIsNotMapped_ThenShouldNotFindCharge()
		{
			var containerCommodityCode = "XX1";
			var rateServiceCommodityGroup = "XXA";

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = containerCommodityCode;

			AssertNotEquals
			(
				"Precondition: RefCommodity CommodityGroup is not mapped.",
				rateServiceCommodityGroup,
				commodity.RH_UniversalCommodityGroup
			);

			AssertCommodityGroup
			(
				message: @"GIVEN Commodity-Group is not mapped to RefCommodity WHEN AutoRate Then should not find charge.",
				containerCommodityCode,
				rateServiceCommodityGroup,
				expectedChargeFound: false,
				expectedRatingSummaryLines: new[] { "RateEntry Filtered Wise Costing TRASPROV1 reason: Commodity Code didn't match job" }
			);
		}

		public void TestCommodityGroup_GivenCommodityGroupIsMappedToRefCommodity_ThenShouldFindCharge()
		{
			var containerCommodityCode = "XX1";
			var rateServiceCommodityGroup = "XXA";

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = containerCommodityCode;
			commodity.RH_UniversalCommodityGroup = rateServiceCommodityGroup;

			AssertEquals
			(
				"Precondition: RefCommodity CommodityGroup is mapped.",
				rateServiceCommodityGroup,
				commodity.RH_UniversalCommodityGroup
			);

			AssertCommodityGroup
			(
				message: @"GIVEN Commodity-Group is mapped to RefCommodity WHEN AutoRate Then should find charge.",
				containerCommodityCode,
				rateServiceCommodityGroup,
				expectedChargeFound: true,
				expectedRatingSummaryLines: new[] { "Information: RateLine Found ODOC-FLT-Wise Costing TRASPROV1" }
			);
		}

		public void TestCommodityGroup_GivenContainerHasEmptyCommodityCode_ThenShouldNotFindCharge()
		{
			AssertCommodityGroup
			(
				message: @"GIVEN container has empty CommmodityCode WHEN AutoRate Then should not find charge.",
				containerCommodityCode: string.Empty,
				rateServiceCommodityGroup: "XXA",
				expectedChargeFound: false,
				expectedRatingSummaryLines: new[] { "RateEntry Filtered Wise Costing TRASPROV1 reason: Commodity Code didn't match job GEN." }
			);
		}

		public void TestCommodityGroup_GivenContainerHasEmptyCommodityCode_ThenShouldFindGENCharge()
		{
			var commodity = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, "GEN");
			commodity.RH_UniversalCommodityGroup = "GENL";

			Factory.Save();

			AssertCommodityGroup
			(
				message: @"GIVEN container has empty CommmodityCode WHEN AutoRate Then should find GEN charge.",
				containerCommodityCode: string.Empty,
				rateServiceCommodityGroup: "GENL",
				expectedChargeFound: true,
				expectedRatingSummaryLines: new[] { "Information: RateLine Found ODOC-FLT-Wise Costing TRASPROV1" }
			);
		}

		public void TestCommodityGroup_GivenContainerDoesNotExist_ThenShouldNotFindCharge()
		{
			AssertCommodityGroup
			(
				message: @"GIVEN container does not exist WHEN AutoRate Then should not find charge.",
				containerCommodityCode: null,
				rateServiceCommodityGroup: "XXA",
				expectedChargeFound: false,
				expectedRatingSummaryLines: new[] { "RateEntry Filtered Wise Costing TRASPROV1 reason: Commodity Code didn't match job empty." }
			);
		}

		void AssertCommodityGroup(string message, string containerCommodityCode, string rateServiceCommodityGroup, bool expectedChargeFound, string[] expectedRatingSummaryLines = default)
		{
			var localClient = NewClient;

			var rateServiceRate = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUBNE", "GBSUN", "", rateServiceCommodityGroup, "", "Emirates", localClient.OH_Code, "");
			rateServiceRate.Charges.Add(CreateFlatCharge("ODOC", "AUD", 20m));

			var rates = new[] { rateServiceRate };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(rates, rates));

			var expectedCharges = expectedChargeFound
				? new[]
				{
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_OSCostAmt = 20m
					},
				}
				: Array.Empty<AssertionCharge>();

			AssertCommodityGroup
			(
				message,
				containerCommodityCode,
				testContext,
				expectedCharges,
				expectedRatingSummaryLines
			);
		}

		void AssertCommodityGroup(string message, string containerCommodityCode, MockRatesServiceContext testContext, AssertionCharge[] expectedCharges, string[] expectedRatingSummaryLines = default)
		{
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_ConsolMode = ContainerModes.FCL;

			if (containerCommodityCode != null)
			{
				var container = consol.Containers.AddNew();
				container.JC_RC = Helper.Containers["20GP"].PK;
				container.JC_RH_NKContainerCommodityCode = containerCommodityCode;
				container.JC_ContainerMode = ContainerModes.FCL;
			}

			Factory.Save();

			AutorateAndAssertRatesService(message, expectedCharges, shipment, testContext);

			var autoRatingSummary = GetAutoRatingSummary(string.Empty, shipment, expectedWarnings: null, expectedErrors: null);
			foreach (var expectedRatingSummaryLine in expectedRatingSummaryLines)
			{
				AssertContains($"Rating Summary doesn't contain matching line: '{expectedRatingSummaryLine}'", expectedRatingSummaryLine, autoRatingSummary);
			}
		}

		public void TestCommodityGroup_WhenMapToMultipleRefCommodity_ThenShouldFindMultipleCharges()
		{
			var containerCommodityCode = "XX1";
			var rateServiceCommodityGroupA = "XXA";
			var rateServiceCommodityGroupB = RefCommodityCode.PERS;

			var localClient = NewClient;

			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = containerCommodityCode;
			commodity1.RH_UniversalCommodityGroup = rateServiceCommodityGroupA;
			commodity1.RH_IsPerishable = true;

			var rateServiceRate1 = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUBNE", "GBSUN", "", rateServiceCommodityGroupA, "", "Emirates", localClient.OH_Code, "");
			rateServiceRate1.Charges.Add(CreateFlatCharge("ODOC", "AUD", 10m));

			var rateServiceRate2 = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUBNE", "GBSUN", "", rateServiceCommodityGroupB, "", "Emirates", localClient.OH_Code, "");
			rateServiceRate2.Charges.Add(CreateFlatCharge("OCART", "AUD", 20m));

			var rates = new[] { rateServiceRate1, rateServiceRate2 };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(rates, rates));

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_Desc = "Origin Documentation Fee",
					JR_OSCostAmt = 10m
				},
				new AssertionCharge
				{
					ChargeCode = "OCART",
					JR_OSCostAmt = 20m
				},
			};

			AssertCommodityGroup
			(
				message: "GIVEN consol has commodity with XXA CommodityGroup and PERS, WHEN autorate THEN should find rateService rates with commodityGroup of XXA and PERS",
				containerCommodityCode,
				testContext: testContext,
				expectedCharges: expectedCharges,
				expectedRatingSummaryLines: new[]
				{
					"Information: RateLine Found OCART-FLT-Wise Costing TRASPROV1",
					"Information: RateLine Found ODOC-FLT-Wise Costing TRASPROV1"
				}
			);
		}

		#endregion

		[TestDate(2018, 06, 20)]
		public void TestRatesContainingJobIDArePreferred()
		{
			var localClient = NewClient;
			var rate = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT1 = CreatePerUnitCharge("FRT", "KG", "AUD", 15m);
			chargeFRT1.RateLineID = 1;
			rate.Charges.Add(chargeFRT1);

			var rate2 = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT2 = CreatePerUnitCharge("FRT", "KG", "AUD", 10m);
			chargeFRT2.RateLineID = 2;
			rate2.Charges.Add(chargeFRT2);

			var testContext = new MockRatesServiceContext(CreateAIRRatesSearchResponse(new[] { rate, rate2 }, new[] { rate }));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = EmiratesAirlines.PK;
			consol.JK_OA_ShippingLineAddress = EmiratesAirlines.MainAddress.PK;

			Factory.Save();

			JobHeader job = new JobHeader.Loader(shipment).TryLoadOrCreate();

			rate2.ReservedForJobIDs = new string[] { job.JH_JobNum };

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1000m,
				}
			};

			AutorateAndAssertRatesService("Should only find reserved rate", expected, shipment, testContext);
		}

		[TestDate(2018, 06, 20)]
		public void TestRatesContainingMultipleJobIDsReturnsPreferredRate()
		{
			var localClient = NewClient;
			var rate = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT1 = CreatePerUnitCharge("FRT", "KG", "AUD", 15m);
			chargeFRT1.RateLineID = 1;
			rate.Charges.Add(chargeFRT1);

			var rate2 = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT2 = CreatePerUnitCharge("FRT", "KG", "AUD", 10m);
			chargeFRT2.RateLineID = 2;
			rate2.Charges.Add(chargeFRT2);

			var rate3 = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT3 = CreatePerUnitCharge("FRT", "KG", "AUD", 5m);
			chargeFRT3.RateLineID = 3;
			rate3.Charges.Add(chargeFRT3);

			var rate4 = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT4 = CreatePerUnitCharge("FRT", "KG", "AUD", 20m);
			chargeFRT4.RateLineID = 4;
			rate4.Charges.Add(chargeFRT4);

			var rate5 = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT5 = CreatePerUnitCharge("CAF", "KG", "AUD", 50m);
			chargeFRT5.RateLineID = 5;
			rate5.Charges.Add(chargeFRT5);

			var rate6 = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT6 = CreatePerUnitCharge("BAF", "KG", "AUD", 60m);
			chargeFRT5.RateLineID = 6;
			rate6.Charges.Add(chargeFRT6);

			var testContext = new MockRatesServiceContext(
				CreateAIRRatesSearchResponse(
					new[] { rate, rate2, rate3, rate4, rate5, rate6 },
					new[] { rate }));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = EmiratesAirlines.PK;
			consol.JK_OA_ShippingLineAddress = EmiratesAirlines.MainAddress.PK;

			Factory.Save();

			JobHeader job = new JobHeader.Loader(shipment).TryLoadOrCreate();

			rate2.ReservedForJobIDs = new string[] { job.JH_JobNum };
			rate3.ReservedForJobIDs = new[] { "NotThisShipment", "AnotherShipment" };

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1000m,
				}
			};

			AutorateAndAssertRatesService("Since there are reserved rates for this job, regardless of having rates with different charge codes, only reserved rate(s) should be considered", expected, shipment, testContext);

			var actualLogs = testContext.TestLogger.Infos.ToArray();

			var expectedLogs = new[]
			{
				"Info:Rates Service: 6 entries from Rates Service found. 6 CGGD, 2 reserved",
				"Info:Rates Service: 6 converted rate(s) including 1 reserved. The reserved rate(s) will take priority and the rest will be filtered"
			};

			AssertCollectionContains(expectedLogs[0], actualLogs);
			AssertCollectionContains(expectedLogs[1], actualLogs);
		}

		[TestDate(2018, 06, 20)]
		public void TestRatesWithoutJobIDAreReturned()
		{
			var localClient = NewClient;
			var rate = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT1 = CreatePerUnitCharge("FRT", "KG", "AUD", 15m);
			rate.Charges.Add(chargeFRT1);

			var rate2 = CreateTestRate("AIR", "LCL", "AUSYD", "USLAX", "", "", "", "Emirates", "", "");
			var chargeFRT2 = CreatePerUnitCharge("FRT", "KG", "AUD", 10m);
			rate2.Charges.Add(chargeFRT2);

			var testContext = new MockRatesServiceContext(CreateAIRRatesSearchResponse(new[] { rate, rate2 }, new[] { rate }));

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = EmiratesAirlines.PK;
			consol.JK_OA_ShippingLineAddress = EmiratesAirlines.MainAddress.PK;

			Factory.Save();

			JobHeader job = new JobHeader.Loader(shipment).TryLoadOrCreate();

			rate2.ReservedForJobIDs = new[] { "NotThisShipment" };

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1500m,
				},
					new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1000m,
				}
			};

			AutorateAndAssertRatesService("Should not take reserved rate if not the same jobID", expected, shipment, testContext);
		}

		public void TestContractNumbersUsedForRatesService()
		{
			const string matchableContractNumber = "MTH123";
			const string unmatchedContractNumber = "???";
			var client = NewClient;

			var costing = CreateTestRate(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "", "", "", "Emirates", client.OH_Code, "");
			costing.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 5));

			var contractedCosting = CreateTestRate(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "", "", "", "Emirates", client.OH_Code, "");
			contractedCosting.Charges.Add(CreatePerUnitCharge("BAF", "KG", "AUD", 1));
			contractedCosting.ContractNumber = matchableContractNumber;
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, ZGuid.Empty, ZGuid.Empty, "AUSYD", "USLAX", 100, .1m);
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", EmiratesAirlines, shipment);
			consol.CreditorPK = EmiratesAirlines.PK;
			Factory.Save();

			var costings = new[] { costing, contractedCosting };
			var testContext = new MockRatesServiceContext(CreateAIRRatesSearchResponse(costings, costings));

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 500,
					},
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSCostAmt = 100,
					}
				};

				AutorateAndAssertRatesService("Non-contract specified consol should use all Rates Service", expected, shipment, testContext);

				consol.JK_CarrierContractNumber = matchableContractNumber;
				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSCostAmt = 100,
					}
				};

				AutorateAndAssertRatesService("Contract specified consol should use contract Rates Service", expected, shipment, testContext);

				consol.JK_CarrierContractNumber = unmatchedContractNumber;
				expected = Array.Empty<AssertionCharge>();

				//check WI00566519 for more details
				AutorateAndAssertRatesService("unmatched contract number should not bring any charges", expected, shipment, testContext);
			}

			using (Globals.SetIsUserInteractiveForTest(true))
			{
				consol.JK_CarrierContractNumber = "";
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 500,
					},
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSCostAmt = 100,
					}
				};

				AutorateAndAssertRatesService("Non-contract specified consol should use all Rates Service", expected, shipment, testContext);

				consol.JK_CarrierContractNumber = matchableContractNumber;
				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSCostAmt = 100,
					}
				};

				AutorateAndAssertRatesService("Contract specified consol should use contract Rates Service", expected, shipment, testContext);

				consol.JK_CarrierContractNumber = unmatchedContractNumber;
				expected = Array.Empty<AssertionCharge>();

				//check WI00566519 for more details
				AutorateAndAssertRatesService("unmatched contract number should not bring any charges", expected, shipment, testContext);
			}
		}

		#region Named Accounts

		public void TestAutorateConsolWithNamedAccount_RatesWithMatchingNamedAccount_ShouldHaveHigherPriority()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			Factory.Save();

			var costingWithoutNamedAccounts = CreateTestRate(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "", "", "", "Emirates", NewClient.OH_Code, "");
			costingWithoutNamedAccounts.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 5));

			var costingWithNamedAccounts = CreateTestRate(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "", "", "", "Emirates", NewClient.OH_Code, "");
			costingWithNamedAccounts.Charges.Add(CreatePerUnitCharge("FRT", "KG", "AUD", 1));
			costingWithNamedAccounts.NamedAccounts = new[] { "ABC", "DEF", "GHK" };

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100, .1m);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", EmiratesAirlines, shipment, PaymentType.Prepaid);
			consol.CreditorPK = EmiratesAirlines.PK;
			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-1);
			consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(1);

			var testContext = new MockRatesServiceContext(
				CreateAIRRatesSearchResponse(
					new[] { costingWithoutNamedAccounts, costingWithNamedAccounts },
					new[] { new RefChargeCode { Code = "FRT", Group = "FRT" } }));

			Assert("Precondition: empty consol numbers", consol.Numbers.IsNullOrEmpty());

			var expectedNoNamedAccountCost = new[] { new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 500, } };
			AutoCostAndAssertRatesService("Rate without named account should be selected", expectedNoNamedAccountCost, consol, testContext);

			var contractNamedAccount = consol.Numbers.AddNew();
			contractNamedAccount.CE_EntryType = ReferenceNumbersCodes.ContractNamedAccount;
			contractNamedAccount.CE_EntryNum = "DEF";

			var expectedNamedAccountCost = new[] { new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100, } };
			AutoCostAndAssertRatesService("Rate with named accounts should be selected", expectedNamedAccountCost, consol, testContext);

			contractNamedAccount.CE_EntryNum = "Not in the list";
			AutoCostAndAssertRatesService("Fallback to rate without named accounts", expectedNoNamedAccountCost, consol, testContext);
		}

		public void TestAutorateConsolWithNamedAccount_RateNACsDifferentFromJobNAC_Popup_AnswerYes_ShouldApplyRatesAndFirstNACInAlphabeticalOrder()
		{
			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100, },
			};
			var message = "Job NAC should be overridden with the top NAC in A-Z order";

			AssertAutorateConsolWithNamedAccount_Popup("DEF", false, true, new[] { DialogResult.Yes }, message, "AAAA", expectedCosts);
		}

		public void TestAutorateConsolWithNamedAccount_RateNACsDifferentFromJobNAC_Popup_AnswerNo_ShouldApplyRatesOnly()
		{
			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100, },
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 500, },
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 700, },
			};
			var message = "Job NAC should not be overridden";

			AssertAutorateConsolWithNamedAccount_Popup("DEF", false, true, new[] { DialogResult.No }, message, "DEF", expectedCosts);
		}

		public void TestAutorateConsolWithNamedAccount_RateNACsDifferentFromJobNAC_Popup_AnswerIgnore_ShouldNotApplyRatesNorNAC()
		{
			var message = "All rates are discarded and Job NAC should not be overridden";
			AssertAutorateConsolWithNamedAccount_Popup("DEF", false, true, new[] { DialogResult.Ignore }, message, "DEF", Array.Empty<AssertionCost>());
		}

		public void TestAutorateConsolWithNamedAccount_JobNACEmpty_MultipleRateNACs_NoPopup_ShouldNotApplyNAC()
		{
			var expectedCosts = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100, },
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 500, },
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 700, },
			};
			var message = "Job NAC should still be empty";
			AssertAutorateConsolWithNamedAccount_Popup(null, false, false, null, message, null, expectedCosts);
		}

		void AssertAutorateConsolWithNamedAccount_Popup(string jobNAC, bool singleRateNAC, bool shouldHavePopup, DialogResult[] enqueuedAnswers, string assertionMessage, string expectedNAC, AssertionCost[] expectedCosts)
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			Factory.Save();

			var costingWithNamedAccount1 = CreateTestRate(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "", "", "", "Emirates", NewClient.OH_Code, "");
			costingWithNamedAccount1.Charges.Add(CreateSlidingChargeWithBreak("FRT", "KG", "AUD", ">=", 0, 7));
			costingWithNamedAccount1.NamedAccounts = new[] { "ABC" };

			var costingWithNamedAccount2 = CreateTestRate(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "", "", "", "Emirates", NewClient.OH_Code, "");
			costingWithNamedAccount2.Charges.Add(CreateSlidingChargeWithBreak("FRT", "KG", "AUD", ">=", 10, 5));
			costingWithNamedAccount2.NamedAccounts = new[] { singleRateNAC ? "ABC" : "BCD" };

			var costingWithNamedAccount3 = CreateTestRate(RatingConstants.RateCategory.AIR, RateMode.LCL, "AUSYD", "USLAX", "", "", "", "Emirates", NewClient.OH_Code, "");
			costingWithNamedAccount3.Charges.Add(CreateSlidingChargeWithBreak("FRT", "KG", "AUD", ">=", 20, 1));
			costingWithNamedAccount3.NamedAccounts = new[] { singleRateNAC ? "ABC" : "AAAA" };

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100, .1m);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", EmiratesAirlines, shipment, PaymentType.Prepaid);
			consol.CreditorPK = EmiratesAirlines.PK;
			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-1);
			consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Now.AddDays(1);

			var testInteractor = new TestInteractor();
			var testContext = new MockRatesServiceContext(
				CreateAIRRatesSearchResponse(
					new[] { costingWithNamedAccount1, costingWithNamedAccount2, costingWithNamedAccount3 },
					new[] { new RefChargeCode { Code = "FRT", Group = "FRT" } }),
				testInteractor: testInteractor);

			if (!string.IsNullOrWhiteSpace(jobNAC))
			{
				var contractNamedAccount = consol.Numbers.AddNew();
				contractNamedAccount.CE_EntryType = ReferenceNumbersCodes.ContractNamedAccount;
				contractNamedAccount.CE_EntryNum = jobNAC;
			}

			if (expectedCosts.Length == 0)
			{
				testInteractor.Answers.Add(false);
			}
			AutoCostAndAssertRatesService("Rates pass the filter", expectedCosts, consol, testContext, enqueuedAnswers: enqueuedAnswers);

			var actualMessages = UnitTestUserNotification.Instance.PreviousMessages;
			var expectedPopupMessage = $"During Autorating operation, Named Account '{(singleRateNAC ? "ABC" : "AAAA")}' is found on the rates to be applied, which is different from the Named Account 'DEF' input in the job. How would you like to proceed?";
			var message = $"Popup window asking for rates and NAC applying\r\nExpected:\r\n{expectedPopupMessage}\r\nFound:\r\n{string.Join("\r\n", actualMessages)}";

			AssertEquals(message, shouldHavePopup, actualMessages.ContainsMessageContainingThisText(expectedPopupMessage));

			var consolNumbers = consol.Numbers.Cast<CusEntryNumber>();
			if (string.IsNullOrWhiteSpace(expectedNAC))
			{
				Assert(assertionMessage, consolNumbers.All(n => n.CE_EntryType != ReferenceNumbersCodes.ContractNamedAccount));
			}
			else
			{
				AssertEquals(assertionMessage, expectedNAC, consolNumbers.Single(n => n.CE_EntryType == ReferenceNumbersCodes.ContractNamedAccount).CE_EntryNum);
			}
		}

		#endregion

		#region Calculation Costs with Container Quality

		/*
		 * Assumption:
		 *
		 *	C1=Container with blank Quality
		 *	C2=Container with Quality=GOH
		 *	C3 = Container with Quality=GGG
		 *	R1 = Rate with blank Quality
		 *	R2 = Rate with Quality=GOH
		 *	R3 = Rate with Quality=GGG
		 */

		public void TestCalculationCostsWithContainerQuality_C1()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var frtCharge = Helper.ChargeCodes["FRT"];
			var bafCharge = Helper.ChargeCodes["BAF"];
			var cafCharge = Helper.ChargeCodes["CAF"];

			var wiseCostEmpty = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", GP20.RC_Code, "Emirates", localClient.OH_Code, "");
			wiseCostEmpty.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 11m));
			wiseCostEmpty.Charges.Add(CreatePerUnitCharge(bafCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 12m));

			var wiseCostGOH = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", GP20.RC_Code, "Emirates", localClient.OH_Code, "");
			SetContainerQuality(wiseCostGOH, "GOH");
			wiseCostGOH.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 21m));
			wiseCostGOH.Charges.Add(CreatePerUnitCharge(cafCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 22m));

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "KIKI4100011";
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerCount = 1;
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.PackLines.Add(packline1);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 11m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 11.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = bafCharge.AC_Code,
					JR_OSCostAmt = 12m,
					CostCalculationDescription = "BAF: 1 20GP Container(s) @ AUD 12.00/Container"
				},
			};
			var costings = new[] { wiseCostEmpty };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);
			AutorateAndAssertRatesService("C1 / R1 => 1xR1", expected, shipment, testContext);

			expected = Array.Empty<AssertionCharge>();
			costings = new[] { wiseCostGOH };
			testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);
			AutorateAndAssertRatesService("C1 / R2 => None", expected, shipment, testContext);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 11m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 11.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = bafCharge.AC_Code,
					JR_OSCostAmt = 12m,
					CostCalculationDescription = "BAF: 1 20GP Container(s) @ AUD 12.00/Container"
				},
			};
			costings = new[] { wiseCostEmpty, wiseCostGOH };
			testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);
			AutorateAndAssertRatesService("C1 / R1, R2 => 1xR1", expected, shipment, testContext);
		}

		public void TestCalculationCostsWithContainerQuality_C2()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var frtCharge = Helper.ChargeCodes["FRT"];
			var bafCharge = Helper.ChargeCodes["BAF"];
			var cafCharge = Helper.ChargeCodes["CAF"];

			var wiseCostEmpty = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", GP20.RC_Code, "Emirates", localClient.OH_Code, "");
			wiseCostEmpty.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 11m));
			wiseCostEmpty.Charges.Add(CreatePerUnitCharge(bafCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 12m));

			var wiseCostGOH = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", GP20.RC_Code, "Emirates", localClient.OH_Code, "");
			SetContainerQuality(wiseCostGOH, "GOH");
			wiseCostGOH.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 21m));
			wiseCostGOH.Charges.Add(CreatePerUnitCharge(cafCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 22m));

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packline2 = shipment.OuterPackLines.AddNew();

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "KIKI4100022";
			container2.JC_RC = GP20.PK;
			container2.JC_ContainerQuality = "GOH";
			container2.JC_ContainerCount = 1;
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.PackLines.Add(packline2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 11m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 11.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = bafCharge.AC_Code,
					JR_OSCostAmt = 12m,
					CostCalculationDescription = "BAF: 1 20GP Container(s) @ AUD 12.00/Container"
				},
			};
			var costings = new[] { wiseCostEmpty };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);
			AutorateAndAssertRatesService("C2 / R1 => 1xR1", expected, shipment, testContext);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 21m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 21.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = cafCharge.AC_Code,
					JR_OSCostAmt = 22m,
					CostCalculationDescription = "CAF: 1 20GP Container(s) @ AUD 22.00/Container"
				},
			};
			costings = new[] { wiseCostGOH };
			testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);
			AutorateAndAssertRatesService("C2 / R2 => 1xR2", expected, shipment, testContext);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 21m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 21.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = bafCharge.AC_Code,
					JR_OSCostAmt = 12m,
					CostCalculationDescription = "BAF: 1 20GP Container(s) @ AUD 12.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = cafCharge.AC_Code,
					JR_OSCostAmt = 22m,
					CostCalculationDescription = "CAF: 1 20GP Container(s) @ AUD 22.00/Container"
				},
			};
			costings = new[] { wiseCostEmpty, wiseCostGOH };
			testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);
			AutorateAndAssertRatesService("C2 / R1, R2 => 1xFRT from R2 + 1xCAF + 1xBAF", expected, shipment, testContext);
		}

		public void TestCalculationCostsWithContainerQuality_C1andC2()
		{
			var localClient = NewClient;
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var frtCharge = Helper.ChargeCodes["FRT"];
			var bafCharge = Helper.ChargeCodes["BAF"];
			var cafCharge = Helper.ChargeCodes["CAF"];

			var wiseCostEmpty = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", GP20.RC_Code, "Emirates", localClient.OH_Code, "");
			wiseCostEmpty.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 11m));
			wiseCostEmpty.Charges.Add(CreatePerUnitCharge(bafCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 12m));

			var wiseCostGOH = CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", GP20.RC_Code, "Emirates", localClient.OH_Code, "");
			SetContainerQuality(wiseCostGOH, "GOH");
			wiseCostGOH.Charges.Add(CreatePerUnitCharge(frtCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 21m));
			wiseCostGOH.Charges.Add(CreatePerUnitCharge(cafCharge.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 22m));

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = QuantityUnit.KG;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = QuantityUnit.M3;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostInsuranceAndFreight;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "KIKI4100011";
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerCount = 1;
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.PackLines.Add(packline1);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "KIKI4100022";
			container2.JC_RC = GP20.PK;
			container2.JC_ContainerQuality = "GOH";
			container2.JC_ContainerCount = 1;
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.PackLines.Add(packline2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 22m,
					CostCalculationDescription = @"FRT: 2 20GP Container(s) @ AUD 11.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = bafCharge.AC_Code,
					JR_OSCostAmt = 24m,
					CostCalculationDescription = "BAF: 2 20GP Container(s) @ AUD 12.00/Container"
				},
			};
			var costings = new[] { wiseCostEmpty };
			var testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);
			AutorateAndAssertRatesService("C1, C2 / R1 => 2xR1", expected, shipment, testContext);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 21m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 21.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = cafCharge.AC_Code,
					JR_OSCostAmt = 22m,
					CostCalculationDescription = "CAF: 1 20GP Container(s) @ AUD 22.00/Container"
				},
			};
			costings = new[] { wiseCostGOH };
			testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);
			AutorateAndAssertRatesService("C1, C2 / R2 => 1xR2", expected, shipment, testContext);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 11m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 11.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = frtCharge.AC_Code,
					JR_OSCostAmt = 21m,
					CostCalculationDescription = @"FRT: 1 20GP Container(s) @ AUD 21.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = bafCharge.AC_Code,
					JR_OSCostAmt = 24m,
					CostCalculationDescription = "BAF: 2 20GP Container(s) @ AUD 12.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = cafCharge.AC_Code,
					JR_OSCostAmt = 22m,
					CostCalculationDescription = "CAF: 1 20GP Container(s) @ AUD 22.00/Container"
				},
			};
			costings = new[] { wiseCostEmpty, wiseCostGOH };
			testContext = new MockRatesServiceContext(CreateSEARatesSearchResponse(costings, costings), useCW1RatesProvider: true);
			AutorateAndAssertRatesService("C1, C2 / R1, R2 => 1xFRT from R1 + 1xFRT from R2 + 2xBAF + 1xCAF", expected, shipment, testContext);
		}

		#endregion

		[GuiTest]
		public void TestWiseRates_AutoRatingLogsShouldBePopulated_CargoSphereRateSelector_Enabled()
		{
			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration enabled. Rate selection applied.",
				new[] { "Booboo, Rates Client Service has a warning" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: false,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration enabled. Rate selection skipped.",
				new[] { "Booboo, Rates Client Service has a warning" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: false,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration enabled. Rate selection applied.",
				new[] { "Warning: Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: false,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration enabled. Rate selection skipped.",
				new[] { "Warning: Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: false,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration disabled. Rate selection applied.",
				new[] { "Request will not be sent to Rates Service because CGSP integration is disabled in the registry" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: false,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration disabled. Rate selection skipped.",
				new[] { "Request will not be sent to Rates Service because CGSP integration is disabled in the registry" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: false,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration disabled. Rate selection applied.",
				new[] { "Warning: Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: false,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration disabled. Rate selection skipped.",
				new[] { "Warning: Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for SEA-FCL: AutoRating -> Rates Service -> Rates Service Subscription" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: false,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: true
			);
		}

		[GuiTest]
		public void TestWiseRates_AutoRatingLogsShouldBePopulated_CargoguideRateSelector_Enabled()
		{
			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration enabled. Rate selection applied.",
				new[] { "Booboo, Rates Client Service has a warning" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: true,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration enabled. Rate selection skipped.",
				new[] { "Booboo, Rates Client Service has a warning" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: true,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration enabled. Rate selection applied.",
				new[] { "Warning: Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for AIR-LSE: AutoRating -> Rates Service -> Rates Service" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: true,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration enabled. Rate selection skipped.",
				new[] { "Warning: Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for AIR-LSE: AutoRating -> Rates Service -> Rates Service Subscription" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: true,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				" Rate service enabled, Integration disabled. Rate selection applied.",
				new[] { "Request will not be sent to Rates Service because CGGD integration is disabled in the registry" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: true,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration disabled. Rate selection skipped.",
				new[] { "Request will not be sent to Rates Service because CGGD integration is disabled in the registry" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: true,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				" Rate service disabled, Integration disabled. Rate selection applied.",
				new[] { "Warning: Request will not be sent to Rates Service because CGGD integration is disabled in the registry" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: true,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: true
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration disabled. Rate selection skipped.",
				new[] { "Warning: Request will not be sent to Rates Service because CGGD integration is disabled in the registry" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: true,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: true
			);
		}

		[GuiTest]
		public void TestWiseRates_AutoRatingLogsShouldBePopulated_CargoSphereRateSelector_Disabled()
		{
			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration enabled.",
				new[] { "Booboo, Rates Client Service has a warning" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				isAir: false,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: false
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration enabled.",
				new[] { "Information: Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				isAir: false,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: false
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration disabled.",
				new[] { "Request will not be sent to Rates Service because CGSP integration is disabled in the registry" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				isAir: false,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: false
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration disabled.",
				new[] { "Information: Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				isAir: false,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: false
			);
		}

		[GuiTest]
		public void TestWiseRates_AutoRatingLogsShouldBePopulated_CargoguideRateSelector_Disabled()
		{
			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration enabled.",
				new[] { "Booboo, Rates Client Service has a warning" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				isAir: true,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: false
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration enabled.",
				new[] { "Information: Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				isAir: true,
				cgcsIntegrationSetting: true,
				expectRateSelectorOrChooserForm: false
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service enabled, Integration disabled.",
				new[] { "Request will not be sent to Rates Service because CGGD integration is disabled in the registry" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				isAir: true,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: false
			);

			AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages
			(
				"Rate service disabled, Integration disabled.",
				new[] { "Information: Rates Service subscription is disabled in the registry: AutoRating -> Rates Service -> Rates Service Subscription" },
				"Booboo, Rates Client Service has a warning",
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				RatesServiceRegistrySettingsCollection.GetDisabled(),
				isAir: true,
				cgcsIntegrationSetting: false,
				expectRateSelectorOrChooserForm: false
			);
		}

		[GuiTest]
		public void TestGivenRateServiceEnabled_WhenAutoRating_ThenAutoRatingLogAboutQueryAndServerUrlShouldNotBePopulated()
		{
			var unexpectedLogMessages = new[]
			{
				"Request rates from Rates Service: ServiceAddress =",
				"Searching for costs on Rates Service with the following filter:",
				"Searching for costs with the following filter:",
				"Request rates from Rates Service: Query Info =",
				"sending request to Rates Service: Request ="
			};

			AssertAutoRatingLogNotContainsQueryAndServerUrlMessages
			(
				"Rate service enabled, Integration enabled. Rate selection applied.",
				unexpectedLogMessages,
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: false,
				cgcsIntegrationSetting: true
			);

			AssertAutoRatingLogNotContainsQueryAndServerUrlMessages
			(
				"Rate service enabled, Integration enabled. Rate selection skipped.",
				unexpectedLogMessages,
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: false,
				cgcsIntegrationSetting: true
			);

			AssertAutoRatingLogNotContainsQueryAndServerUrlMessages
			(
				"Rate service enabled, Integration enabled. Rate selection applied.",
				unexpectedLogMessages,
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: true,
				cgcsIntegrationSetting: true
			);

			AssertAutoRatingLogNotContainsQueryAndServerUrlMessages
			(
				"Rate service enabled, Integration enabled. Rate selection skipped.",
				unexpectedLogMessages,
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: true,
				cgcsIntegrationSetting: true
			);

			AssertAutoRatingLogNotContainsQueryAndServerUrlMessages
			(
				"Rate service enabled, Integration disabled. Rate selection applied.",
				unexpectedLogMessages,
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: false,
				cgcsIntegrationSetting: false
			);

			AssertAutoRatingLogNotContainsQueryAndServerUrlMessages
			(
				"Rate service enabled, Integration disabled. Rate selection skipped.",
				unexpectedLogMessages,
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: false,
				cgcsIntegrationSetting: false
			);

			AssertAutoRatingLogNotContainsQueryAndServerUrlMessages
			(
				"Rate service enabled, Integration disabled. Rate selection applied.",
				unexpectedLogMessages,
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: false,
				isAir: true,
				cgcsIntegrationSetting: false
			);

			AssertAutoRatingLogNotContainsQueryAndServerUrlMessages
			(
				"Rate service enabled, Integration disabled. Rate selection skipped.",
				unexpectedLogMessages,
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				RatesServiceRegistrySettingsCollection.GetEnabled(),
				skipSelection: true,
				isAir: true,
				cgcsIntegrationSetting: false
			);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void AssertAutoRatingLogContainsExpectedRateServiceRelatedMessages(
			string message,
			string[] expectedLogMessages,
			string injectedRateServiceClientError,
			RatesServiceRegistrySettingsCollection rateServiceSubscriptionSetting, RatesServiceRegistrySettingsCollection rateSelectorSubscriptionSetting,
			bool skipSelection = false, bool isAir = false, bool cgcsIntegrationSetting = false, bool expectRateSelectorOrChooserForm = true)
		{
			// This mocking is needed to avoid WiseRatesClientMock preventing the production of errors
			// that would otherwise occur from RatesServiceClient.
			var mockFactory = new Mock<IWiseRatesClientFactory>();
			var mockClient = new Mock<IWiseRatesClient>();
			mockFactory
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((new RatesServiceClient(mockClient.Object), string.Empty));
			mockClient
				.Setup(f => f.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse() { Warnings = new[] { injectedRateServiceClientError } }));

			using (ObjectFactory.Substitute(mockFactory.Object))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				if (isAir)
				{
					consol.JK_TransportMode = TransportModes.Air;
				}
				else
				{
					consol.JK_TransportMode = TransportModes.Sea;
				}

				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_PrepaidCollect = "PPD";

				if (!isAir)
				{
					var c = consol.Containers.AddNew();
					c.JC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
					c.JC_ContainerCount = 1;
				}

				var rateSelectorOrChooserSeen = false;

				using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
				using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rateServiceSubscriptionSetting))
				using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rateSelectorSubscriptionSetting))
				using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, cgcsIntegrationSetting))
				using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, cgcsIntegrationSetting))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown((form) =>
					{
						if (form is RateChooserForm rateChooser)
						{
							rateSelectorOrChooserSeen = true;
							Application.DoEvents();

							if (skipSelection)
							{
								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
								rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
							}
							else
							{
								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
								var applyButton =
									rateChooser.Controls.Find("toolStrip", true)
									.OfType<ZToolStrip>()
									.Select(t => t.Items.OfType<ZToolStripButton>().SingleOrDefault(x => x.Text == "Apply"))
									.WhereNotNull()
									.Single();
								applyButton.Enabled = true;
								applyButton.PerformClick();
							}
						}
						else if (form is RateSelectorForm rateSelector)
						{
							rateSelectorOrChooserSeen = true;
							Application.DoEvents();

							if (skipSelection)
							{
								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.None;
								rateSelector.BtnSkipRateSelectionClick(null, EventArgs.Empty);
							}
							else
							{
								var rateViewModel = new Mock<RateViewModel>();
								((NonContainerizedRatesViewModel)rateSelector.ViewModel).SelectedRate = rateViewModel.Object;

								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
								var applyButton =
									rateSelector.Controls.Find("zToolStrip1", true)
									.OfType<ZToolStrip>()
									.Select(t => t.Items.OfType<ZToolStripButton>().SingleOrDefault(x => x.Text == "Apply"))
									.WhereNotNull()
									.Single();
								applyButton.Enabled = true;
								applyButton.PerformClick();
							}
						}
					});

					AutoCostAndAssert
					(
						"No assertions here. Just autorating",
						null,
						null,
						consol,
						autorateRevenue: false
					);

					AssertEquals("Rateselector presence check fail", expectRateSelectorOrChooserForm, rateSelectorOrChooserSeen);

					AssertAutoratingAuditLogNoteContainsLines(consol, message, expectedLogMessages);

					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void AssertAutoRatingLogNotContainsQueryAndServerUrlMessages(
			string message,
			string[] unexpectedLogMessages,
			RatesServiceRegistrySettingsCollection rateServiceSubscriptionSetting, RatesServiceRegistrySettingsCollection rateSelectorSubscriptionSetting,
			bool skipSelection = false, bool isAir = false, bool cgcsIntegrationSetting = false)
		{
			// This mocking is needed to avoid WiseRatesClientMock preventing the production of errors
			// that would otherwise occur from RatesServiceClient.
			var mockFactory = new Mock<IWiseRatesClientFactory>();
			var mockClient = new Mock<IWiseRatesClient>();
			mockFactory
				.Setup(f => f.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((new RatesServiceClient(mockClient.Object), string.Empty));
			mockClient
				.Setup(f => f.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse()));

			using (ObjectFactory.Substitute(mockFactory.Object))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				if (isAir)
				{
					consol.JK_TransportMode = TransportModes.Air;
				}
				else
				{
					consol.JK_TransportMode = TransportModes.Sea;
				}

				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_PrepaidCollect = "PPD";

				if (!isAir)
				{
					var c = consol.Containers.AddNew();
					c.JC_RC = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
					c.JC_ContainerCount = 1;
				}

				using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
				using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rateServiceSubscriptionSetting))
				using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rateSelectorSubscriptionSetting))
				using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, cgcsIntegrationSetting))
				using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, cgcsIntegrationSetting))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown((form) =>
					{
						if (form is RateChooserForm rateChooser)
						{
							Application.DoEvents();

							if (skipSelection)
							{
								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
								rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
							}
							else
							{
								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
								var applyButton =
									rateChooser.Controls.Find("toolStrip", true)
									.OfType<ZToolStrip>()
									.Select(t => t.Items.OfType<ZToolStripButton>().SingleOrDefault(x => x.Text == "Apply"))
									.WhereNotNull()
									.Single();
								applyButton.Enabled = true;
								applyButton.PerformClick();
							}
						}
						else if (form is RateSelectorForm rateSelector)
						{
							Application.DoEvents();

							if (skipSelection)
							{
								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.None;
								rateSelector.BtnSkipRateSelectionClick(null, EventArgs.Empty);
							}
							else
							{
								var rateViewModel = new Mock<RateViewModel>();
								((NonContainerizedRatesViewModel)rateSelector.ViewModel).SelectedRate = rateViewModel.Object;

								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
								var applyButton =
									rateSelector.Controls.Find("zToolStrip1", true)
									.OfType<ZToolStrip>()
									.Select(t => t.Items.OfType<ZToolStripButton>().SingleOrDefault(x => x.Text == "Apply"))
									.WhereNotNull()
									.Single();
								applyButton.Enabled = true;
								applyButton.PerformClick();
							}
						}
					});

					AutoCostAndAssert
					(
						"No assertions here. Just autorating",
						null,
						null,
						consol,
						autorateRevenue: false
					);

					foreach (var unexpectedLogMessage in unexpectedLogMessages)
					{
						AssertAutoratingAuditLogNotContains(consol, unexpectedLogMessage, message);
					}

					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
				}
			}
		}

		#region Implementation

		void AutorateAndAssertRatesService<T>(string message, IEnumerable<AssertionCharge> expected, T jobParent, MockRatesServiceContext ratingContext, OrgHeader agent = null, Job job = null, bool expectNoErrorsLogged = true) where T : IJobHeaderParent, IBusiness
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var createNewJob = job == null;

			using (job = job ?? new Job.Loader(jobParent).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				if (createNewJob)
				{
					job.Charges.RemoveAndDeleteAll();
					job.ExchangeRates.RemoveAndDeleteAll();
					job.JH_OA_LocalChargesAddr = NewClient.MainAddress.PK;

					if (agent != null)
					{
						job.JH_OA_AgentCollectAddr = agent.MainAddress.PK;
					}
				}

				new AutoRatingStarter(new[] { (IBusiness)jobParent }, ratingContext).ExecuteAutorating(AutoRateOptions.AutorateCosts.With(billingType: BillingType.Invoicing));
				var testLogger = ratingContext.TestLogger;

				if (expectNoErrorsLogged)
				{
					AssertContainsExactElementsInAnyOrder("Autorating is supposed to log no Errors", Enumerable.Empty<string>(), testLogger.Errors);
				}

				message = string.Concat(message, "\r\n", testLogger);
				AssertCharges(message, expected, job);
			}
		}

		void AutorateAndAssertRatesServiceFor(string message, int expectedCost, int givenWeight, OrgHeader carrier, Rate costing, string transport = TransportModes.Air, string unit = "KG", string origin = "AUSYD", string destination = "USLAX", string carrierCode = "Emirates", string chargeCode = "FRT")
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = transport;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = givenWeight;
			shipment.JS_UnitOfWeight = unit;
			shipment.JS_ActualVolume = .1;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_INCO = "CIF";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = transport;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var assertion = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode,
					JR_OSCostAmt = expectedCost,
				},
			};

			var costings = new[] { costing };
			var ratesSearchResponse = transport == TransportModes.Air
				? CreateAIRRatesSearchResponse(costings, costings, carrierCode, carrier.OH_FullName)
				: CreateSEARatesSearchResponse(costings, costings, carrierCode, carrier.OH_FullName, carrier.SCACCode);
			var context = new MockRatesServiceContext(ratesSearchResponse);

			AutorateAndAssertRatesService(message, assertion, shipment, context);
		}

		void AutoCostAndAssertRatesService(string message,
			IEnumerable<AssertionCost> expectedCosts,
			IGenericJobCostPlugIn costsSupporter,
			MockRatesServiceContext ratingContext,
			bool expectNoErrorsLogged = true,
			string[] expectedWarnings = null,
			string[] expectedErrors = null,
			DialogResult[] enqueuedAnswers = null,
			RatesServiceRegistrySettingsCollection ratesServiceRegistrySettings = null)
		{
			UnitTestUserNotification.Instance.ClearMessages();
			enqueuedAnswers?.ForEach(a => UnitTestUserNotification.Instance.AddAnswer(a));

			DeleteExistingCosts(costsSupporter.CostSupporter.PK);

			if (ratesServiceRegistrySettings == null)
			{
				ratesServiceRegistrySettings = RatesServiceRegistrySettingsCollection.GetEnabled();
			}

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ratesServiceRegistrySettings))
			using (RatesServiceClientTestHelper.TemporarilyEnsureCurrentUserHasEmail())
			{
				new AutoRatingStarter(new[] { (IBusiness)costsSupporter }, ratingContext).ExecuteAutorating(AutoRateOptions.AutorateCosts.With(billingType: BillingType.Apportionment));

				if (expectNoErrorsLogged)
				{
					AssertContainsExactElementsInAnyOrder("Autorating is supposed to log no Errors", Enumerable.Empty<string>(), ratingContext.TestLogger?.Errors ?? ratingContext.TestInteractor.Errors.Select(x => x.ToString()));
				}

				message = string.Concat(message, "\r\n", ratingContext.TestLogger?.ToString() ?? "Autorating");
			}

			var autoRatingSummary = GetAutoRatingSummary(message, costsSupporter as IStmNoteParent, expectedWarnings, expectedErrors);
			AssertCosts(autoRatingSummary, costsSupporter, expectedCosts);

			(costsSupporter as IBusiness)?.Factory?.Save(); // If we do not save, used objects of ZGlobalMutex are not going to be disposed.
		}

		RatesSearchResponse CreateSEARatesSearchResponse(Rate[] rates, Rate[] ratesForGettingChargeCodes,
			string carrierCode = "Emirates", string carrierFullName = "Transport Provider 1", string scacCode = "SCAC")
			=> Helper.CreateSEARatesSearchResponse(rates, ratesForGettingChargeCodes, carrierCode, carrierFullName, scacCode);

		RatesSearchResponse CreateAIRRatesSearchResponse(Rate[] rates, Rate[] ratesForGettingChargeCodes,
			string carrierCode = "Emirates", string carrierFullName = "Emirates Airlines", string iataCode = "EK")
		{
			return new RatesSearchResponse
			{
				Rates = rates,
				Carriers = new[]
				{
					new RefCarrier { Code = carrierCode, IATACode = iataCode, Name = carrierFullName }
				},
				ChargeCodes = GetChargeCodesFromRates(ratesForGettingChargeCodes)
			};
		}

		static RatesSearchResponse CreateAIRRatesSearchResponse(Rate[] rates, RefChargeCode[] chargeCodes,
			string carrierCode = "Emirates", string carrierFullName = "Emirates Airlines", string iataCode = "EK")
		{
			return new RatesSearchResponse
			{
				Rates = rates,
				Carriers = new[]
				{
					new RefCarrier { Code = carrierCode, IATACode = iataCode, Name = carrierFullName }
				},
				ChargeCodes = chargeCodes
			};
		}

		Rate CreateRate(
			string transport = "AIR",
			string containerMode = "FCL",
			string origin = "UAIEV",
			string destination = "AUSYD",
			string container = "20GP",
			string commodity = "GENL",
			string carrier = "Emirates")
		{
			var refContainer = Factory.LoadTop1<MasterFiles.Business.RefContainer>(new ZQuery(RefContainerSchema.RC_Code, container));

			return new Rate
			{
				Provider = WRConstants.RateProviders.CargoSphere,
				Origin = origin,
				Destination = destination,
				StartDate = DateTime.Now.AddDays(-1000),
				TransportMode = transport,
				ContainerMode = containerMode,
				Container = refContainer != null
					? new RefContainer
					{
						Code = transport == WRConstants.TransportModes.AIR ? refContainer.RC_Code : refContainer.RC_ISOType,
						ISOType = refContainer.RC_ISOType
					}
					: null,
				Commodity = commodity,
				Carrier = carrier,
				Charges = new List<Charge>()
			};
		}

		#endregion
	}
}
