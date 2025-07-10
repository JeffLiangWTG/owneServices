using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class BuyersConsolIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestDebtorOrgs()
		{
			Helper.ChargeCodes.New("LOD1", "Origin Terminal Handling Fee 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Helper.ChargeCodes.New("LOD2", "Origin Terminal Handling Fee 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Helper.ChargeCodes.New("LOD3", "Origin Terminal Handling Fee 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Helper.ChargeCodes.New("LOD4", "Origin Terminal Handling Fee 4", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Factory.Save();

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_FullName = "Local Client";
			localClient.OH_RL_NKClosestPort = "AUSYD";
			localClient.OH_Code = "LC1";

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_FullName = "Controlling Customer";
			controllingCustomer.OH_RL_NKClosestPort = "AUSYD";
			controllingCustomer.OH_Code = "CCUS1";
			controllingCustomer.OH_IsControllingCustomer = true;

			var clientRateCNR = Helper.NewClientRate(Consignor);
			clientRateCNR.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD1", 10m);
			var clientRateCNE = Helper.NewClientRate(Consignee);
			clientRateCNE.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD2", 20m);
			var clientRateCCUS = Helper.NewClientRate(controllingCustomer);
			clientRateCCUS.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD3", 30m);
			var clientRateLC = Helper.NewClientRate(localClient);
			clientRateLC.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD4", 40m);

			Consignor.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;
			controllingCustomer.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;
			localClient.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var (leadShipment, _) = GetLeadAndSubShipment(leadShipmentIncoterm: IncoTerms.FreeCarrierSeller, subShipmentIncoterm: IncoTerms.FreeCarrierSeller);
			leadShipment.ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();

			Factory.Save();

			var exportPrepaidPriorities = SetupRatesPriorities(new[] { RatingDebtorOrgTypes.CNR, RatingDebtorOrgTypes.CCUS, RatingDebtorOrgTypes.LC });
			using (RatingDataRegistry.Instance.ExportPrepaidPriorities.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, exportPrepaidPriorities))
			{
				AutorateAndAssert
				(
					"WHEN autorate FC1 THEN should return exportPrepaidPriorities i.e. Consignor, ControllingCustomer, LocalClient",
					expected: new[]
					{
						new AssertionCharge { ChargeCode = "LOD1", JR_OSSellAmt = 10m, RevenueCalculationDescription = "LOD1: Base Rate AUD 10.00" },
						new AssertionCharge { ChargeCode = "LOD3", JR_OSSellAmt = 30m, RevenueCalculationDescription = "LOD3: Base Rate AUD 30.00" },
						new AssertionCharge { ChargeCode = "LOD4", JR_OSSellAmt = 40m, RevenueCalculationDescription = "LOD4: Base Rate AUD 40.00" },
					},
					leadShipment,
					localClient,
					autorateCosts: false
				);
			}

			leadShipment.JS_INCO = IncoTerms.FreeCarrierBuyer;
			Factory.Save();

			var exportCollectPriorities = SetupRatesPriorities(new[] { RatingDebtorOrgTypes.CNE, RatingDebtorOrgTypes.CCUS });
			using (RatingDataRegistry.Instance.ExportCollectPriorities.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, exportCollectPriorities))
			{
				AutorateAndAssert
				(
					"WHEN autorate FC2 THEN should return exportPrepaidPriorities i.e. Consignee, ControllingCustomer",
					expected: new[]
					{
						new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 20m, RevenueCalculationDescription = "LOD2: Base Rate AUD 20.00" },
						new AssertionCharge { ChargeCode = "LOD3", JR_OSSellAmt = 30m, RevenueCalculationDescription = "LOD3: Base Rate AUD 30.00" },
					},
					leadShipment,
					localClient,
					autorateCosts: false
				);
			}
		}

		static RatesPrioritiesCollection SetupRatesPriorities(RatingDebtorOrgTypes[] ratesPriorities)
		{
			var exportCollectPriorities = new RatesPrioritiesCollection();
			exportCollectPriorities.RemoveAndDeleteAll();

			foreach (var ratesPrioritie in ratesPriorities)
			{
				exportCollectPriorities.AddNew(ratesPrioritie);
			}

			return exportCollectPriorities;
		}

		#region Autorate BBK BLK ROR BCN

		public void TestAutorateBCN_SEA_ClientRate() => AssertAutorateBCN_SEA(ratingHeader: Helper.NewClientRate(Consignee));

		public void TestAutorateBCN_SEA_CompanyTariff() => AssertAutorateBCN_SEA(Helper.NewCompanyTariff());

		public void TestAutorateBCN_SEA_Costing() => AssertAutorateBCN_SEA(ratingHeader: Helper.NewCosting(Creditor));

		public void TestAutorateBCN_SEA_Revenue()
		{
			var ratingHeader = Helper.NewClientRate(Consignee);
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.BCN, "AUSYD", "USLAX", "BAF", 20m, currency: "USD", description: "FCL-BCN");
			ratingHeader.Factory.Save();

			var costSell = ratingHeader.IsCosting()
			   ? CostSell.Cost
			   : CostSell.Revenue;

			#region Containerized

			var expectedCharges = costSell == CostSell.Revenue
				? new[]
				{
					NewAssertionCharge(costSell, "BAF", "FCL-BCN", 20m),
				}
				: new[]
				{
					NewAssertionCharge(costSell, "BAF", "FCL-BCN", 20m),
				};
			AssertNoExceptionThrown(
				() =>
				AssertAutorateBCN(registryEnabled: true, TransportModes.Sea, "20GP", costSell, expectedCharges, "GIVEN enabled registry THEN should pick BCN rates", CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CLC)
			);

			#endregion
		}

		public void AssertAutorateBCN_SEA(RatingHeader ratingHeader)
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			Consignee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "BAF", 10m, currency: "USD", container: "20GP", description: "FCL-SEA");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.BCN, "AUSYD", "USLAX", "BAF", 20m, currency: "USD", description: "FCL-BCN");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX", "BAF", 30m, currency: "USD", description: "LCL-LCL");
			var lclBCNRateEntry = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.BCN, "AUSYD", "USLAX", "BAF", 40m, currency: "USD", description: "LCL-BCN-BAF");
			lclBCNRateEntry.AddFlatRateLine("CAF", amount: 50m, currency: "USD", description: "LCL-BCN-CAF");

			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "USLAX", "ODOC", 100m, currency: "USD", container: "20GP", description: "ORG-FCL");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUSYD", "USLAX", "ODOC", 200m, currency: "USD", description: "ORG-LCL");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.BCN, "AUSYD", "USLAX", "ODOC", 300m, currency: "USD", description: "ORG-BCN");

			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FCL, "AUSYD", "USLAX", "DDOC", 1000m, currency: "USD", container: "20GP", description: "DST-FCL");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LCL, "AUSYD", "USLAX", "DDOC", 2000m, currency: "USD", description: "DST-LCL");
			ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.BCN, "AUSYD", "USLAX", "DDOC", 3000m, currency: "USD", description: "DST-BCN");

			ratingHeader.Factory.Save();

			var costSell = ratingHeader.IsCosting()
			   ? CostSell.Cost
			   : CostSell.Revenue;

			#region Containerized

			var expectedCharges = costSell == CostSell.Revenue
				? new[]
				{
					NewAssertionCharge(costSell, "BAF", "FCL-SEA", 10m),
					NewAssertionCharge(costSell, "ODOC", "ORG-LCL", 200m),
					NewAssertionCharge(costSell, "DDOC", "DST-FCL", 1000m),
				}
				: new[]
				{
					NewAssertionCharge(costSell, "BAF", "FCL-SEA", 10m),
					NewAssertionCharge(costSell, "ODOC", "ORG-LCL", 200m),
					NewAssertionCharge(costSell, "DDOC", "DST-FCL", 1000m),
					NewAssertionCharge(costSell, "ODOC", "ORG-LCL", 200m), // ODOC is charged 2x i.e. lead and subshipment
				};
			AssertAutorateBCN(registryEnabled: false, TransportModes.Sea, "20GP", costSell, expectedCharges, "GIVEN disabled registry THEN should not pick BCN rates");

			expectedCharges = costSell == CostSell.Revenue
				? new[]
				{
					NewAssertionCharge(costSell, "BAF", "FCL-BCN", 20m),
					NewAssertionCharge(costSell, "CAF", "LCL-BCN-CAF", 50m), // For same Chargecode, LCL-BCN has less priority to FCL-BCN
					NewAssertionCharge(costSell, "ODOC", "ORG-BCN", 300m), // For BCN, origin always LCL
					NewAssertionCharge(costSell, "DDOC", "DST-BCN", 3000m)
				}
				: new[]
				{
					NewAssertionCharge(costSell, "BAF", "FCL-BCN", 20m),
					NewAssertionCharge(costSell, "CAF", "LCL-BCN-CAF", 50m), // For same Chargecode, LCL-BCN has less priority to FCL-BCN
					NewAssertionCharge(costSell, "ODOC", "ORG-BCN", 300m), // For BCN, origin always LCL
					NewAssertionCharge(costSell, "DDOC", "DST-BCN", 3000m),
					NewAssertionCharge(costSell, "ODOC", "ORG-BCN", 300m), // ODOC is charged 2x i.e. lead and subshipment
				};
			AssertAutorateBCN(registryEnabled: true, TransportModes.Sea, "20GP", costSell, expectedCharges, "GIVEN enabled registry THEN should pick BCN rates");

			#endregion

			#region Not Containerized

			expectedCharges = costSell == CostSell.Revenue
				? new[]
				{
					NewAssertionCharge(costSell, "BAF", "LCL-LCL", 30m),
					NewAssertionCharge(costSell, "ODOC", "ORG-LCL", 200m),
					NewAssertionCharge(costSell, "DDOC", "DST-LCL", 2000m),
				}
				: new[]
				{
					NewAssertionCharge(costSell, "BAF", "LCL-LCL", 30m),
					NewAssertionCharge(costSell, "ODOC", "ORG-LCL", 200m),
					NewAssertionCharge(costSell, "DDOC", "DST-LCL", 2000m),
					NewAssertionCharge(costSell, "ODOC", "ORG-LCL", 200m), // ODOC is charged 2x i.e. lead and subshipment
				};
			AssertAutorateBCN(registryEnabled: false, TransportModes.Sea, container: string.Empty, costSell, expectedCharges, message: "GIVEN disabled registry THEN should not pick BCN rates");

			expectedCharges = costSell == CostSell.Revenue
				? new[]
				{
					NewAssertionCharge(costSell, "BAF", "FCL-BCN", 20m),
					NewAssertionCharge(costSell, "CAF", "LCL-BCN-CAF", 50m), // For same Chargecode, LCL-BCN has less priority to FCL-BCN
					NewAssertionCharge(costSell, "ODOC", "ORG-BCN", 300m), // For BCN, origin always LCL
					NewAssertionCharge(costSell, "DDOC", "DST-BCN", 3000m),
				}
				: new[]
				{
					NewAssertionCharge(costSell, "BAF", "FCL-BCN", 20m),
					NewAssertionCharge(costSell, "CAF", "LCL-BCN-CAF", 50m), // For same Chargecode, LCL-BCN has less priority to FCL-BCN
					NewAssertionCharge(costSell, "ODOC", "ORG-BCN", 300m), // For BCN, origin always LCL
					NewAssertionCharge(costSell, "DDOC", "DST-BCN", 3000m),
					NewAssertionCharge(costSell, "ODOC", "ORG-BCN", 300m), // ODOC is charged 2x i.e. lead and subshipment
				};
			AssertAutorateBCN(registryEnabled: true, TransportModes.Sea, container: string.Empty, costSell, expectedCharges, message: "GIVEN enabled registry THEN should pick BCN rates");

			#endregion
		}

		static AssertionCharge NewAssertionCharge(CostSell costSell, ZString chargeCode, ZString description, ZDecimal amount)
		{
			return costSell == CostSell.Cost
				? new AssertionCharge { ChargeCode = chargeCode, JR_OSCostAmt = amount }
				: new AssertionCharge { ChargeCode = chargeCode, JR_Desc = description, JR_OSSellAmt = amount };
		}
		public void TestAutorateBCN_AIR()
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var clientRate = Helper.NewClientRate(Consignee);

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "BAF", 10m, currency: "USD", container: "LD-7", description: "AIR-ULD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 30m, currency: "USD", description: "AIR-LSE");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.BCN, "AUSYD", "USLAX", "BAF", 40m, currency: "USD", description: "AIR-BCN");

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ULD, "AUSYD", "USLAX", "ODOC", 100m, currency: "USD", container: "LD-7", description: "ORG-ULD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LSE, "AUSYD", "USLAX", "ODOC", 200m, currency: "USD", description: "ORG-LSE");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.BCN, "AUSYD", "USLAX", "ODOC", 300m, currency: "USD", description: "ORG-BCN");

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ULD, "AUSYD", "USLAX", "DDOC", 1000m, currency: "USD", container: "LD-7", description: "DST-ULD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LSE, "AUSYD", "USLAX", "DDOC", 2000m, currency: "USD", description: "DST-LSE");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.BCN, "AUSYD", "USLAX", "DDOC", 3000m, currency: "USD", description: "DST-BCN");

			#region Containerized

			AssertAutorateBCN
			(
				registryEnabled: false,
				transportMode: TransportModes.Air,
				container: "LD-7",
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "AIR-ULD", JR_OSSellAmt = 10m, },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-LSE", JR_OSSellAmt = 200m, },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-ULD", JR_OSSellAmt = 1000m, },
				},
				message: "GIVEN disabled registry THEN should not pick BCN rates"
			);

			AssertAutorateBCN
			(
				registryEnabled: true,
				transportMode: TransportModes.Air,
				container: "LD-7",
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "AIR-BCN", JR_OSSellAmt = 40m },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-BCN", JR_OSSellAmt = 300m }, // For BCN, origin always LCL
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-BCN", JR_OSSellAmt = 3000m },
				},
				message: "GIVEN enabled registry THEN should pick BCN rates"
			);

			#endregion

			#region Not Containerized

			AssertAutorateBCN
			(
				registryEnabled: false,
				transportMode: TransportModes.Air,
				container: string.Empty,
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "AIR-LSE", JR_OSSellAmt = 30m, },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-LSE", JR_OSSellAmt = 200m, },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-LSE", JR_OSSellAmt = 2000m, },
				},
				message: "GIVEN disabled registry THEN should not pick BCN rates"
			);

			AssertAutorateBCN
			(
				registryEnabled: true,
				transportMode: TransportModes.Air,
				container: string.Empty,
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "AIR-BCN", JR_OSSellAmt = 40m },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-BCN", JR_OSSellAmt = 300m }, // For BCN, origin always LCL
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-BCN", JR_OSSellAmt = 3000m },
				},
				message: "GIVEN enabled registry THEN should pick BCN rates"
			);

			#endregion
		}

		public void TestAutorateBCN_ROAD()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("CAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");

			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var clientRate = Helper.NewClientRate(Consignee);

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.ROA, "AUSYD", "USLAX", "BAF", 10m, currency: "USD", container: "CHIP", description: "FCL-ROA");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.BCN, "AUSYD", "USLAX", "BAF", 20m, currency: "USD", description: "FCL-BCN");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LRO, "AUSYD", "USLAX", "BAF", 30m, currency: "USD", description: "LCL-LRO");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.FTL, "AUSYD", "USLAX", "BAF", 40m, currency: "USD", description: "LCL-FTL");
			var lclBCNRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.BCN, "AUSYD", "USLAX", "BAF", 50m, currency: "USD", description: "LCL-BCN-BAF");
			lclBCNRateEntry.AddFlatRateLine("CAF", amount: 60m, currency: "USD", description: "LCL-BCN-CAF");

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FRO, "AUSYD", "USLAX", "ODOC", 100m, currency: "USD", container: "CHIP", description: "ORG-FRO");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LRO, "AUSYD", "USLAX", "ODOC", 200m, currency: "USD", description: "ORG-LRO");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FTL, "AUSYD", "USLAX", "ODOC", 300m, currency: "USD", container: "CHIP", description: "ORG-FTL");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.BCN, "AUSYD", "USLAX", "ODOC", 400m, currency: "USD", description: "ORG-BCN");

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FRO, "AUSYD", "USLAX", "DDOC", 1000m, currency: "USD", container: "CHIP", description: "DST-FRO");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LRO, "AUSYD", "USLAX", "DDOC", 2000m, currency: "USD", description: "DST-LRO");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FTL, "AUSYD", "USLAX", "ODOC", 3000m, currency: "USD", container: "CHIP", description: "DST-FTL");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.BCN, "AUSYD", "USLAX", "DDOC", 4000m, currency: "USD", description: "DST-BCN");

			#region Containerized

			AssertAutorateBCN
			(
				registryEnabled: false,
				transportMode: TransportModes.Road,
				container: "CHIP",
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "FCL-ROA", JR_OSSellAmt = 10m, },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-LRO", JR_OSSellAmt = 200m, },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-FRO", JR_OSSellAmt = 1000m, },
				},
				message: "GIVEN disabled registry THEN should not pick BCN rates"
			);

			AssertAutorateBCN
			(
				registryEnabled: true,
				transportMode: TransportModes.Road,
				container: "CHIP",
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "FCL-BCN", JR_OSSellAmt = 20m },
					new AssertionCharge { ChargeCode = "CAF", JR_Desc = "LCL-BCN-CAF", JR_OSSellAmt = 60m }, // For same Chargecode, LCL-BCN has less priority to FCL-BCN
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-BCN", JR_OSSellAmt = 400m }, // For BCN, origin always LCL
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-BCN", JR_OSSellAmt = 4000m },
				},
				message: "GIVEN enabled registry THEN should pick BCN rates"
			);

			#endregion

			#region Not Containerized

			AssertAutorateBCN
			(
				registryEnabled: false,
				transportMode: TransportModes.Road,
				container: string.Empty,
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "LCL-LRO", JR_OSSellAmt = 30m, },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-LRO", JR_OSSellAmt = 200m, },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-LRO", JR_OSSellAmt = 2000m, },
				},
				message: "GIVEN disabled registry THEN should not pick BCN rates"
			);

			AssertAutorateBCN
			(
				registryEnabled: true,
				transportMode: TransportModes.Road,
				container: string.Empty,
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "FCL-BCN", JR_OSSellAmt = 20m },
					new AssertionCharge { ChargeCode = "CAF", JR_Desc = "LCL-BCN-CAF", JR_OSSellAmt = 60m }, // For same Chargecode, LCL-BCN has less priority to FCL-BCN
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-BCN", JR_OSSellAmt = 400m }, // For BCN, origin always LCL
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-BCN", JR_OSSellAmt = 4000m },
				},
				message: "GIVEN enabled registry THEN should pick BCN rates"
			);

			#endregion
		}

		public void TestAutorateBCN_RAIL()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("CAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");

			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var clientRate = Helper.NewClientRate(Consignee);

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.RAI, "AUSYD", "USLAX", "BAF", 10m, currency: "USD", container: "20GP", description: "FCL-RAI");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.BCN, "AUSYD", "USLAX", "BAF", 20m, currency: "USD", description: "FCL-BCN");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LRA, "AUSYD", "USLAX", "BAF", 30m, currency: "USD", description: "LCL-LRA");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.FWL, "AUSYD", "USLAX", "BAF", 40m, currency: "USD", description: "LCL-FWL");
			var lclBCNRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.BCN, "AUSYD", "USLAX", "BAF", 50m, currency: "USD", description: "LCL-BCN-BAF");
			lclBCNRateEntry.AddFlatRateLine("CAF", amount: 60m, currency: "USD", description: "LCL-BCN-CAF");

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FRA, "AUSYD", "USLAX", "ODOC", 100m, currency: "USD", container: "20GP", description: "ORG-FRA");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LRA, "AUSYD", "USLAX", "ODOC", 200m, currency: "USD", description: "ORG-LRA");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.FWL, "AUSYD", "USLAX", "ODOC", 300m, currency: "USD", description: "ORG-FWL");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.BCN, "AUSYD", "USLAX", "ODOC", 400m, currency: "USD", description: "ORG-BCN");

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FRA, "AUSYD", "USLAX", "DDOC", 1000m, currency: "USD", container: "20GP", description: "DST-FRA");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LRA, "AUSYD", "USLAX", "DDOC", 2000m, currency: "USD", description: "DST-LRA");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FWL, "AUSYD", "USLAX", "ODOC", 3000m, currency: "USD", description: "DST-FWL");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.BCN, "AUSYD", "USLAX", "DDOC", 4000m, currency: "USD", description: "DST-BCN");

			#region Containerized

			AssertAutorateBCN
			(
				registryEnabled: false,
				transportMode: TransportModes.Rail,
				container: "20GP",
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "FCL-RAI", JR_OSSellAmt = 10m, },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-LRA", JR_OSSellAmt = 200m, },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-FRA", JR_OSSellAmt = 1000m, },
				},
				message: "GIVEN disabled registry THEN should not pick BCN rates"
			);

			AssertAutorateBCN
			(
				registryEnabled: true,
				transportMode: TransportModes.Rail,
				container: "20GP",
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "FCL-BCN", JR_OSSellAmt = 20m },
					new AssertionCharge { ChargeCode = "CAF", JR_Desc = "LCL-BCN-CAF", JR_OSSellAmt = 60m }, // For same Chargecode, LCL-BCN has less priority to FCL-BCN
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-BCN", JR_OSSellAmt = 400m }, // For BCN, origin always LCL
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-BCN", JR_OSSellAmt = 4000m },
				},
				message: "GIVEN enabled registry THEN should pick BCN rates"
			);

			#endregion

			#region Not Containerized

			AssertAutorateBCN
			(
				registryEnabled: false,
				transportMode: TransportModes.Rail,
				container: string.Empty,
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "LCL-LRA", JR_OSSellAmt = 30m, },
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-LRA", JR_OSSellAmt = 200m, },
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-LRA", JR_OSSellAmt = 2000m, },
				},
				message: "GIVEN disabled registry THEN should not pick BCN rates"
			);

			AssertAutorateBCN
			(
				registryEnabled: true,
				transportMode: TransportModes.Rail,
				container: string.Empty,
				costSell: CostSell.Revenue,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BAF", JR_Desc = "FCL-BCN", JR_OSSellAmt = 20m },
					new AssertionCharge { ChargeCode = "CAF", JR_Desc = "LCL-BCN-CAF", JR_OSSellAmt = 60m }, // For same Chargecode, LCL-BCN has less priority to FCL-BCN
					new AssertionCharge { ChargeCode = "ODOC", JR_Desc = "ORG-BCN", JR_OSSellAmt = 400m }, // For BCN, origin always LCL
					new AssertionCharge { ChargeCode = "DDOC", JR_Desc = "DST-BCN", JR_OSSellAmt = 4000m },
				},
				message: "GIVEN enabled registry THEN should pick BCN rates"
			);

			#endregion
		}

		void AssertAutorateBCN(bool registryEnabled, string transportMode, string container, CostSell costSell, AssertionCharge[] expectedCharges, string message = default, string contractNumberType = null)
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var consol = Helper.CreateBuyersConsolConsol("AUSYD", "USLAX", Creditor);
			if (!string.IsNullOrEmpty(container))
			{
				var forwardingContainer = consol.Containers.AddNew();
				forwardingContainer.JC_RC = Helper.Containers[container].PK;
				forwardingContainer.JC_ContainerMode = ContainerModes.BuyersConsol;
			}

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, weight: 10m, volume: 20m, transportMode, contractNumberType);
			var coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, weight: 30m, volume: 400m);

			// We add some inner pack lines as well to make sure they don't affect calculations
			leadShipment.AddInnerPackLine(100, "BOX", 5);
			leadShipment.AddInnerPackLine(50, "PLT", 3);
			coloadShipment.AddInnerPackLine(30, "BOX", 2);
			coloadShipment.AddInnerPackLine(20, "PLT", 6);

			Factory.Save();

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEnabled))
			{
				AutorateAndAssert(message, expectedCharges, leadShipment, Consignee, autorateCosts: costSell == CostSell.Cost, autorateRevenue: costSell == CostSell.Revenue);
			}
		}

		public void TestAutorateBCN_RegistryEnabled_Costing_ShouldMatchOnCorrectContainerClass()
		{
			var container20Class = Factory.NewWithValidTestData<RefContainer>();
			container20Class.RC_FreightRateClass = "20GN";
			container20Class.RC_Code = "20DC";

			var container40Class = Factory.NewWithValidTestData<RefContainer>();
			container40Class.RC_FreightRateClass = "40GN";
			container40Class.RC_Code = "40DC";

			GP20.RC_FreightRateClass = container20Class.RC_FreightRateClass;
			GP40.RC_FreightRateClass = container40Class.RC_FreightRateClass;

			Factory.Save();

			var costing = Helper.NewCosting(Creditor);
			CreateRateEntryWithPerUnitRateLine(costing, "CN", 100, "FRT", RateMode.BCN, RatingConstants.RateCategory.FCL, "US", "AU", "20DC")
				.TI_MatchContainerRateClass = true;
			CreateRateEntryWithPerUnitRateLine(costing, "CN", 140, "FRT", RateMode.BCN, RatingConstants.RateCategory.FCL, "US", "AU", "40DC")
				.TI_MatchContainerRateClass = true;

			var consol = Helper.CreateBuyersConsolConsol("USLAX", "AUSYD", Creditor, "SEA", PaymentType.Collect);

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = GP20.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = GP20.PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.BuyersConsol;

			Factory.Save();

			RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 200,
						CostCalculationDescription = "FRT: 2 20DC Container(s) @ USD 100.00/Container",
					}
			};

			AutoCostAndAssert("Should correctly match rate by container class", null, expectedCosts, consol, autorateRevenue: false);
		}

		public void TestAutorateBCN_RegistryEnabled_Costing_NoChargesWhenNoMatchingRateForContainer()
		{
			var costing = Helper.NewCosting(Creditor);
			CreateRateEntryWithPerUnitRateLine(costing, "CN", 100, "FRT", RateMode.BCN, RatingConstants.RateCategory.FCL, "US", "AU", "40GP");
			CreateRateEntryWithPerUnitRateLine(costing, "CN", 140, "FRT", RateMode.BCN, RatingConstants.RateCategory.FCL, "US", "AU", "40HC");

			var consol = Helper.CreateBuyersConsolConsol("USLAX", "AUSYD", Creditor, "SEA", PaymentType.Collect);

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = GP20.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = GP20.PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.BuyersConsol;

			Factory.Save();

			RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var expectedCosts = Array.Empty<AssertionCost>();
			AutoCostAndAssert("Should be no charges since no rate matches job's 20Gp container", null, expectedCosts, consol, autorateRevenue: false);
		}

		public void TestAutorateBCN_RateOrigin()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, "BCN", "DEBER", "", "FRT", 5000m);

			var consol = Helper.CreateBuyersConsolConsol("DEHAM", "AUSYD", transportMode: "AIR");
			var shipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, transportMode: "AIR");
			shipment.JS_RL_NKFreightRateOrigin = "DEBER";
			Factory.Save();

			RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 5000m }
			};

			AutorateAndAssert("Should have a FRT charge via RateOrigin", expectedCharges, shipment, Consignee);
		}

		public void TestAutorateBCN_RateDestination()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, "BCN", "", "AUMEL", "FRT", 5000m);

			var consol = Helper.CreateBuyersConsolConsol("DEHAM", "AUSYD", transportMode: "AIR");
			var shipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, transportMode: "AIR");
			shipment.JS_RL_NKFreightRateDestination = "AUMEL";
			Factory.Save();

			RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 5000m }
			};

			AutorateAndAssert("Should have a FRT charge via RateDestination", expectedCharges, shipment, Consignee);
		}

		#endregion

		#region Filtered Log

		public void TestORGCharge_FilteredLog_LowestBill()
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Apportion;

			var chargeCode = Helper.ChargeCodes["ODOC"];
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.LW, perUnit: 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.ALL);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			var consol = Helper.CreateBuyersConsolConsol("AUSYD", "USLAX", Creditor);
			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 10m, 20m);
			var subShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 30m, 40m);

			Factory.Save();

			AutorateAndAssert
			(
				"Autorate leadShipment",
				new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 10m, RevenueCalculationDescription = "ODOC: 1 Lowest Bill(s) @ USD 10.00/Lowest Bill" } },
				leadShipment,
				Consignee,
				autorateCosts: false
			);
			AssertAutoratingAuditLogNoteContainsLines
			(
				leadShipment,
				"WHEN autorate leadShipment THEN ORG charge filtered message should be from 'not applicable rating criteria'",
				"Information: RateLine Filtered ODOC-UNT-LW-Client Rate CONSIGNEE1\treason:\tORG charge code group is not listed among applicable on the rating criteria"
			);

			AutorateAndAssert
			(
				"Autorate subShipment",
				new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 0m, RevenueCalculationDescription = "ODOC: 0 Lowest Bill(s) @ USD 10.00/Lowest Bill" } },
				subShipment,
				Consignee,
				autorateCosts: false
			);
			AssertAutoratingAuditLogNoteContainsLines
			(
				subShipment,
				"WHEN autorate subShipment THEN ORG charge filtered message should be from 'not applicable rating criteria'",
				"Information: RateLine Filtered ODOC-UNT-LW-Client Rate CONSIGNEE1\treason:\tORG charge code group is not listed among applicable on the rating criteria"
			);
		}

		public void TestORGCharge_FilteredLog_HouseBill()
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Apportion;

			var chargeCode = Helper.ChargeCodes["ODOC"];
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.HB, perUnit: 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.ALL);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			var consol = Helper.CreateBuyersConsolConsol("AUSYD", "USLAX", Creditor);
			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 10m, 20m);
			var subShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 30m, 40m);

			Factory.Save();

			AutorateAndAssert
			(
				"Autorate leadShipment",
				new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 10m, RevenueCalculationDescription = "ODOC: 1 House Bill(s) @ USD 10.00/House Bill" } },
				leadShipment,
				Consignee,
				autorateCosts: false
			);
			AssertAutoratingAuditLogNoteContainsLines
			(
				leadShipment,
				"WHEN autorate leadShipment THEN ORG charge filtered message should be from 'not applicable rating criteria'",
				"Information: RateLine Filtered ODOC-UNT-HB-Client Rate CONSIGNEE1\treason:\tORG charge code group is not listed among applicable on the rating criteria"
			);

			AutorateAndAssert
			(
				"Autorate subShipment",
				new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 0m, RevenueCalculationDescription = "ODOC: 0 House Bill(s) @ USD 10.00/House Bill" } },
				subShipment,
				Consignee,
				autorateCosts: false
			);
			AssertAutoratingAuditLogNoteContainsLines
			(
				subShipment,
				"WHEN autorate subShipment THEN ORG charge filtered message should be from 'not applicable rating criteria'",
				"Information: RateLine Filtered ODOC-UNT-HB-Client Rate CONSIGNEE1\treason:\tORG charge code group is not listed among applicable on the rating criteria"
			);
		}

		#endregion

		#region Rate Selector

		public void TestRateSelector_MasterInvoicingStyle()
		{
			var costing = Helper.NewCosting(Creditor);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", "DDOC", 50);
			Factory.Save();

			AssertSelector
			(
				invoicingStyle: ConsolInvoicingStyles.Master,
				expectedLeadShipmentCharges: new[] { new AssertionCharge { ChargeCode = "DDOC", JR_OSCostAmt = 50m, CostCalculationDescription = "DDOC: Base Rate AUD 50.00" }, },
				expectedSubShipmentCharges: Array.Empty<AssertionCharge>(),
				expectedConsolCharges: Array.Empty<AssertionCharge>()
			);
		}

		public void TestRateSelector_ApportionInvoicingStyle()
		{
			var costing = Helper.NewCosting(Creditor);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", "DDOC", 50);
			Factory.Save();

			AssertSelector
			(
				invoicingStyle: ConsolInvoicingStyles.Apportion,
				expectedLeadShipmentCharges: new[] { new AssertionCharge { ChargeCode = "DDOC", JR_OSCostAmt = 50m, CostCalculationDescription = "DDOC: Base Rate AUD 50.00" }, },
				expectedSubShipmentCharges: new[] { new AssertionCharge { ChargeCode = "DDOC", JR_OSCostAmt = 50m, CostCalculationDescription = "DDOC: Base Rate AUD 50.00" }, },
				expectedConsolCharges: Array.Empty<AssertionCharge>()
			);
		}

		public void TestRateSelector_ApportionInvoiceMasterInvoicingStyle()
		{
			var costing = Helper.NewCosting(Creditor);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", "DDOC", 50);
			Factory.Save();

			AssertSelector
			(
				invoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				expectedLeadShipmentCharges: new[] { new AssertionCharge { ChargeCode = "DDOC", JR_OSCostAmt = 50m, CostCalculationDescription = "DDOC: Base Rate AUD 50.00" }, },
				expectedSubShipmentCharges: new[] { new AssertionCharge { ChargeCode = "DDOC", JR_OSCostAmt = 50m, CostCalculationDescription = "DDOC: Base Rate AUD 50.00" }, },
				expectedConsolCharges: Array.Empty<AssertionCharge>()
			);
		}

		void AssertSelector(string invoicingStyle, AssertionCharge[] expectedLeadShipmentCharges, AssertionCharge[] expectedSubShipmentCharges, AssertionCharge[] expectedConsolCharges)
		{
			var consol = Helper.CreateBuyersConsolConsol("CNSHA", "AUSYD", Creditor, prepaidCollect: PaymentType.Prepaid);
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;

			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = invoicingStyle;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 900, 15);
			var subShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 1000, 15);

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			var registry = new RatesServiceRegistrySettingsCollection
			{
				new RatesServiceRegistrySettings(TransportModes.Sea, ContainerModes.BuyersConsol, true)
			};

			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registry))
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

				AutorateWithManualSelectAndAssert("Autorate LeadShipment", expectedLeadShipmentCharges, leadShipment, Consignee, autorateRevenue: false);
				AssertEquals("WHEN autorate LeadShipment THEN RateSelector should not show", 0, timesRateSelectorFormIsShown);

				AutorateWithManualSelectAndAssert("Autorate SubShipment", expectedSubShipmentCharges, subShipment, Consignee, autorateRevenue: false);
				AssertEquals("WHEN autorate SubShipment THEN RateSelector should not show", 0, timesRateSelectorFormIsShown);

				CreateJob(consol, "CONSOL1");
				AutorateWithManualSelectAndAssert("Autorate consol", expectedConsolCharges, consol, Consignee, autorateRevenue: false);
				AssertEquals("WHEN autorate Consol THEN RateSelector should show", 1, timesRateSelectorFormIsShown);
			}
		}

		#endregion

		#region Incoterm

		[TestDate(2021, 01, 01)]
		public void TestIncoterm_AutorateSubShipment_Apportion_AutoratedFor()
		{
			Helper.ChargeCodes.New("LOD1", "Origin Terminal Handling Fee 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Factory.Save();

			var clientRateCNR = Helper.NewClientRate(Consignor);
			clientRateCNR.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD1", 20m);

			Consignor.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Apportion;

			var (leadShipment, subShipment) = GetLeadAndSubShipment(leadShipmentIncoterm: IncoTerms.FreeCarrier, subShipmentIncoterm: IncoTerms.FreeCarrierSeller);
			leadShipment.JS_INCO = IncoTerms.FreeCarrierSeller;
			subShipment.JS_INCO = IncoTerms.FreeCarrierSeller;

			Factory.Save();

			AutorateAndAssert
			(
				"WHEN autorate from leadShipment THEN autorated-for should show leadShipment JobNumber",
				expected: new[]
				{
					new AssertionCharge
					{
						ChargeCode = "LOD1",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = string.Format(expectedRevenueCalculationDescription, leadShipment.JobNumber)
					}
				},
				leadShipment,
				Consignor,
				autorateCosts: false
			);

			AutorateAndAssert
			(
				"WHEN autorate from subShipment THEN autorated-for should show subShipment JobNumber",
				expected: new[]
				{
					new AssertionCharge
					{
						ChargeCode = "LOD1",
						JR_OSSellAmt = 10m,
						RevenueCalculationDescription = string.Format(expectedRevenueCalculationDescription, subShipment.JobNumber)
					}
				},
				subShipment,
				Consignor,
				autorateCosts: false
			);
		}

		const string expectedRevenueCalculationDescription = @"This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3)

LOD1: Base Rate AUD 20.00

Origin Terminal Handling Fee 1

Charge located in CONSIGNOR1 client rate with the following details:

Payment Term:		Prepaid
Mode:			ALL
Charge Code Group:	LOD
Start Date:		01 January 2021
End Date:		01 July 2021
Origin:			AUSYD
Destination:		USLAX
Commodity Code:		GEN
Currency:		AUD
Autorated for:		Shipment {0} (House Bill='{0}')
Leg:			AUSYD-USLAX";

		public void TestIncoterm_AutorateSubShipment_Apportion()
		{
			Helper.ChargeCodes.New("LOD1", "Origin Terminal Handling Fee 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Helper.ChargeCodes.New("LOD2", "Origin Terminal Handling Fee 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Factory.Save();

			var clientRateCNR = Helper.NewClientRate(Consignor);
			clientRateCNR.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD1", 20m);
			var clientRateCNE = Helper.NewClientRate(Consignee);
			clientRateCNE.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD2", 20m);

			AssertIncoterm
			(
				consignorInvoicingStyle: ConsolInvoicingStyles.Apportion,
				consigneeInvoicingStyle: ConsolInvoicingStyles.Master,
				autorateLeadShipment: false,
				expectedCharges: new Dictionary<string, AssertionCharge[]>()
				{
					{ IncoTerms.FreeCarrier,  new[] { new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3" } } },
					{ IncoTerms.FreeCarrierSeller,  new[] { new AssertionCharge { ChargeCode = "LOD1", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3" } } },
					{ IncoTerms.FreeCarrierBuyer,  new[] { new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3" } } }
				},
				message: "WHEN autorate sub-shipment with invoicing-style=APP THEN should get charge based on sub-shipment incoterm"
			);
		}

		public void TestIncoterm_AutorateSubShipment_ApportionInvoiceMaster()
		{
			Helper.ChargeCodes.New("LOD1", "Origin Terminal Handling Fee 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Helper.ChargeCodes.New("LOD2", "Origin Terminal Handling Fee 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Factory.Save();

			var clientRateCNR = Helper.NewClientRate(Consignor);
			clientRateCNR.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD1", 20m);
			var clientRateCNE = Helper.NewClientRate(Consignee);
			clientRateCNE.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD2", 20m);

			AssertIncoterm
			(
				consignorInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				consigneeInvoicingStyle: ConsolInvoicingStyles.Master,
				autorateLeadShipment: false,
				expectedCharges: new Dictionary<string, AssertionCharge[]>()
				{
					{ IncoTerms.FreeCarrier,  new[] { new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3" } } },
					{ IncoTerms.FreeCarrierSeller,  new[] { new AssertionCharge { ChargeCode = "LOD1", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3" } } },
					{ IncoTerms.FreeCarrierBuyer,  new[] { new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3" } } }
				},
				message: "WHEN autorate sub-shipment with invoicing-style=MAB THEN should get charge based on sub-shipment incoterm"
			);
		}

		public void TestIncoterm_AutorateLeadShipment_Master()
		{
			Helper.ChargeCodes.New("LOD1", "Origin Terminal Handling Fee 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Helper.ChargeCodes.New("LOD2", "Origin Terminal Handling Fee 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Factory.Save();

			var clientRateCNR = Helper.NewClientRate(Consignor);
			clientRateCNR.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD1", 20m);
			var clientRateCNE = Helper.NewClientRate(Consignee);
			clientRateCNE.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD2", 20m);

			AssertIncoterm
			(
				consignorInvoicingStyle: ConsolInvoicingStyles.Master,
				consigneeInvoicingStyle: ConsolInvoicingStyles.Apportion,
				autorateLeadShipment: true,
				expectedCharges: new Dictionary<string, AssertionCharge[]>()
				{
					{ IncoTerms.FreeCarrier,  new[] { new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 20m, RevenueCalculationDescription = "LOD2: Base Rate AUD 20.00" } } },
					{ IncoTerms.FreeCarrierSeller,  new[] { new AssertionCharge { ChargeCode = "LOD1", JR_OSSellAmt = 20m, RevenueCalculationDescription = "LOD1: Base Rate AUD 20.00" } } },
					{ IncoTerms.FreeCarrierBuyer,  new[] { new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 20m, RevenueCalculationDescription = "LOD2: Base Rate AUD 20.00" } } }
				},
				message: $"WHEN autorate lead-shipment with invoicing-style=MAS THEN should pick lead-shipment incoterm"
			);
		}

		public void TestIncoterm_AutorateLeadShipment_Apportion()
		{
			Helper.ChargeCodes.New("LOD1", "Origin Terminal Handling Fee 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Helper.ChargeCodes.New("LOD2", "Origin Terminal Handling Fee 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Factory.Save();

			var clientRateCNR = Helper.NewClientRate(Consignor);
			clientRateCNR.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD1", 20m);
			var clientRateCNE = Helper.NewClientRate(Consignee);
			clientRateCNE.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD2", 20m);

			AssertIncoterm
			(
				consignorInvoicingStyle: ConsolInvoicingStyles.Apportion,
				consigneeInvoicingStyle: ConsolInvoicingStyles.Master,
				autorateLeadShipment: true,
				expectedCharges: new Dictionary<string, AssertionCharge[]>()
				{
					{ IncoTerms.FreeCarrier,  new[] { new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3)" } } },
					{ IncoTerms.FreeCarrierSeller,  new[] { new AssertionCharge { ChargeCode = "LOD1", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3)" } } },
					{ IncoTerms.FreeCarrierBuyer,  new[] { new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3)" } } }
				},
				message: $"WHEN autorate lead-shipment with invoicing-style=APP THEN should pick lead-shipment incoterm"
			);
		}

		public void TestIncoterm_AutorateLeadShipment_ApportionInvoiceMaster()
		{
			Helper.ChargeCodes.New("LOD1", "Origin Terminal Handling Fee 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Helper.ChargeCodes.New("LOD2", "Origin Terminal Handling Fee 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			Factory.Save();

			var clientRateCNR = Helper.NewClientRate(Consignor);
			clientRateCNR.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD1", 20m);
			var clientRateCNE = Helper.NewClientRate(Consignee);
			clientRateCNE.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "LOD2", 20m);

			AssertIncoterm
			(
				consignorInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				consigneeInvoicingStyle: ConsolInvoicingStyles.Master,
				autorateLeadShipment: true,
				expectedCharges: new Dictionary<string, AssertionCharge[]>()
				{
					{ IncoTerms.FreeCarrier,  new[] { new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3)" } } },
					{ IncoTerms.FreeCarrierSeller,  new[] { new AssertionCharge { ChargeCode = "LOD1", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3)" } } },
					{ IncoTerms.FreeCarrierBuyer,  new[] { new AssertionCharge { ChargeCode = "LOD2", JR_OSSellAmt = 10m, RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 10.000 M3, Buyers Consol Total: 20.000 M3)" } } }
				},
				message: $"WHEN autorate lead-shipment with invoicing-style=MAB THEN should pick lead-shipment incoterm"
			);
		}

		(ForwardingShipment, ForwardingShipment) GetLeadAndSubShipment(string leadShipmentIncoterm, string subShipmentIncoterm)
		{
			var consol = Helper.CreateBuyersConsolConsol("AUSYD", "USLAX", Creditor);
			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.BuyersConsol;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, weight: 10m, volume: 10m);
			leadShipment.JS_INCO = leadShipmentIncoterm;

			var job = new JobHeader.Loader(leadShipment).TryLoadOrCreate() as Job;
			job.LocalZAddressWithContact.OrgPK = Consignor.PK;

			var subShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, weight: 10m, volume: 10m);
			subShipment.JS_INCO = subShipmentIncoterm;

			return (leadShipment, subShipment);
		}

		void AssertIncoterm(string consignorInvoicingStyle, string consigneeInvoicingStyle, bool autorateLeadShipment, Dictionary<string, AssertionCharge[]> expectedCharges, string message = "")
		{
			Consignor.CompanyData.OB_ARBuyersConsolInvoicingStyle = consignorInvoicingStyle;
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = consigneeInvoicingStyle;

			var (leadShipment, subShipment) = GetLeadAndSubShipment(leadShipmentIncoterm: IncoTerms.FreeCarrier, subShipmentIncoterm: IncoTerms.FreeCarrierSeller);
			var currentShipment = autorateLeadShipment ? leadShipment : subShipment;

			foreach (var shipmentIncoterm in expectedCharges.Keys)
			{
				currentShipment.JS_INCO = shipmentIncoterm;
				Factory.Save();
				AutorateAndAssert
				(
					$"{message}: {shipmentIncoterm}",
					expected: expectedCharges[shipmentIncoterm],
					currentShipment,
					Consignor,
					autorateCosts: false
				);
			}
		}

		#endregion

		#region No apportionment

		public void TestNoApportionment_FlatCalculator_BuyersConsolInvoicingStyleAPP()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FCL, "AUSYD", "USLAX", "DDOC", 1000m);

			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				rateEntry,
				leadShipmentWeight: 0m,
				subShipmentWeight: 0m,
				leadShipmentVolume: 100m,
				subShipmentVolume: 300m,
				expectedLeadShipmentSellAmt: 1000m,
				expectedSubShipmentSellAmt: 1000m,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and FlatCalculator THEN Lead and SubShipment.Charge = BasePrice"
			);
		}

		public void TestNoApportionment_FlatCalculator_BuyersConsolInvoicingStyleMAB()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FCL, "AUSYD", "USLAX", "DDOC", 1000m);

			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				rateEntry,
				leadShipmentWeight: 0m,
				subShipmentWeight: 0m,
				leadShipmentVolume: 100m,
				subShipmentVolume: 300m,
				expectedLeadShipmentSellAmt: 1000m,
				expectedSubShipmentSellAmt: 1000m,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and FlatCalculator THEN Lead and SubShipment.Charge = BasePrice"
			);
		}

		#region Unit Calulator with HB unit

		public void TestNoApportionment_UnitCalculator_HBUnit_BuyersConsolInvoicingStyleMAS()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.HB, perUnit: 1000m);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Master,
				rateEntry,
				leadShipmentWeight: 0m,
				subShipmentWeight: 0m,
				leadShipmentVolume: 100m,
				subShipmentVolume: 300m,
				expectedLeadShipmentSellAmt: 1000m,
				expectedSubShipmentSellAmt: null,
				expectedLeadShipmentRevenueCalculationDescription: "DDOC: 1 House Bill(s) @ USD 1000.00/House Bill",
				expectedSubShipmentRevenueCalculationDescription: "reason:\t'HB' unit is calculated only for Lead Shipments but NOT Sub-Shipments in BCN",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=HB THEN LeadShipment.Charge = Total LeadShipments x PerUnitPrice",
				expectedSubShipmentNotification: "Error This Shipment is part of a Buyer's Consol. Generally, you should Autorate and invoice from the Buyer's Consol Master - Shipment S00001000"
			);
		}

		public void TestNoApportionment_UnitCalculator_HBUnit_BuyersConsolInvoicingStyleAPP()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.HB, perUnit: 1000m);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				rateEntry,
				leadShipmentWeight: 0m,
				subShipmentWeight: 0m,
				leadShipmentVolume: 100m,
				subShipmentVolume: 300m,
				expectedLeadShipmentSellAmt: 1000m,
				expectedSubShipmentSellAmt: null,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "reason:\t'HB' unit is calculated only for Lead Shipments but NOT Sub-Shipments in BCN",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=HB THEN LeadShipment.Charge = Total LeadShipments x PerUnitPrice"
			);
		}

		public void TestNoApportionment_UnitCalculator_HBUnit_BuyersConsolInvoicingStyleMAB()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.HB, perUnit: 1000m);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				rateEntry,
				leadShipmentWeight: 0m,
				subShipmentWeight: 0m,
				leadShipmentVolume: 100m,
				subShipmentVolume: 300m,
				expectedLeadShipmentSellAmt: 1000m,
				expectedSubShipmentSellAmt: null,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "reason:\t'HB' unit is calculated only for Lead Shipments but NOT Sub-Shipments in BCN",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=HB THEN LeadShipment.Charge = Total LeadShipments x PerUnitPrice"
			);
		}

		#endregion

		public void TestNoApportionment_UnitCalculator_LWUnit_BuyersConsolInvoicingStyleAPP()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.LW, perUnit: 1000m);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				rateEntry,
				leadShipmentWeight: 0m,
				subShipmentWeight: 0m,
				leadShipmentVolume: 100m,
				subShipmentVolume: 300m,
				expectedLeadShipmentSellAmt: null,
				expectedSubShipmentSellAmt: 1000m,
				expectedLeadShipmentRevenueCalculationDescription: "reason:\t'LW' unit is calculated only for Sub-Shipments but NOT Lead Shipments in BCN",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=LW THEN SubShipments charge = Total SubShipments x PerUnitPrice"
			);
		}

		public void TestNoApportionment_UnitCalculator_LWUnit_BuyersConsolInvoicingStyleMAB()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.LW, perUnit: 1000m);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				rateEntry,
				leadShipmentWeight: 0m,
				subShipmentWeight: 0m,
				leadShipmentVolume: 100m,
				subShipmentVolume: 300m,
				expectedLeadShipmentSellAmt: null,
				expectedSubShipmentSellAmt: 1000m,
				expectedLeadShipmentRevenueCalculationDescription: "reason:\t'LW' unit is calculated only for Sub-Shipments but NOT Lead Shipments in BCN",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=LW THEN SubShipments charge = Total SubShipments x PerUnitPrice"
			);
		}

		#region Weight (KG)

		public void TestNoApportionment_UnitCalculator_KGUnit_BuyersConsolInvoicingStyleMAS()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.KG, perUnit: 1000m);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Master,
				rateEntry,
				leadShipmentWeight: 10m,
				subShipmentWeight: 20m,
				leadShipmentVolume: 0m,
				subShipmentVolume: 0m,
				expectedLeadShipmentSellAmt: 30000m,
				expectedSubShipmentSellAmt: null,
				expectedLeadShipmentRevenueCalculationDescription: "DDOC: 30 Kilogram(s) @ USD 1000.00/KG",
				expectedSubShipmentRevenueCalculationDescription: null,
				message: "GIVEN BuyerConsolInvoicingStyle=MAS and RateLine.UnitFactor=BCN and UnitCalculator with Unit=KG THEN charge is only on LeadShipment",
				expectedSubShipmentNotification: "Error This Shipment is part of a Buyer's Consol. Generally, you should Autorate and invoice from the Buyer's Consol Master - Shipment S00001000"
			);
		}

		public void TestNoApportionment_UnitCalculator_KGUnit_BuyersConsolInvoicingStyleAPP()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.KG, perUnit: 1000m);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				rateEntry,
				leadShipmentWeight: 10m,
				subShipmentWeight: 20m,
				leadShipmentVolume: 0m,
				subShipmentVolume: 0m,
				expectedLeadShipmentSellAmt: 10000m,
				expectedSubShipmentSellAmt: 20000m,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=KG THEN lead and SubShipments charge = Weight x PerUnitPrice"
			);
		}

		public void TestNoApportionment_UnitCalculator_KGUnit_BuyersConsolInvoicingStyleMAB()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.KG, perUnit: 1000m);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				rateEntry,
				leadShipmentWeight: 10m,
				subShipmentWeight: 20m,
				leadShipmentVolume: 0m,
				subShipmentVolume: 0m,
				expectedLeadShipmentSellAmt: 10000m,
				expectedSubShipmentSellAmt: 20000m,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=KG THEN lead and SubShipments charge = Weight x PerUnitPrice"
			);
		}

		public void TestNoApportionment_CMBCalculator_KGUnit_BuyersConsolInvoicingStyleAPP()
		{
			var rateEntry = CreateRateEntryWithCMBCalculator(lessThan100Price: 2000m, greaterThan100Price: 1000m, QuantityUnit.KG);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				rateEntry,
				leadShipmentWeight: 10m,
				subShipmentWeight: 20m,
				leadShipmentVolume: 0m,
				subShipmentVolume: 0m,
				expectedLeadShipmentSellAmt: 20000m,
				expectedSubShipmentSellAmt: 40000m,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and CMBCalculator THEN lead and SubShipments charge = Weight x PerUnitPrice"
			);
		}

		public void TestNoApportionment_CMBCalculator_KGUnit_BuyersConsolInvoicingStyleMAB()
		{
			var rateEntry = CreateRateEntryWithCMBCalculator(lessThan100Price: 2000m, greaterThan100Price: 1000m, QuantityUnit.KG);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				rateEntry,
				leadShipmentWeight: 10m,
				subShipmentWeight: 20m,
				leadShipmentVolume: 0m,
				subShipmentVolume: 0m,
				expectedLeadShipmentSellAmt: 20000m,
				expectedSubShipmentSellAmt: 40000m,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=KG THEN lead and SubShipments charge = Weight x PerUnitPrice"
			);
		}

		#endregion

		#region Volume (M3)

		public void TestNoApportionment_UnitCalculator_M3Unit_BuyersConsolInvoicingStyleAPP()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.M3, perUnit: 10m);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				rateEntry,
				leadShipmentWeight: 100m,
				subShipmentWeight: 200m,
				leadShipmentVolume: 10m,
				subShipmentVolume: 20m,
				expectedLeadShipmentSellAmt: 100m,
				expectedSubShipmentSellAmt: 200m,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=M3 THEN lead and SubShipments charge = Volume x PerUnitPrice"
			);
		}

		public void TestNoApportionment_UnitCalculator_M3Unit_BuyersConsolInvoicingStyleMAB()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.M3, perUnit: 10m);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				rateEntry,
				leadShipmentWeight: 100m,
				subShipmentWeight: 200m,
				leadShipmentVolume: 10m,
				subShipmentVolume: 20m,
				expectedLeadShipmentSellAmt: 100m,
				expectedSubShipmentSellAmt: 200m,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=M3 THEN lead and SubShipments charge = Volume x PerUnitPrice"
			);
		}

		public void TestNoApportionment_CMBCalculator_M3Unit_BuyersConsolInvoicingStyleAPP()
		{
			var rateEntry = CreateRateEntryWithCMBCalculator(lessThan100Price: 20m, greaterThan100Price: 10m, QuantityUnit.M3);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				rateEntry,
				leadShipmentWeight: 100m,
				subShipmentWeight: 200m,
				leadShipmentVolume: 10m,
				subShipmentVolume: 20m,
				expectedLeadShipmentSellAmt: 200m,
				expectedSubShipmentSellAmt: 400m,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and CMBCalculator THEN lead and SubShipments charge = Volume x PerUnitPrice"
			);
		}

		public void TestNoApportionment_CMBCalculator_M3Unit_BuyersConsolInvoicingStyleMAB()
		{
			var rateEntry = CreateRateEntryWithCMBCalculator(lessThan100Price: 20m, greaterThan100Price: 10m, QuantityUnit.M3);
			AssertNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				rateEntry,
				leadShipmentWeight: 100m,
				subShipmentWeight: 200m,
				leadShipmentVolume: 10m,
				subShipmentVolume: 20m,
				expectedLeadShipmentSellAmt: 200m,
				expectedSubShipmentSellAmt: 400m,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor=BCN and UnitCalculator with Unit=KG THEN lead and SubShipments charge = Volume x PerUnitPrice"
			);
		}

		#endregion

		#region Container (CN)

		public void TestNoApportionment_UnitCalculator_CNUnit_BuyersConsolInvoicingStyleAPP()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.CN, perUnit: 10m);
			rateEntry.TI_RC = GP20.PK;

			var (_, leadShipment, subShipment) = ArrangeNoApportionment_UnitCalculator_CNUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				rateEntry,
				containerCount: 1,
				leadShipmentWeight: 1m,
				subShipmentWeight: 2m,
				leadShipmentVolume: 10m,
				subShipmentVolume: 20
			);

			AssertNoApportionment
			(
				leadShipment,
				expectedSellAmt: null,
				expectedRevenueCalculationDescription: string.Empty,
				expectedNotification: "Information: RateLine Filtered DDOC-UNT-CN-20GP-Client Rate CONSIGLAX\treason:\t'CN' unit is NOT supported by Unit Factor 'BCN - No Apportionment'",
				expectedError: null,
				message: "LeadShipment: CTN unit is not supported BECAUSE illogical to charge 1 consol container to both leadShipment and subShipment"
			);

			AssertNoApportionment
			(
				subShipment,
				expectedSellAmt: null,
				expectedRevenueCalculationDescription: string.Empty,
				expectedNotification: "Information: RateLine Filtered DDOC-UNT-CN-20GP-Client Rate CONSIGLAX\treason:\t'CN' unit is NOT supported by Unit Factor 'BCN - No Apportionment'",
				expectedError: null,
				message: "SubShipment: CTN unit is not supported BECAUSE illogical to charge 1 consol container to both leadShipment and subShipment"
			);
		}

		public void TestNoApportionment_UnitCalculator_CNUnit_BuyersConsolInvoicingStyleMAB()
		{
			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.CN, perUnit: 10m);
			rateEntry.TI_RC = GP20.PK;

			var (_, leadShipment, subShipment) = ArrangeNoApportionment_UnitCalculator_CNUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				rateEntry,
				containerCount: 1,
				leadShipmentWeight: 1m,
				subShipmentWeight: 2m,
				leadShipmentVolume: 10m,
				subShipmentVolume: 20
			);

			AssertNoApportionment
			(
				leadShipment,
				expectedSellAmt: null,
				expectedRevenueCalculationDescription: string.Empty,
				expectedNotification: "Information: RateLine Filtered DDOC-UNT-CN-20GP-Client Rate CONSIGLAX\treason:\t'CN' unit is NOT supported by Unit Factor 'BCN - No Apportionment'",
				expectedError: null,
				message: "LeadShipment: CTN unit is not supported BECAUSE illogical to charge 1 consol container to both leadShipment and subShipment"
			);

			AssertNoApportionment
			(
				subShipment,
				expectedSellAmt: null,
				expectedRevenueCalculationDescription: string.Empty,
				expectedNotification: "Information: RateLine Filtered DDOC-UNT-CN-20GP-Client Rate CONSIGLAX\treason:\t'CN' unit is NOT supported by Unit Factor 'BCN - No Apportionment'",
				expectedError: null,
				message: "SubShipment: CTN unit is not supported BECAUSE illogical to charge 1 consol container to both leadShipment and subShipment"
			);
		}

		(ForwardingConsol, ForwardingShipment, ForwardingShipment) ArrangeNoApportionment_UnitCalculator_CNUnit(string buyersConsolInvoicingStyle, RateEntry rateEntry, short containerCount, decimal leadShipmentWeight, decimal subShipmentWeight, decimal leadShipmentVolume, decimal subShipmentVolume)
		{
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			Factory.Save();

			var (consol, leadShipment, subShipment) = ArrangeNoApportionment
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				rateEntry,
				leadShipmentWeight,
				subShipmentWeight,
				leadShipmentVolume,
				subShipmentVolume
			);

			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;
			container.JC_ContainerCount = containerCount;

			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = buyersConsolInvoicingStyle;

			Factory.Save();

			return (consol, leadShipment, subShipment);
		}

		#endregion

		#region Service

		#region Delivery-Labour service

		public void TestApportionment_UnitCalculator_SVUnit_DeliveryLabour_BuyersConsolInvoicingStyleAPP()
		{
			var chargeCode = Helper.ChargeCodes["DLAB"];
			CreateRateEntryWithUnitCalculator(QuantityUnit.SV, perUnit: 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE);

			AssertNoApportionment_UnitCalculator_SVUnit_DeliveryLabour
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 4m, // 1 Service x $10 x 2kg/5kg
				expectedSubShipmentSellAmt: 6m, // 1 Service x $10 x 3kg/5kg
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 20.000 KG, Buyers Consol Total: 50.000 KG)",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 30.000 KG, Buyers Consol Total: 50.000 KG)",
				message: "GIVEN RateLine.UnitFactor != BCN and UnitCalculator with Unit = SV THEN Lead/Sub-Shipments charge = Lead-ServiceCount x PerUnitPrice x apportion (Sub-shipments service is ignore)"
			);
		}

		public void TestApportionment_UnitCalculator_SVUnit_DeliveryLabour_BuyersConsolInvoicingStyleMAB()
		{
			var chargeCode = Helper.ChargeCodes["DLAB"];
			CreateRateEntryWithUnitCalculator(QuantityUnit.SV, perUnit: 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE);

			AssertNoApportionment_UnitCalculator_SVUnit_DeliveryLabour
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 4m, // 1 Service x $10 x 2kg/5kg
				expectedSubShipmentSellAmt: 6m, // 1 Service x $10 x 3kg/5kg
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 20.000 KG, Buyers Consol Total: 50.000 KG)",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 30.000 KG, Buyers Consol Total: 50.000 KG)",
				message: "GIVEN RateLine.UnitFactor != BCN and UnitCalculator with Unit = SV THEN lead and SubShipments charge = ServiceCount x PerUnitPrice x apportiont"
			);
		}

		public void TestNoApportionment_UnitCalculator_SVUnit_DeliveryLabour_BuyersConsolInvoicingStyleAPP()
		{
			var chargeCode = Helper.ChargeCodes["DLAB"];

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.SV, perUnit: 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_SVUnit_DeliveryLabour
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 10m, // 1 service-delivery-labour x $10
				expectedSubShipmentSellAmt: 10m, // 1 service-delivery-labour x $10
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = SV THEN lead and SubShipments charge = ServiceCount x PerUnitPrice (not apportioned)"
			);
		}

		public void TestNoApportionment_UnitCalculator_SVUnit_DeliveryLabour_BuyersConsolInvoicingStyleMAS()
		{
			var chargeCode = Helper.ChargeCodes["DLAB"];

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.SV, perUnit: 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_SVUnit_DeliveryLabour
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Master,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 10m, // 1 service-delivery-labour x $10
				expectedSubShipmentSellAmt: null,
				expectedLeadShipmentRevenueCalculationDescription: "DLAB: 1 Destination Labor @ USD 10.00/Destination Labor",
				expectedSubShipmentRevenueCalculationDescription: null,
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = SV and BuyersConsolInvoicingStyle = MAS THEN leadShipments charge = ServiceCount x PerUnitPrice (not apportioned)",
				expectedSubShipmentNotification: "Error This Shipment is part of a Buyer's Consol. Generally, you should Autorate and invoice from the Buyer's Consol Master - Shipment S00001000"
			);
		}

		public void TestNoApportionment_UnitCalculator_SVUnit_DeliveryLabour_BuyersConsolInvoicingStyleMAB()
		{
			var chargeCode = Helper.ChargeCodes["DLAB"];

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.SV, perUnit: 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_SVUnit_DeliveryLabour
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 10m, // 1 service-delivery-labour x $10
				expectedSubShipmentSellAmt: 10m, // 1 service-delivery-labour x $10
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = SV THEN lead and SubShipments charge = ServiceCount x PerUnitPrice (not apportioned)"
			);
		}

		public void AssertNoApportionment_UnitCalculator_SVUnit_DeliveryLabour(string buyersConsolInvoicingStyle, decimal leadShipmentWeight, decimal subShipmentWeight, string expectedChargeCode, decimal expectedLeadShipmentSellAmt, decimal? expectedSubShipmentSellAmt, string expectedLeadShipmentRevenueCalculationDescription, string expectedSubShipmentRevenueCalculationDescription, string message, string expectedSubShipmentNotification = null)
		{
			var consol = Helper.CreateBuyersConsolConsol("AUSYD", "USLAX", Creditor, TransportModes.Air);

			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = buyersConsolInvoicingStyle;

			var baseDate = ZDateTime.MinSmallDateTimeValue;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, weight: leadShipmentWeight, volume: 0m, transportMode: TransportModes.Air);
			leadShipment.DocsAndCartage.JP_DeliveryLabourTime = baseDate.AddHours(2);
			leadShipment.JS_INCO = IncoTerms.DeliveredDutyPaid;

			var subShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, weight: subShipmentWeight, volume: 0m);
			subShipment.DocsAndCartage.JP_DeliveryLabourTime = baseDate.AddHours(3);
			subShipment.JS_INCO = IncoTerms.DeliveredDutyPaid;

			Factory.Save();

			AutorateAndAssert
			(
				$"leadShipment: {message}",
				expected: new[]
				{
					new AssertionCharge
					{
						ChargeCode = expectedChargeCode,
						JR_OSSellAmt = expectedLeadShipmentSellAmt,
						RevenueCalculationDescription = expectedLeadShipmentRevenueCalculationDescription
					},
				},
				leadShipment,
				Consignee,
				autorateCosts: false
			);

			var expectedErrors = string.IsNullOrEmpty(expectedSubShipmentNotification) ? null : new[] { expectedSubShipmentNotification };
			AutorateAndAssert
			(
				$"subShipment: {message}",
				expected: expectedSubShipmentSellAmt != null
				? new[]
				{
					new AssertionCharge
					{
						ChargeCode = expectedChargeCode,
						JR_OSSellAmt = expectedSubShipmentSellAmt.Value,
						RevenueCalculationDescription = expectedSubShipmentRevenueCalculationDescription
					},
				}
				: Array.Empty<AssertionCharge>(),
				subShipment,
				Consignee,
				expectedErrors: expectedErrors,
				autorateCosts: false
			);
		}

		#endregion

		#region Cleaning Service

		public void TestApportionment_UnitCalculator_SVUnit_Cleaning_BuyersConsolInvoicingStyleAPP()
		{
			var chargeCode = Helper.ChargeCodes.New("TSTCLN", "Cleaning Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Cleaning);

			CreateRateEntryWithUnitCalculator(QuantityUnit.SV, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);

			AssertNoApportionment_UnitCalculator_SVUnit_Cleaning
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentCleaningServiceCount: 3m,
				subShipmentCleaningServiceCount: 7m,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 12m, // 3-lead-shipment-service x $10 x 20kg / 50kg
				expectedSubShipmentSellAmt: 18m, // 3-lead--shipment-service x $10 x 30kg / 50kg
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 20.000 KG, Buyers Consol Total: 50.000 KG)",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 30.000 KG, Buyers Consol Total: 50.000 KG)",
				message: "GIVEN RateLine.UnitFactor != BCN and UnitCalculator with Unit = SV THEN lead/Sub-Shipment charge = LeadShipment-ServiceCount x PerUnitPrice x apportioned (subShipment service count is ignored)"
			);
		}

		public void TestApportionment_UnitCalculator_SVUnit_Cleaning_BuyersConsolInvoicingStyleMAB()
		{
			var chargeCode = Helper.ChargeCodes.New("TSTCLN", "Cleaning Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Cleaning);

			CreateRateEntryWithUnitCalculator(QuantityUnit.SV, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);

			AssertNoApportionment_UnitCalculator_SVUnit_Cleaning
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				leadShipmentCleaningServiceCount: 3m,
				subShipmentCleaningServiceCount: 7m,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 12m, // 3-lead-shipment-service x $10 x 20kg / 50kg
				expectedSubShipmentSellAmt: 18m, // 3-lead--shipment-service x $10 x 30kg / 50kg
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 20.000 KG, Buyers Consol Total: 50.000 KG)",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 30.000 KG, Buyers Consol Total: 50.000 KG)",
				message: "GIVEN RateLine.UnitFactor != BCN and UnitCalculator with Unit = SV THEN lead/Sub-Shipment charge = LeadShipment-ServiceCount x PerUnitPrice x apportioned (subShipment service count is ignored)"
			);
		}

		public void TestNoApportionment_UnitCalculator_SVUnit_Cleaning_BuyersConsolInvoicingStyleAPP()
		{
			var chargeCode = Helper.ChargeCodes.New("TSTCLN", "Cleaning Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Cleaning);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.SV, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_SVUnit_Cleaning
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentCleaningServiceCount: 4m,
				subShipmentCleaningServiceCount: 5m,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 40m, // 4 lead-shipment cleaning-service x $10
				expectedSubShipmentSellAmt: 50m, // 5 sub-shipment cleanign-service x $10
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = SV THEN leadShipment charge = ServiceCount x PerUnitPrice (not apportioned) while and SubShipments charge = 0"
			);
		}

		public void TestNoApportionment_UnitCalculator_SVUnit_Cleaning_BuyersConsolInvoicingStyleMAB()
		{
			var chargeCode = Helper.ChargeCodes.New("TSTCLN", "Cleaning Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Cleaning);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.SV, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_SVUnit_Cleaning
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				leadShipmentCleaningServiceCount: 4m,
				subShipmentCleaningServiceCount: 5m,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 40m, // 4 cleaning-service-count x $10
				expectedSubShipmentSellAmt: 50m, // 5 sub-shipment cleanign-service x $10
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = SV THEN leadShipment charge = ServiceCount x PerUnitPrice (not apportioned) while and SubShipments charge = 0"
			);
		}

		public void TestNoApportionment_UnitCalculator_SVUnit_Cleaning_BuyersConsolInvoicingStyleAPP_ServiceOnlyOnSubShipment()
		{
			var chargeCode = Helper.ChargeCodes.New("TSTCLN", "Cleaning Service", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Cleaning);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.SV, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_SVUnit_Cleaning
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentCleaningServiceCount: null,
				subShipmentCleaningServiceCount: 5m,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: null,
				expectedSubShipmentSellAmt: 50m, // 5 sub-shipment cleanign-service x $10
				expectedLeadShipmentRevenueCalculationDescription: null,
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = SV THEN leadShipment charge = ServiceCount x PerUnitPrice (not apportioned) while and SubShipments charge = 0"
			);
		}

		public void AssertNoApportionment_UnitCalculator_SVUnit_Cleaning(string buyersConsolInvoicingStyle, decimal? leadShipmentCleaningServiceCount, decimal? subShipmentCleaningServiceCount, decimal leadShipmentWeight, decimal subShipmentWeight, string expectedChargeCode, decimal? expectedLeadShipmentSellAmt, decimal? expectedSubShipmentSellAmt, string expectedLeadShipmentRevenueCalculationDescription, string expectedSubShipmentRevenueCalculationDescription, string message)
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = buyersConsolInvoicingStyle;

			var consol = Helper.CreateBuyersConsolConsol("AUSYD", "USLAX", Creditor, TransportModes.Air);

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, leadShipmentWeight, volume: 0m, transportMode: TransportModes.Air);
			leadShipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = ZDateTime.Now.AddDays(-2); // To make CLN service as DST so ForwardingShipmentRatingAdapter won't have charge
			if (leadShipmentCleaningServiceCount != null)
			{
				var leadShipmentCleaningService = leadShipment.DocsAndCartage.Services.AddNew();
				leadShipmentCleaningService.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
				leadShipmentCleaningService.ES_ServiceCount = leadShipmentCleaningServiceCount.Value;
				leadShipmentCleaningService.ES_Completed = ZDateTime.Now.AddDays(-1);
			}

			var subShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, subShipmentWeight, volume: 0m);
			subShipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = ZDateTime.Now.AddDays(-2);
			if (subShipmentCleaningServiceCount != null)
			{
				var subShipmentCleaningService = subShipment.DocsAndCartage.Services.AddNew();
				subShipmentCleaningService.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
				subShipmentCleaningService.ES_ServiceCount = subShipmentCleaningServiceCount.Value;
				subShipmentCleaningService.ES_Completed = ZDateTime.Now.AddDays(-1);
			}

			Factory.Save();

			AutorateAndAssert
			(
				$"LeadShipment: {message}",
				expected: expectedLeadShipmentSellAmt != null
					? new[]
					{
						new AssertionCharge
						{
							ChargeCode = expectedChargeCode,
							JR_OSSellAmt = expectedLeadShipmentSellAmt.Value,
							RevenueCalculationDescription = expectedLeadShipmentRevenueCalculationDescription
						},
					}
					: Array.Empty<AssertionCharge>(),
				leadShipment,
				Consignee,
				autorateCosts: false
			);

			AutorateAndAssert
			(
				$"SubShipment: {message}",
				expected: expectedSubShipmentSellAmt != null
					? new[]
					{
						new AssertionCharge
						{
							ChargeCode = expectedChargeCode,
							JR_OSSellAmt = expectedSubShipmentSellAmt.Value,
							RevenueCalculationDescription = expectedSubShipmentRevenueCalculationDescription
						},
					}
					: Array.Empty<AssertionCharge>(),
				subShipment,
				Consignee,
				autorateCosts: false
			);
		}

		#endregion

		#endregion

		RateEntry CreateRateEntryWithCMBCalculator(decimal lessThan100Price, decimal greaterThan100Price, string lineUnit)
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("DDOC", CombinedCalculator.Code, lineUnit);
			var calculator = rateLine.Calculator;
			calculator["-100"] = (ZDecimal)lessThan100Price;
			calculator["+100"] = (ZDecimal)greaterThan100Price;

			return rateEntry;
		}

		RateEntry CreateRateEntryWithPerUnitRateLine(RatingHeader ratingHeader, string lineUnit, decimal perUnit, string chargeCode = "DDOC", string rateMode = "FCL", string rateCategory = "DST", string origin = "AUSYD", string destination = "USLAX", string container = "")
		{
			var rateEntry = ratingHeader.AddRateEntry(rateCategory, rateMode, origin, destination, container: container, removeLines: true);
			rateEntry.TI_RateStartDate = ZDateTime.Today.Date.AddDays(-60);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, lineUnit);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = perUnit;

			return rateEntry;
		}

		RateEntry CreateRateEntryWithUnitCalculator(string lineUnit, decimal perUnit, string chargeCode = "DDOC", string rateMode = "FCL", string rateCategory = "DST", string origin = "AUSYD", string destination = "USLAX")
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(rateCategory, rateMode, origin, destination);
			rateEntry.TI_RateStartDate = ZDateTime.Today.Date.AddDays(-60);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, lineUnit);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = perUnit;

			return rateEntry;
		}

		void AssertNoApportionment(string buyersConsolInvoicingStyle, RateEntry rateEntry, decimal leadShipmentWeight, decimal subShipmentWeight, decimal leadShipmentVolume, decimal subShipmentVolume, decimal? expectedLeadShipmentSellAmt, decimal? expectedSubShipmentSellAmt, string expectedLeadShipmentRevenueCalculationDescription, string expectedSubShipmentRevenueCalculationDescription, string message, string expectedSubShipmentNotification = null)
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = buyersConsolInvoicingStyle;

			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_UnitFactor = UnitFactorList.Codes.BCN;

			var (_, leadShipment, subShipment) = ArrangeNoApportionment(buyersConsolInvoicingStyle, rateEntry, leadShipmentWeight, subShipmentWeight, leadShipmentVolume, subShipmentVolume);

			Factory.Save();

			AssertNoApportionment
			(
				leadShipment,
				expectedSellAmt: expectedLeadShipmentSellAmt,
				expectedRevenueCalculationDescription: expectedLeadShipmentRevenueCalculationDescription,
				expectedNotification: null,
				expectedError: null,
				message: $"LeadShipment: {message}"
			);

			AssertNoApportionment
			(
				subShipment,
				expectedSellAmt: expectedSubShipmentSellAmt,
				expectedRevenueCalculationDescription: expectedSubShipmentRevenueCalculationDescription,
				expectedNotification: null,
				expectedError: expectedSubShipmentNotification,
				message: $"SubShipment: {message}"
			);
		}

		void AssertNoApportionment(ForwardingShipment shipment, decimal? expectedSellAmt, string expectedRevenueCalculationDescription, string expectedNotification, string expectedError, string message)
		{
			var expectedShipment = expectedSellAmt != null
				? new[]
				{
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_OSSellAmt = expectedSellAmt.Value,
						RevenueCalculationDescription = expectedRevenueCalculationDescription
					}
				}
				: Array.Empty<AssertionCharge>();

			var expectedErrors = string.IsNullOrEmpty(expectedError) ? null : new[] { expectedError };
			AutorateAndAssert($"Shipment: {message}", expectedShipment, shipment, Consignee, autorateCosts: false, expectedErrors: expectedErrors);

			if (!string.IsNullOrEmpty(expectedNotification))
			{
				AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedNotification);
			}
		}

		(ForwardingConsol, ForwardingShipment, ForwardingShipment) ArrangeNoApportionment(string buyersConsolInvoicingStyle, RateEntry rateEntry, decimal leadShipmentWeight, decimal subShipmentWeight, decimal leadShipmentVolume, decimal subShipmentVolume)
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = buyersConsolInvoicingStyle;

			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_UnitFactor = UnitFactorList.Codes.BCN;

			var consol = Helper.CreateBuyersConsolConsol("AUSYD", "USLAX", Creditor);
			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;

			Consignor.OH_RL_NKClosestPort = "AUSYD";
			Consignee.OH_RL_NKClosestPort = "USLAX";

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, leadShipmentWeight, leadShipmentVolume);
			leadShipment.JS_INCO = ZString.Empty;
			var subShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, subShipmentWeight, subShipmentVolume);
			subShipment.JS_INCO = ZString.Empty;

			Factory.Save();

			return (consol, leadShipment, subShipment);
		}

		#region Time Unit

		#region Time Unit (HR)

		public void TestApportionment_UnitCalculator_HRUnit_BuyersConsolInvoicingStyleAPP()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);
			CreateRateEntryWithUnitCalculator(QuantityUnit.HR, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);

			AssertNoApportionment_UnitCalculator_TimeUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				leadShipmentServiceDuration: new TimeSpan(3, 0, 0),
				subShipmentServiceDuration: new TimeSpan(4, 0, 0),
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 12m, // 3hr X 20kg/(20kg+30kg) x $10 = $12
				expectedSubShipmentSellAmt: 18m, // 3hr X 30kg/(20kg+30kg) x $10 = $18
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 20.000 KG, Buyers Consol Total: 50.000 KG)",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 30.000 KG, Buyers Consol Total: 50.000 KG)",
				expectedSubShipmentNotification: null,
				message: "GIVEN RateLine.UnitFactor != BCN and UnitCalculator with Unit = HR THEN Lead/Sub-Shipments charge = ServiceDuration x WeightApportion x PerUnitPrice"
			);
		}

		public void TestApportionment_UnitCalculator_HRUnit_BuyersConsolInvoicingStyleMAS()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.HR, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_TimeUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Master,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				leadShipmentServiceDuration: new TimeSpan(3, 0, 0),
				subShipmentServiceDuration: new TimeSpan(4, 0, 0),
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 30m, // 3hr * $10
				expectedSubShipmentSellAmt: null,
				expectedLeadShipmentRevenueCalculationDescription: "DSTFUM1: 3 Hour(s) @ USD 10.00/Hour",
				expectedSubShipmentRevenueCalculationDescription: null,
				expectedSubShipmentNotification: "Error This Shipment is part of a Buyer's Consol. Generally, you should Autorate and invoice from the Buyer's Consol Master - Shipment S00001000",
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = HR and BuyersConsolInvoicingStyle = MAS THEN leadShipment charge = ServiceDuration x PerUnitPrice (not apportioned)"
			);
		}

		public void TestNoApportionment_UnitCalculator_HRUnit_BuyersConsolInvoicingStyleAPP_ServiceOnlyOnLeadShipment()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.HR, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_TimeUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				leadShipmentServiceDuration: new TimeSpan(3, 0, 0),
				subShipmentServiceDuration: null,
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 30m, // 3hr x $10
				expectedSubShipmentSellAmt: null,
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: null,
				expectedSubShipmentNotification: null,
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = HR and BuyersConsolInvoicingStyle = APP THEN leadShipment charge = ServiceDuration x PerUnitPrice (not apportioned)"
			);
		}

		public void TestNoApportionment_UnitCalculator_HRUnit_BuyersConsolInvoicingStyleAPP_ServiceOnlyOnSubShipment()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.HR, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_TimeUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				leadShipmentServiceDuration: null,
				subShipmentServiceDuration: new TimeSpan(4, 0, 0),
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: null,
				expectedSubShipmentSellAmt: 40m, // 4hr x $10
				expectedLeadShipmentRevenueCalculationDescription: null,
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentNotification: null,
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = HR and BuyersConsolInvoicingStyle = APP THEN subShipment charge = ServiceDuration x PerUnitPrice (not apportioned)"
			);
		}

		public void TestNoApportionment_UnitCalculator_HRUnit_BuyersConsolInvoicingStyleAPP()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.HR, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_TimeUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				leadShipmentServiceDuration: new TimeSpan(3, 0, 0),
				subShipmentServiceDuration: new TimeSpan(4, 0, 0),
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 30m, // 3hr x $10
				expectedSubShipmentSellAmt: 40m, // 4hr x $10
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentNotification: null,
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = HR THEN Lead/Sub-Shipment charge = ServiceDuration x PerUnitPrice (not apportioned)"
			);
		}

		public void TestNoApportionment_UnitCalculator_HRUnit_BuyersConsolInvoicingStyleMAB()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.HR, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_TimeUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				leadShipmentServiceDuration: new TimeSpan(3, 0, 0),
				subShipmentServiceDuration: new TimeSpan(4, 0, 0),
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 30m, // 3hr x $10
				expectedSubShipmentSellAmt: 40m, // 4hr x $10
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentNotification: null,
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = HR THEN Lead/Sub-Shipment charge = ServiceDuration x PerUnitPrice (not apportioned)"
			);
		}

		#endregion

		#region Time Unit (DY)

		public void TestNoApportionment_UnitCalculator_DYUnit_BuyersConsolInvoicingStyleAPP()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.DY, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_TimeUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				leadShipmentServiceDuration: new TimeSpan(3, 0, 0, 0),
				subShipmentServiceDuration: new TimeSpan(4, 0, 0, 0),
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 30m, // 3dy x $10
				expectedSubShipmentSellAmt: 40m, // 4dy x $10
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentNotification: null,
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = DY THEN Lead/Sub-Shipment charge = ServiceDuration x PerUnitPrice (not apportioned)"
			);
		}

		public void TestNoApportionment_UnitCalculator_DYUnit_BuyersConsolInvoicingStyleMAB()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.DY, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_TimeUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				leadShipmentServiceDuration: new TimeSpan(3, 0, 0, 0),
				subShipmentServiceDuration: new TimeSpan(4, 0, 0, 0),
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 30m, // 3dy x $10
				expectedSubShipmentSellAmt: 40m, // 4dy x $10
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentNotification: null,
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = DY THEN Lead/Sub-Shipment charge = ServiceDuration x PerUnitPrice (not apportioned)"
			);
		}

		#endregion

		#region Time Unit (WK)

		public void TestNoApportionment_UnitCalculator_WKUnit_BuyersConsolInvoicingStyleAPP()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.WK, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_TimeUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.Apportion,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				leadShipmentServiceDuration: new TimeSpan(21, 0, 0, 0),
				subShipmentServiceDuration: new TimeSpan(28, 0, 0, 0),
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 30m, // 3wk x $10
				expectedSubShipmentSellAmt: 40m, // 4wk x $10
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentNotification: null,
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = WK THEN Lead/Sub-Shipment charge = ServiceDuration x PerUnitPrice (not apportioned)"
			);
		}

		public void TestNoApportionment_UnitCalculator_WKUnit_BuyersConsolInvoicingStyleMAB()
		{
			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("DSTFUM1", "Fumigation 1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation);

			var rateEntry = CreateRateEntryWithUnitCalculator(QuantityUnit.WK, 10m, chargeCode: chargeCode.AC_Code, rateMode: RateMode.LSE, rateCategory: RatingConstants.RateCategory.DST);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.BCN;

			AssertNoApportionment_UnitCalculator_TimeUnit
			(
				buyersConsolInvoicingStyle: ConsolInvoicingStyles.ApportionInvoiceMaster,
				leadShipmentWeight: 20m,
				subShipmentWeight: 30m,
				leadShipmentServiceDuration: new TimeSpan(21, 0, 0, 0),
				subShipmentServiceDuration: new TimeSpan(28, 0, 0, 0),
				expectedChargeCode: chargeCode.AC_Code,
				expectedLeadShipmentSellAmt: 30m, // 3wk x $10
				expectedSubShipmentSellAmt: 40m, // 4wk x $10
				expectedLeadShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentRevenueCalculationDescription: "This charge was autorated through a Buyers Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is BCN - No Apportionment",
				expectedSubShipmentNotification: null,
				message: "GIVEN RateLine.UnitFactor = BCN and UnitCalculator with Unit = WK THEN Lead/Sub-Shipment charge = ServiceDuration x PerUnitPrice (not apportioned)"
			);
		}

		#endregion

		public void AssertNoApportionment_UnitCalculator_TimeUnit(string buyersConsolInvoicingStyle, decimal leadShipmentWeight, decimal subShipmentWeight, TimeSpan? leadShipmentServiceDuration, TimeSpan? subShipmentServiceDuration, string expectedChargeCode, decimal? expectedLeadShipmentSellAmt, decimal? expectedSubShipmentSellAmt, string expectedLeadShipmentRevenueCalculationDescription, string expectedSubShipmentRevenueCalculationDescription, string expectedSubShipmentNotification, string message)
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = buyersConsolInvoicingStyle;

			var consol = Helper.CreateBuyersConsolConsol("AUSYD", "USLAX", Creditor, TransportModes.Air);

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, weight: leadShipmentWeight, volume: 0m, transportMode: TransportModes.Air);
			leadShipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = ZDateTime.Now.AddDays(-2); // To make CLN service as DST so ForwardingShipmentRatingAdapter won't have charge
			if (leadShipmentServiceDuration != null)
			{
				var leadShipmentCleaningService = leadShipment.DocsAndCartage.Services.AddNew();
				leadShipmentCleaningService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
				leadShipmentCleaningService.ES_ServiceCount = 1;
				leadShipmentCleaningService.ES_Duration = leadShipmentServiceDuration.Value;
				leadShipmentCleaningService.ES_Completed = ZDateTime.Now.AddDays(-1);
			}

			var subShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, weight: subShipmentWeight, volume: 0m);
			subShipment.TransportsIncludingRelated.DepartureTransport.JW_ATD = ZDateTime.Now.AddDays(-2);
			if (subShipmentServiceDuration != null)
			{
				var subShipmentCleaningService = subShipment.DocsAndCartage.Services.AddNew();
				subShipmentCleaningService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
				subShipmentCleaningService.ES_ServiceCount = 1;
				subShipmentCleaningService.ES_Duration = subShipmentServiceDuration.Value;
				subShipmentCleaningService.ES_Completed = ZDateTime.Now.AddDays(-1);
			}

			Factory.Save();

			AutorateAndAssert
			(
				$"LeadShipment: {message}",
				expected: expectedLeadShipmentSellAmt != null
					? new[]
					{
						new AssertionCharge
						{
							ChargeCode = expectedChargeCode,
							JR_OSSellAmt = expectedLeadShipmentSellAmt.Value,
							RevenueCalculationDescription = expectedLeadShipmentRevenueCalculationDescription
						},
					}
					: Array.Empty<AssertionCharge>(),
				leadShipment,
				Consignee,
				autorateCosts: false
			);

			var expectedErrors = string.IsNullOrEmpty(expectedSubShipmentNotification) ? null : new[] { expectedSubShipmentNotification };

			AutorateAndAssert
			(
				$"SubShipment: {message}",
				expected: expectedSubShipmentSellAmt != null
					? new[]
					{
						new AssertionCharge
						{
							ChargeCode = expectedChargeCode,
							JR_OSSellAmt = expectedSubShipmentSellAmt.Value,
							RevenueCalculationDescription = expectedSubShipmentRevenueCalculationDescription
						},
					}
					: Array.Empty<AssertionCharge>(),
				subShipment,
				Consignee,
				expectedErrors: expectedErrors,
				autorateCosts: false
			);
		}

		#endregion

		#endregion

		#region Coload Shipment

		public void TestColoadShipment_Costing()
		{
			var costing = Helper.NewCosting(Creditor);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", "DDOC", 50);
			Factory.Save();

			var consol = Helper.CreateBuyersConsolConsol("CNSHA", "AUSYD", Creditor);
			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 900, 15);
			var coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 1000, 15);

			Factory.Save();

			AutorateAndAssert(Array.Empty<AssertionCharge>(), coloadShipment, Consignee, autorateRevenue: false, expectedErrors: new[] { "Error This Shipment is part of a Buyer's Consol. Generally, you should Autorate and invoice from the Buyer's Consol Master - Shipment S00001000" });
		}

		public void TestColoadShipment_ClientRate()
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.ApportionInvoiceMaster;

			var clientRate = Helper.NewClientRate(Consignee);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU", "DDOC", 50);

			var consol = Helper.CreateBuyersConsolConsol("CNSHA", "AUSYD", Creditor);
			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 900, 15);
			var coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 1000, 15);

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "DDOC",
							JR_OSSellAmt = 25m,
							RevenueCalculationDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 15.000 M3, Buyers Consol Total: 30.000 M3)"
						}
				};

			AutorateAndAssert(expected, coloadShipment, Consignee, autorateCosts: false);
		}

		public void TestColoadShipment_Costing_HouseBillUnitDoesNotApply()
		{
			var costing = Helper.NewCosting(Creditor);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "");
			var costLine = costEntry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.HB);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var consol = Helper.CreateBuyersConsolConsol("AUSYD", "USBOS", Creditor);
			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee);
			var coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee);

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSCostAmt = 5m
						},
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSCostAmt = 0
						}
				};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AutorateAndAssert(expected, leadShipment, Creditor);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AutorateAndAssert(Array.Empty<AssertionCharge>(), coloadShipment, Creditor, expectedErrors: new[] { "Error This Shipment is part of a Buyer's Consol. Generally, you should Autorate and invoice from the Buyer's Consol Master - Shipment S00001000" });

			coloadShipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			coloadShipment.JS_PackingMode = ContainerModes.LCL;

			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_OSCostAmt = 5m
						}
				};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AutorateAndAssert(expected, coloadShipment, Creditor);
		}

		#endregion

		#region Consol

		public void TestConsol_Costing_HouseBillAndLowerBill()
		{
			var cost = Helper.NewCosting(Creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			costEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.HB).GetCalculator<UnitCalculator>().PerUnit = 5;
			costEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.LW).GetCalculator<UnitCalculator>().PerUnit = 10;

			var consol = Helper.CreateBuyersConsolConsol("USLAX", "AUSYD", Creditor, TransportModes.Air, PaymentType.Prepaid);
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 1500);
			leadShipment.JS_TransportMode = TransportModes.Air;
			var coloadShipment1 = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 1000);
			var coloadShipment2 = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 500);
			var anotherShipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 500, 0, consol);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 30,
					}
			};

			AutoCostAndAssert("", null, expectedCosts, consol, autorateRevenue: false);
		}

		#endregion

		#region Lead Shipment

		public void TestLeadShipment_ClientRate_HouseBillAndLowerBillUnitsApply()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.HB).GetCalculator<UnitCalculator>().PerUnit = 20;
			rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.LW).GetCalculator<UnitCalculator>().PerUnit = 25;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_CreditorAddress = Creditor.MainAddress.PK;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 1500);
			leadShipment.JS_TransportMode = TransportModes.Air;
			var coloadShipment1 = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 1000);
			var coloadShipment2 = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 500);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 70m,
					RevenueCalculationDescription = @"FRT: 2 Lowest Bill(s) @ AUD 25.00/Lowest Bill"
				}
			};

			AutorateAndAssert(expected, leadShipment, Consignee);
		}

		public void TestLeadShipment_ClientRate_ViaPortApplies()
		{
			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "CN", "AU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			rateEntry.TI_RC = GP20.PK;
			rateEntry.TI_ViaLRC = "AUBTB";
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 500m;

			Factory.Save();

			var consol = Helper.CreateBuyersConsolConsol("CNCAN", "AUBTB", Creditor);
			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 10000, 65);
			leadShipment.JS_RL_NKDestination = "AUSYD";
			var packline = leadShipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;

			var coloadShipment1 = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 8000, 40);
			var coloadShipment2 = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 2000, 15);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 500m,
				}
			};

			var message = "Should consider AUBTB as a VIA port for this Buyer's Consolidation Shipment because it's destination does not match the Consol's Discharge";
			AutorateAndAssert(message, expected, leadShipment, Consignee, autorateCosts: false);
		}

		public void TestLeadShipment_ZeroChargeableAmount_GenerateError()
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.ApportionInvoiceMaster;

			var chargeCode = Helper.ChargeCodes.New("AAA", "AAA", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "USLAX", "AUBNE");
			rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HB).GetCalculator<UnitCalculator>().PerUnit = 60;
			rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.LW).GetCalculator<UnitCalculator>().PerUnit = 36;

			var costing = Helper.NewCosting(Creditor);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "USLAX", "AUBNE");
			costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HB).GetCalculator<UnitCalculator>().PerUnit = 100;
			costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.LW).GetCalculator<UnitCalculator>().PerUnit = 200;

			var consol = Helper.CreateBuyersConsolConsol("USLAX", "AUBNE", Creditor);
			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee);
			var coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "AAA",
					JR_OSCostAmt = 300,
				}
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			var expectedErrors = new[]
			{
				@"Error Autorating has encountered an error:
The chargeable amount for all shipment in this Buyers Consol could not be calculated.
Please check the chargeable entered on each shipment has been specified correctly."
			};

			AutorateAndAssert(expected, leadShipment, Consignee, expectedErrors: expectedErrors);
		}

		public void TestAutorateLeadShipment_MultiplePacklinesWithDifferentCommoditiesInSameContainer_ShouldProperlyIdentifyContainers()
		{
			// Consol
			//		Container1	20GP
			//		Container2  20GP
			//		Container3	20GP	FISH
			//
			//	Shipment1 (BCN lead)
			//		Container1	10 Packs of GAME
			//		Container2	20 Packs of EFRT
			//	Shipment2 (BCN related)
			//		Container1	10 Packs of GAME
			//		Container2	20 Packs of GLUE
			//	Shipment3 (FCL)
			//		Container1	10 Packs of FELT
			//		Container2	20 Packs of DAIR
			//		Container3  30 Packs of FISH
			//
			//	Expectation:
			//		1x20GP GAME		<-	Both BCN shipments have packlines with GAME commodity in container 1
			//		1x20GP			<-  BCN shipments have packlines with different commodities in container 2

			var consol = Helper.CreateBuyersConsolConsol("UAIEV", "AUSYD", Creditor);
			var shipment1 = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee);
			var shipment2 = Helper.CreateColoadShipment(consol, shipment1, Consignor, Consignee);
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_PackingMode = ContainerModes.FCL;

			consol.AddContainer(commodity: ZString.Empty, packLines: new[]
			{
				shipment1.AddPackLine(commodity: "GAME"),	// BCN Lead
				shipment2.AddPackLine(commodity: "GAME"),	// BCN CoLoad
				shipment3.AddPackLine(commodity: "FELT")
			});
			consol.AddContainer(commodity: ZString.Empty, packLines: new[]
			{
				shipment1.AddPackLine(commodity: "EFRT"),	// BCN Lead
				shipment2.AddPackLine(commodity: "GLUE"),	// BCN CoLoad
				shipment3.AddPackLine(commodity: "DAIR")
			});
			consol.AddContainer(commodity: "FISH", packLines: new[]
			{
				shipment3.AddPackLine(commodity: "FISH")
			});

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "UAIEV", "AUSYD", commodity: "GAME");
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 1000;
			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "UAIEV", "AUSYD", commodity: "GEN");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 2000;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1000m,
					RevenueCalculationDescription = "1 Container(s) @ USD 1000.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2000m,
					RevenueCalculationDescription = "1 Container(s) @ USD 2000.00/Container"
				}
			};

			var message = "Should autorate 1x20GP GAME container and 1x20GP container";
			AutorateAndAssert(message, expected, shipment1, Consignee, autorateCosts: false);
		}

		public void TestAutoRateLeadShipment_MultipleSubShipmentsWithChargeable_ShouldNotUpdateChargeableOnLeadShipment()
		{
			var consol = Helper.CreateBuyersConsolConsol("UAIEV", "AUSYD", Creditor, transportMode: "AIR");
			var shipment1 = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, transportMode: "AIR");
			shipment1.JS_ActualWeight = 100;
			shipment1.JS_UnitOfWeight = "KG";
			var shipment2 = Helper.CreateColoadShipment(consol, shipment1, Consignor, Consignee);
			shipment2.JS_ActualWeight = 200;
			shipment2.JS_UnitOfWeight = "KG";

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "UAIEV", "AUSYD");
			rateEntry1.RateLines.RemoveAndDeleteAll();
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG).GetCalculator<UnitCalculator>().PerUnit = 2;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 600.00m,
					RevenueCalculationDescription = "FRT: 300 Kilogram(s) @ UAH 2.00/KG"
				}
			};

			AutorateAndAssert("", expected, shipment1, Consignee, autorateCosts: false);
			AssertEquals(
				"This is the original amount on the lead shipment and it should not be updated with the total amount from all shipments", 
				100m, 
				shipment1.JS_ActualChargeable
			);
		}

		#endregion

		#region Invoicing Styles

		public void TestInvoicingStyle_APP()
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Apportion;

			var chargeCode = Helper.ChargeCodes.New("AAA", "Junk", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "USLAX", "AUBNE");
			rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HB).GetCalculator<UnitCalculator>().PerUnit = 60;
			rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.LW).GetCalculator<UnitCalculator>().PerUnit = 36;

			var costing = Helper.NewCosting(Creditor);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "USLAX", "AUBNE");
			costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HB).GetCalculator<UnitCalculator>().PerUnit = 100;
			costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.LW).GetCalculator<UnitCalculator>().PerUnit = 200;

			var consol = Helper.CreateBuyersConsolConsol("USLAX", "AUBNE", Creditor);
			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 200);
			var coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 100);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "AAA",
					JR_OSSellAmt = 64m,
					JR_OSCostAmt = 300,
				}
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AutorateAndAssert(expected, leadShipment, Consignee);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "AAA",
					JR_OSSellAmt = 32m,
					JR_OSCostAmt = 300,
				}
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AutorateAndAssert(expected, coloadShipment, Consignee);

			Factory.Save();

			var chargeAmounts = ((Job)leadShipment.Job).Charges.Cast<JobCharge>().Select(x => x.JR_OSSellAmt).ToList();
			AssertEquals(1, chargeAmounts.Count);
			AssertEquals(64m, chargeAmounts[0]);

			chargeAmounts = ((Job)coloadShipment.Job).Charges.Cast<JobCharge>().Select(x => x.JR_OSSellAmt).ToList();
			AssertEquals(1, chargeAmounts.Count);
			AssertEquals(32m, chargeAmounts[0]);
		}

		public void TestInvoicingStyle_MAS()
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var chargeCode = Helper.ChargeCodes.New("AAA", "AAA", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "US", "AU");
			rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HB).GetCalculator<UnitCalculator>().PerUnit = 60;
			rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.LW).GetCalculator<UnitCalculator>().PerUnit = 36;

			var costing = Helper.NewCosting(Creditor);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "US", "AU");
			costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HB).GetCalculator<UnitCalculator>().PerUnit = 100;
			costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.LW).GetCalculator<UnitCalculator>().PerUnit = 200;

			#region Create Consol and Shipments and Jobs

			var consol = Helper.CreateBuyersConsolConsol("USLAX", "AUBNE", Creditor);
			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 200);
			var coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 200);

			#endregion

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "AAA",
					JR_OSSellAmt = 96,
					JR_OSCostAmt = 300,
				}
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AutorateAndAssert(expected, leadShipment, Consignee);
		}

		public void TestInvoicingStyle_MAB()
		{
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.ApportionInvoiceMaster;

			var chargeCode = Helper.ChargeCodes.New("AAA", "AAA", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			var clientRate = Helper.NewClientRate(Consignee);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "USLAX", "AUBNE");
			rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HB).GetCalculator<UnitCalculator>().PerUnit = 60;
			rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.LW).GetCalculator<UnitCalculator>().PerUnit = 36;

			var costing = Helper.NewCosting(Creditor);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "USLAX", "AUBNE");
			costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.HB).GetCalculator<UnitCalculator>().PerUnit = 100;
			costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.LW).GetCalculator<UnitCalculator>().PerUnit = 200;

			#region Create Consol and shipments

			var consol = Helper.CreateBuyersConsolConsol("USLAX", "AUBNE", Creditor);
			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 200);
			var coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 100);

			#endregion

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "AAA",
					JR_OSSellAmt = 64m,
					JR_OSCostAmt = 300,
				}
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AutorateAndAssert(expected, leadShipment, Consignee);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "AAA",
					JR_OSSellAmt = 32m,
					JR_OSCostAmt = 300,
				}
			};

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AutorateAndAssert(expected, coloadShipment, Consignee);

			Factory.Save();

			((IAutoRatingAccountingUtils)leadShipment.Job).ReloadChargesFromAdditionalJobs();

			var sellAmounts = ((Job)leadShipment.Job).Charges.Cast<JobCharge>().Select(x => x.JR_OSSellAmt).ToList();
			AssertEquals(2, sellAmounts.Count);
			AssertContainsExactElementsInAnyOrder("Sub Shipment charge has to be shown on Lead Shipment Job when Buyer's consol style is MAB", new ZDecimal[] { 64m, 32m }, sellAmounts);

			var costAmounts = ((Job)leadShipment.Job).Charges.Cast<JobCharge>().Select(x => x.JR_OSCostAmt).ToList();
			AssertEquals(2, costAmounts.Count);
			AssertContainsExactElementsInAnyOrder("Sub Shipment charge has to be shown on Lead Shipment Job when Buyer's consol style is MAB", new ZDecimal[] { 300m, 300m }, costAmounts);
		}

		#endregion

		#region TestAdapterProvider Quick Calculate

		public void TestAdapterProvider_GetAdapterForQuickCalculate()
		{
			#region Setup

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.BuyersConsol;

			var leadShipment = consol.Shipments.AddNew();
			leadShipment.JS_TransportMode = TransportModes.Sea;
			leadShipment.JS_PackingMode = ContainerModes.BuyersConsol;
			leadShipment.JS_ShipmentType = ShipmentTypes.BuyersConsolLead;
			leadShipment.ConsigneePK = Consignee.PK;

			var subShipment = consol.Shipments.AddNew();
			subShipment.JS_TransportMode = TransportModes.Sea;
			subShipment.JS_PackingMode = ContainerModes.BuyersConsol;
			subShipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
			subShipment.ConsigneePK = Consignee.PK;

			consol.AddContainer("20GP", count: 2, containerMode: "BCN", packLines: new[]
			{
				leadShipment.AddPackLine(count: 10, packType: "PLT", weight: 100, volume: 1),
				subShipment.AddPackLine(count: 20, packType: "PLT", weight: 200, volume: 2),
				subShipment.AddPackLine(count: 5, packType: "BOX", weight: 50, volume: 0.5m)
			});

			consol.AddContainer("40GP", count: 4, containerMode: "BCN", packLines: new[]
			{
				leadShipment.AddPackLine(count: 40, packType: "PLT", weight: 400, volume: 4),
			});

			Factory.Save();

			#endregion

			using var job1 = new Job.Loader(leadShipment).TryLoadOrCreateWithMutex();
			using var job2 = new Job.Loader(subShipment).TryLoadOrCreateWithMutex();

			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Apportion;

			var leadAdapter = ((IRatingSupporter)leadShipment).AdaptersProvider.GetForQuickCalculate(null);
			var subAdapter = ((IRatingSupporter)subShipment).AdaptersProvider.GetForQuickCalculate(null);

			CombineAssertions(() =>
			{
				var leadMeasures = (RateableMeasureSet)leadAdapter.QuickMeasures;
				AssertEquals("lead shipment weight is from lead shipment only", 500m, leadMeasures.GetActual(MeasureType.Weight));
				AssertEquals("lead shipment volume is from lead shipment only", 5m, leadMeasures.GetActual(MeasureType.Volume));
				AssertEquals("lead shipment packages is from lead shipment only", 50m, leadMeasures.GetActual(MeasureType.Package));
				AssertEquals("lead shipment containers count is from lead shipment only", 6m, leadMeasures.GetActual(MeasureType.ContainerCount));

				var subMeasures = (RateableMeasureSet)subAdapter.QuickMeasures;
				AssertEquals("lead shipment weight is from lead shipment only", 250m, subMeasures.GetActual(MeasureType.Weight));
				AssertEquals("lead shipment volume is from lead shipment only", 2.5m, subMeasures.GetActual(MeasureType.Volume));
				AssertEquals("lead shipment packages is from lead shipment only", 25m, subMeasures.GetActual(MeasureType.Package));
				AssertEquals("lead shipment containers count is from lead shipment only", 2m, subMeasures.GetActual(MeasureType.ContainerCount));
			});

			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			leadAdapter = ((IRatingSupporter)leadShipment).AdaptersProvider.GetForQuickCalculate(null);
			subAdapter = ((IRatingSupporter)subShipment).AdaptersProvider.GetForQuickCalculate(null);

			CombineAssertions(() =>
			{
				var leadMeasures = (RateableMeasureSet)leadAdapter.QuickMeasures;
				AssertEquals("lead shipment weight is from lead and sub shipment", 750m, leadMeasures.GetActual(MeasureType.Weight));
				AssertEquals("lead shipment volume is from lead and sub shipment", 7.5m, leadMeasures.GetActual(MeasureType.Volume));
				AssertEquals("lead shipment packages is from lead and sub shipment", 75m, leadMeasures.GetActual(MeasureType.Package));
				AssertEquals("lead shipment containers count is from lead and sub shipment", 6m, leadMeasures.GetActual(MeasureType.ContainerCount));

				var subMeasures = (RateableMeasureSet)subAdapter.QuickMeasures;
				AssertEquals("lead shipment weight is from lead shipment only", 250m, subMeasures.GetActual(MeasureType.Weight));
				AssertEquals("lead shipment volume is from lead shipment only", 2.5m, subMeasures.GetActual(MeasureType.Volume));
				AssertEquals("lead shipment packages is from lead shipment only", 25m, subMeasures.GetActual(MeasureType.Package));
				AssertEquals("lead shipment containers count is from lead shipment only", 2m, subMeasures.GetActual(MeasureType.ContainerCount));
			});
		}

		#endregion

		#region Container Type

		public void TestAutorateWithContainerType_InvoiceTypeMAS_ByContainerCount()
		{
			var (leadShipment, _) = SetupForTestAutorateWithContainerType(QuantityUnit.CN);
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var autoratedForDescription = DescriptionHelpers.FormatWithTab("Autorated for:");
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 500,
						JR_LocalSellAmt = 600,
						CostCalculationDescription = "FRT: 1 20GP Container(s) @ AUD 500.00/Container",
						RevenueCalculationDescription = "FRT: 1 Container(s) @ AUD 600.00/Container"
					},
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalCostAmt = 100,
						JR_LocalSellAmt = 200,
						CostCalculationDescription = "DDOC: 1 Container(s) @ AUD 100.00/Container",
						RevenueCalculationDescription = "DDOC: 1 Container(s) @ AUD 200.00/Container"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 100,
						JR_LocalSellAmt = 200,
						CostCalculationDescription = @"ODOC: 1 Container(s) @ AUD 100.00/Container
