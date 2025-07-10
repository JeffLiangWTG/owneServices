using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.Integration;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Rating.Business.FreightInclusiveCalculator;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using Transport = Enterprise.Freight.Business.Transport;
using ZQuery = CargoWise.EntityFramework.ZQuery;

namespace Enterprise.Rating.Testing.GUI
{
	// Use this file for autorating forwarding shipment integration test
	//
	// Best practices:
	//	1. Use helper methods as much as possible.
	//	2. All test unrelated configuration should be in helper methods. Test specific configuration has to be in the test itself.
	//	3. Tests must be as short as possible including only test related setup and logic so that it is easy to read and maintain.
	//	4. Don't use integration tests as unit tests. Don't create a new integration test for each edge case. All edge cases
	//	   have to be tested in unit tests closer to the logic being tested. For example, if you test some calculator, there
	//	   has to be dozens of unit tests inside calculator testing all possible setups and conditions, and 1 or 2 integration
	//	   tests here testing autorating shipment using this calculator.

	public class AutoratingForwardingShipmentIntegrationTest : BaseRatingIntegrationTest
	{
		[TestDate(2020, 01, 01)]
		public void TestAutorateDateFilter()
		{
			var rateEntry1 = ClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "BAF", 10m, currency: "USD");
			rateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			rateEntry1.TI_RateEndDate = new ZDate(2020, 2, 16);

			var rateEntry2 = ClientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "BAF", 20m, currency: "USD");
			rateEntry2.TI_RateStartDate = new ZDate(2020, 2, 19);
			rateEntry2.TI_RateEndDate = new ZDate(2020, 2, 21);

			var shipment1 = CreateShipment(TransportModes.Sea, ContainerModes.FCL, origin: "AUSYD", destination: "USLAX");
			shipment1.JS_E_DEP = new ZDateTime(2020, 2, 5);
			shipment1.JS_E_ARV = new ZDateTime(2020, 2, 10);

			var consol1 = CreateConsol();
			consol1.Shipments.Add(shipment1);

			var shipment2 = CreateShipment(TransportModes.Sea, ContainerModes.FCL, origin: "AUSYD", destination: "USLAX");
			consol1.Shipments.Add(shipment2);

			AddContainer(consol1, "20GP", "CONT001", packLines: new[] { shipment2.AddPackLine(weight: 10) });
			AddContainer(consol1, "20GP", "CONT002", packLines: new[] { shipment2.AddPackLine(weight: 10) }, fclWharfGateIn: new ZDateTime(2020, 2, 5));
			AddContainer(consol1, "20GP", "CONT003", packLines: new[] { shipment2.AddPackLine(weight: 10) }, fclWharfGateIn: new ZDateTime(2020, 2, 25));
			AddContainer(consol1, "20GP", "CONT004", packLines: new[] { shipment1.AddPackLine(weight: 10) });
			AddContainer(consol1, "20GP", "CONT005", packLines: new[] { shipment1.AddPackLine(weight: 10) }, fclWharfGateIn: new ZDateTime(2020, 2, 15));
			AddContainer(consol1, "20GP", "CONT006", packLines: new[] { shipment1.AddPackLine(weight: 10) }, fclWharfGateIn: new ZDateTime(2020, 2, 20));

			AssertAutorateDateFilter
			(
				shipment1,
				JobInvoicingConsumerTypes.ShipmentCode,
				TransportModes.Sea,
				ChargeCodeGroupList.Codes.Freight,
				JobDateTypes.Codes.FirstContainerGateInDate,
				expectedCharges: new[] { NewAssertionCharge(CostSell.Revenue, "BAF", "BAF: Base Rate USD 10.00", 10m), },
				message: "GIVEN FirstContainerGateInDate THEN should use the first container-gate-in-date"
			);

			AssertAutorateDateFilter
			(
				shipment1,
				JobInvoicingConsumerTypes.ShipmentCode,
				TransportModes.Sea,
				ChargeCodeGroupList.Codes.Freight,
				JobDateTypes.Codes.LastContainerGateInDate,
				expectedCharges: new[] { NewAssertionCharge(CostSell.Revenue, "BAF", "BAF: Base Rate USD 20.00", 20m), },
				message: "GIVEN LastContainerGateInDate THEN should use the last container-gate-in-date"
			);

			CommonContainer AddContainer(ForwardingConsol consol, string containerType, string containerNumber, IEnumerable<PackLine> packLines = null, ZDateTime fclWharfGateIn = default)
			{
				var container = consol.AddContainer(containerType, packLines: packLines);
				container.JC_ContainerNum = containerNumber;
				container.JC_FCLWharfGateIn = fclWharfGateIn;
				return container;
			}