" + autoratedForDescription + "Shipment S00001000",
						RevenueCalculationDescription = @"ODOC: 1 Container(s) @ AUD 200.00/Container
" + autoratedForDescription + "Shipment S00001000"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 100,
						JR_LocalSellAmt = 200,
						CostCalculationDescription = @"ODOC: 1 Container(s) @ AUD 100.00/Container
" + autoratedForDescription + "Shipment S00001001",
						RevenueCalculationDescription = @"ODOC: 1 Container(s) @ AUD 200.00/Container
" + autoratedForDescription + "Shipment S00001001"
					},
				};

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, leadShipment, Consignee);
			}
		}

		public void TestAutorateWithContainerType_MultipleColoadShipments_InvoiceTypeMAS_ByContainerCount()
		{
			var (leadShipment, _) = SetupForTestAutorateWithContainerType(QuantityUnit.CN, hasAdditionalColoadShipment: true);
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var autoratedForDescription = DescriptionHelpers.FormatWithTab("Autorated for:");
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 500,
						JR_LocalSellAmt = 600,
						CostCalculationDescription = "FRT: 1 20GP Container(s) @ AUD 500.00/Container",
						RevenueCalculationDescription = "FRT: 1 Container(s) @ AUD 600.00/Container"
					},
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalCostAmt = 100,
						JR_LocalSellAmt = 200,
						CostCalculationDescription = "DDOC: 1 Container(s) @ AUD 100.00/Container",
						RevenueCalculationDescription = "DDOC: 1 Container(s) @ AUD 200.00/Container"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 100,
						JR_LocalSellAmt = 200,
						CostCalculationDescription = @"ODOC: 1 Container(s) @ AUD 100.00/Container
" + autoratedForDescription + "Shipment S00001000",
						RevenueCalculationDescription = @"ODOC: 1 Container(s) @ AUD 200.00/Container
" + autoratedForDescription + "Shipment S00001000"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 100,
						JR_LocalSellAmt = 200,
						CostCalculationDescription = @"ODOC: 1 Container(s) @ AUD 100.00/Container
" + autoratedForDescription + "Shipment S00001001",
						RevenueCalculationDescription = @"ODOC: 1 Container(s) @ AUD 200.00/Container
" + autoratedForDescription + "Shipment S00001001"
					},
					new AssertionCharge
					{
					ChargeCode = "ODOC",
					JR_LocalCostAmt = 100,
					JR_LocalSellAmt = 200,
					CostCalculationDescription = @"ODOC: 1 Container(s) @ AUD 100.00/Container
" + autoratedForDescription + "Shipment S00001002",
					RevenueCalculationDescription = @"ODOC: 1 Container(s) @ AUD 200.00/Container
" + autoratedForDescription + "Shipment S00001002"
					}
				};

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, leadShipment, Consignee);
			}
		}

		public void TestAutorateWithContainerType_InvoiceTypeAPP_ByContainerCount() =>
			AssertAutorateWithContainerType_InvoiceTypeMABOrAPP_ByContainerCount(ConsolInvoicingStyles.Apportion);

		public void TestAutorateWithContainerType_InvoiceTypeMAB_ByContainerCount() =>
			AssertAutorateWithContainerType_InvoiceTypeMABOrAPP_ByContainerCount(ConsolInvoicingStyles.ApportionInvoiceMaster);

		void AssertAutorateWithContainerType_InvoiceTypeMABOrAPP_ByContainerCount(ZString invoicingType)
		{
			var (leadShipment, coloadShipment) = SetupForTestAutorateWithContainerType(QuantityUnit.CN);
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = invoicingType;

			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var revenueDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 0.750 M3, Buyers Consol Total: 1.000 M3)";
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 500,
						JR_LocalSellAmt = 450,  // apportioned by chargeable
						CostCalculationDescription = "FRT: 1 20GP Container(s) @ AUD 500.00/Container",
						RevenueCalculationDescription = revenueDescription
					},
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalCostAmt = 100,
						JR_LocalSellAmt = 150, // apportioned by chargeable
						CostCalculationDescription = "DDOC: 1 Container(s) @ AUD 100.00/Container",
						RevenueCalculationDescription = revenueDescription
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 100,
						JR_LocalSellAmt = 200,
						CostCalculationDescription = "ODOC: 1 Container(s) @ AUD 100.00/Container",
						RevenueCalculationDescription = "ODOC: 1 Container(s) @ AUD 200.00/Container"
					},
				};
				AutorateAndAssert(expected, leadShipment, Consignee);

				revenueDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 0.250 M3, Buyers Consol Total: 1.000 M3)";
				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 500,
						JR_LocalSellAmt = 150,  // apportioned by chargeable
						CostCalculationDescription = "FRT: 1 20GP Container(s) @ AUD 500.00/Container",
						RevenueCalculationDescription = revenueDescription
					},
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalCostAmt = 100,
						JR_LocalSellAmt = 50,  // apportioned by chargeable
						CostCalculationDescription = "DDOC: 1 Container(s) @ AUD 100.00/Container",
						RevenueCalculationDescription = revenueDescription
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 100,
						JR_LocalSellAmt = 200,
						CostCalculationDescription = "ODOC: 1 Container(s) @ AUD 100.00/Container",
						RevenueCalculationDescription = "ODOC: 1 Container(s) @ AUD 200.00/Container"
					},
				};
				AutorateAndAssert(expected, coloadShipment, Consignee);
			}
		}

		public void TestAutorateWithContainerType_InvoiceTypeMAS_ByChargeable()
		{
			var (leadShipment, _) = SetupForTestAutorateWithContainerType(QuantityUnit.KG);
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var autoratedForDescription = DescriptionHelpers.FormatWithTab("Autorated for:");
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 500000,
						JR_LocalSellAmt = 600000,
						CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 500.00/KG",
						RevenueCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 600.00/KG"
					},
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalCostAmt = 100000,
						JR_LocalSellAmt = 200000,
						CostCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 100.00/KG",
						RevenueCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 200.00/KG"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 75000,
						JR_LocalSellAmt = 150000,
						CostCalculationDescription = @"ODOC: 750 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001000",
						RevenueCalculationDescription = @"ODOC: 750 Kilogram(s) @ AUD 200.00/KG