			void AssertAutorateDateFilter(ForwardingShipment shipment, string jobType, string mode, string chargeGroup, string dateType, AssertionCharge[] expectedCharges, string message = default)
			{
				var configuration = new AutoRateDateByChargeGroupConfiguration { FilterType = RatingDateFilterTypes.Codes.Custom, };
				var autoRateDate = new AutoRateDate { JobType = jobType, Mode = mode, DirectionCode = FreightShipmentDirection.Code.All, DateType = dateType };
				var autoRateDateByChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().Single(item => item.ChargeGroup == chargeGroup);
				autoRateDateByChargeGroup.ChargeGroupSettings.Add(autoRateDate);
				AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration);
				AutorateAndAssert(message, expectedCharges, shipment, Consignee, autorateCosts: false, autorateRevenue: true);
			}
		}

		#region Invalid Transport Mode

		public void TestAutoRate_InvalidTransportMode()
		{
			var shipment = CreateShipmenAndJob(transportMode: "???");
			AutorateAndAssert
			(
				"GIVEN shipment with invalid TransportMode '???' WHEN autorate THEN autorate should not run and give error",
				Array.Empty<AssertionCharge>(),
				shipment,
				shipment.Consignor,
				expectedErrors: new[] { "Error Autorating cannot be run because transport '???' is invalid." }
			);
		}

		ForwardingShipment CreateShipmenAndJob(string transportMode)
		{
			var shipment = CreateForwardingShipment(Factory, transportMode, "LCL", Consignor.PK, Consignee.PK, "AUSYD", "USNYK", 100m);

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.JH_OA_LocalChargesAddr = Consignor.MainAddress.PK;

			return shipment;
		}

		public void TestAutoRate_InvalidTransportMode_SpotRate()
		{
			var shipment = CreateShipmenAndJob(transportMode: "???");
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_UnitFreightRate = 10m;
			AutorateAndAssert
			(
				"GIVEN shipment with invalid TransportMode '???' and spotRate WHEN autorate THEN autorate should not run and give error",
				Array.Empty<AssertionCharge>(),
				shipment,
				shipment.Consignor,
				expectedErrors: new[] { "Error Autorating cannot be run because transport '???' is invalid." }
			);
		}

		#endregion

		#region COU OBC UNA

		public void TestAutorateOBC_ClientRate() => AssertAutorateCOU_UNA_OBC(ratingHeader: ClientRate, RateMode.OBC, ContainerModes.OnBoardCourier);

		public void TestAutorateOBC_ClientRateWithCOU() => AssertAutorateCOU_UNA_OBC(ratingHeader: ClientRate, RateMode.COU, ContainerModes.OnBoardCourier);

		public void TestAutorateUNA_ClientRate() => AssertAutorateCOU_UNA_OBC(ratingHeader: ClientRate, RateMode.UNA, ContainerModes.Unaccompanied);

		public void TestAutorateUNA_ClientRateWithCOU() => AssertAutorateCOU_UNA_OBC(ratingHeader: ClientRate, RateMode.COU, ContainerModes.Unaccompanied);

		public void TestAutorateOBC_CompanyTariff() => AssertAutorateCOU_UNA_OBC(ratingHeader: Helper.NewCompanyTariff(), RateMode.OBC, ContainerModes.OnBoardCourier);

		public void TestAutorateUNA_CompanyTariff() => AssertAutorateCOU_UNA_OBC(ratingHeader: Helper.NewCompanyTariff(), RateMode.UNA, ContainerModes.Unaccompanied);

		public void TestAutorateOBC_Costing() => AssertAutorateCOU_UNA_OBC(ratingHeader: Costing, RateMode.OBC, ContainerModes.OnBoardCourier);

		public void TestAutorateUNA_Costing() => AssertAutorateCOU_UNA_OBC(ratingHeader: Costing, RateMode.UNA, ContainerModes.Unaccompanied);

		void AssertAutorateCOU_UNA_OBC(RatingHeader ratingHeader, string rateMode, string shipmentContainerMode)
		{
			var costSell = ratingHeader.IsCosting() ? CostSell.Cost : CostSell.Revenue;
			Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = false; // So COU mode don't filter rate with reason 'charge code is flagged as Consol Level. Consol Level Costs don't apply'
			var expectedCharge = new List<AssertionCharge>();

			if (rateMode != RateMode.COU)
			{
				ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX", "BAF", 10m, currency: "USD");
				ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, rateMode, "AUSYD", "USLAX", "BAF", 20m, currency: "USD");
				expectedCharge.Add(NewAssertionCharge(costSell, "BAF", "BAF: Base Rate USD 20.00", 20m));
			}

			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUSYD", "USLAX", "BAF", 30m, currency: "USD");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "BAF", 70m, currency: "USD");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, rateMode, "AUSYD", "USLAX", "BAF", 40m, currency: "USD");
			expectedCharge.Add(NewAssertionCharge(costSell, "BAF", "BAF: Base Rate USD 40.00", 40m));

			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LCL, "AUSYD", "USLAX", "BAF", 50m, currency: "USD");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "AUSYD", "USLAX", "BAF", 80m, currency: "USD");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, rateMode, "AUSYD", "USLAX", "BAF", 60m, currency: "USD");
			expectedCharge.Add(NewAssertionCharge(costSell, "BAF", "BAF: Base Rate USD 60.00", 60m));

			var shipment = CreateShipment(transportMode: "COU", containerMode: shipmentContainerMode, origin: "AUSYD", destination: "USLAX");
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			ratingHeader.Factory.Save();

			AutorateAndAssert
			(
				expectedCharge,
				shipment,
				Consignee,
				autorateCosts: costSell == CostSell.Cost,
						autorateRevenue: costSell == CostSell.Revenue
			);
		}

		public void TestAutorateOBC_InterCompanyTariff() => AssertAutorateCOU_InterCompanyTariff(RateMode.OBC, ContainerModes.OnBoardCourier);

		public void TestAutorateUNA_InterCompanyTariff() => AssertAutorateCOU_InterCompanyTariff(RateMode.UNA, ContainerModes.Unaccompanied);

		void AssertAutorateCOU_InterCompanyTariff(string rateMode, string shipmentContainerMode)
		{
			var chargeCode = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			chargeCode.AC_IsGroupageCharge = false;
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;

			var tariff = Helper.NewIntercompanyTariff(orgProxy);

			tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, rateMode, "AUMEL", "USLAX", "FRT1", 200);
			tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUMEL", "USLAX", "FRT1", 200);
			tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, rateMode, "AUMEL", "USLAX", "FRT1", 100);
			tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUMEL", "USLAX", "FRT1", 100);
			var shipment = CreateStandaloneGatewayShipment("AUMEL", "USLAX", "COU", shipmentContainerMode, orgProxy.PK);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					CostAccountCode = orgProxy.OH_Code,
					JR_OSCostAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					CostAccountCode = orgProxy.OH_Code,
					JR_OSCostAmt = 100m
				}
			};

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert(expected, shipment, Consignor, autorateRevenue: false);
			}
		}

		#endregion

		#region Autorate BBK BLK ROR

		public void TestAutorateBBK_ClientRate() => AssertAutorateBBK_BLK_ROR(ratingHeader: ClientRate, RateMode.BBK, ContainerModes.BreakBulk);

		public void TestAutorateBLK_ClientRate() => AssertAutorateBBK_BLK_ROR(ratingHeader: ClientRate, RateMode.BLK, ContainerModes.Bulk);

		public void TestAutorateROR_ClientRate() => AssertAutorateBBK_BLK_ROR(ratingHeader: ClientRate, RateMode.ROR, ContainerModes.RollOnRollOff);

		public void TestAutorateBBK_CompanyTariff() => AssertAutorateBBK_BLK_ROR(ratingHeader: Helper.NewCompanyTariff(), RateMode.BBK, ContainerModes.BreakBulk);

		public void TestAutorateBLK_CompanyTariff() => AssertAutorateBBK_BLK_ROR(ratingHeader: Helper.NewCompanyTariff(), RateMode.BLK, ContainerModes.Bulk);

		public void TestAutorateROR_CompanyTariff() => AssertAutorateBBK_BLK_ROR(ratingHeader: Helper.NewCompanyTariff(), RateMode.ROR, ContainerModes.RollOnRollOff);

		public void TestAutorateBBK_Costing() => AssertAutorateBBK_BLK_ROR(Costing, RateMode.BBK, ContainerModes.BreakBulk);

		public void TestAutorateBLK_Costing() => AssertAutorateBBK_BLK_ROR(Costing, RateMode.BLK, ContainerModes.Bulk);

		public void TestAutorateROR_Costing() => AssertAutorateBBK_BLK_ROR(Costing, RateMode.ROR, ContainerModes.RollOnRollOff);

		void AssertAutorateBBK_BLK_ROR(RatingHeader ratingHeader, string rateMode, string shipmentContainerMode)
		{
			Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = false; // So BBK ROR mode don't filter rate with reason 'charge code is flagged as Consol Level. Consol Level Costs don't apply'

			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX", "BAF", 10m, currency: "USD");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, rateMode, "AUSYD", "USLAX", "BAF", 20m, currency: "USD");

			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUSYD", "USLAX", "BAF", 30m, currency: "USD");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, rateMode, "AUSYD", "USLAX", "BAF", 40m, currency: "USD");

			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LCL, "AUSYD", "USLAX", "BAF", 50m, currency: "USD");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, rateMode, "AUSYD", "USLAX", "BAF", 60m, currency: "USD");

			var shipment = CreateShipment(containerMode: shipmentContainerMode, origin: "AUSYD", destination: "USLAX");
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			ratingHeader.Factory.Save();

			AssertAutorateBBK_BLK_ROR
			(
				shipment,
				rateMode,
				shipmentContainerMode,
				costSell: ratingHeader.IsCosting()
					? CostSell.Cost
					: CostSell.Revenue
			);
		}

		void AssertAutorateBBK_BLK_ROR(ForwardingShipment shipment, string rateMode, string shipmentContainerMode, CostSell costSell)
		{
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutorateAndAssert
				(
					expected: new[]
					{
						NewAssertionCharge(costSell, "BAF", "BAF: Base Rate USD 10.00", 10m),
						NewAssertionCharge(costSell, "BAF", "BAF: Base Rate USD 30.00", 30m),
						NewAssertionCharge(costSell, "BAF", "BAF: Base Rate USD 50.00", 50m)
					},
					shipment,
					Consignee,
					autorateCosts: costSell == CostSell.Cost,
					autorateRevenue: costSell == CostSell.Revenue
				);
			}

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert
				(
					expected: new[]
					{
						NewAssertionCharge(costSell, "BAF", "BAF: Base Rate USD 20.00", 20m),
						NewAssertionCharge(costSell, "BAF", "BAF: Base Rate USD 40.00", 40m),
						NewAssertionCharge(costSell, "BAF", "BAF: Base Rate USD 60.00", 60m)
					},
					shipment,
					Consignee,
					autorateCosts: costSell == CostSell.Cost,
					autorateRevenue: costSell == CostSell.Revenue
				);
			}
		}

		static AssertionCharge NewAssertionCharge(CostSell costSell, ZString chargeCode, ZString description, ZDecimal amount)
		{
			return costSell == CostSell.Cost
				? new AssertionCharge { ChargeCode = chargeCode, JR_OSCostAmt = amount, CostCalculationDescription = description }
				: new AssertionCharge { ChargeCode = chargeCode, JR_OSCostAmt = amount, RevenueCalculationDescription = description };
		}

		public void TestAutorateNonBBK_BLK_ROR()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX", "BAF", 10m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUSYD", "USLAX", "BAF", 30m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LCL, "AUSYD", "USLAX", "BAF", 50m, currency: "USD");
			var shipment = CreateShipment(containerMode: ContainerModes.LCL, origin: "AUSYD", destination: "USLAX");

			Factory.Save();

			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutorateAndAssert
				(
					expected: new[]
					{
						new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 10m, RevenueCalculationDescription = "BAF: Base Rate USD 10.00" },
						new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 30m, RevenueCalculationDescription = "BAF: Base Rate USD 30.00" },
						new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 50m, RevenueCalculationDescription = "BAF: Base Rate USD 50.00" }
					},
					shipment,
					Consignee,
					autorateCosts: false
				);
			}

			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert
				(
					expected: new[]
					{
						new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 10m, RevenueCalculationDescription = "BAF: Base Rate USD 10.00" },
						new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 30m, RevenueCalculationDescription = "BAF: Base Rate USD 30.00" },
						new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 50m, RevenueCalculationDescription = "BAF: Base Rate USD 50.00" }
					},
					shipment,
					Consignee,
					autorateCosts: false
				);
			}
		}

		#endregion

		#region PerUnitCalculator

		public void TestPerUnitCalculator_WeightVolumeUnit_ShouldCalculateUsingShipmentMeasures()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;
			Helper.ChargeCodes["OFUMI"].AC_IsGroupageCharge = false;

			CreateCostRate(container: "20GP", commodity: "ALUM").AddPerUnitCharge("FRT", 100, "KG");
			CreateCostRate(container: "20GP", commodity: "SALT").AddPerUnitCharge("FRT", 200, "KG");
			CreateCostRate(container: "40GP", commodity: "ALUM").AddPerUnitCharge("FRT", 300, "KG");
			CreateCostRate(container: "40GP", commodity: "SALT").AddPerUnitCharge("FRT", 400, "KG");
			CreateCostRate(category: "DST", mode: "ALL", container: null, commodity: null).AddPerUnitCharge("DDOC", 20, "KG", unitFactor: UnitFactorList.Codes.InnerPack);
			CreateCostRate(category: "DST", mode: "ALL", container: null, commodity: "GEN").AddPerUnitCharge("DDOC", 40, "KG", unitFactor: UnitFactorList.Codes.InnerPack);
			CreateCostRate(category: "DST", mode: "ALL", container: null, commodity: "SALT").AddPerUnitCharge("DDOC", 50, "KG", unitFactor: UnitFactorList.Codes.InnerPack);
			CreateCostRate(category: "ORG", mode: "FCL", container: "40GP", commodity: null).AddPerUnitCharge("OFUMI", 100, "KG");

			var shipment = CreateShipment();
			shipment.JS_INCO = ZString.Empty;
			var service = shipment.Services.AddNew();
			service.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDate.Today;
			service.ES_ServiceCount = 1;

			var anotherShipment = CreateShipment();

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(anotherShipment);

			consol.AddContainer("20GP", "SALT", packLines: new[]
			{
				shipment.AddPackLine(commodity: "SALT", weight: 30, innerPacks: new []
				{
					shipment.AddInnerPackLine(weight: 10),
					shipment.AddInnerPackLine(weight: 15)
				}),
				shipment.AddPackLine(commodity: "SALT", weight: 40),
				shipment.AddPackLine(commodity: "ALUM", weight: 200, innerPacks: new []
				{
					shipment.AddInnerPackLine(weight: 150)
				}),
				shipment.AddPackLine(commodity: null, weight: 100, innerPacks: new []
				{
					shipment.AddInnerPackLine(weight: 90)
				}),
			});
			consol.AddContainer("40GP", "ALUM", packLines: new[]
			{
				shipment.AddPackLine(commodity: "SALT", weight: 50, innerPacks: new []
				{
					shipment.AddInnerPackLine(weight: 20),
					shipment.AddInnerPackLine(weight: 10)
				}),
				anotherShipment.AddPackLine(commodity: "ALUM", weight: 150),
				anotherShipment.AddPackLine(commodity: "GEN", weight: 250, innerPacks: new []
				{
					anotherShipment.AddInnerPackLine(weight: 200)
				}),
			});
			consol.AddContainer("40GP", "SALT", packLines: new[]
			{
				anotherShipment.AddPackLine(commodity: "SALT", weight: 666, innerPacks: new []
				{
					anotherShipment.AddInnerPackLine(weight: 600)
				}),
				anotherShipment.AddPackLine(commodity: "ALUM", weight: 999),
			});

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// 30 KG + 40 KG of SALT packed in 20 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 14000.00m,
					CostCalculationDescription = "FRT: 70 Kilogram(s) @ USD 200.00/KG"
				},
				new AssertionCharge
				{
					// 200 KG of ALUM packed in 20 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 20000.00m,
					CostCalculationDescription = "FRT: 200 Kilogram(s) @ USD 100.00/KG"
				},
				new AssertionCharge
				{
					// 50 KG of SALT packed in 40 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 20000.00m,
					CostCalculationDescription = "FRT: 50 Kilogram(s) @ USD 400.00/KG"
				},
				new AssertionCharge
				{
					// 90 KG from inner packs with empty commodity using a rate with GEN commodity since the pack has no commodity
					ChargeCode = "DDOC",
					JR_OSCostAmt = 3600m,
					CostCalculationDescription = "DDOC: 90 Kilogram(s) @ AUD 40.00/KG"
				},
				new AssertionCharge
				{
					// 150 KG of ALUM in 20 GP in Inner Packs using a rate with empty commodity since there are no rate for ALUM
					ChargeCode = "DDOC",
					JR_OSCostAmt = 3000m,
					CostCalculationDescription = "DDOC: 150 Kilogram(s) @ AUD 20.00/KG"
				},
				new AssertionCharge
				{
					// 25 KG of SALT in 20GP + 30 KG of SALT in 40GP in Inner Packs using SALT commodity rate
					ChargeCode = "DDOC",
					JR_OSCostAmt = 2750m,
					CostCalculationDescription = "DDOC: 55 Kilogram(s) @ AUD 50.00/KG"
				},
				new AssertionCharge
				{
					// 50 KG from 40 GP container in outer pack lines
					ChargeCode = "OFUMI",
					JR_OSCostAmt = 5000m,
					CostCalculationDescription = "OFUMI: 50 Kilogram(s) @ UAH 100.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestPerUnitCalculator_PackTypeUnit_ShouldCalculateUsingShipmentMeasures()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			CreateCostRate(container: "20GP")
				.AddPerUnitCharge("FRT", 100, PkgUnit.Box)
				.AddPerUnitCharge("FRT", 200, PkgUnit.Pallet);

			CreateCostRate(container: "40GP")
				.AddPerUnitCharge("FRT", 300, PkgUnit.Box)
				.AddPerUnitCharge("FRT", 400, PkgUnit.Bag);

			CreateCostRate(category: "DST", mode: "ALL", commodity: null)
				.AddPerUnitCharge("DDOC", 5, unit: PkgUnit.Pallet, unitFactor: UnitFactorList.Codes.InnerPack)
				.AddPerUnitCharge("DDOC", 10, unit: PkgUnit.Box, unitFactor: UnitFactorList.Codes.InnerPack);

			CreateCostRate(category: "DST", mode: "ALL", commodity: "GEN")
				.AddPerUnitCharge("DDOC", 15, unit: PkgUnit.Pallet, unitFactor: UnitFactorList.Codes.InnerPack)
				.AddPerUnitCharge("DDOC", 20, unit: PkgUnit.Box, unitFactor: UnitFactorList.Codes.InnerPack);

			CreateCostRate(category: "DST", mode: "ALL", commodity: "SALT")
				.AddPerUnitCharge("DDOC", 25, unit: PkgUnit.Pallet, unitFactor: UnitFactorList.Codes.InnerPack)
				.AddPerUnitCharge("DDOC", 30, unit: PkgUnit.Box, unitFactor: UnitFactorList.Codes.InnerPack);

			var shipment = CreateShipment();
			var anotherShipment = CreateShipment();

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(anotherShipment);

			consol.AddContainer("20GP", packLines: new[]
			{
				shipment.AddPackLine(commodity: "SALT", packType: PkgUnit.Box, count: 10, innerPacks: new []
				{
					shipment.AddInnerPackLine(count: 20, packType: PkgUnit.Box),
					shipment.AddInnerPackLine(count: 5, packType: PkgUnit.Pallet)
				}),
				shipment.AddPackLine(commodity: "ALUM", packType: PkgUnit.Pallet, count: 20, innerPacks: new []
				{
					shipment.AddInnerPackLine(count: 10, packType: PkgUnit.Box),
					shipment.AddInnerPackLine(count: 20, packType: PkgUnit.Pallet),
				}),
				anotherShipment.AddPackLine(commodity: "ALUM", packType: PkgUnit.Box, count: 5, innerPacks: new []
				{
					anotherShipment.AddInnerPackLine(count: 50, packType: PkgUnit.Box),
					anotherShipment.AddInnerPackLine(count: 10, packType: PkgUnit.Pallet)
				})
			});

			consol.AddContainer("40GP", packLines: new[]
			{
				shipment.AddPackLine(commodity: "SALT", packType: PkgUnit.Box, count: 30, innerPacks: new []
				{
					shipment.AddInnerPackLine(count: 40, packType: PkgUnit.Box),
					shipment.AddInnerPackLine(count: 30, packType: PkgUnit.Pallet),
					shipment.AddInnerPackLine(count: 20, packType: PkgUnit.Carton)	// DDOC $30 per Carton
				}),
				shipment.AddPackLine(commodity: "ALUM", packType: PkgUnit.Pallet, count: 40)
			});

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// 20 PLT + 10 BOX in 20 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 5000.00m,
					CostCalculationDescription = @"This charge is calculated from multiple rates
FRT: 20 Pallet(s) @ USD 200.00/Pallet
FRT: 10 Box(s) @ USD 100.00/Box"
				},
				new AssertionCharge
				{
					// 30 BOX in 40 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 9000.00m,
					CostCalculationDescription = "FRT: 30 Box(s) @ USD 300.00/Box"
				},
				new AssertionCharge
				{
					// We calculate all pallets and boxes from all inner packs regardless of container type and commodity.
					// We ignore container type because inner pack lines are not related to concrete container.
					// We ignore commodity because historically we don't match units by container, so, a rate with GEN commodity
					// is used. But this may be a bug, don't see any reason to not use commodities like we do for per Package rates.
					ChargeCode = "DDOC",
					JR_OSCostAmt = 2225m,
					CostCalculationDescription = @"This charge is calculated from multiple rates
DDOC: 55 Pallet(s) @ AUD 15.00/Pallet
DDOC: 70 Box(s) @ AUD 20.00/Box"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestPerUnitCalculator_PackageUnit_ShouldCalculateUsingShipmentMeasures()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			CreateCostRate(container: "20GP", commodity: "SALT").AddPerUnitCharge("FRT", 100, "PK");
			CreateCostRate(container: "40GP", commodity: null).AddPerUnitCharge("FRT", 200, "PK");
			CreateCostRate(category: "DST", mode: "ALL", container: null, commodity: null)
				.AddPerUnitCharge("DDOC", 20, unit: "PK", unitFactor: UnitFactorList.Codes.InnerPack)
				.AddPerUnitCharge("DCART", 20, unit: "PK", unitFactor: UnitFactorList.Codes.InnerPack);
			CreateCostRate(category: "DST", mode: "ALL", container: null, commodity: "GEN")
				.AddPerUnitCharge("DCART", 30, unit: "PK", unitFactor: UnitFactorList.Codes.InnerPack);
			CreateCostRate(category: "DST", mode: "ALL", container: null, commodity: "SALT")
				.AddPerUnitCharge("DDOC", 40, unit: "PK", unitFactor: UnitFactorList.Codes.InnerPack)
				.AddPerUnitCharge("DCART", 40, unit: "PK", unitFactor: UnitFactorList.Codes.InnerPack);
			CreateCostRate(category: "DST", mode: "ALL", container: null, commodity: "ALUM")
				.AddPerUnitCharge("DDOC", 50, unit: "PK", unitFactor: UnitFactorList.Codes.InnerPack)
				.AddPerUnitCharge("DCART", 50, unit: "PK", unitFactor: UnitFactorList.Codes.InnerPack);

			var shipment = CreateShipment();
			var anotherShipment = CreateShipment();

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(anotherShipment);

			consol.AddContainer("20GP", packLines: new[]
			{
				shipment.AddPackLine(commodity: "SALT", packType: PkgUnit.Box, count: 10),
				shipment.AddPackLine(commodity: "ALUM", packType: PkgUnit.Pallet, count: 20),
				anotherShipment.AddPackLine(commodity: "ALUM", packType: PkgUnit.Box, count: 5)
			});

			consol.AddContainer("40GP", packLines: new[]
			{
				shipment.AddPackLine(commodity: "SALT", packType: PkgUnit.Box, count: 30),
				shipment.AddPackLine(commodity: "ALUM", packType: PkgUnit.Pallet, count: 40)
			});

			shipment.AddInnerPackLine(count: 10, packType: "BOX");
			shipment.AddInnerPackLine(count: 15, packType: "BAG");
			shipment.AddInnerPackLine(count: 20, packType: "PLT");
			anotherShipment.AddInnerPackLine(count: 30, packType: "BOX");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// 10 Packages of SALT in 20 GP Container (the rate is per SALT packages in 20GP container)
					ChargeCode = "FRT",
					JR_OSCostAmt = 1000.00m,
					CostCalculationDescription = "FRT: 10 Package(s) @ USD 100.00/Package"
				},
				new AssertionCharge
				{
					// 30 Packages of SALT + 40 Packages of ALUM in 40 GP Container (the rate is per packages in 40GP container)
					ChargeCode = "FRT",
					JR_OSCostAmt = 14000.00m,
					CostCalculationDescription = "FRT: 70 Package(s) @ USD 200.00/Package"
				},
				new AssertionCharge
				{
					// Inner Pack Lines are unlinked and don't have commodities.  Normally we would use GEN rate to calculate this parts, but GEN rate doesn't
					// for ODOC, and other rates for other commodities have the same match score and thus autorating can't decide which one to use, so, it
					// is autorated with 0 amount and the user asked to enter amount manually.
					ChargeCode = "DDOC",
					JR_OSCostAmt = 0m,
					CostCalculationDescription = @"DDOC: Base Rate AUD 0.00"
				},
				new AssertionCharge
				{
					// 45 Packages form Inner Pack Lines using rate with GEN commodity since unlinked inner pack lines don't have commodities
					// and thus a rate with GEN commodity wins over rates with SALT and ALUM commodities
					ChargeCode = "DCART",
					JR_OSCostAmt = 1350m,
					CostCalculationDescription = @"DCART: 45 Package(s) @ AUD 30.00/Package"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestPerUnitCalculator_ContainerUnit_ShouldCalculateUsingConsolMeasures()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			CreateCostRate(container: "20GP", commodity: "ALUM").AddPerUnitCharge("FRT", 1000, "CN");
			CreateCostRate(container: "20GP", commodity: "SALT").AddPerUnitCharge("FRT", 2000, "CN");
			CreateCostRate(container: "40GP", commodity: "ALUM").AddPerUnitCharge("FRT", 3000, "CN");
			CreateCostRate(container: "40GP", commodity: "SALT").AddPerUnitCharge("FRT", 4000, "CN");

			var shipment = CreateShipment();
			var anotherShipment = CreateShipment();

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(anotherShipment);

			// Should use 20 GP SALT rate since the shipment is packed in this container
			consol.AddContainer("20GP", "SALT", count: 2, packLines: new[]
			{
				shipment.AddPackLine(commodity: "SALT", weight: 30),
				shipment.AddPackLine(commodity: "ALUM", weight: 200)
			});

			// Should use 40 GP ALUM rate since the shipment is packed in this container
			consol.AddContainer("40GP", "ALUM", number: "AA1111111", packLines: new[]
			{
				shipment.AddPackLine(commodity: "SALT", weight: 50),
				anotherShipment.AddPackLine(commodity: "ALUM", weight: 150),
			});

			// Should use 40 GP ALUM rate since the shipment is packed in this container
			consol.AddContainer("40GP", "ALUM", number: "BB2222222", packLines: new[]
			{
				shipment.AddPackLine(commodity: "ALUM", weight: 250),
			});

			// Should not be rated as the shipment is not packed in this container
			consol.AddContainer("40GP", "ALUM", number: "CC333333", packLines: new[]
			{
				anotherShipment.AddPackLine(commodity: "ALUM", weight: 500),
			});

			// Should not be rated as the shipment is not packed in this container
			consol.AddContainer("40GP", "SALT", packLines: new[]
			{
				anotherShipment.AddPackLine(commodity: "SALT", weight: 666),
				anotherShipment.AddPackLine(commodity: "ALUM", weight: 999),
			});

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// 2 20GP SALT Container as the shipment is packed in this container
					ChargeCode = "FRT",
					JR_OSCostAmt = 4000.00m,
					CostCalculationDescription = "FRT: 2 20GP Container(s) @ USD 2000.00/Container",
				},
				new AssertionCharge
				{
					// 2 40GP ALUM Containers (AA1111111 and BB2222222) as the shipment is packed in this container
					ChargeCode = "FRT",
					JR_OSCostAmt = 6000.00m,
					CostCalculationDescription = "FRT: 2 40GP Container(s) @ USD 3000.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		#endregion

		#region Flat Calculator

		public void TestFlatCalculator_CommodityMatching_ConsolAndShipmentHaveCommodity_ShouldMatchBoth()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;
			Helper.ChargeCodes["CAF"].AC_IsGroupageCharge = false;
			Helper.ChargeCodes["BAF"].AC_IsGroupageCharge = false;

			CreateCostRate(container: "20GP", commodity: "GEN").AddFlatCharge("FRT", 1000);     // Matches consol commodity
			CreateCostRate(container: "20GP", commodity: "HAZ").AddFlatCharge("CAF", 2000);     // Matches shipment commodity
			CreateCostRate(container: "20GP", commodity: "SALT").AddFlatCharge("BAF", 3000);
			CreateCostRate(container: "20GP", commodity: null).AddFlatCharge("FRT", 1000);

			var shipment = CreateShipment();
			var anotherShipment = CreateShipment();

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(anotherShipment);
			consol.AddContainer("20GP", "GEN", packLines: new[]
			{
				shipment.AddPackLine(commodity: "HAZ", weight: 30),
				anotherShipment.AddPackLine(commodity: "SALT", weight: 150),
			});

			shipment.AddInnerPackLine(weight: 100);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1000.00m,
					CostCalculationDescription = "FRT: Base Rate USD 1000.00"
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSCostAmt = 2000.00m,
					CostCalculationDescription = "CAF: Base Rate USD 2000.00"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestFlatCalculator_CommodityMatching_ConsolCommodityIsEmpty_ShouldFallbackToShipmentCommodity()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			CreateCostRate(container: "20GP", commodity: "GEN").AddFlatCharge("FRT", 1000);
			CreateCostRate(container: "20GP", commodity: "HAZ").AddFlatCharge("FRT", 2000);     // Matches shipment commodity
			CreateCostRate(container: "20GP", commodity: "SALT").AddFlatCharge("FRT", 3000);

			var shipment = CreateShipment();
			var anotherShipment = CreateShipment();

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(anotherShipment);
			consol.AddContainer("20GP", "", packLines: new[]
			{
				shipment.AddPackLine(commodity: "HAZ", weight: 30),
				anotherShipment.AddPackLine(commodity: "SALT", weight: 150),
			});

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 2000.00m,
					CostCalculationDescription = "FRT: Base Rate USD 2000.00"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestFlatCalculator_CommodityMatching_NotLinkedInnerPacks_ShouldIgnoreInnerPacksCommodity()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			CreateClientRate(category: "ORG", mode: "LCL", container: null, commodity: "GEN").AddFlatCharge("ODOC", 1000);
			CreateClientRate(category: "ORG", mode: "LCL", container: null, commodity: "HAZ").AddFlatCharge("ODOC", 2000);

			var shipment = CreateShipment(containerMode: "LCL");
			shipment.AddPackLine(commodity: "HAZ", weight: 30);
			shipment.AddInnerPackLine(weight: 25);

			Factory.Save();

			var expected = new[]
			{
				// Normally, if a job has HAZ commodity and empty/GEN commodity, we would use a rate with GEN commodity. But inner packs don't have commodity at all
				// and thus their "empty" commodity should not count. It only counts if an inner pack line is linked to an outer pack line, but in this case it still
				// doesn't have its own commodity but inherits the one from the linked outer pack line.
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 2000.00m,
					RevenueCalculationDescription = "ODOC: Base Rate UAH 2000.00"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: true);
		}

		public void TestFlatCalculator_FCLRateLines_PrioritizeEmptyContainer()
		{
			var entry20GP = CreateCostRate(category: "FCL", container: "20GP");
			var entry40GP = CreateCostRate(category: "FCL", container: "40GP");
			var entryEmpty = CreateCostRate(category: "FCL", container: "");

			entry20GP.AddFlatCharge("FRT", 10);
			entry40GP.AddFlatCharge("FRT", 15);
			entryEmpty.AddFlatCharge("FRT", 12);

			entry20GP.AddFlatCharge("CAF", 30);
			entry40GP.AddFlatCharge("WAR", 40);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP");
			consol.AddContainer("40GP");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 12.00m,
					CostCalculationDescription = "FRT: Base Rate USD 12.00"
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSCostAmt = 30.00m,
					CostCalculationDescription = "CAF: Base Rate USD 30.00"
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					JR_OSCostAmt = 40.00m,
					CostCalculationDescription = "WAR: Base Rate USD 40.00"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		#endregion

		#region CostBasedCalculator

		/// <summary>
		/// 	This is a regression test testing a case when multiple costs from CargoSphere exists for different containers and
		/// 	multiple client rates per container with CST calculator exist in the system. All client rates should come through
		/// 	and be calculated based on the corresponding matching cost charge. Before the fix, two client rates would be filtered
		/// 	out with the similarity check.
		/// </summary>
		public void TestCSTCalculator_MultipleLinesWithPerUnitIncreaseAndMultiplePerUnitExistingCharges_ShouldApplyIncreasesToCorrespondingCosts()
		{
			var rate1 = CreateClientRate(container: "20GP").AddRateLine("FRT", "CST");
			rate1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 1;

			var rate2 = CreateClientRate(container: "40GP").AddRateLine("FRT", "CST");
			rate2.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 2;

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", packLines: new[] { shipment.AddPackLine(weight: 100) });
			consol.AddContainer("40GP", packLines: new[] { shipment.AddPackLine(weight: 200) });

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				job.AddPerUnitCharge("FRT", 100, "KG", costRate: 10, containerType: "20GP");
				job.AddPerUnitCharge("FRT", 200, "KG", costRate: 20, containerType: "40GP");

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 1000.00m,
						JR_OSSellAmt = 1100.00m,
						RevenueCalculationDescription = "FRT: 100 Kilogram(s) @ USD 11.00/KG"
					},
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSCostAmt = 4000.00m,
						JR_OSSellAmt = 4400.00m,
						RevenueCalculationDescription = "FRT: 200 Kilogram(s) @ USD 22.00/KG"
					}
				};

				AutorateAndAssert(expected, shipment, Consignee, job: job, autorateCosts: true, autorateRevenue: true);
			}
		}

		public void TestCSTCalculator_CostIsCalculatedWithPerUnitRateWithMultiply_ShouldApplyPerUnitIncreaseConsideringUnitMultiply()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			var costRate = CreateCostRate(category: "LCL", mode: "LCL", container: null).AddPerUnitCharge("FRT", 1000, "KG");
			costRate.TL_WeightVolumeMultiple = 2000;

			var clientRate = CreateClientRate(category: "LCL", mode: "LCL", container: null).AddRateLine("FRT", "CST");
			clientRate.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 500;

			var shipment = CreateShipment(containerMode: "LCL");
			shipment.JS_ActualWeight = 6000;
			shipment.JS_UnitOfWeight = "KG";

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 3000.00m,
					JR_OSSellAmt = 4500.00m,
					CostCalculationDescription = "FRT: 6000 Kilogram(s) @ USD 1000.00/2000 KG",
					RevenueCalculationDescription = "FRT: 6000 Kilogram(s) @ USD 1500.00/2000 KG"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee);
		}

		public void TestCSTCalculator_RatingConsolCostThenShipmentRevenue_OneShipment_ShouldApplyPerUnitIncrease()
		{
			const string origin = "AUSYD";
			const string dest = "USLAX";
			var costEntry = CreateCostRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var costLine = costEntry.AddRateLine(Helper.ChargeCodes["FRT"], CombinedCalculator.Code, Weight.Kilograms, "AUD");
			var costLineCalc = costLine.GetCalculator<CombinedCalculator>();
			costLineCalc["-100"] = (ZDecimal)3.1;
			costLineCalc["+100"] = (ZDecimal)1.25;
			costLineCalc.Minimum = 50m;
			costLine.RateLineItems[0].TM_BreakWeightVolume = QuantityUnit.KG;

			costLine.SetViewAgentRatesWithoutRefreshBinding(true);
			costLineCalc["-100"] = (ZDecimal)3.5;
			costLineCalc["+100"] = (ZDecimal)1.5;
			costLine.SetViewAgentRatesWithoutRefreshBinding(false);

			var revenueEntry = CreateClientRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var revenueLine = revenueEntry.AddRateLine(Helper.ChargeCodes["FRT"], CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var calc = revenueLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calc.PerUnit = 0.5;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(true);
			calc.PerUnit = 0.5;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(false);

			var shipment = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment.JS_ActualWeight = 500;
			shipment.JS_UnitOfWeight = Weight.Kilograms;

			var consol = CreateConsol(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			consol.Shipments.Add(shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			// Rate consol costs only, not revenue
			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 625,
						CostCalculationDescription = "FRT: 500 Kilogram(s) @ AUD 1.25/KG",
					}
			};

			AutoCostAndAssert("Consol cost", null, expectedCosts, consol, autorateRevenue: false);

			// Shipment revenue
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 625m,
					JR_OSSellAmt = 875m,
					JR_AgentDeclaredCostAmt = 750m, // 500 * 1.5
					JR_AgentDeclaredSellAmt = 1000m, // 500 * 2.0
					CostCalculationDescription = "FRT: 500 Kilogram(s) @ AUD 1.25/KG",
					RevenueCalculationDescription = "FRT: 500 Kilogram(s) @ AUD 1.75/KG"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee);
		}

		public void TestCSTCalculator_RatingConsolCostThenShipmentRevenue_MultipleShipments_PerUnitRate()
		{
			const string origin = "AUSYD";
			const string dest = "USLAX";
			var costEntry = CreateCostRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var costLine = costEntry.AddRateLine(Helper.ChargeCodes["FRT"], CombinedCalculator.Code, Weight.Kilograms, "AUD");
			var costLineCalc = costLine.GetCalculator<CombinedCalculator>();
			costLineCalc["-100"] = (ZDecimal)3.1;
			costLineCalc["+100"] = (ZDecimal)1.25;
			costLineCalc.Minimum = 50m;
			costLine.RateLineItems[0].TM_BreakWeightVolume = QuantityUnit.KG;

			costLine.SetViewAgentRatesWithoutRefreshBinding(true);
			costLineCalc["-100"] = (ZDecimal)3.5;
			costLineCalc["+100"] = (ZDecimal)1.5;
			costLine.SetViewAgentRatesWithoutRefreshBinding(false);

			var revenueEntry = CreateClientRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var revenueLine = revenueEntry.AddRateLine(Helper.ChargeCodes["FRT"], CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var calc = revenueLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calc.PerUnit = 0.5;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(true);
			calc.PerUnit = 0.5;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(false);

			// Shipments with different weights...
			var shipment1 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment1.JS_ActualWeight = 200;
			shipment1.JS_UnitOfWeight = Weight.Kilograms;

			var shipment2 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment2.JS_ActualWeight = 800;
			shipment2.JS_UnitOfWeight = Weight.Kilograms;

			var consol = CreateConsol(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			// Rate consol costs only, not revenue
			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 1250,
						CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 1.25/KG",
					}
			};

			AutoCostAndAssert("Consol cost", null, expectedCosts, consol, autorateRevenue: false);

			// Shipment 1 revenue
			var expected1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 250m,
					JR_OSSellAmt = 350m,
					JR_AgentDeclaredCostAmt = 300m,
					JR_AgentDeclaredSellAmt = 400m,
					RevenueCalculationDescription = "FRT: 200 Kilogram(s) @ AUD 1.75/KG"
				}
			};

			AutorateAndAssert(expected1, shipment1, Consignee);

			// Shipment 2 revenue
			var expected2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1000m,
					JR_OSSellAmt = 1400m,
					JR_AgentDeclaredCostAmt = 1200m,
					JR_AgentDeclaredSellAmt = 1600m,
					RevenueCalculationDescription = "FRT: 800 Kilogram(s) @ AUD 1.75/KG"
				}
			};

			AutorateAndAssert(expected2, shipment2, Consignee);
		}

		public void TestCSTCalculator_RatingConsolCostThenShipmentRevenue_MultipleShipments_PerUnitWithMultiplier()
		{
			const string origin = "AUSYD";
			const string dest = "USLAX";
			var costEntry = CreateCostRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var costLine = costEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, Weight.Kilograms, "AUD");
			costLine.TL_WeightVolumeMultiple = 100m;
			costLine.TL_Rounding = RatingRoundingTypes.NoRounding;

			// $150 per 100KG ($200 for agent)
			var costLineCalc = costLine.GetCalculator<UnitCalculator>();
			costLineCalc.PerUnit = 150m;
			costLine.SetViewAgentRatesWithoutRefreshBinding(true);
			costLineCalc.PerUnit = 200m;
			costLine.SetViewAgentRatesWithoutRefreshBinding(false);

			// $+10 per unit ($15 for agent)
			var revenueEntry = CreateClientRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var revenueLine = revenueEntry.AddRateLine(Helper.ChargeCodes["FRT"], CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var calc = revenueLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calc.PerUnit = 10;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(true);
			calc.PerUnit = 15;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(false);

			// Shipments with different weights...
			var shipment1 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment1.JS_ActualWeight = 200;
			shipment1.JS_UnitOfWeight = Weight.Kilograms;

			var shipment2 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment2.JS_ActualWeight = 800;
			shipment2.JS_UnitOfWeight = Weight.Kilograms;

			var consol = CreateConsol(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			// Rate consol costs only, not revenue
			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 1500,
						CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 150.00/100 KG",
					}
			};

			AutoCostAndAssert("Consol cost", null, expectedCosts, consol, autorateRevenue: false);

			// Shipment 1 revenue
			var expected1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 300m,
					JR_OSSellAmt = 320m,
					JR_AgentDeclaredCostAmt = 400m,
					JR_AgentDeclaredSellAmt = 430m,
					RevenueCalculationDescription = "FRT: 200 Kilogram(s) @ AUD 160.00/100 KG"
				}
			};

			AutorateAndAssert(expected1, shipment1, Consignee);

			// Shipment 2 revenue
			var expected2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1200m,
					JR_OSSellAmt = 1280m,
					JR_AgentDeclaredCostAmt = 1600m,
					JR_AgentDeclaredSellAmt = 1720m,
					RevenueCalculationDescription = "FRT: 800 Kilogram(s) @ AUD 160.00/100 KG"
				}
			};

			AutorateAndAssert(expected2, shipment2, Consignee);
		}

		public void TestCSTCalculator_RatingConsolCostThenShipmentRevenue_MultipleShipments_FlatRate()
		{
			const string origin = "AUSYD";
			const string dest = "USLAX";
			var flatChargeCode = Helper.ChargeCodes["FSC"];

			// Flat cost rate of $100 ($120 for agent)...
			var costEntry = CreateCostRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var costLine = costEntry.AddFlatCharge(flatChargeCode.AC_Code, 100);
			var costLineCalc = costLine.GetCalculator<FlatCalculator>();
			costLine.SetViewAgentRatesWithoutRefreshBinding(true);
			costLineCalc.BaseRate = 120;
			costLine.SetViewAgentRatesWithoutRefreshBinding(false);

			// Cost calculator bumps all base rates by 10 (20 for agent)
			var revenueEntry = CreateClientRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var revenueLine = revenueEntry.AddRateLine(flatChargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var calc = revenueLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calc.BaseRate = 10;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(true);
			calc.BaseRate = 20;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(false);

			// Shipments with different weights...
			// Apportionment is by chargeable unit (weight for AIR).
			var shipment1 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment1.JS_ActualWeight = 200;
			shipment1.JS_UnitOfWeight = Weight.Kilograms;

			var shipment2 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment2.JS_ActualWeight = 800;
			shipment2.JS_UnitOfWeight = Weight.Kilograms;

			var consol = CreateConsol(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			// Rate consol costs only, not revenue
			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = "FSC",
						E6_OSCostAmount = 100,
						CostCalculationDescription = "FSC: Base Rate AUD 100.00",
					}
			};

			AutoCostAndAssert("Consol cost", null, expectedCosts, consol, autorateRevenue: false);

			// Shipment 1 revenue
			var expected1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FSC",
					JR_OSCostAmt = 20m,
					JR_OSSellAmt = 30m,
					JR_AgentDeclaredCostAmt = 24m,
					JR_AgentDeclaredSellAmt = 44m,
					RevenueCalculationDescription = "FSC: Base Rate AUD 20.00 + Base Rate AUD 10.00"
				}
			};

			AutorateAndAssert(expected1, shipment1, Consignee);

			// Shipment 2 revenue
			var expected2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FSC",
					JR_OSCostAmt = 80m,
					JR_OSSellAmt = 90m,
					JR_AgentDeclaredCostAmt = 96m,
					JR_AgentDeclaredSellAmt = 116m,
					RevenueCalculationDescription = "FSC: Base Rate AUD 80.00 + Base Rate AUD 10.00"
				}
			};

			AutorateAndAssert(expected2, shipment2, Consignee);
		}

		public void TestCSTCalculator_RatingConsolCostThenShipmentRevenue_MultipleShipments_FlatAndPerUnitRate()
		{
			const string origin = "AUSYD";
			const string dest = "USLAX";
			var chargeCode = Helper.ChargeCodes["FRT"];

			// Flat cost rate of $50 + $10/KG ($65 + $12/KG for agent)...
			var costEntry = CreateCostRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var costLine = costEntry.AddRateLine(chargeCode, FlatPlusPerUnitCalculator.Code, Weight.Kilograms, "AUD");
			var costLineCalc = costLine.GetCalculator<FlatPlusPerUnitCalculator>();
			costLineCalc.BaseRate = 50m;
			costLineCalc.PerUnit = 10m;
			costLine.SetViewAgentRatesWithoutRefreshBinding(true);
			costLineCalc.BaseRate = 65m;
			costLineCalc.PerUnit = 12m;
			costLine.SetViewAgentRatesWithoutRefreshBinding(false);

			// Cost calculator bumps per-unit rate by 1 (1.2 for agent)
			var revenueEntry = CreateClientRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var revenueLine = revenueEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var calc = revenueLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calc.PerUnit = 1m;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(true);
			calc.PerUnit = 1.2m;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(false);

			// Shipments with different weights...
			// Apportionment is by chargeable unit (weight for AIR).
			var shipment1 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment1.JS_ActualWeight = 200;
			shipment1.JS_UnitOfWeight = Weight.Kilograms;

			var shipment2 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment2.JS_ActualWeight = 800;
			shipment2.JS_UnitOfWeight = Weight.Kilograms;

			var consol = CreateConsol(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			// Rate consol costs only, not revenue
			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = chargeCode.AC_Code,
						E6_OSCostAmount = 10050,
						CostCalculationDescription = "FRT: Base Rate AUD 50.00 + 1000 Kilogram(s) @ AUD 10.00/KG",
					}
			};

			AutoCostAndAssert("Consol cost", null, expectedCosts, consol, autorateRevenue: false);

			// Shipment 1 revenue
			var expected1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 2010m,
					JR_OSSellAmt = 2210m,
					JR_AgentDeclaredCostAmt = 2413m, // $65 x 0.2 + 200 x $12
					JR_AgentDeclaredSellAmt = 2413m, // mix of payment bases not currently supported for agent - use flat cost
					RevenueCalculationDescription = "FRT: Base Rate AUD 10.00 + 200 Kilogram(s) @ AUD 11.00/KG"
				}
			};

			AutorateAndAssert(expected1, shipment1, Consignee);

			// Shipment 2 revenue
			var expected2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 8040m,
					JR_OSSellAmt = 8840m,
					JR_AgentDeclaredCostAmt = 9652m, // 65 * 0.8 + 800 * 12
					JR_AgentDeclaredSellAmt = 9652m,// mix of payment bases not currently supported for agent - use flat cost
					RevenueCalculationDescription = "FRT: Base Rate AUD 40.00 + 800 Kilogram(s) @ AUD 11.00/KG"
				}
			};

			AutorateAndAssert(expected2, shipment2, Consignee);
		}

		public void TestCSTCalculator_RatingConsolCostThenShipmentRevenue_OneShipment_MinAndPerUnit()
		{
			const string origin = "AUSYD";
			const string dest = "USLAX";
			var chargeCode = Helper.ChargeCodes["FRT"];

			var costEntry = CreateCostRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var costLine = costEntry.AddRateLine(chargeCode, MinimumOrPerUnitCalculator.Code, Weight.Kilograms, "AUD");
			var costLineCalc = costLine.GetCalculator<MinimumOrPerUnitCalculator>();
			costLineCalc.Minimum = 5000;
			costLineCalc.PerUnit = 1m;
			costLine.SetViewAgentRatesWithoutRefreshBinding(true);
			costLineCalc.Minimum = 5000;
			costLineCalc.PerUnit = 1m;
			costLine.SetViewAgentRatesWithoutRefreshBinding(false);

			// Cost calculator bumps per-unit rate by 9
			// pushing the amount above the minimum
			var revenueEntry = CreateClientRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var revenueLine = revenueEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var calc = revenueLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calc.PerUnit = 9m;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(true);
			calc.PerUnit = 9m;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(false);

			var shipment1 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment1.JS_ActualWeight = 1000;
			shipment1.JS_UnitOfWeight = Weight.Kilograms;

			var consol = CreateConsol(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			consol.Shipments.Add(shipment1);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			// Rate consol costs only, not revenue
			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = chargeCode.AC_Code,
						E6_OSCostAmount = 5000,
						CostCalculationDescription = "FRT: Minimum AUD 5000.00",
					}
			};

			AutoCostAndAssert("Consol cost", null, expectedCosts, consol, autorateRevenue: false);

			// Shipment 1 revenue
			var expected1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 5000m,
					JR_OSSellAmt = 10000m,
					JR_AgentDeclaredCostAmt = 5000m,
					JR_AgentDeclaredSellAmt = 10000m,
					RevenueCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 10.00/KG"
				}
			};

			AutorateAndAssert(expected1, shipment1, Consignee);
		}

		public void TestCSTCalculator_RatingConsolCostThenShipmentRevenue_MultipleShipments_MinAndPerUnit_AboveMin()
		{
			// Cost is below min (so min amount applies)
			// Revenue cost based calculator increases per unit rate to put the amount above the min.
			const string origin = "AUSYD";
			const string dest = "USLAX";
			var chargeCode = Helper.ChargeCodes["FRT"];

			var costEntry = CreateCostRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var costLine = costEntry.AddRateLine(chargeCode, MinimumOrPerUnitCalculator.Code, Weight.Kilograms, "AUD");
			var costLineCalc = costLine.GetCalculator<MinimumOrPerUnitCalculator>();
			costLineCalc.Minimum = 5000;
			costLineCalc.PerUnit = 1m;
			costLine.SetViewAgentRatesWithoutRefreshBinding(true);
			costLineCalc.Minimum = 5000;
			costLineCalc.PerUnit = 1m;
			costLine.SetViewAgentRatesWithoutRefreshBinding(false);

			// Cost calculator bumps per-unit rate by 5
			// Ship 1 is above split min of 1000 (6 x 200 = 1200)
			// Ship 2 is above split min of 4000 (6 * 800 = 4800)
			var revenueEntry = CreateClientRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, dest, container: null);
			var revenueLine = revenueEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			var calc = revenueLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
			calc.PerUnit = 5m;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(true);
			calc.PerUnit = 5m;
			revenueLine.SetViewAgentRatesWithoutRefreshBinding(false);

			// Shipments with different weights...
			// Apportionment is by chargeable unit (weight for AIR).
			var shipment1 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment1.JS_ActualWeight = 200;
			shipment1.JS_UnitOfWeight = Weight.Kilograms;

			var shipment2 = CreateShipment(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			shipment2.JS_ActualWeight = 800;
			shipment2.JS_UnitOfWeight = Weight.Kilograms;

			var consol = CreateConsol(transportMode: TransportModes.Air, containerMode: ContainerModes.Loose, origin, dest);
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			// Rate consol costs only, not revenue
			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = chargeCode.AC_Code,
						E6_OSCostAmount = 5000,
						CostCalculationDescription = "FRT: Minimum AUD 5000.00",
					}
			};

			AutoCostAndAssert("Consol cost", null, expectedCosts, consol, autorateRevenue: false);

			// Shipment 1 revenue
			var expected1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 1000m,
					JR_OSSellAmt = 1200m,
					JR_AgentDeclaredCostAmt = 1000m,
					JR_AgentDeclaredSellAmt = 1200m,
					RevenueCalculationDescription = "FRT: 200 Kilogram(s) @ AUD 6.00/KG"
				}
			};

			AutorateAndAssert(expected1, shipment1, Consignee);

			// Shipment 2 revenue
			var expected2 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 4000m,
					JR_OSSellAmt = 4800m,
					JR_AgentDeclaredCostAmt = 4000m,
					JR_AgentDeclaredSellAmt = 4800m,
					RevenueCalculationDescription = "FRT: 800 Kilogram(s) @ AUD 6.00/KG"
				}
			};

			AutorateAndAssert(expected2, shipment2, Consignee);
		}

		#endregion

		#region Percentage Calculator

		public void TestPercentageCalculator_MultipleChargesApplyTo_Scenario1()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			CreateCostRate(container: "20GP").AddPerUnitCharge("FRT", 100, "KG");
			CreateCostRate(container: "40GP").AddPerUnitCharge("FRT", 200, "KG");
			CreateCostRate(container: "").AddPercentageCharge("BAF", Helper.ChargeCodes["FRT"], 10);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", packLines: new[] { shipment.AddPackLine(weight: 30) });
			consol.AddContainer("40GP", packLines: new[] { shipment.AddPackLine(weight: 70) });

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// 30 KG packed in 20 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 3000.00m,
					CostCalculationDescription = "FRT: 30 Kilogram(s) @ USD 100.00/KG"
				},
				new AssertionCharge
				{
					// 70 KG packed in 40 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 14000.00m,
					CostCalculationDescription = "FRT: 70 Kilogram(s) @ USD 200.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 1700.00m,
					CostCalculationDescription = "BAF: 10.00% of (USD 17000.00 (FRT 3000.00 + FRT 14000.00))"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestPercentageCalculator_MultipleChargesApplyTo_Scenario2()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			CreateCostRate(container: "").AddPerUnitCharge("FRT", 100, "KG");
			CreateCostRate(container: "20GP").AddPercentageCharge("BAF", Helper.ChargeCodes["FRT"], 10);
			CreateCostRate(container: "40GP").AddPercentageCharge("BAF", Helper.ChargeCodes["FRT"], 20);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", packLines: new[] { shipment.AddPackLine(weight: 30) });
			consol.AddContainer("40GP", packLines: new[] { shipment.AddPackLine(weight: 70) });

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// 30 KG + packed in 20 GP Container + 70 KG packed in 40 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 10000.00m,
					CostCalculationDescription = "FRT: 100 Kilogram(s) @ USD 100.00/KG"
				},

				// Percentage calculation may not be correct. Basically both 20GP percentage and 40GP percentage are applied to a single
				// FRT charge calculated for all containers. Will be addressed later when product specialists decide how to deal in cases like this.
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 1000.00m,
					CostCalculationDescription = "BAF: 10.00% of (USD 10000.00 (FRT))"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 2000.00m,
					CostCalculationDescription = "BAF: 20.00% of (USD 10000.00 (FRT))"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestPercentageCalculator_MultipleChargesApplyTo_Scenario3()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			CreateCostRate(container: "20GP", commodity: "ALUM").AddPerUnitCharge("FRT", 100, "KG");
			CreateCostRate(container: "20GP", commodity: "SALT").AddPerUnitCharge("FRT", 200, "KG");
			CreateCostRate(container: "40GP", commodity: "ALUM").AddPerUnitCharge("FRT", 300, "KG");
			CreateCostRate(container: "40GP", commodity: "SALT").AddPerUnitCharge("FRT", 500, "KG");
			CreateCostRate(container: "20GP", commodity: "").AddPercentageCharge("BAF", Helper.ChargeCodes["FRT"], 10);
			CreateCostRate(container: "", commodity: "").AddPercentageCharge("BAF", Helper.ChargeCodes["FRT"], 20);

			var shipment = CreateShipment();

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			consol.AddContainer("20GP", packLines: new[]
			{
				shipment.AddPackLine(commodity: "SALT", weight: 30),
				shipment.AddPackLine(commodity: "ALUM", weight: 200)
			});
			consol.AddContainer("40GP", packLines: new[]
			{
				shipment.AddPackLine(commodity: "", weight: 70),
				shipment.AddPackLine(commodity: "SALT", weight: 50),
			});

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// 30 KG of SALT packed in 20 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 6000.00m,
					CostCalculationDescription = "FRT: 30 Kilogram(s) @ USD 200.00/KG"
				},
				new AssertionCharge
				{
					// 200 KG of ALUM packed in 20 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 20000.00m,
					CostCalculationDescription = "FRT: 200 Kilogram(s) @ USD 100.00/KG"
				},
				new AssertionCharge
				{
					// 50 KG of SALT packed in 40 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 25000.00m,
					CostCalculationDescription = "FRT: 50 Kilogram(s) @ USD 500.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 2600.00m,
					CostCalculationDescription = "BAF: 10.00% of (USD 26000.00 (FRT 6000.00 + FRT 20000.00))"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestPercentageCalculator_DeclarationChargeDependsOnShipmentCharge_ShouldStillMatchAndCalculateBasedOnIt()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			#region Create Rate

			Factory.Save();

			Env.Registry.Rating.SetOriginBrokerageRatedCodes("BRK");

			var rate = Helper.NewClientRate(localClient);

			var rateEntry1 = rate.AddRateEntry("AIR", "LSE", "AUBNE", "USLAX");
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine("FRT", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 200;

			var rateEntry2 = rate.AddRateEntry("ORG", "ALL", "AUBNE", "USLAX");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine("CCLR", PercentageCalculator.Code);
			rateLine2.GetCalculator<PercentageCalculator>().Percent = 10;
			rateLine2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;

			#endregion

			#region Create shipment

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			#endregion

			#region Create declaration

			var declaration = (BaseJobDeclaration)Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			declaration.JE_TransportMode = "AIR";
			declaration.JE_ContainerMode = "LSE";
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKOrigin = "AUBNE";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_JS = shipment.PK;

			#endregion

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  200m,
							RevenueCalculationDescription = "FRT: Base Rate AUD 200.00"
						},
					new AssertionCharge
						{
							ChargeCode = "CCLR",
							JR_OSSellAmt =  20m,
							RevenueCalculationDescription = "CCLR: 10.00% of (AUD 200.00 (FRT))"
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		#endregion

		#region CMB Calculator

		public void TestCMBCalculator_ContainerPivotBreak_OverridesBreakValue()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			CreateCostRate(container: "20GP").AddCMBCalculatorAccumulated("FRT")
				.AddSlidingMinusRateLine(45m, 20m, 200m)
				.AddSlidingPlusRateLine(45m, 10m, 100m);
			CreateCostRate(container: "40GP").AddCMBCalculatorAccumulated("FRT")
				.AddSlidingMinusRateLine(45m, 20m, 200m)
				.AddSlidingPlusRateLine(45m, 10m, 100m);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", pivotBreak: 25m, packLines: new[] { shipment.AddPackLine(weight: 30) });
			consol.AddContainer("40GP", pivotBreak: 50m, packLines: new[] { shipment.AddPackLine(weight: 70) });

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// Break value: 25 is overridden for 20 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 650.00m,
					CostCalculationDescription = "FRT: Base Rate USD 100.00 + 25 Kilogram(s) @ USD 20.00/KG + 5 Kilogram(s) @ USD 10.00/KG"
				},
				new AssertionCharge
				{
					// Break value: 50 is overridden for 40 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 1300.00m,
					CostCalculationDescription = "FRT: Base Rate USD 100.00 + 50 Kilogram(s) @ USD 20.00/KG + 20 Kilogram(s) @ USD 10.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestCMBCalculator_BreaksPer()
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			var entry = CreateCostRate(container: "20GP");
			entry.AddCMBCalculatorAccumulated("FRT")
				.AddSlidingMinusRateLine(45m, 20m, 200m)
				.AddSlidingPlusRateLine(45m, 10m, 100m)
				.SetBreaksPerAsContainer();
			entry.AddCMBCalculatorAccumulated("BAF")
				.AddSlidingMinusRateLine(45m, 20m, 200m)
				.AddSlidingPlusRateLine(45m, 10m, 100m)
				.SetBreaksPerAsContainerTypeOrClass();

			CreateCostRate(container: "40GP")
				.AddCMBCalculatorAccumulated("WAR")
				.AddSlidingMinusRateLine(45m, 20m, 200m)
				.AddSlidingPlusRateLine(45m, 10m, 100m)
				.SetBreaksPerAsContainer();

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", packLines: new[] { shipment.AddPackLine(weight: 24), shipment.AddPackLine(weight: 24) });
			consol.AddContainer("20GP", packLines: new[] { shipment.AddPackLine(weight: 44) });
			consol.AddContainer("40GP", packLines: new[] { shipment.AddPackLine(weight: 45) });

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 2110.00m,
					CostCalculationDescription = "FRT: Base Rate USD 100.00 + Base Rate USD 200.00 + 89 Kilogram(s) @ USD 20.00/KG + 3 Kilogram(s) @ USD 10.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 2120.00m,
					CostCalculationDescription = "BAF: Base Rate USD 100.00 + Base Rate USD 200.00 + 90 Kilogram(s) @ USD 20.00/KG + 2 Kilogram(s) @ USD 10.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					JR_OSCostAmt = 1100.00m,
					CostCalculationDescription = "WAR: Base Rate USD 200.00 + 45 Kilogram(s) @ USD 20.00/KG"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestCMB_PerContainerRateWithWeightBreak_UseGrossContainerWeightForBreaksSearch()
		{
			var container = Helper.Containers["20GP"];
			container.RC_TareWeight = 200m;

			var cost1 = CreateCostRate(container: "20GP");

			var frt = cost1.AddRateLine("FRT", "CMB", "CN").GetCalculator<CombinedCalculator>();
			frt.AddRateLineItem(Calculator.Items.Operator.Minus, 100, 1000, breakUnit: "KG");
			frt.AddRateLineItem(Calculator.Items.Operator.Plus, 100, 900);
			frt.AddRateLineItem(Calculator.Items.Operator.Plus, 400, 800);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", packLines: new[] { shipment.AddPackLine(weight: 300) });

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// Should use gross container weight (200 KG + 300 KG = 500 KG) for the break search which results in $800 per container.
					ChargeCode = "FRT",
					JR_OSCostAmt = 800m,
					CostCalculationDescription = "FRT: 1 20GP Container(s) @ USD 800.00/Container"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestCMB_UnitIsPackage()
		{
			var cost1 = CreateCostRate();

			var frtLine = cost1.AddRateLine("FRT", "CMB", "BOX");
			var frtCalc = frtLine.GetCalculator<CombinedCalculator>();
			frtCalc.AddRateLineItem(Calculator.Items.Operator.Minus, 10, 100);
			frtCalc.AddRateLineItem(Calculator.Items.Operator.Plus, 10, 50);

			var bafLine = cost1.AddRateLine("BAF", "CMB", "PLT");
			var badCalc = bafLine.GetCalculator<CombinedCalculator>();
			badCalc.AddRateLineItem(Calculator.Items.Operator.Minus, 20, 200, breakUnit: "PLT");
			badCalc.AddRateLineItem(Calculator.Items.Operator.Plus, 20, 100);

			var cafLine = cost1.AddRateLine("CAF", "CMB", "CTN");
			var cafCalc = cafLine.GetCalculator<CombinedCalculator>();
			cafCalc.AddRateLineItem(Calculator.Items.Operator.Minus, 30, 20, breakUnit: "PK");
			cafCalc.AddRateLineItem(Calculator.Items.Operator.Plus, 30, 10);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer(packLines: new[]
			{
				shipment.AddPackLine(packType: "BOX", count: 5),
				shipment.AddPackLine(packType: "PLT", count: 25),
				shipment.AddPackLine(packType: "PLT", count: 15),
				shipment.AddPackLine(packType: "CTN", count: 20),
				shipment.AddPackLine(packType: "CTN", count: 40),
			});

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 500m,
					CostCalculationDescription = "FRT: 5 Box(s) @ USD 100.00/Box"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 4000,
					CostCalculationDescription = "BAF: 40 Pallet(s) @ USD 100.00/Pallet"
				},
				new AssertionCharge
				{
					// Calculated per individual pack line since the break unit is PK
					ChargeCode = "CAF",
					JR_OSCostAmt = 800m,
					CostCalculationDescription = "CAF: 20 Carton(s) @ USD 20.00/Carton + 40 Carton(s) @ USD 10.00/Carton"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestCMB_UnitIsPackageAndBreakPerWeight_UseActualAmountTicked_UseActualAmountForBreakSearch()
		{
			var cost1 = CreateCostRate();

			var frtLine = cost1.AddRateLine("FRT", "CMB", "BOX");
			frtLine.ConversionFactor = new ConversionFactor(100, "KG", "M3");
			frtLine.UseOnlyActualWeightMeasure = true;

			var calc = frtLine.GetCalculator<CombinedCalculator>();
			calc.AddRateLineItem(Calculator.Items.Operator.Minus, 200, 1000, breakUnit: "KG");
			calc.AddRateLineItem(Calculator.Items.Operator.Plus, 200, 2000);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", packLines: new[]
			{
				shipment.AddPackLine(weight: 300, volume: 5 /* 500 KG */, packType: "BOX", count: 2),
			});

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// Should use actual packages weight (300 KG / 2 BOX = 150 KG/BOX) for the break search which results in $1000 per box
					ChargeCode = "FRT",
					JR_OSCostAmt = 2000m,
					CostCalculationDescription = "FRT: 2 Box(s) @ USD 1000.00/Box"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestCMB_UnitIsPackageAndBreakPerWeight_UseActualAmountUnticked_UseChargeableAmountForBreakSearch()
		{
			var cost1 = CreateCostRate();

			var frtLine = cost1.AddRateLine("FRT", "CMB", "BOX");
			frtLine.ConversionFactor = new ConversionFactor(100, "KG", "M3");
			frtLine.UseOnlyActualWeightMeasure = false;

			var calc = frtLine.GetCalculator<CombinedCalculator>();
			calc.AddRateLineItem(Calculator.Items.Operator.Minus, 200, 1000, breakUnit: "KG");
			calc.AddRateLineItem(Calculator.Items.Operator.Plus, 200, 2000);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", packLines: new[]
			{
				shipment.AddPackLine(weight: 300, volume: 5 /* 500 KG */, packType: "BOX", count: 2),
			});

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// Should use actual packages weight (500 KG / 2 BOX = 250 KG/BOX) for the break search which results in $2000 per box
					ChargeCode = "FRT",
					JR_OSCostAmt = 4000m,
					CostCalculationDescription = "FRT: 2 Box(s) @ USD 2000.00/Box"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestCMB_UnitIsContainerAndBreakPerWeight_UseContainerCountPropertyOnContainerToIdentifyNumberOfContainers()
		{
			var cost1 = CreateCostRate();
			Helper.Containers["20GP"].RC_TareWeight = 100;

			var frtLine = cost1.AddRateLine("FRT", "CMB", "CN");
			frtLine.UseOnlyActualWeightMeasure = true;

			var calc = frtLine.GetCalculator<CombinedCalculator>();
			calc.AddRateLineItem(Calculator.Items.Operator.Minus, 1000, 100, breakUnit: "KG");
			calc.AddRateLineItem(Calculator.Items.Operator.Plus, 1000, 200);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer(count: 5, packLines: new[] { shipment.AddPackLine(weight: 2000) });
			consol.AddContainer(count: 2, packLines: new[] { shipment.AddPackLine(weight: 5000) });
			consol.AddContainer(count: 1, number: "CNT000001", packLines: new[] { shipment.AddPackLine(weight: 500) });

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 1000m,
					CostCalculationDescription = "FRT: 2 20GP Container(s) @ USD 200.00/Container + 5 20GP Container(s) @ USD 100.00/Container + 1 20GP Container(s) @ USD 100.00/Container"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		public void TestCMB_CalculatorHasMinChargeable_UseMinChargeableIfApplicable()
		{
			var cost1 = CreateCostRate(container: "20GP");

			var frt = cost1.AddRateLine("FRT", "CMB", "KG").GetCalculator<CombinedCalculator>();
			frt.AddRateLineItem(Calculator.Items.Operator.Minus, 500, 2);
			frt.AddRateLineItem(Calculator.Items.Operator.Plus, 500, 1);
			frt.AddRateLineItem(Calculator.Items.Operator.MIN, 1000, 0);

			var baf = cost1.AddRateLine("BAF", "CMB", "KG").GetCalculator<CombinedCalculator>();
			baf.AddRateLineItem(Calculator.Items.Operator.Minus, 500, 2);
			baf.AddRateLineItem(Calculator.Items.Operator.Plus, 500, 1);
			baf.AddRateLineItem(Calculator.Items.Operator.MIN, 200, 0);

			var cost2 = CreateCostRate(container: "40GP");

			var war = cost2.AddRateLine("WAR", "CMB", "KG").GetCalculator<CombinedCalculator>();
			war.AddRateLineItem(Calculator.Items.Operator.Minus, 500, 2);
			war.AddRateLineItem(Calculator.Items.Operator.Plus, 500, 1);
			war.AddRateLineItem(Calculator.Items.Operator.MIN, 1000, 0);

			var caf = cost2.AddRateLine("CAF", "CMB", "KG").GetCalculator<CombinedCalculator>();
			caf.AddRateLineItem(Calculator.Items.Operator.Minus, 500, 2);
			caf.AddRateLineItem(Calculator.Items.Operator.Plus, 500, 1);
			caf.AddRateLineItem(Calculator.Items.Operator.MIN, 0, 100);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP", packLines: new[] { shipment.AddPackLine(weight: 300) });
			consol.AddContainer("40GP", pivotBreak: 300m, packLines: new[] { shipment.AddPackLine(weight: 200) });

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					// Rate has min chargeable 1000 KG which is greater than shipment weight (300 KG)
					ChargeCode = "FRT",
					JR_OSCostAmt = 1000m,
					CostCalculationDescription = "FRT: Min 1000 Kilogram(s) @ USD 1.00/KG"
				},
				new AssertionCharge
				{
					// Shipment weight (300 KG) is greater than rate min chargeable (200 KG)
					ChargeCode = "BAF",
					JR_OSCostAmt = 600m,
					CostCalculationDescription = "BAF: 300 Kilogram(s) @ USD 2.00/KG"
				},
				new AssertionCharge
				{
					// Pivot weight on the container (300 KG) is greater than shipment weight (200 KG).
					// And pivot weight on the container overrides min chargeable on the rate (1000 KG)
					ChargeCode = "WAR",
					JR_OSCostAmt = 600m,
					CostCalculationDescription = "WAR: 300 Kilogram(s) @ USD 2.00/KG"
				},
				new AssertionCharge
				{
					// Shipment weight (200 KG) is used as the rate has no min chargeable.
					// We don't fallback to container pivot break (300 KG) as it only overrides rate min chargeable weight if it exists,
					// if it doesn't - there is nothing to override and thus it is ignored.
					ChargeCode = "CAF",
					JR_OSCostAmt = 400m,
					CostCalculationDescription = "CAF: 200 Kilogram(s) @ USD 2.00/KG"
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateRevenue: false);
		}

		#endregion

		#region HighestChargeCalculator

		public void TestHighestChargeCalculator()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = Helper.Containers["20GP"].PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_Width = 45;
			packline1.JL_Length = 45;
			packline1.JL_Height = 35;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_Width = 45;
			packline2.JL_Length = 45;
			packline2.JL_Height = 35;

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_Width = 11500;
			packline3.JL_Length = 7700;
			packline3.JL_Height = 6600;
			packline3.JL_UnitOfDimension = Length.Centimetres;

			var o200 = Helper.ChargeCodes.New("O200", "O200", "FLT", ChargeCodeGroupList.Codes.Freight);
			var o60 = Helper.ChargeCodes.New("O60", "O60", "FLT", ChargeCodeGroupList.Codes.Freight);
			var a120 = Helper.ChargeCodes.New("A120", "A120", "FLT", ChargeCodeGroupList.Codes.Freight);
			var ohcc = Helper.ChargeCodes.New("OHCC", "OHCC", "FLT", ChargeCodeGroupList.Codes.Freight);

			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rl1 = rateEntry.AddFlatRateLine(o200.AC_Code, 50);
			rl1.TL_Condition = RateLineConditions.UserDefined;
			rl1.TL_ConditionalExpression = "OuterPackLines.Any({JL_Calc_LargestDimension + JL_Calc_Girth > 200})";
			var rl2 = rateEntry.AddFlatRateLine(o60.AC_Code, 100);
			rl2.TL_Condition = RateLineConditions.UserDefined;
			rl2.TL_ConditionalExpression = "OuterPackLines.Any({Convert(JL_Calc_LargestDimension, JL_UnitOfDimension, \"M\") > 60})";
			var rl3 = rateEntry.AddFlatRateLine(a120.AC_Code, 200);
			rl3.TL_Condition = RateLineConditions.UserDefined;
			rl3.TL_ConditionalExpression = "OuterPackLines.Any({Convert(JL_Calc_LargestDimension, JL_UnitOfDimension, \"M\") > 120})";

			var hccLine = rateEntry.AddRateLine(ohcc.AC_Code, HighestChargeCalculator.Code);
			CreateChargeLineItem(hccLine.GetCalculator<HighestChargeCalculator>(), o200);
			CreateChargeLineItem(hccLine.GetCalculator<HighestChargeCalculator>(), o60);
			CreateChargeLineItem(hccLine.GetCalculator<HighestChargeCalculator>(), a120);

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "O60", JR_OSSellAmt = 100 } };
			AutorateAndAssert(expected, shipment, client);

			void CreateChargeLineItem(IDependentCalculator calculator, AccChargeCode accChargeCode)
			{
				var rli = calculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode);
				rli.TM_AC = accChargeCode.PK;
			}
		}

		#endregion

		#region Highest Rate Calculator

		public void TestHighestRateCalculator_ContainersAndPackLines()
		{
			var perUnitFee = Helper.ChargeCodes.New("PUNIT", "Per Unit Fee", MinimumOrPerUnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var perHrcFee = Helper.ChargeCodes.New("PHRC", "HRC Fee", HighestRateCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var perHrcFee2 = Helper.ChargeCodes.New("PHRC2", "HRC Fee No Container", HighestRateCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			CreateClientRate(container: "20GP")
				.AddHighestRateCharge(perHrcFee.AC_Code, weightRate: 10m, weightUnit: "M3", volumeRate: 10m, volumeUnit: "T")
				.AddUnitCharge(perUnitFee.AC_Code, rate: 10m, unit: "M3");

			CreateClientRate(container: "")
				.AddHighestRateCharge(perHrcFee2.AC_Code, weightRate: 10m, weightUnit: "M3", volumeRate: 10m, volumeUnit: "T");

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var outer20 = shipment.AddPackLine(weight: 2, weightUnit: "T", volume: 5);
			shipment.AddInnerPackLine(weight: 2, weightUnit: "T", volume: 5);
			consol.AddContainer("20GP", packLines: new[]
			{
				outer20,
			});

			var outer40 = shipment.AddPackLine(weight: 20, weightUnit: "T", volume: 50);
			consol.AddContainer("40GP", packLines: new[]
			{
				outer40
			});

			Factory.Save();

			var expected = new[] {
				// PHRC: should match only 20' GP
				//
				// Highest rate = $10/M3 * 5 = $50
				new AssertionCharge { ChargeCode = "PHRC", JR_OSSellAmt = 50, RevenueCalculationDescription = "PHRC: 5 Cubic Meter(s) @ USD 10.00/M3" },

				// PHRC2: should match outer packs only
				//
				// Highest rate: $10/M3 * 55 = $550
				new AssertionCharge { ChargeCode = "PHRC2", JR_OSSellAmt = 550, RevenueCalculationDescription = "PHRC2: 55 Cubic Meter(s) @ USD 10.00/M3" },

				// Per-unit charge is here to demonstrate that inner pack lines
				// and unmatched containers are filtered-out from totals on this
				// calculator.
				new AssertionCharge { ChargeCode = "PUNIT", JR_OSSellAmt = 50, RevenueCalculationDescription = "PUNIT: 5 Cubic Meter(s) @ USD 10.00/M3" },
			};

			AutorateAndAssert(expected, shipment, Consignee);
		}

		#endregion

		#region charge with contract number takes priority over blank contract number in consol

		public void TestContractNumberComparer()
		{
			// Given:
			// - Costings with entries differentiated by contract numbers
			// - Consol with different contract number
			// When:
			// - Auto Cost Consol
			// Then:
			// - Create charges for only rates matching job contract numbers
			// - Rates with blank contract number are always accepted but if the same charge exists with contract number then it takes priority

			#region Costing

			var costEntry1 = CreateCostRate();
			var noContractNumberFRT = new RateLineChargeCodes("FRT", 10);
			var noContractNumberCAF = new RateLineChargeCodes("CAF", 20);
			costEntry1.AddFlatCharge(noContractNumberFRT.ChargeCode, noContractNumberFRT.Amount);
			costEntry1.AddFlatCharge(noContractNumberCAF.ChargeCode, noContractNumberCAF.Amount);

			var costEntry2 = CreateCostRate();
			costEntry2.TI_ContractNumber = "AAA";
			var aaaContractNumberFRT = new RateLineChargeCodes("FRT", 30);
			var aaaContractNumberBAF = new RateLineChargeCodes("BAF", 40);
			costEntry2.AddFlatCharge(aaaContractNumberFRT.ChargeCode, aaaContractNumberFRT.Amount);
			costEntry2.AddFlatCharge(aaaContractNumberBAF.ChargeCode, aaaContractNumberBAF.Amount);

			var costEntry3 = CreateCostRate();
			costEntry3.TI_ContractNumber = "BBB";
			var bbbContractNumberFRT = new RateLineChargeCodes("FRT", 50);
			var bbbContractNumberWAR = new RateLineChargeCodes("WAR", 60);
			costEntry3.AddFlatCharge(bbbContractNumberFRT.ChargeCode, bbbContractNumberFRT.Amount);
			costEntry3.AddFlatCharge(bbbContractNumberWAR.ChargeCode, bbbContractNumberWAR.Amount);

			#endregion

			#region Shipment and Consol

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP");
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			#endregion

			#region scenarios and assert

			AutorateAndAssertContractNumbers(
				consol,
				shipment,
				presetJobContractNumber: "",
				new[] { noContractNumberFRT, noContractNumberCAF, aaaContractNumberFRT, aaaContractNumberBAF, bbbContractNumberFRT, bbbContractNumberWAR }
			);

			// entries with matched contract numbers are more preferable than those with empty ones so the latter are filtered out.
			AutorateAndAssertContractNumbers(
				consol,
				shipment,
				presetJobContractNumber: "AAA",
				new[] { aaaContractNumberFRT, aaaContractNumberBAF }
			);

			#endregion
		}

		public void TestContractNumberComparer_IncludedCharges()
		{
			#region Costing

			var costEntry1 = CreateCostRate();
			var noContractNumberFRT = new RateLineChargeCodes("FRT", 10);
			var noContractNumberCAF = new RateLineChargeCodes("CAF", 20);
			var noContractNumberBAF = new RateLineChargeCodes("BAF", 30);
			costEntry1.AddFlatCharge(noContractNumberFRT.ChargeCode, noContractNumberFRT.Amount);
			costEntry1.AddFlatCharge(noContractNumberCAF.ChargeCode, noContractNumberCAF.Amount);
			costEntry1.AddFlatCharge(noContractNumberBAF.ChargeCode, noContractNumberBAF.Amount);

			var costEntry2 = CreateCostRate();
			costEntry2.TI_ContractNumber = "AAA";
			var aaaContractNumberFRT = new RateLineChargeCodes("FRT", 30);
			var aaaContractNumberBAF = new RateLineChargeCodes("BAF", 0);
			costEntry2.AddFlatCharge(aaaContractNumberFRT.ChargeCode, aaaContractNumberFRT.Amount);
			costEntry2.AddRateLine(aaaContractNumberBAF.ChargeCode, Code).GetCalculator<FreightInclusiveCalculator>().FreightCalcType = FreightCalcTypes.Included;

			#endregion

			#region Shipment and Consol

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP");
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			Factory.Save();

			#endregion

			#region assert

			// entries with matched contract numbers are more preferable than those with empty ones so the latter are filtered out.
			AutorateAndAssertContractNumbers(
				consol,
				shipment,
				presetJobContractNumber: "AAA",
				new[] { aaaContractNumberFRT }
			);

			//WI00566519 : when we have contract number and no Empty CON in numbers, Only Rates with Contract Number loaded
			AutorateAndAssertContractNumbers(
				consol,
				shipment,
				presetJobContractNumber: "BBB",
				Array.Empty<RateLineChargeCodes>()
			);

			// WI00566519: when we have no contract number and no Empty CON in numbers : Rates with Blank Carrier Contract No. Loaded. Rates with ANY Carrier Contract No. Loaded
			AutorateAndAssertContractNumbers(
				consol,
				shipment,
				presetJobContractNumber: "",
				new[] { noContractNumberBAF, noContractNumberCAF, noContractNumberFRT, aaaContractNumberFRT }
			);

			#endregion
		}

		public void TestContractNumberComparer_WhenAutorateRevenue()
		{
			#region Costing

			var clientRate1 = CreateClientRate();
			var noContractNumberFRT = new RateLineChargeCodes("FRT", 10);
			var noContractNumberCAF = new RateLineChargeCodes("CAF", 20);
			clientRate1.AddFlatCharge(noContractNumberFRT.ChargeCode, noContractNumberFRT.Amount);
			clientRate1.AddFlatCharge(noContractNumberCAF.ChargeCode, noContractNumberCAF.Amount);

			var clientRate2 = CreateClientRate();
			clientRate2.TI_ContractNumber = "AAA";
			var aaaContractNumberFRT = new RateLineChargeCodes("FRT", 30);
			var aaaContractNumberBAF = new RateLineChargeCodes("BAF", 40);
			clientRate2.AddFlatCharge(aaaContractNumberFRT.ChargeCode, aaaContractNumberFRT.Amount);
			clientRate2.AddFlatCharge(aaaContractNumberBAF.ChargeCode, aaaContractNumberBAF.Amount);

			#endregion

			#region Shipment and Consol

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP");

			shipment.Numbers.RemoveAndDeleteAll();
			var jobHeader = new JobHeader.Loader(shipment).TryCreateWithoutMutexForTestOnly();

			Factory.Save();

			#endregion

			#region assert

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m, JR_OSSellAmt = 20m },
				new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 30m, JR_OSSellAmt = 30m },
				new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 40m, JR_OSSellAmt = 40m }
			};

			AutorateAndAssert(expected, shipment, Consignee);
			AssertEquals("AAA", jobHeader.JH_ClientContractNumber);

			#endregion
		}

		public void TestContractNumber_WhenNotBlank_ShouldCreateCharges()
		{
			const string contractNumber = "AAA";
			const string origin = "UAIEV";
			const string destination = "USLAX";

			Helper.ChargeCodes["ODOC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["OTHC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["OCART"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DDOC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DTHC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DCART"].AC_IsGroupageCharge = true;

			var costRateFRTNoContractNumber = CreateCostRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination, container: "");
			costRateFRTNoContractNumber.TI_RX_NKCurrency = CurrencyCodes.Australia;
			var noContractNumberFRT = new RateLineChargeCodes("FRT", 10);
			var noContractNumberBAF = new RateLineChargeCodes("BAF", 20);
			costRateFRTNoContractNumber.AddFlatCharge(noContractNumberFRT.ChargeCode, noContractNumberFRT.Amount);
			costRateFRTNoContractNumber.AddFlatCharge(noContractNumberBAF.ChargeCode, noContractNumberBAF.Amount);

			var costRateFRTWithContractNumber = CreateCostRate(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination, container: "");
			costRateFRTNoContractNumber.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costRateFRTWithContractNumber.TI_ContractNumber = contractNumber;
			var aaaContractNumberFRT = new RateLineChargeCodes("FRT", 30);
			var aaaContractNumberFSC = new RateLineChargeCodes("FSC", 40);
			costRateFRTWithContractNumber.AddFlatCharge(aaaContractNumberFRT.ChargeCode, aaaContractNumberFRT.Amount);
			costRateFRTWithContractNumber.AddFlatCharge(aaaContractNumberFSC.ChargeCode, aaaContractNumberFSC.Amount);

			var costRateORGNoContractNumber = CreateCostRate(RatingConstants.RateCategory.ORG, RateMode.ALL, origin, destination, container: "");
			costRateORGNoContractNumber.TI_RX_NKCurrency = CurrencyCodes.Australia;
			var noContractNumberODOC = new RateLineChargeCodes("ODOC", 10);
			var noContractNumberOCART = new RateLineChargeCodes("OCART", 20);
			costRateORGNoContractNumber.AddFlatCharge(noContractNumberODOC.ChargeCode, noContractNumberODOC.Amount);
			costRateORGNoContractNumber.AddFlatCharge(noContractNumberOCART.ChargeCode, noContractNumberOCART.Amount);

			var costRateORGWithContractNumber = CreateCostRate(RatingConstants.RateCategory.ORG, RateMode.ALL, origin, destination, container: "");
			costRateORGNoContractNumber.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costRateORGWithContractNumber.TI_ContractNumber = contractNumber;
			var aaaContractNumberODOC = new RateLineChargeCodes("ODOC", 30);
			var aaaContractNumberOTHC = new RateLineChargeCodes("OTHC", 40);
			costRateORGWithContractNumber.AddFlatCharge(aaaContractNumberODOC.ChargeCode, aaaContractNumberODOC.Amount);
			costRateORGWithContractNumber.AddFlatCharge(aaaContractNumberOTHC.ChargeCode, aaaContractNumberOTHC.Amount);

			var costRateDSTNoContractNumber = CreateCostRate(RatingConstants.RateCategory.DST, RateMode.ALL, origin, destination, container: "");
			costRateDSTNoContractNumber.TI_RX_NKCurrency = CurrencyCodes.Australia;
			var noContractNumberDDOC = new RateLineChargeCodes("DDOC", 10);
			var noContractNumberDCART = new RateLineChargeCodes("DCART", 20);
			costRateDSTNoContractNumber.AddFlatCharge(noContractNumberDDOC.ChargeCode, noContractNumberDDOC.Amount);
			costRateDSTNoContractNumber.AddFlatCharge(noContractNumberDCART.ChargeCode, noContractNumberDCART.Amount);

			var costRateDSTWithContractNumber = CreateCostRate(RatingConstants.RateCategory.DST, RateMode.ALL, origin, destination, container: "");
			costRateDSTNoContractNumber.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costRateDSTWithContractNumber.TI_ContractNumber = contractNumber;
			var aaaContractNumberDDOC = new RateLineChargeCodes("DDOC", 30);
			var aaaContractNumberDTHC = new RateLineChargeCodes("DTHC", 40);
			costRateDSTWithContractNumber.AddFlatCharge(aaaContractNumberDDOC.ChargeCode, aaaContractNumberDDOC.Amount);
			costRateDSTWithContractNumber.AddFlatCharge(aaaContractNumberDTHC.ChargeCode, aaaContractNumberDTHC.Amount);

			Factory.Save();

			var shipment = CreateShipment(TransportModes.Air, ContainerModes.Loose, origin, destination);
			var consol = CreateConsol(TransportModes.Air, ContainerModes.Loose, origin, destination);
			consol.Shipments.Add(shipment);

			consol.JK_CarrierContractNumber = contractNumber;
			var expectedAssertionCost = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_OSCostAmount = 30 },
				new AssertionCost { ChargeCode = "FSC" },
				new AssertionCost { ChargeCode = "ODOC", E6_OSCostAmount = 30 },
				new AssertionCost { ChargeCode = "OTHC" },
				new AssertionCost { ChargeCode = "OCART" },
				new AssertionCost { ChargeCode = "DDOC", E6_OSCostAmount = 30 },
				new AssertionCost { ChargeCode = "DTHC" },
				new AssertionCost { ChargeCode = "DCART" },
			};
			AutoCostAndAssert("AutoCostAndAssert charges", null, expectedAssertionCost, consol, false);
		}

		struct RateLineChargeCodes
		{
			public RateLineChargeCodes(ZString chargeCode, ZDecimal amount)
			{
				ChargeCode = chargeCode;
				Amount = amount;
			}

			public ZString ChargeCode { get; }
			public ZDecimal Amount { get; }
		}

		void AutorateAndAssertContractNumbers(
			ForwardingConsol consol,
			ForwardingShipment shipment,
			ZString presetJobContractNumber,
			params RateLineChargeCodes[] expectedChargeCodes)
		{
			DeleteExistingCosts(consol.CostSupporter.PK);

			List<AssertionCharge> expectedAssertionCharge = new List<AssertionCharge>();
			List<AssertionCost> expectedAssertionCost = new List<AssertionCost>();

			foreach (var chargeCode in expectedChargeCodes)
			{
				expectedAssertionCharge.Add(new AssertionCharge { ChargeCode = chargeCode.ChargeCode, JR_OSCostAmt = chargeCode.Amount, JR_OSSellAmt = chargeCode.Amount });
				expectedAssertionCost.Add(new AssertionCost { ChargeCode = chargeCode.ChargeCode, E6_OSCostAmount = chargeCode.Amount });
			}

			consol.JK_CarrierContractNumber = presetJobContractNumber;
			AutorateAndAssert(expectedAssertionCharge, shipment, Consignee, autorateRevenue: false);

			consol.JK_CarrierContractNumber = presetJobContractNumber;
			AutoCostAndAssert("AutoCostAndAssert charges", null, expectedAssertionCost, consol, false);
		}

		#endregion

		#region Test Sell rated for disbursement charge

		public void TestSellRatedAndCostRated_ForDisbursementCharge_AfterAutorating()
		{
			var dsbCharge1 = Helper.ChargeCodes.New("DSB-1", "Disbursement  Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			dsbCharge1.AC_ChargeType = ChargeType.Disbursement;

			var dsbCharge2 = Helper.ChargeCodes.New("DSB-2", "Disbursement  Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			dsbCharge2.AC_ChargeType = ChargeType.Disbursement;

			var dsbCharge3 = Helper.ChargeCodes.New("DSB-3", "Disbursement  Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			dsbCharge3.AC_ChargeType = ChargeType.Disbursement;

			var dsbCharge4 = Helper.ChargeCodes.New("DSB-4", "Disbursement  Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			dsbCharge4.AC_ChargeType = ChargeType.Disbursement;

			var clientRate = CreateClientRate();
			clientRate.AddFlatCharge("FRT", 10m);
			clientRate.AddFlatCharge(dsbCharge1.AC_Code, 20m);

			var costing = CreateCostRate();
			costing.AddFlatCharge("PSS", 30);
			costing.AddFlatCharge(dsbCharge2.AC_Code, 40);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP");
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);

			//DSB Charge Manally entered
			var charge1 = testJob.Charges.AddNew();
			charge1.JR_AC = dsbCharge3.PK;
			charge1.JR_OSCostAmt = 50m;

			//DSB Charge enetered using Quick Calculator
			var charge2 = testJob.Charges.AddNew();
			charge2.JR_AC = dsbCharge4.PK;
			charge2.ApplyCustomQuickCalculator(shipment.RatingAdapter, costAmount: 60m, sellAmount: 0m);

			//Add FRT and DSB charge and make it REA for both Cost and Sell
			//Autorate must delete this charge and retains the one comes from client rate and must have SellRated true
			var charge3 = testJob.Charges.AddNew();
			charge3.JR_AC = Helper.ChargeCodes["FRT"].PK;
			charge3.JR_OSCostAmt = 70m;
			charge3.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;
			charge3.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

			var charge4 = testJob.Charges.AddNew();
			charge4.JR_AC = dsbCharge1.PK;
			charge4.JR_OSCostAmt = 80m;
			charge4.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;
			charge4.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 10m, JR_OSSellAmt = 10m, JR_SellRated =  true, JR_CostRated = false },
				new AssertionCharge { ChargeCode = dsbCharge1.AC_Code, JR_OSCostAmt = 20m, JR_OSSellAmt = 20m, JR_SellRated = true, JR_CostRated = true },
				new AssertionCharge { ChargeCode = "PSS", JR_OSCostAmt = 30m, JR_OSSellAmt = 30m, JR_SellRated =  false, JR_CostRated = true },
				new AssertionCharge { ChargeCode = dsbCharge2.AC_Code, JR_OSCostAmt = 40m, JR_OSSellAmt = 40m, JR_SellRated = true, JR_CostRated = true },
				new AssertionCharge { ChargeCode = dsbCharge3.AC_Code, JR_OSCostAmt = 50m, JR_OSSellAmt = 50m, JR_SellRated = false, JR_CostRated = false },
				new AssertionCharge { ChargeCode = dsbCharge4.AC_Code, JR_OSCostAmt = 60m, JR_OSSellAmt = 60m, JR_SellRated = false, JR_CostRated = false },
			};

			AutorateAndAssert(expected, shipment, NewClient, job: testJob);
			AssertEquals(6, testJob.Charges.Count);

			//If we manually change an AUTO-RATED DSB charge, JR_SellRated and JR_CostRated remains unchanged. Meaning that they will remain both as true.
			//This case is not only for DSB charge, but also for other charges.

			Charge autoratedFRTCharge = testJob.Charges.ContainsChargeCode(Helper.ChargeCodes["FRT"]);
			autoratedFRTCharge.JR_OSCostAmt = 11m;
			AssertEquals(10m, autoratedFRTCharge.JR_OSSellAmt);
			AssertEquals(true, autoratedFRTCharge.JR_SellRated);
			AssertEquals(false, autoratedFRTCharge.JR_CostRated);

			Charge autoratedDSBCharge = testJob.Charges.ContainsChargeCode(dsbCharge1);
			autoratedDSBCharge.JR_OSCostAmt = 21m;
			AssertEquals(21m, autoratedDSBCharge.JR_OSSellAmt);
			AssertEquals(true, autoratedDSBCharge.JR_SellRated);
			AssertEquals(true, autoratedDSBCharge.JR_CostRated);

			Charge manualDSBCharge = testJob.Charges.ContainsChargeCode(dsbCharge3);
			manualDSBCharge.JR_OSCostAmt = 55m;
			AssertEquals(55m, manualDSBCharge.JR_OSSellAmt);
			AssertEquals(false, manualDSBCharge.JR_SellRated);
			AssertEquals(false, manualDSBCharge.JR_CostRated);

			Charge quickCalcDSBCharge = testJob.Charges.ContainsChargeCode(dsbCharge3);
			quickCalcDSBCharge.JR_OSCostAmt = 66m;
			AssertEquals(66m, quickCalcDSBCharge.JR_OSSellAmt);
			AssertEquals(false, quickCalcDSBCharge.JR_SellRated);
			AssertEquals(false, quickCalcDSBCharge.JR_CostRated);
		}

		#endregion

		#region Autorating Consol Costs On Shipment with Rate selector

		[GuiTest]
		public void TestAutoratingOnShipment_WhenOneShipmentOneConsolWithSameContainerMode_ShouldShowCargoGuideRateSelector()
		{
			var carrier = TransportProvider1;

			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			consignee.OH_IsDebtor = true;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "USLAX", 100);
			shipment.JS_PackingMode = "LSE";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_AWBServiceLevel = "STD";

			var transport = consol.Transports[0];
			// when dates are empty, Consol form will have validation errors but that should not stop RS showing up with today's date.
			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_ETA = ZDateTime.Empty;

			JobHeader job = new JobHeader.Loader(shipment).TryLoadOrCreate();

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateSelectorForm selectorForm)
					{
						timesRateSelectorFormIsShown++;
						selectorForm.BtnSkipRateSelectionClick(null, null);
					}
				});

				AutorateWithManualSelectAndAssert("Autorate", null, shipment, consignee);

				AssertEquals("Rate Selector should be shown once", 1, timesRateSelectorFormIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingOnShipment_WhenOneShipmentOneConsolWithSameContainerMode_ShouldShowCargoSphereRateSelector()
		{
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			consignee.OH_IsDebtor = true;

			var shipment = CreateForwardingShipment(TransportModes.Sea, consignor.PK, consignee.PK, "AUSYD", "USLAX", 100);
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_PrepaidCollect = "PPD";

			var c = consol.Containers.AddNew();
			c.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			c.JC_ContainerCount = 1;

			var transport = consol.Transports[0];
			// when dates are empty, Consol form will have validation errors but that should not stop RS showing up with today's date.
			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_ETA = ZDateTime.Empty;

			JobHeader job = new JobHeader.Loader(shipment).TryLoadOrCreate();

			Factory.Save();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var timesRateSelectorFormIsShown = 0;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (form is RateChooserForm rateChooser)
					{
						timesRateSelectorFormIsShown++;
						rateChooser.SkipRateSelectionButton_Click(null, EventArgs.Empty);
					}
				});

				AutorateWithManualSelectAndAssert("Autorate", null, shipment, consignee);

				AssertEquals("Rate Selector should be shown once", 1, timesRateSelectorFormIsShown);
			}
		}

		[GuiTest]
		public void TestAutoratingOnShipment_WhenOneShipmentOneConsolWithSameContainerMode_RateSelectorWillUseConsolPaymentTermAsDefault()
		{
			var carrier = TransportProvider1;

			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();
			consignee.OH_IsDebtor = true;

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, consignee.PK, "AUSYD", "USLAX", 100);
			shipment.JS_PackingMode = "LSE";
			shipment.JS_INCO = IncoTerms.ExWorks;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;
			consol.CreditorPK = carrier.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol.JK_AWBServiceLevel = "STD";

			var transport = consol.Transports[0];
			// when dates are empty, Consol form will have validation errors but that should not stop RS showing up with today's date.
			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_ETA = ZDateTime.Empty;

			JobHeader job = new JobHeader.Loader(shipment).TryLoadOrCreate();

			Factory.Save();

			Assert("Shipment payment term is collect", shipment.IsCollect);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var filterValueIsPrepaid = false;
				var filterValueIsCollect = false;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown((form) =>
				{
					if (form is RateSelectorForm selectorForm)
					{
						Application.DoEvents();
						var filterControl = (RateSelectorFilterStripControl)selectorForm.Controls.Find("stripControl", true)[0];
						var activeModuleFilters = filterControl.FilterBusinessObject.ActiveModuleFilters;

						var paymentTermFiler = activeModuleFilters.OfType<WiseRatesModuleTextFilter>().Single(x => x.OriginalCode == RateEntryFilterUtility.Constants.Codes.PaymentTerm);

						filterValueIsPrepaid = paymentTermFiler.Property == "PPD";
						filterValueIsCollect = paymentTermFiler.Property == "CCX";

						selectorForm.BtnSkipRateSelectionClick(null, null);
					}
				});

				consol.JK_PrepaidCollect = "PPD";
				AutorateWithManualSelectAndAssert("Autorate", null, shipment, consignee);
				Assert("Filter default value should be prepaid", filterValueIsPrepaid);

				consol.JK_PrepaidCollect = "CCX";
				AutorateWithManualSelectAndAssert("Autorate", null, shipment, consignee);
				Assert("Filter default value should be collect", filterValueIsCollect);
			}
		}

		#endregion

		#region HBL Delivery Priority

		public void TestHBLDeliveryPriority()
		{
			var clientRate1 = CreateClientRate();
			var frt1 = new RateLineChargeCodes("FRT", 10);
			var caf1 = new RateLineChargeCodes("CAF", 20);
			clientRate1.AddFlatCharge(frt1.ChargeCode, frt1.Amount);
			clientRate1.AddFlatCharge(caf1.ChargeCode, caf1.Amount);

			var clientRate2 = CreateClientRate();
			clientRate2.TI_HBLDeliveryMode = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			var frt2 = new RateLineChargeCodes("FRT", 30);
			var baf2 = new RateLineChargeCodes("BAF", 40);
			clientRate2.AddFlatCharge(frt2.ChargeCode, frt2.Amount);
			clientRate2.AddFlatCharge(baf2.ChargeCode, baf2.Amount);

			#region Shipment and Consol

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP");
			shipment.JS_HBLContainerPackModeOverride = string.Empty;

			Factory.Save();

			#endregion

			var revenueCalculationDescription = DescriptionHelpers.FormatWithTab("HBL Delivery Mode:");

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 10m, JR_OSSellAmt = 10m },
				new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m, JR_OSSellAmt = 20m },
			};

			//shipment's JS_HBLContainerPackModeOverride is empty, only Rates with Blank HBL Delivery Mode should be matched
			AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false, autorateRevenue: true);

			#region assert

			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m, JR_OSSellAmt = 20m },
				new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 30m, JR_OSSellAmt = 30m, RevenueCalculationDescription = $"{revenueCalculationDescription}DOOR/DOOR" },
				new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 40m, JR_OSSellAmt = 40m, RevenueCalculationDescription = $"{revenueCalculationDescription}DOOR/DOOR" }
			};

			//No registry settings for HBL Delivery Priority, use Shipment HBL Delivery as Priority
			AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false, autorateRevenue: true);

			var configurations = new HBLDeliveryPriorityConfigCollection();
			var configuration = configurations.AddNew();
			configuration.ContainerMode = "FCL";
			configuration.HBLDeliveryMode = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			var setting = configuration.Settings.AddNew();
			setting.HBLDeliveryModePriority = Constants.HBLDeliveryModes.Codes.CFS_DOOR;
			var setting1 = configuration.Settings.AddNew();
			setting1.HBLDeliveryModePriority = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			var setting2 = configuration.Settings.AddNew();
			setting2.HBLDeliveryModePriority = Constants.HBLDeliveryModes.Codes.CFS_CFS;

			using (RatingDataRegistry.Instance.HBLDeliveryPriority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				var clientRate3 = CreateClientRate();
				clientRate3.TI_HBLDeliveryMode = Constants.HBLDeliveryModes.Codes.CFS_DOOR;
				var frt3 = new RateLineChargeCodes("FRT", 50);
				var war3 = new RateLineChargeCodes("WAR", 60);
				clientRate3.AddFlatCharge(frt3.ChargeCode, frt3.Amount);
				clientRate3.AddFlatCharge(war3.ChargeCode, war3.Amount);

				Factory.Save();

				expected = new[]
				{
					new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m, JR_OSSellAmt = 20m },
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 50m, JR_OSSellAmt = 50m, RevenueCalculationDescription = $"{revenueCalculationDescription}CFS/DOOR" }, // CFS/DOOR priority is greater than DOOR/DOOR in registry
					new AssertionCharge { ChargeCode = "WAR", JR_OSCostAmt = 60m, JR_OSSellAmt = 60m, RevenueCalculationDescription = $"{revenueCalculationDescription}CFS/DOOR" },
					new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 40m, JR_OSSellAmt = 40m, RevenueCalculationDescription = $"{revenueCalculationDescription}DOOR/DOOR" }
				};

				AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false, autorateRevenue: true);
			}

			configuration.Settings.Remove(setting1.PK);

			using (RatingDataRegistry.Instance.HBLDeliveryPriority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations))
			{
				//we do not have DOOR/DOOR in registry, so we filter rates with DOOR/DOOR modes.
				//From Sarah: The value itself should always be in the list or we further complicate the things.
				expected = new[]
				{
					new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m, JR_OSSellAmt = 20m },
					new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 50m, JR_OSSellAmt = 50m, RevenueCalculationDescription = $"{revenueCalculationDescription}CFS/DOOR" },
					new AssertionCharge { ChargeCode = "WAR", JR_OSCostAmt = 60m, JR_OSSellAmt = 60m, RevenueCalculationDescription = $"{revenueCalculationDescription}CFS/DOOR" },
				};

				AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false, autorateRevenue: true);
			}

			#endregion
		}

		#endregion

		#region Cross Trade Comparer

		public void TestCrossTradeComparer()
		{
			var origin = "HKHKG";
			var destination = "SGSIN";

			var clientRate1 = Helper.NewClientRate(Consignor);
			var rateEntry = clientRate1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, string.Empty, string.Empty);
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_IsCrossTrade = true;

			var frt1 = new RateLineChargeCodes("FRT", 10);
			var caf1 = new RateLineChargeCodes("CAF", 20);
			rateEntry.AddFlatCharge(frt1.ChargeCode, frt1.Amount);
			rateEntry.AddFlatCharge(caf1.ChargeCode, caf1.Amount);

			var rateEntry2 = clientRate1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, string.Empty, string.Empty);
			rateEntry2.TI_IsCrossTrade = false;
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var frt2 = new RateLineChargeCodes("FRT", 30);
			var baf2 = new RateLineChargeCodes("BAF", 40);
			rateEntry2.AddFlatCharge(frt2.ChargeCode, frt2.Amount);
			rateEntry2.AddFlatCharge(baf2.ChargeCode, baf2.Amount);

			var shipment = CreateShipment(origin: origin, destination: destination);
			var consol = CreateConsol(origin: origin, destination: destination);
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP");

			Factory.Save();

			Assert(shipment.IsCrossTrade());

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 10m, JR_OSSellAmt = 10m }, //Cross trade takes priority over non cross trade when origin and destination are empty
				new AssertionCharge { ChargeCode = "CAF", JR_OSCostAmt = 20m, JR_OSSellAmt = 20m },
				new AssertionCharge { ChargeCode = "BAF", JR_OSCostAmt = 40m, JR_OSSellAmt = 40m },
			};

			AutorateAndAssert(expected, shipment, Consignor, autorateCosts: false, autorateRevenue: true);

			var expectedLogLines = new[]
			{
				"Information: RateLine Filtered FRT-FLT-Client Rate CONSIGNOR1\treason:\toverridden by FRT-FLT-Client Rate CONSIGNOR1 by Cross Trade comparer"
			};

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should have rate line removal reasons:", expectedLogLines);
		}

		#endregion

		#region DAP Shipment

		[TestDate(2020, 01, 01)]
		public void TestFRT_ORG_Charges_AreNot_Autorated_When_DAP_Shipment_IsAtDestination()
		{
			Helper.NewInternationalZone("OCEA", null, "AU", "NZ");

			var clientRate1 = CreateClientRate(origin: "OCEA", destination: "CA");
			var frt1 = new RateLineChargeCodes("FRT", 10);
			clientRate1.AddFlatCharge(frt1.ChargeCode, frt1.Amount);

			var clientRate2 = CreateClientRate(category: "ORG", origin: "OCEA", destination: "CA");
			var org = new RateLineChargeCodes("ODOC", 10);
			clientRate2.AddFlatCharge(org.ChargeCode, org.Amount);

			var clientRate3 = CreateClientRate(category: "DST", origin: "", destination: "CA");
			var dst = new RateLineChargeCodes("DDOC", 20);
			clientRate3.AddFlatCharge(dst.ChargeCode, dst.Amount);

			#region Shipment and Consol

			var shipment = CreateShipment(origin: "AUALX", destination: "CATOR");
			var consol = CreateConsol(origin: "AUALX", destination: "CATOR");
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP");
			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;

			Factory.Save();

			#endregion

			var importCollection = RatingDataRegistry.Instance.ImportPrepaidPriorities.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			importCollection.AddNew(RatingDebtorOrgTypes.LC);
			importCollection.AddNew(RatingDebtorOrgTypes.CNE);
			RatingDataRegistry.Instance.ImportPrepaidPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, importCollection);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Canada))
			{
				var expected = new[]
				{
					new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 20m },
				};

				Assert("Shipment is import", shipment.IsImport());
				var adapterToTest = shipment.RatingAdapter;
				AssertNull("Via", adapterToTest.GetVia(CostSell.Cost));
				AssertNull("Via", adapterToTest.GetVia(CostSell.Revenue));

				AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false, autorateRevenue: true);

				var expectedLogLines = new[]
				{
					"Information: RateLine Filtered FRT-FLT-20GP-Client Rate CONSIGNEE1\treason:\tFRT charge group is not applicable for AUALX-CATOR Import PPD\r\n",
					"Information: RateLine Filtered ODOC-FLT-20GP-Client Rate CONSIGNEE1\treason:\tORG charge group is not applicable for AUALX-CATOR Import PPD\r\n",
				};

				AssertAutoratingAuditLogNoteContainsLines(shipment, "Should have rate line removal reasons:", expectedLogLines);
				// by setting new Destination different than Consol, now we have Via
				shipment.JS_RL_NKDestination = "CAADL";

				Factory.Save();

				adapterToTest = shipment.RatingAdapter;

				AssertEquals("CATOR", adapterToTest.GetVia(CostSell.Cost).Code);
				AssertEquals("CATOR", adapterToTest.GetVia(CostSell.Revenue).Code);
				Assert("Shipment is import", shipment.IsImport());

				AutorateAndAssert(expected, shipment, Consignee, autorateCosts: false, autorateRevenue: true);
				AssertAutoratingAuditLogNoteContainsLines(shipment, "Should have rate line removal reasons:", expectedLogLines);
			}
		}

		#endregion

		#region Events

		public void TestAutorate_ShouldCreateCAREventWithAuditInfo()
		{
			CreateCostRate(container: "20GP").AddPerUnitCharge("FRT", 100, unit: "CN");

			var shipment = CreateShipment();
			shipment.JS_UniqueConsignRef = "666";

			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			consol.AddContainer("20GP", packLines: new[]
			{
				shipment.AddPackLine(weight: 10)
			});

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
					CostCalculationDescription = "FRT: 1 20GP Container(s) @ USD 100.00/Container",
				},
			};

			AutorateAndAssert(expectedCharges, shipment, Consignee, autorateRevenue: false);

			var log = shipment.Logs.MostRecentLogByEventTime(AutoEvents.ChargesHaveBeenAutoRated);
			AssertEquals(
				"Expected log parameter 'Type' to match 'Shipment'.",
				"Shipment",
				log.Parameters[EventReferenceParameters.Type]
			);
			AssertEquals(
				"Expected log parameter 'JobNumber' to match '666'.",
				"666",
				log.Parameters[EventReferenceParameters.JobNumber]
			);
		}

		#endregion

		#region AWB

		public void TestAutorateShipment_WithOuterAndInnerPackLines_AWBGrossWeightShouldBeOuterWeight()
		{
			// SETUP: outer pack 120KG, inner pack 100KG, spot rate AUD 10/KG.
			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", weight: 120m);
			shipment.AddInnerPackLine(weight: 100);

			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_UnitFreightRate = 10m;
			shipment.JS_RX_NKFrtRateCurrency = "AUD";

			var expectedCharge = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalCostAmt = 1200
				}
			};
			AutorateAndAssert(expectedCharge, shipment, NewClient, autorateCosts: false);

			shipment.PopulateAWB();
			var awbRateLines = shipment.AWBHeaderManager.AWBHeader.AWBRateLines.OfType<ExportAWBRateLine>();
			var rateLine = awbRateLines.Single(x => !x.ER_RateClass.IsEmpty);
			AssertEquals("AWB should show outer weight", (ZDecimal)120, rateLine.ER_GrossWeight);
		}

		public void TestAutorateConsol_WithOuterAndInnerPackLines_AWBGrossWeightShouldBeOuterWeight()
		{
			const string origin = "AUSYD";
			const string destination = "USLAX";

			var standardCosting = Helper.NewCosting(null);
			var entryTACT = standardCosting.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination, "FRT", 150, "KG", currency: "AUD");
			entryTACT.TI_IsTact = true;

			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, origin, destination, "FRT", 100, "KG", currency: "AUD");

			Factory.Save();

			// SETUP: outer pack 120KG, inner pack 100KG
			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination, weight: 120m);
			shipment.AddInnerPackLine(weight: 100);
			var consol = CreateForwardingConsol(TransportModes.Air, origin, destination, TransportProvider1, shipment, PaymentType.Prepaid);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_LocalCostAmount = 12000
				}
			};
			AutoCostAndAssert("", null, expectedCosts, consol, autorateRevenue: false);

			shipment.PopulateAWB();
			var awbRateLines = shipment.AWBHeaderManager.AWBHeader.AWBRateLines.OfType<ExportAWBRateLine>();
			AssertEquals(
				"AWB should show outer weight",
				(ZDecimal)120,
				awbRateLines.Single(x => !x.ER_RateClass.IsEmpty).ER_GrossWeight
			);

			consol.PopulateAWB();
			awbRateLines = consol.AWBHeaderManager.AWBHeader.AWBRateLines.OfType<ExportAWBRateLine>();
			AssertEquals(
				"AWB should show outer weight",
				(ZDecimal)120,
				awbRateLines.Single(x => !x.ER_RateClass.IsEmpty).ER_GrossWeight
			);
		}

		public void TestAWB_ShipmentIsNotAutoratedByDestinationSide_ShouldStillShowPrepaidAndCollectCharges()
		{
			// Destination company
			var destinationCompany = Factory.NewWithValidTestData<GlbCompany>();
			destinationCompany.GC_RN_NKCountryCode = "UA";
			var destinationOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var destinationBranch = destinationCompany.Branches.AddNew();
			destinationBranch.GB_Code = "UAA";
			destinationBranch.GB_OH_OrgProxy = destinationOrgProxy.PK;
			destinationBranch.GB_RL_NKHomePort = "UAIEV";
			destinationBranch.GB_RN_NKCountryCode = "UA";
			destinationBranch.GB_GC = destinationCompany.PK;

			Factory.Save();

			var originHeader = Helper.NewClientRate(Consignor);
			var originEntry = originHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AUSYD", "UAIEV");
			originEntry.AddFlatCharge("ODOC", 200);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, destinationBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var destinationHeader = Helper.NewClientRate(Consignee);
				var destinationEntry1 = destinationHeader.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV");
				destinationEntry1.AddPerUnitCharge("FRT", 10, "KG");
				var destinationEntry2 = destinationHeader.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "AUSYD", "UAIEV");
				destinationEntry2.AddFlatCharge("DDOC", 100);
			}

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "UAIEV", weight: 666m);
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			var config = collection.AddNew();
			config.TransportMode = "AIR";
			config.ExportCountry = "AU";
			config.ImportCountry = "UA";

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AutorateAndAssert(
					new[]
					{
						new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_LocalSellAmt = 200m,
							RevenueCalculationDescription = "ODOC: Base Rate AUD 200.00"
						}
					},
					shipment,
					Consignor,
					autorateCosts: false);

				shipment.JobHeader.AgentCollectPK = destinationOrgProxy.PK;
				Factory.Save();

				shipment.PopulateAWB();

				var expectedCharges = new[]
				{
					new { EO_ChargeDescription = "Origin Documentation Fee", EO_PPDCLT = "PPD", EO_Amount = 200m },
					new { EO_ChargeDescription = "Destination Documentation Fee", EO_PPDCLT = "PPD", EO_Amount = 100m }
				};

				var actualCharges = shipment.AWBHeaderManager.AWBHeader.AWBOtherCharges.Select(c => $"{c.EO_ChargeDescription}|{c.EO_PPDCLT}|{c.EO_Amount}").ToArray();
				var expectedChargesStrings = expectedCharges.Select(c => $"{c.EO_ChargeDescription}|{c.EO_PPDCLT}|{c.EO_Amount}.00").ToArray();

				AssertContainsExactElementsInAnyOrder(
					"Even though overseas company did not autorate the shipment, collect charges should come through",
					expectedChargesStrings,
					actualCharges
				);

				// Check that it didn't create any additional job headers or charges. It should autorate
				// the destination charges in memory and don't do anything to the destination job
				// (like creating job header or charges).
				var jobHeaders = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK));
				var actualJobHeaderPKs = jobHeaders.Select(job => job.PK).ToArray();
				var expectedJobHeaderPKs = new[] { shipment.JobHeader.PK };

				AssertContainsExactElementsInAnyOrder(
					"The job headers must match the shipment's job header PK",
					expectedJobHeaderPKs,
					actualJobHeaderPKs
				);
			}
		}

		public void TestAWB_ShipmentIsAutoRatedByAnotherOriginCompany_ShouldStillShowItsCharges()
		{
			var anotherAustralianCompany = Factory.NewWithValidTestData<GlbCompany>();
			anotherAustralianCompany.GC_RN_NKCountryCode = "AU";
			var anotherAustralianBranch = anotherAustralianCompany.Branches.AddNew();
			anotherAustralianBranch.GB_Code = "AUU";
			anotherAustralianBranch.GB_RL_NKHomePort = "AUSYD";
			anotherAustralianBranch.GB_RN_NKCountryCode = "AU";
			anotherAustralianBranch.GB_GC = anotherAustralianCompany.PK;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "UAIEV", weight: 666m);
			shipment.JS_INCO = "CFR";

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, anotherAustralianBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var ratingHeader = Helper.NewClientRate(Consignor);
				var originEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AUSYD", "UAIEV");
				originEntry.AddFlatCharge("ODOC", 200);
				var frtEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV");
				frtEntry.RateLines.RemoveAndDeleteAll();
				frtEntry.AddPerUnitCharge("FRT", 10, "KG");

				Factory.Save();

				AutorateAndAssert(
					new[]
					{
						new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_LocalSellAmt = 200,
							RevenueCalculationDescription = "ODOC: Base Rate AUD 200.00"
						},
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_LocalSellAmt = 6660,
							RevenueCalculationDescription = "FRT: 666 Kilogram(s) @ AUD 10.00/KG"
						}
					},
					shipment,
					Consignor,
					autorateCosts: false);

				Factory.Save();
			}

			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			var config = collection.AddNew();
			config.TransportMode = "AIR";
			config.ExportCountry = "AU";
			config.ImportCountry = "UA";

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				new JobHeader.Loader(shipment).TryLoadOrCreate();

				shipment.PopulateAWB();

				var expectedOtherCharges = new[]
				{
					"Origin Documentation Fee|PPD|200.0000"
				};

				var actualOtherCharges = shipment.AWBHeaderManager.AWBHeader.AWBOtherCharges.Select(c => 
					$"{c.EO_ChargeDescription.ToString()}|{c.EO_PPDCLT.ToString()}|{c.EO_Amount}").ToArray();

				AssertContainsExactElementsInAnyOrder(
					"Even though this company didn't autorate the shipment, there is another origin company that did, so, we load charges from it",
					expectedOtherCharges,
					actualOtherCharges
				);
			}
		}

		public void TestPopulateRateLinesSectionInfo_BcnShipment_ShouldPopulateWeightFromTheShipmentItSelf()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV");
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG).GetCalculator<UnitCalculator>().PerUnit = 1;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "UAIEV";
			consol.JK_ConsolMode = ContainerModes.BuyersConsol;

			var leadShipment = consol.Shipments.AddNew();
			leadShipment.ConsigneePK = Consignee.PK;
			leadShipment.ConsignorPK = Consignor.PK;
			leadShipment.JS_RL_NKLoadPort = "AUSYD";
			leadShipment.JS_RL_NKDestination = "UAIEV";
			leadShipment.JS_TransportMode = TransportModes.Air;
			leadShipment.JS_PackingMode = ContainerModes.BuyersConsol;
			leadShipment.JS_ShipmentType = ShipmentTypes.BuyersConsolLead;
			leadShipment.JS_ActualWeight = 400;
			leadShipment.JS_ActualChargeable = 450;
			leadShipment.JS_INCO = ZString.Empty;

			var subShipment = consol.Shipments.AddNew();
			subShipment.ConsigneePK = Consignee.PK;
			subShipment.ConsignorPK = Consignor.PK;
			subShipment.JS_RL_NKLoadPort = "AUSYD";
			subShipment.JS_RL_NKDestination = "UAIEV";
			subShipment.JS_TransportMode = TransportModes.Air;
			subShipment.JS_PackingMode = ContainerModes.BuyersConsol;
			subShipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
			subShipment.JS_ActualWeight = 300;
			subShipment.JS_ActualChargeable = 350;

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalSellAmt = 800,
					RevenueCalculationDescription = "FRT: 800 Kilogram(s) @ AUD 1.00/KG"
				}
			};

			AutorateAndAssert(null, expectedCharges, leadShipment, Consignee, autorateCosts: false);

			leadShipment.PopulateAWB();

			var line = leadShipment.AWBHeader.AWBRateLines[0];
			CombineAssertions("Weight should come from the shipment itself rather than from calculation", () =>
			{
				AssertEquals(nameof(line.ER_GrossWeight), 400m, line.ER_GrossWeight);
				AssertEquals(nameof(line.ER_ChargeableWeight), 450m, line.ER_ChargeableWeight);
				AssertEquals(nameof(line.ER_Total), 800m, line.ER_Total);
			});
		}

		public void TestPopulateRateLinesSectionInfo_ScnShipment_ShouldPopulateWeightFromTheShipmentItSelf()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.SCN, "AUSYD", "UAIEV");
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG).GetCalculator<UnitCalculator>().PerUnit = 1;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "UAIEV";
			consol.JK_ConsolMode = ContainerModes.ShippersConsol;

			var leadShipment = consol.Shipments.AddNew();
			leadShipment.JS_RL_NKLoadPort = "AUSYD";
			leadShipment.JS_RL_NKDestination = "UAIEV";
			leadShipment.JS_TransportMode = TransportModes.Air;
			leadShipment.JS_PackingMode = ContainerModes.ShippersConsol;
			leadShipment.JS_ShipmentType = ShipmentTypes.ShippersConsolLead;
			leadShipment.JS_ActualWeight = 400;
			leadShipment.JS_ActualChargeable = 450;
			leadShipment.ConsignorPK = Consignor.PK;
			leadShipment.JS_INCO = ZString.Empty;

			var subShipment = consol.Shipments.AddNew();
			subShipment.JS_RL_NKLoadPort = "AUSYD";
			subShipment.JS_RL_NKDestination = "UAIEV";
			subShipment.JS_TransportMode = TransportModes.Air;
			subShipment.JS_PackingMode = ContainerModes.ShippersConsol;
			subShipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
			subShipment.JS_ActualWeight = 300;
			subShipment.JS_ActualChargeable = 350;
			subShipment.ConsignorPK = Consignor.PK;

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalSellAmt = 800,
					RevenueCalculationDescription = "FRT: 800 Kilogram(s) @ AUD 1.00/KG"
				}
			};

			AutorateAndAssert(null, expectedCharges, leadShipment, Consignor, autorateCosts: false);

			leadShipment.PopulateAWB();

			var line = leadShipment.AWBHeader.AWBRateLines[0];
			CombineAssertions("Weight should come from the shipment itself rather than from calculation", () =>
			{
				AssertEquals(nameof(line.ER_GrossWeight), 400m, line.ER_GrossWeight);
				AssertEquals(nameof(line.ER_ChargeableWeight), 450m, line.ER_ChargeableWeight);
				AssertEquals(nameof(line.ER_Total), 800m, line.ER_Total);
			});
		}

		#endregion

		#region HBL

		public void TestHBL_ShipmentIsNotAutoratedByDestinationSide_ShouldStillShowPrepaidAndCollectCharges()
		{
			// Destination company
			var destinationCompany = Factory.NewWithValidTestData<GlbCompany>();
			destinationCompany.GC_RN_NKCountryCode = "UA";
			var destinationOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var destinationBranch = destinationCompany.Branches.AddNew();
			destinationBranch.GB_Code = "UAA";
			destinationBranch.GB_OH_OrgProxy = destinationOrgProxy.PK;
			destinationBranch.GB_RL_NKHomePort = "UAIEV";
			destinationBranch.GB_RN_NKCountryCode = "UA";
			destinationBranch.GB_GC = destinationCompany.PK;

			Factory.Save();

			var originHeader = Helper.NewClientRate(Consignor);
			var originEntry = originHeader.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "UAIEV");
			originEntry.AddFlatCharge("ODOC", 200);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, destinationBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var destinationHeader = Helper.NewClientRate(Consignee);
				var destinationEntry1 = destinationHeader.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "UAIEV");
				destinationEntry1.AddPerUnitCharge("FRT", 10, "KG");
				var destinationEntry2 = destinationHeader.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "AUSYD", "UAIEV");
				destinationEntry2.AddFlatCharge("DDOC", 100);
			}

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "UAIEV", weight: 666m);
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			var config = collection.AddNew();
			config.TransportMode = "SEA";
			config.ExportCountry = "AU";
			config.ImportCountry = "UA";

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AutorateAndAssert(
					new[]
					{
						new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_LocalSellAmt = 200,
							RevenueCalculationDescription = "ODOC: Base Rate AUD 200.00"
						}
					},
					shipment,
					Consignor,
					autorateCosts: false);

				shipment.JobHeader.AgentCollectPK = destinationOrgProxy.PK;
				Factory.Save();

				var chargesCollection = new ChargesCollection(shipment, new HouseBillLookups(Factory), false, true);
				var actualCharges = chargesCollection.All.Select(c => new
				{
					ChargeCode = (string)c.ChargeCode.Code,
					IsPrepaid = (bool)c.IsPrepaid,
					Amount = c.LocalSell.Amount
				})
				.Select(c => $"{c.ChargeCode}|{c.IsPrepaid}|{c.Amount}")
				.ToArray();

				var expectedCharges = new[]
				{
					"ODOC|True|200.00",
					"FRT|False|6660.00",
					"DDOC|False|100.00"
				};

				AssertContainsExactElementsInAnyOrder(
					"Even though overseas company did not autorate the shipment, collect charges should come through",
					expectedCharges,
					actualCharges
				);

				// Check that it didn't create any additional job headers or charges. It should autorate
				// the destination charges in memory and don't do anything to the destination job
				// (like creating job header or charges).
				var jobHeaders = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK));
				var actualJobHeadersPK = jobHeaders.Select(job => job.PK).ToArray();
				var expectedJobHeadersPK = new[] { shipment.JobHeader.PK };

				AssertContainsExactElementsInAnyOrder(expectedJobHeadersPK, actualJobHeadersPK);
			}
		}

		#endregion

		#region Matching Locations

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_AllCorrectMatchingLocations_WhenMultiRouteAutoratingDisabled()
			=> AssertAutoratingWithMatchingLocations_AllCorrectMatchingLocations(false);

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_AllCorrectMatchingLocations_WhenMultiRouteAutoratingEnabled()
			=> AssertAutoratingWithMatchingLocations_AllCorrectMatchingLocations(true);

		public void AssertAutoratingWithMatchingLocations_AllCorrectMatchingLocations(bool multiRouteAutoratingEnabled)
		{
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, multiRouteAutoratingEnabled))
			{
				AssertAutoratingWithMatchingLocations
				(
					"Entry1 must takes priority because of all correct matching locations",
					locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
					{
						{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
						{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
						{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
						{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
					},
					locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>(),
					expectedEntry1: true
				);
			}
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_WrongFirstLoad()
		{
			AssertAutoratingWithMatchingLocations
			(
				"Entry1 must be removed because of the wrong First Load Port",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "SGSIN" },
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>(),
				expectedEntry1: false,
				expectedLogMessage: "Information: RateEntry Filtered Standard Costs (TACT/General Rates) reason: First Load didn't match job AUBNE,AU,AUEC."
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_WrongLastDischarge()
		{
			AssertAutoratingWithMatchingLocations
			(
				"Entry1 must be removed because of the wrong Last Discharge Port",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
					{ RateEntrySchema.TI_LastDischargeLRC, "SGSIN" },
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>(),
				expectedEntry1: false,
				expectedLogMessage: "Information: RateEntry Filtered Standard Costs (TACT/General Rates) reason: Last Discharge didn't match job DEBER,DE."
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_WrongFirstRouteSetLoadPort()
		{
			AssertAutoratingWithMatchingLocations
			(
				"Entry1 must be removed because of the wrong First Route Set Load Port",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "SGSIN" },
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>(),
				expectedEntry1: false,
				expectedLogMessage: "Information: RateEntry Filtered Standard Costs (TACT/General Rates) reason: First Route Set Load Port didn't match job AUSYD,AU,AUEC."
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_WrongLastRouteSetDischargePort()
		{
			AssertAutoratingWithMatchingLocations
			(
				"Entry1 must be removed because of the wrong Last Route Set Discharge Port",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "SGSIN" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>(),
				expectedEntry1: false,
				expectedLogMessage: "Information: RateEntry Filtered Standard Costs (TACT/General Rates) reason: Last Route Set Discharge Port didn't match job DEHAM,DE."
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_FirstLoad_TakesPriorityOver_LastDischarge()
		{
			AssertAutoratingWithMatchingLocations
			(
				"FirstLoad_TakesPriorityOver_LastDischarge",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_FirstLoadLRC comparer"
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_FirstLoad_TakesPriorityOver_FirstRouteSetLoad()
		{
			AssertAutoratingWithMatchingLocations
			(
				"FirstLoad_TakesPriorityOver_FirstRouteSetLoad",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_FirstLoadLRC comparer"
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_FirstLoad_TakesPriorityOver_LastRouteSetDischarge()
		{
			AssertAutoratingWithMatchingLocations
			(
				"FirstLoad_TakesPriorityOver_LastRouteSetDischarge",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstLoadLRC, "AUBNE" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_FirstLoadLRC comparer"
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_LastDischarge_TakesPriorityOver_FirstRouteSetLoad()
		{
			AssertAutoratingWithMatchingLocations
			(
				"LastDischarge_TakesPriorityOver_FirstRouteSetLoad",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_LastDischargeLRC comparer"
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_LastDischarge_TakesPriorityOver_LastRouteSetDischarge()
		{
			AssertAutoratingWithMatchingLocations
			(
				"LastDischarge_TakesPriorityOver_LastRouteSetDischarge",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastDischargeLRC, "DEBER" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_LastDischargeLRC comparer"
			);
		}

		[GuiTest]
		public void TestAutoratingWithMatchingLocations_FirstRouteSetLoad_TakesPriorityOver_LastRouteSetDischarge()
		{
			AssertAutoratingWithMatchingLocations
			(
				"FirstRouteSetLoad_TakesPriorityOver_LastRouteSetDischarge",
				locationFieldsForEntry1: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_FirstRouteSetLoadPortLRC, "AUSYD" },
				},
				locationFieldsForEntry2: new Dictionary<SchemaStringColumn, ZString>
				{
					{ RateEntrySchema.TI_LastRouteSetDischargePortLRC, "DEHAM" },
				},
				expectedEntry1: true,
				expectedLogMessage: "Information: RateLine Filtered BAF-FLT-20GP-Standard Costs (TACT/General Rates)\treason:\toverridden by BAF-FLT-20GP-Standard Costs (TACT/General Rates) by TI_FirstRouteSetLoadPortLRC comparer"
			);
		}

		void AssertAutoratingWithMatchingLocations
		(
			string testFailMessage,
			Dictionary<SchemaStringColumn, ZString> locationFieldsForEntry1,
			Dictionary<SchemaStringColumn, ZString> locationFieldsForEntry2,
			bool expectedEntry1,
			string expectedLogMessage = null
		)
		{
			var origin = "AUBNE";
			var destination = "DEBER";

			var costing = Helper.NewCosting(null);

			var entry1 = costing.AddRateEntryWithFlatRateLine("FCL", "SEA", origin, destination, "BAF", 10m, "USD", "20GP");
			foreach (var locationField in locationFieldsForEntry1)
			{
				entry1[locationField.Key] = locationField.Value;
			}

			var entry2 = costing.AddRateEntryWithFlatRateLine("FCL", "SEA", origin, destination, "BAF", 20m, "USD", "20GP");
			foreach (var locationField in locationFieldsForEntry2)
			{
				entry2[locationField.Key] = locationField.Value;
			}

			var consol = CreateConsol("SEA", "FCL", origin, destination);
			consol.AddContainer("20GP");
			SetTransport("ROA", "PRE", "AUBNE", "AUSYD", consol.Transports[0]);
			SetTransport("SEA", "OTH", "AUSYD", "HKHKG", consol.Transports.AddNew());
			SetTransport("RAI", "OTH", "HKHKG", "SGSIN", consol.Transports.AddNew());
			SetTransport("SEA", "ONF", "SGSIN", "DEHAM", consol.Transports.AddNew());
			SetTransport("ROA", "ONF", "DEHAM", "DEBER", consol.Transports.AddNew());

			var shipment = CreateShipment(TransportModes.Sea, ContainerModes.FCL, origin: origin, destination: destination);
			consol.Shipments.Add(shipment);

			var expectedCosts = new[] { new AssertionCharge { ChargeCode = "BAF", JR_LocalCostAmt = expectedEntry1 ? 10m : 20M } };
			AutorateAndAssert(expectedCosts, shipment, Consignee, autorateRevenue: false, autorateCosts: true);

			if (!string.IsNullOrEmpty(expectedLogMessage))
			{
				AssertAutoratingAuditLogNoteContainsLines(shipment, testFailMessage, expectedLogMessage);
			}

			void SetTransport(string mode, string type, string loadPort, string dischargePort, Transport transport)
			{
				transport.JW_IsLinked = false;
				transport.JW_TransportMode = mode;
				transport.JW_TransportType = type;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = dischargePort;
			}
		}

		public void TestGivenChargesWithEmptyOriginOrDestination_WhenAutorateRevenueForImportCFRShipmentInPPDConsol_ThenORGAndFRTChargesShouldNotBeLoaded()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var consignor = Helper.NewOrgHeader();
			consignor.OH_RL_NKClosestPort = "DEHAM";
			consignor.OH_IsConsignor = true;

			var consignee = Helper.NewOrgHeader();
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_IsConsignee = true;

			var clientRate1 = Helper.NewClientRate(consignor);
			var clientORGRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "DE", "AU", "ODOC", 100m, currency: "AUD");
			clientORGRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientORGRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientDSTRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "DE", "AU", "DPCH", 200m, currency: "AUD");
			clientDSTRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientDSTRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientFRTRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "DE", "AU", "BAF", 300m, currency: "AUD");
			clientFRTRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientFRTRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientRate2 = Helper.NewClientRate(consignee);
			var clientORGRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "DE", "AU", "ODOC", 100m, currency: "AUD");
			clientORGRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientORGRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientDSTRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "DE", "AU", "DPCH", 200m, currency: "AUD");
			clientDSTRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientDSTRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientFRTRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "DE", "AU", "BAF", 300m, currency: "AUD");
			clientFRTRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientFRTRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			Factory.Save();

			var consol = CreateConsol(TransportModes.Air, ContainerModes.Loose, origin: "DEHAM", destination: "NZAKL");
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateShipment(TransportModes.Air, ContainerModes.Loose, origin: "DEHAM", destination: "AUSYD");
			shipment.JS_E_DEP = new ZDateTime(2020, 3, 5);
			shipment.JS_E_ARV = new ZDateTime(2020, 3, 10);
			shipment.JS_INCO = "CFR";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			consol.Shipments.Add(shipment);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DPCH",
					JR_OSCostAmt = 200m
				},
			};

			AutorateAndAssert(expected, shipment, consignee, autorateRevenue: true, autorateCosts: false);

			clientORGRateEntry1.TI_OriginLRC = "";
			clientORGRateEntry2.TI_OriginLRC = "";
			clientDSTRateEntry1.TI_DestinationLRC = "";
			clientDSTRateEntry2.TI_DestinationLRC = "";
			clientFRTRateEntry1.TI_OriginLRC = "";
			clientFRTRateEntry1.TI_DestinationLRC = "";
			clientFRTRateEntry2.TI_OriginLRC = "";
			clientFRTRateEntry2.TI_DestinationLRC = "";

			Factory.Save();

			AutorateAndAssert(expected, shipment, consignee, autorateRevenue: true, autorateCosts: false);

			consol.JK_RL_NKDischargePort = "AUSYD";

			AutorateAndAssert(expected, shipment, consignee, autorateRevenue: true, autorateCosts: false);
		}

		public void TestGivenChargesWithEmptyOriginOrDestination_WhenAutorateRevenueForImportCFRShipmentInCCXConsol_ThenORGChargesShouldNotBeLoaded()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var consignor = Helper.NewOrgHeader();
			consignor.OH_RL_NKClosestPort = "DEHAM";
			consignor.OH_IsConsignor = true;

			var consignee = Helper.NewOrgHeader();
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_IsConsignee = true;

			var clientRate1 = Helper.NewClientRate(consignor);
			var clientORGRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "DE", "AU", "ODOC", 100m, currency: "AUD");
			clientORGRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientORGRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientDSTRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "DE", "AU", "DPCH", 200m, currency: "AUD");
			clientDSTRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientDSTRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientFRTRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "DE", "AU", "BAF", 300m, currency: "AUD");
			clientFRTRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientFRTRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientRate2 = Helper.NewClientRate(consignee);
			var clientORGRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "DE", "AU", "ODOC", 100m, currency: "AUD");
			clientORGRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientORGRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientDSTRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "DE", "AU", "DPCH", 200m, currency: "AUD");
			clientDSTRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientDSTRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientFRTRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "DE", "AU", "BAF", 300m, currency: "AUD");
			clientFRTRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientFRTRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			Factory.Save();

			var consol = CreateConsol(TransportModes.Air, ContainerModes.Loose, origin: "DEHAM", destination: "NZAKL");
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var shipment = CreateShipment(TransportModes.Air, ContainerModes.Loose, origin: "DEHAM", destination: "AUSYD");
			shipment.JS_E_DEP = new ZDateTime(2020, 3, 5);
			shipment.JS_E_ARV = new ZDateTime(2020, 3, 10);
			shipment.JS_INCO = "CFR";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			consol.Shipments.Add(shipment);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DPCH",
					JR_OSCostAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 300m
				}
			};

			AutorateAndAssert(expected, shipment, consignee, autorateRevenue: true, autorateCosts: false);

			clientORGRateEntry1.TI_OriginLRC = "";
			clientORGRateEntry2.TI_OriginLRC = "";
			clientDSTRateEntry1.TI_DestinationLRC = "";
			clientDSTRateEntry2.TI_DestinationLRC = "";
			clientFRTRateEntry1.TI_OriginLRC = "";
			clientFRTRateEntry1.TI_DestinationLRC = "";
			clientFRTRateEntry2.TI_OriginLRC = "";
			clientFRTRateEntry2.TI_DestinationLRC = "";

			Factory.Save();

			AutorateAndAssert(expected, shipment, consignee, autorateRevenue: true, autorateCosts: false);

			consol.JK_RL_NKDischargePort = "AUSYD";

			AutorateAndAssert(expected, shipment, consignee, autorateRevenue: true, autorateCosts: false);
		}

		public void TestGivenChargesWithEmptyOriginOrDestination_WhenAutorateRevenueForExportFOBShipmentInCCXConsol_ThenDSTAndFRTChargesShouldNotBeLoaded()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var consignor = Helper.NewOrgHeader();
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Helper.NewOrgHeader();
			consignee.OH_RL_NKClosestPort = "DEHAM";
			consignee.OH_IsConsignee = true;

			var clientRate1 = Helper.NewClientRate(consignor);
			var clientORGRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "DE", "ODOC", 100m, currency: "AUD");
			clientORGRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientORGRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientDSTRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "AU", "DE", "DPCH", 200m, currency: "AUD");
			clientDSTRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientDSTRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientFRTRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "DE", "BAF", 300m, currency: "AUD");
			clientFRTRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientFRTRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientRate2 = Helper.NewClientRate(consignee);
			var clientORGRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "DE", "ODOC", 100m, currency: "AUD");
			clientORGRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientORGRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientDSTRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "AU", "DE", "DPCH", 200m, currency: "AUD");
			clientDSTRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientDSTRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientFRTRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "DE", "BAF", 300m, currency: "AUD");
			clientFRTRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientFRTRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			Factory.Save();

			var consol = CreateConsol(TransportModes.Air, ContainerModes.Loose, origin: "NZAKL", destination: "DEHAM");
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var shipment = CreateShipment(TransportModes.Air, ContainerModes.Loose, origin: "AUSYD", destination: "DEHAM");
			shipment.JS_E_DEP = new ZDateTime(2020, 3, 5);
			shipment.JS_E_ARV = new ZDateTime(2020, 3, 10);
			shipment.JS_INCO = "FOB";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			consol.Shipments.Add(shipment);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 100m
				},
			};

			AutorateAndAssert(expected, shipment, consignor, autorateRevenue: true, autorateCosts: false);

			clientORGRateEntry1.TI_OriginLRC = "";
			clientORGRateEntry2.TI_OriginLRC = "";
			clientDSTRateEntry1.TI_DestinationLRC = "";
			clientDSTRateEntry2.TI_DestinationLRC = "";
			clientFRTRateEntry1.TI_OriginLRC = "";
			clientFRTRateEntry1.TI_DestinationLRC = "";
			clientFRTRateEntry2.TI_OriginLRC = "";
			clientFRTRateEntry2.TI_DestinationLRC = "";

			Factory.Save();

			AutorateAndAssert(expected, shipment, consignor, autorateRevenue: true, autorateCosts: false);

			consol.JK_RL_NKLoadPort = "AUSYD";

			AutorateAndAssert(expected, shipment, consignor, autorateRevenue: true, autorateCosts: false);
		}

		public void TestGivenChargesWithEmptyOriginOrDestination_WhenAutorateRevenueForExportFOBShipmentInPPDConsol_ThenDSTChargesShouldNotBeLoaded()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var consignor = Helper.NewOrgHeader();
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Helper.NewOrgHeader();
			consignee.OH_RL_NKClosestPort = "DEHAM";
			consignee.OH_IsConsignee = true;

			var clientRate1 = Helper.NewClientRate(consignor);
			var clientORGRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "DE", "ODOC", 100m, currency: "AUD");
			clientORGRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientORGRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientDSTRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "AU", "DE", "DPCH", 200m, currency: "AUD");
			clientDSTRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientDSTRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientFRTRateEntry1 = clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "DE", "BAF", 300m, currency: "AUD");
			clientFRTRateEntry1.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientFRTRateEntry1.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientRate2 = Helper.NewClientRate(consignee);
			var clientORGRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "DE", "ODOC", 100m, currency: "AUD");
			clientORGRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientORGRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientDSTRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "AU", "DE", "DPCH", 200m, currency: "AUD");
			clientDSTRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientDSTRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			var clientFRTRateEntry2 = clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "DE", "BAF", 300m, currency: "AUD");
			clientFRTRateEntry2.TI_RateStartDate = new ZDate(2020, 2, 14);
			clientFRTRateEntry2.TI_RateEndDate = new ZDate(2020, 5, 16);

			Factory.Save();

			var consol = CreateConsol(TransportModes.Air, ContainerModes.Loose, origin: "NZAKL", destination: "DEHAM");
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateShipment(TransportModes.Air, ContainerModes.Loose, origin: "AUSYD", destination: "DEHAM");
			shipment.JS_E_DEP = new ZDateTime(2020, 3, 5);
			shipment.JS_E_ARV = new ZDateTime(2020, 3, 10);
			shipment.JS_INCO = "FOB";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			consol.Shipments.Add(shipment);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 300m
				}
			};

			AutorateAndAssert(expected, shipment, consignor, autorateRevenue: true, autorateCosts: false);

			clientORGRateEntry1.TI_OriginLRC = "";
			clientORGRateEntry2.TI_OriginLRC = "";
			clientDSTRateEntry1.TI_DestinationLRC = "";
			clientDSTRateEntry2.TI_DestinationLRC = "";
			clientFRTRateEntry1.TI_OriginLRC = "";
			clientFRTRateEntry1.TI_DestinationLRC = "";
			clientFRTRateEntry2.TI_OriginLRC = "";
			clientFRTRateEntry2.TI_DestinationLRC = "";

			Factory.Save();

			AutorateAndAssert(expected, shipment, consignor, autorateRevenue: true, autorateCosts: false);

			consol.JK_RL_NKLoadPort = "AUSYD";

			AutorateAndAssert(expected, shipment, consignor, autorateRevenue: true, autorateCosts: false);
		}

		#endregion

		#region RatingBehavior against CostReference

		public void TestAutoratingWithExistingCharge_REA_EmptyCostReference()
		{
			AutorateAndAssertCostReference(JobChargeLookups.ReAutorateCharge, "", false);
		}

		public void TestAutoratingWithExistingCharge_REA_SpecifiedCostReference()
		{
			AutorateAndAssertCostReference(JobChargeLookups.ReAutorateCharge, "RefCost1", false);
		}

		public void TestAutoratingWithExistingCharge_NEW_EmptyCostReference()
		{
			AutorateAndAssertCostReference(JobChargeLookups.CreateNewCharge, "", true);
		}

		public void TestAutoratingWithExistingCharge_NEW_SpecifiedCostReference()
		{
			AutorateAndAssertCostReference(JobChargeLookups.CreateNewCharge, "RefCost1", true);
		}

		public void AutorateAndAssertCostReference(ZString ratingBehavior, ZString costReference, bool addNew)
		{
			var clientRate = CreateClientRate();
			clientRate.AddFlatCharge("FRT", 10m);

			var shipment = CreateShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);
			consol.AddContainer("20GP");
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var testJob = CreateJob(shipment, shipment.JS_UniqueConsignRef);

			var existingCharge = testJob.Charges.AddNew();
			existingCharge.JR_AC = Helper.ChargeCodes["FRT"].PK;
			existingCharge.JR_OSCostAmt = 70m;
			existingCharge.JR_Calc_CostRatingBehavior = ratingBehavior;
			existingCharge.JR_Calc_SellRatingBehavior = ratingBehavior;
			existingCharge.JR_CostReference = costReference;

			Factory.Save();

			var expected = addNew
				? new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT", JR_OSCostAmt = 70m, JR_OSSellAmt = 70m, JR_SellRated = false, JR_CostRated = false, CostReferenceNumber = costReference,
					},
					new AssertionCharge
					{
						ChargeCode = "FRT", JR_OSCostAmt = 10m, JR_OSSellAmt = 10m, JR_SellRated = true, JR_CostRated = false, CostReferenceNumber = "",
					},
				}
				: new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT", JR_OSCostAmt = 10m, JR_OSSellAmt = 10m, JR_SellRated = true, JR_CostRated = false,
					},
				};

			AutorateAndAssert(expected, shipment, NewClient, job: testJob);
		}

		#endregion

		#region Helpers

		// Default argument values should be set in the way so that the default rate matches the default consol/shipment
		// and a developer doesn't care about basic matching in every test. If you add new arguments or change values
		// of the existing ones please make sure that this matching is still in place.

		RateEntry CreateCostRate(
			string category = "FCL",
			string mode = "SEA",
			string origin = "UAIEV",
			string destination = "AUSYD",
			string container = "20GP",
			string commodity = "GEN")
		{
			var entry = Costing.AddRateEntry(category, mode, origin, destination, "STD", container);
			entry.TI_RH_NKCommodityCode = commodity;
			entry.RateLines.RemoveAndDeleteAll();

			return entry;
		}

		RateEntry CreateClientRate(
			string category = "FCL",
			string mode = "SEA",
			string origin = "UAIEV",
			string destination = "AUSYD",
			string container = "20GP",
			string commodity = "GEN")
		{
			var entry = ClientRate.AddRateEntry(category, mode, origin, destination, "STD", container: container, commodity: commodity);
			entry.RateLines.RemoveAndDeleteAll();

			return entry;
		}

		RatingHeader Costing
		{
			get
			{
				if (costing == null)
				{
					costing = Helper.NewCosting(TransportProvider1);
				}

				return costing;
			}
		}

		RatingHeader ClientRate
		{
			get
			{
				if (clientRate == null)
				{
					clientRate = Helper.NewClientRate(Consignee);
				}

				return clientRate;
			}
		}

		RatingHeader costing;
		RatingHeader clientRate;

		ForwardingShipment CreateStandaloneGatewayShipment(string origin, string destination, string transportMode, string containerMode, params ZGuid[] gatewayAgents)
		{
			var shipment = CreateShipment(transportMode: transportMode, containerMode: containerMode, origin: origin, destination: destination);
			foreach (var agent in gatewayAgents)
			{
				shipment.Gateways.AddNew().ForwarderPK = agent;
			}

			return shipment;
		}

		#endregion
	}

	static class AutoratingShipmentTestExtensions
	{
		public static Charge AddPerUnitCharge(this Job job, string chargeCode, decimal chargeable, string unit = "KG", decimal costRate = 0, decimal sellRate = 0, string containerType = null)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = job.Factory.GetChargeCode(chargeCode).PK;

			if (!string.IsNullOrEmpty(containerType))
			{
				var attr = charge.JobChargeAttributes.AddNew();
				attr.EC_Name = JobChargeAttribTypeList.Codes.ContainerCode;
				attr.EC_Value = containerType;
			}

			if (costRate > 0)
			{
				charge.JR_OSCostAmt = costRate * chargeable;

				var basis = charge.PaymentBases.AddNew();
				basis.PBS_ChargeableAmount = chargeable;
				basis.PBS_ChargeableUnit = unit;
				basis.PBS_PerUnitRate = costRate;
				basis.PBS_RateUnit = unit;
				basis.PBS_RX_NKRateCurrency = "AUD";
				basis.PBS_IsCost = true;
			}

			if (sellRate > 0)
			{
				charge.JR_OSSellAmt = sellRate * chargeable;

				var basis = charge.PaymentBases.AddNew();
				basis.PBS_ChargeableAmount = chargeable;
				basis.PBS_ChargeableUnit = unit;
				basis.PBS_PerUnitRate = sellRate;
				basis.PBS_RateUnit = unit;
				basis.PBS_RX_NKRateCurrency = "AUD";
			}

			return charge;
		}
	}
}