" + autoratedForDescription + "Shipment S00001000"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 25000,
						JR_LocalSellAmt = 50000,
						CostCalculationDescription = @"ODOC: 250 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001001",
						RevenueCalculationDescription = @"ODOC: 250 Kilogram(s) @ AUD 200.00/KG
" + autoratedForDescription + "Shipment S00001001"
					},
				};

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, leadShipment, Consignee);
			}
		}

		public void TestAutorateWithContainerType_InvoiceTypeMAS_ByChargeable_SubShipmentHasJob()
		{
			var (leadShipment, subShipment) = SetupForTestAutorateWithContainerType(QuantityUnit.KG);
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			// When sub-shipment has a job, AutorateInfo can have OrderReference as criteria.JobNumber which is sub-shipment JS_UniqueConsignRef
			// While it above test case,  AutorateInfo has empty OrderReference
			using (var job = new Job.Loader(subShipment).TryLoadOrCreateWithMutex())
			{
				Factory.Save();
			}

			var autoratedForDescription = DescriptionHelpers.FormatWithTab("Autorated for:");
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 500000,
						JR_LocalSellAmt = 600000,
						CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 500.00/KG",
						RevenueCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 600.00/KG"
					},
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalCostAmt = 100000,
						JR_LocalSellAmt = 200000,
						CostCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 100.00/KG",
						RevenueCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 200.00/KG"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 75000,
						JR_LocalSellAmt = 150000,
						CostCalculationDescription = @"ODOC: 750 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001000",
						RevenueCalculationDescription = @"ODOC: 750 Kilogram(s) @ AUD 200.00/KG
" + autoratedForDescription + "Shipment S00001000"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 25000,
						JR_LocalSellAmt = 50000,
						CostCalculationDescription = @"ODOC: 250 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001001",
						RevenueCalculationDescription = @"ODOC: 250 Kilogram(s) @ AUD 200.00/KG
" + autoratedForDescription + "Shipment S00001001"
					},
				};

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, leadShipment, Consignee);
			}
		}

		public void TestAutorateWithContainerType_MultipleColoadShipments_InvoiceTypeMAS_ByChargeable()
		{
			var (leadShipment, _) = SetupForTestAutorateWithContainerType(QuantityUnit.KG, hasAdditionalColoadShipment: true);
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var autoratedForDescription = DescriptionHelpers.FormatWithTab("Autorated for:");
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 500000,
						JR_LocalSellAmt = 600000,
						CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 500.00/KG",
						RevenueCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 600.00/KG"
					},
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalCostAmt = 100000,
						JR_LocalSellAmt = 200000,
						CostCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 100.00/KG",
						RevenueCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 200.00/KG"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 75000,
						JR_LocalSellAmt = 150000,
						CostCalculationDescription = @"ODOC: 750 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001000",
						RevenueCalculationDescription = @"ODOC: 750 Kilogram(s) @ AUD 200.00/KG
" + autoratedForDescription + "Shipment S00001000"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 15000,
						JR_LocalSellAmt = 30000,
						CostCalculationDescription = @"ODOC: 150 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001001",
						RevenueCalculationDescription = @"ODOC: 150 Kilogram(s) @ AUD 200.00/KG
" + autoratedForDescription + "Shipment S00001001"
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 10000,
						JR_LocalSellAmt = 20000,
						CostCalculationDescription = @"ODOC: 100 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001002",
						RevenueCalculationDescription = @"ODOC: 100 Kilogram(s) @ AUD 200.00/KG
" + autoratedForDescription + "Shipment S00001002"
					},
				};

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AutorateAndAssert(expected, leadShipment, Consignee);
			}
		}

		public void TestAutorateWithContainerType_MultipleColoadShipments_InvoiceTypeMAS_ByChargeable_CostThenRevenue()
		{
			var (leadShipment, _) = SetupForTestAutorateWithContainerType(QuantityUnit.KG, hasAdditionalColoadShipment: true);
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = ConsolInvoicingStyles.Master;

			var autoratedForDescription = DescriptionHelpers.FormatWithTab("Autorated for:");
			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedFromAutorateCost = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 500000,
						JR_LocalSellAmt = 500000,
						CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 500.00/KG",
						RevenueCalculationDescription = ""
					},
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalCostAmt = 100000,
						JR_LocalSellAmt = 0,
						CostCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 100.00/KG",
						RevenueCalculationDescription = ""
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 75000,
						JR_LocalSellAmt = 0,
						CostCalculationDescription = @"ODOC: 750 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001000",
						RevenueCalculationDescription = ""
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 15000,
						JR_LocalSellAmt = 0,
						CostCalculationDescription = @"ODOC: 150 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001001",
						RevenueCalculationDescription = ""
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 10000,
						JR_LocalSellAmt = 0,
						CostCalculationDescription = @"ODOC: 100 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001002",
						RevenueCalculationDescription = ""
					},
				};

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var gstRegisteredTemp = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
				try
				{
					GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
					AutorateAndAssert(expectedFromAutorateCost, leadShipment, Consignee, autorateRevenue: false);

					Factory.Save();

					var expectedFromAutorateRevenue = new[]
					{
						new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_LocalCostAmt = 500000,
							JR_LocalSellAmt = 600000,
							CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 500.00/KG",
							RevenueCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 600.00/KG"
						},
						new AssertionCharge
						{
							ChargeCode = "DDOC",
							JR_LocalCostAmt = 100000,
							JR_LocalSellAmt = 200000,
							CostCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 100.00/KG",
							RevenueCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 200.00/KG"
						},
						new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_LocalCostAmt = 75000,
							JR_LocalSellAmt = 150000,
							CostCalculationDescription = @"ODOC: 750 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001000",
							RevenueCalculationDescription = @"ODOC: 750 Kilogram(s) @ AUD 200.00/KG
" + autoratedForDescription + "Shipment S00001000"
						},
						new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_LocalCostAmt = 15000,
							JR_LocalSellAmt = 30000,
							CostCalculationDescription = @"ODOC: 150 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001001",
							RevenueCalculationDescription = @"ODOC: 150 Kilogram(s) @ AUD 200.00/KG
" + autoratedForDescription + "Shipment S00001001"
						},
						new AssertionCharge
						{
							ChargeCode = "ODOC",
							JR_LocalCostAmt = 10000,
							JR_LocalSellAmt = 20000,
							CostCalculationDescription = @"ODOC: 100 Kilogram(s) @ AUD 100.00/KG
" + autoratedForDescription + "Shipment S00001002",
							RevenueCalculationDescription = @"ODOC: 100 Kilogram(s) @ AUD 200.00/KG
" + autoratedForDescription + "Shipment S00001002"
						},
					};
					using (var job = new Job.Loader(leadShipment).Load())
					{
						AutorateAndAssert(expectedFromAutorateRevenue, leadShipment, Consignee, autorateCosts: false, job: job);
					}
				}
				finally
				{
					GlbCompany.CurrentCompany.GC_IsGSTRegistered = gstRegisteredTemp;
				}
			}
		}

		public void TestAutorateWithContainerType_InvoiceTypeAPP_ByChargeable() =>
			AssertAutorateWithContainerType_InvoiceTypeMABOrAPP_ByChargeable(ConsolInvoicingStyles.Apportion);

		public void TestAutorateWithContainerType_InvoiceTypeMAB_ByChargeable() =>
			AssertAutorateWithContainerType_InvoiceTypeMABOrAPP_ByChargeable(ConsolInvoicingStyles.ApportionInvoiceMaster);

		void AssertAutorateWithContainerType_InvoiceTypeMABOrAPP_ByChargeable(ZString invoicingType)
		{
			var (leadShipment, coloadShipment) = SetupForTestAutorateWithContainerType(QuantityUnit.KG);
			Consignee.CompanyData.OB_ARBuyersConsolInvoicingStyle = invoicingType;

			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var revenueDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 0.750 M3, Buyers Consol Total: 1.000 M3)";
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 500000,
						JR_LocalSellAmt = 450000, // apportioned by chargeable
						CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 500.00/KG",
						RevenueCalculationDescription = revenueDescription
					},
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalCostAmt = 100000,
						JR_LocalSellAmt = 150000, // apportioned by chargeable
						CostCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 100.00/KG",
						RevenueCalculationDescription = revenueDescription
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 75000,
						JR_LocalSellAmt = 150000,
						CostCalculationDescription = "ODOC: 750 Kilogram(s) @ AUD 100.00/KG",
						RevenueCalculationDescription = "ODOC: 750 Kilogram(s) @ AUD 200.00/KG"
					},
				};
				AutorateAndAssert(expected, leadShipment, Consignee);

				revenueDescription = "This charge was autorated through a Buyers Consol and apportioned by chargeable amount (This shipment: 0.250 M3, Buyers Consol Total: 1.000 M3)";
				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_LocalCostAmt = 500000,
						JR_LocalSellAmt = 150000 /*apportioned by chargeable*/,
						CostCalculationDescription = "FRT: 1000 Kilogram(s) @ AUD 500.00/KG",
						RevenueCalculationDescription = revenueDescription
					},
					new AssertionCharge
					{
						ChargeCode = "DDOC",
						JR_LocalCostAmt = 100000,
						JR_LocalSellAmt = 50000 /*apportioned by chargeable*/,
						CostCalculationDescription = "DDOC: 1000 Kilogram(s) @ AUD 100.00/KG",
						RevenueCalculationDescription = revenueDescription
					},
					new AssertionCharge
					{
						ChargeCode = "ODOC",
						JR_LocalCostAmt = 25000,
						JR_LocalSellAmt = 50000,
						CostCalculationDescription = "ODOC: 250 Kilogram(s) @ AUD 100.00/KG",
						RevenueCalculationDescription = "ODOC: 250 Kilogram(s) @ AUD 200.00/KG"
					},
				};
				AutorateAndAssert(expected, coloadShipment, Consignee);
			}
		}

		#endregion

		#region Lowest Bill unit

		public void TestLowestBill_InvoicingStyleMAS()
			=> TestLowestBill
			(
				ConsolInvoicingStyles.Master,
				expectedLeadShipmentCharges: new[] { new AssertionCharge { JR_LocalSellAmt = 10, RevenueCalculationDescription = "DDOC: 2 Lowest Bill(s) @ AUD 5.00/Lowest Bill" } },
				expectedColoadShipment1Charges: Enumerable.Empty<AssertionCharge>(),
				expectedColoadShipment2Charges: Enumerable.Empty<AssertionCharge>()
			);

		public void TestLowestBill_InvoicingStyleAPP()
		{
			var expectedCharges = new[] { new AssertionCharge { JR_LocalSellAmt = 5, RevenueCalculationDescription = "DDOC: 1 Lowest Bill(s) @ AUD 5.00/Lowest Bill" } };
			TestLowestBill
			(
				ConsolInvoicingStyles.Apportion,
				expectedLeadShipmentCharges: Enumerable.Empty<AssertionCharge>(),
				expectedColoadShipment1Charges: expectedCharges,
				expectedColoadShipment2Charges: expectedCharges
			);
		}

		public void TestLowestBill_InvoicingStyleMAB()
		{
			var expectedCharges = new[] { new AssertionCharge { JR_LocalSellAmt = 5, RevenueCalculationDescription = "DDOC: 1 Lowest Bill(s) @ AUD 5.00/Lowest Bill" } };
			TestLowestBill
			(
				ConsolInvoicingStyles.ApportionInvoiceMaster,
				expectedLeadShipmentCharges: Enumerable.Empty<AssertionCharge>(),
				expectedColoadShipment1Charges: expectedCharges,
				expectedColoadShipment2Charges: expectedCharges
			);
		}

		void TestLowestBill(string invoicingStyle, IEnumerable<AssertionCharge> expectedLeadShipmentCharges, IEnumerable<AssertionCharge> expectedColoadShipment1Charges, IEnumerable<AssertionCharge> expectedColoadShipment2Charges)
		{
			Consignor.CompanyData.OB_ARBuyersConsolInvoicingStyle = invoicingStyle;

			var clientRate = Helper.NewClientRate(Consignor);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "AU", "US");
			var line = entry.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.LW, CurrencyCodes.Australia);
			line.TL_UnitFactor = UnitFactorList.Codes.BCN;
			line.GetCalculator<UnitCalculator>().PerUnit = 5;

			var consol = Helper.CreateBuyersConsolConsol("AUSYD", "USBOS", Creditor, prepaidCollect: PaymentType.Prepaid);
			var container = consol.Containers.AddNew();
			container.JC_RC = GP20.PK;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 1000, 65);
			leadShipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			var packline = leadShipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;
			var coloadShipment1 = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee);
			coloadShipment1.JS_INCO = IncoTerms.DeliveredAtPlace;
			var coloadShipment2 = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee);
			coloadShipment2.JS_INCO = IncoTerms.DeliveredAtPlace;

			Factory.Save();

			AutorateAndAssert(expectedLeadShipmentCharges, leadShipment, Consignor);

			if (expectedColoadShipment1Charges.Any() || expectedColoadShipment2Charges.Any())
			{
				AutorateAndAssert(expectedColoadShipment1Charges, coloadShipment1, Consignor);
				AutorateAndAssert(expectedColoadShipment2Charges, coloadShipment2, Consignor);
			}
		}

		#endregion

		#region Implementation

		(ForwardingShipment, ForwardingShipment) SetupForTestAutorateWithContainerType(string unit, bool hasAdditionalColoadShipment = false)
		{
			#region Costings

			var cost = Helper.NewCosting(Creditor);

			var costFCLEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.BCN, "USLAX", "AUSYD");
			costFCLEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costFCLEntry.TI_RC = GP20.PK;
			costFCLEntry.RateLines.RemoveAndDeleteAll();
			costFCLEntry.AddRateLine("FRT", UnitCalculator.Code, unit).GetCalculator<UnitCalculator>().PerUnit = 500;

			var costORGEntry = cost.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.BCN, "USLAX", "AUSYD");
			costORGEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costORGEntry.RateLines.RemoveAndDeleteAll();
			costORGEntry.AddRateLine("ODOC", UnitCalculator.Code, unit).GetCalculator<UnitCalculator>().PerUnit = 100;

			var costDSTEntry = cost.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.BCN, "USLAX", "AUSYD");
			costDSTEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			costDSTEntry.RateLines.RemoveAndDeleteAll();
			costDSTEntry.AddRateLine("DDOC", UnitCalculator.Code, unit).GetCalculator<UnitCalculator>().PerUnit = 100;

			#endregion

			#region Client Rates

			var clientRate = Helper.NewClientRate(Consignee);

			var clientRateFCLEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.BCN, "USLAX", "AUSYD");
			clientRateFCLEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			clientRateFCLEntry.RateLines.RemoveAndDeleteAll();
			clientRateFCLEntry.AddRateLine("FRT", UnitCalculator.Code, unit).GetCalculator<UnitCalculator>().PerUnit = 600;

			var clientRateORGEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.BCN, "USLAX", "AUSYD");
			clientRateORGEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			clientRateORGEntry.RateLines.RemoveAndDeleteAll();
			clientRateORGEntry.AddRateLine("ODOC", UnitCalculator.Code, unit).GetCalculator<UnitCalculator>().PerUnit = 200;

			var clientRateDSTEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.BCN, "USLAX", "AUSYD");
			clientRateDSTEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			clientRateDSTEntry.RateLines.RemoveAndDeleteAll();
			clientRateDSTEntry.AddRateLine("DDOC", UnitCalculator.Code, unit).GetCalculator<UnitCalculator>().PerUnit = 200;

			#endregion

			#region Consol and Shipments

			var consol = Helper.CreateBuyersConsolConsol("USLAX", "AUSYD", Creditor, TransportModes.Sea, PaymentType.Collect);
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_ContainerNum = "ABC";
			container.JC_ContainerMode = "BCN";
			container.JC_RC = GP20.PK;

			var leadShipment = Helper.CreateBuyerConsolLeadShipment(consol, Consignor, Consignee, 750, .75m);
			leadShipment.JS_TransportMode = TransportModes.Sea;
			leadShipment.JS_INCO = ZString.Empty;

			var leadPackLine = leadShipment.OuterPackLines[0];
			leadPackLine.JL_JC = container.PK;
			leadPackLine.JL_ActualWeight = 750;
			leadPackLine.JL_ActualVolume = .75m;

			ForwardingShipment coloadShipment = null;

			if (hasAdditionalColoadShipment)
			{
				coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 150, .15m);
				coloadShipment.JS_INCO = ZString.Empty;

				var coloadPackLine1 = coloadShipment.OuterPackLines[0];
				coloadPackLine1.JL_JC = container.PK;
				coloadPackLine1.JL_ActualWeight = 150;
				coloadPackLine1.JL_ActualVolume = .15m;

				var coloadShipment2 = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 100, .1m);
				coloadShipment2.JS_INCO = ZString.Empty;

				var coloadPackLine2 = coloadShipment2.OuterPackLines[0];
				coloadPackLine2.JL_JC = container.PK;
				coloadPackLine2.JL_ActualWeight = 100;
				coloadPackLine2.JL_ActualVolume = .1m;
			}
			else
			{
				coloadShipment = Helper.CreateColoadShipment(consol, leadShipment, Consignor, Consignee, 250, .25m);
				coloadShipment.JS_INCO = ZString.Empty;

				var coloadPackLine = coloadShipment.OuterPackLines[0];
				coloadPackLine.JL_JC = container.PK;
				coloadPackLine.JL_ActualWeight = 250;
				coloadPackLine.JL_ActualVolume = .25m;
			}

			#endregion

			Factory.Save();
			return (leadShipment, coloadShipment);
		}

		OrgHeader Creditor => creditor ?? (creditor = Helper.CreateCreditor());
		OrgHeader creditor;

		protected override void SetUp()
		{
			DataRegistryRating.Instance.RatesServiceSubscription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled());
			base.SetUp();
		}

		#endregion
	}
}
