using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using CustomsCommon = Enterprise.Customs.Common;

namespace Enterprise.RatingTests.Testing.GUI
{
	internal class CustomsIntegrationTests : BaseRatingIntegrationTest
	{
		#region Test Standalone and Shipment Brokerage

		#region Shipment Brokerage

		public void TestShipmentBrokerage_AIR()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 100m, currency: "AUD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CAI, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 150m, currency: "AUD");

			Factory.Save();

			var shipment = CreateForwardingShipment(Factory, TransportModes.Air, RateMode.LSE, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000m);
			CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
			var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Air, RateMode.LSE, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, NewClient.PK, null, "AUSYD", "USLAX", shipment.PK, (500m, Constants.Customs.CusEntryFeeTypes.DutyAmount));
			declaration.JE_JS = shipment.PK;

			AssertShipmentBrokerage
			(
				shipment,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 100m, RevenueCalculationDescription = "BRKCHG: Base Rate AUD 100.00" },
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 500m, },
				},
				"WHEN registry is enable/disabled THEN should use only use forwarding rates (not customs rates)"
			);
		}

		public void TestShipmentBrokerage_FCL()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "BRKCHG", 100m, currency: "AUD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CFC, RateMode.SEA, "AUSYD", "USLAX", "BRKCHG", 150m, currency: "AUD");

			Factory.Save();

			var shipment = CreateForwardingShipment(Factory, TransportModes.Sea, ContainerModes.FCL, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000m);
			CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", TransportProvider1, shipment);
			var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Sea, ContainerModes.FCL, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, NewClient.PK, null, "AUSYD", "USLAX", shipment.PK, (500m, Constants.Customs.CusEntryFeeTypes.DutyAmount));
			declaration.JE_JS = shipment.PK;

			AssertShipmentBrokerage
			(
				shipment,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 100m, RevenueCalculationDescription = "BRKCHG: Base Rate AUD 100.00" },
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 500m, },
				},
				"WHEN registry is enable/disabled THEN should use only use forwarding rates (not customs rates)"
			);
		}

		public void TestShipmentBrokerage_LCL_ORG_DST()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX", "BRKCHG", 100m, currency: "AUD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CLC, RateMode.LCL, "AUSYD", "USLAX", "BRKCHG", 150m, currency: "AUD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.SEA, "AUSYD", "USLAX", "BRKCHG", 200m, currency: "AUD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.COR, RateMode.SEA, "AUSYD", "USLAX", "BRKCHG", 250m, currency: "AUD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.SEA, "AUSYD", "USLAX", "BRKCHG", 300m, currency: "AUD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CDS, RateMode.SEA, "AUSYD", "USLAX", "BRKCHG", 350m, currency: "AUD");

			Factory.Save();

			var shipment = CreateForwardingShipment(Factory, TransportModes.Sea, ContainerModes.LCL, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000m);
			CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", TransportProvider1, shipment);
			CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Sea, ContainerModes.LCL, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, NewClient.PK, null, "AUSYD", "USLAX", shipment.PK, (500m, Constants.Customs.CusEntryFeeTypes.DutyAmount));

			AssertShipmentBrokerage
			(
				shipment,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 100m, RevenueCalculationDescription = "BRKCHG: Base Rate AUD 100.00" },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 200m, RevenueCalculationDescription = "BRKCHG: Base Rate AUD 200.00" },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 300m, RevenueCalculationDescription = "BRKCHG: Base Rate AUD 300.00" },
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 500m, },
				},
				"WHEN registry is enable/disabled THEN should use only use forwarding rates (not customs rates)"
			);
		}

		void AssertShipmentBrokerage(ForwardingShipment shipment, OrgHeader localClient, IEnumerable<AssertionCharge> expectedCharges, string assertMessage = default)
		{
			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutorateAndAssert
				(
					assertMessage,
					expected: expectedCharges,
					shipment,
					localClient
				);
			}

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert
				(
					assertMessage,
					expected: expectedCharges,
					shipment,
					localClient
				);
			}
		}

		#endregion

		#region Test StandAlone

		public void TestStandAloneBrokerage_AIR()
		{
			var ratesPrioritiesCollection = new RatesPrioritiesCollection();
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.CUS);
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL);
			RatingDataRegistry.Instance.ImportCollectPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ratesPrioritiesCollection);
			Factory.Save();

			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 300m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CAI, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 350m);
			Factory.Save();

			var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Air, ContainerModes.LCL, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, NewClient.PK, null, "AUSYD", "USLAX", null, (50m, Constants.Customs.CusEntryFeeTypes.DutyAmount));

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: false,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 300m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is disabled THEN should use forwarding rates"
			);

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 350m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is enabled THEN should use customs rates"
			);
		}

		public void TestStandAloneBrokerage_FCL()
		{
			var ratesPrioritiesCollection = new RatesPrioritiesCollection();
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.CUS);
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL);
			RatingDataRegistry.Instance.ImportCollectPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ratesPrioritiesCollection);
			Factory.Save();

			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "BRKCHG", 300m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CFC, RateMode.SEA, "AUSYD", "USLAX", "BRKCHG", 350m);
			Factory.Save();

			var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Sea, ContainerModes.FCL, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, NewClient.PK, null, "AUSYD", "USLAX", null, (50m, Constants.Customs.CusEntryFeeTypes.DutyAmount));
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_FCL_LCL_AIR = ContainerModes.FCL;

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: false,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 300m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is disabled THEN should use forwarding rates"
			);

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 350m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is enabled THEN should use customs rates"
			);
		}

		public void TestStandAloneBrokerage_LCL_ORG_DST()
		{
			var ratesPrioritiesCollection = new RatesPrioritiesCollection();
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.CUS);
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL);
			RatingDataRegistry.Instance.ImportCollectPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ratesPrioritiesCollection);
			Factory.Save();

			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "AUSYD", "USLAX", "BRKCHG", 100m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CDS, RateMode.ALL, "AUSYD", "USLAX", "BRKCHG", 150m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "BRKCHG", 200m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.COR, RateMode.ALL, "AUSYD", "USLAX", "BRKCHG", 250m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX", "BRKCHG", 300m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CLC, RateMode.LCL, "AUSYD", "USLAX", "BRKCHG", 350m);
			Factory.Save();

			var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Sea, ContainerModes.LCL, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, NewClient.PK, null, "AUSYD", "USLAX", null, (50m, Constants.Customs.CusEntryFeeTypes.DutyAmount));

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: false,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 100m },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 200m },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 300m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is disabled THEN should use forwarding rates"
			);

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 150m },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 250m },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 350m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is enabled THEN should use customs rates"
			);
		}

		public void TestStandAloneBrokerage_OriginCharges()
		{
			Env.Registry.Rating.SetBrokerageRatedCodes("OBR,OBO,BRK,ORG,COR");

			var costing = Helper.NewCosting(NewClient);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "USLAX", "AUSYD", "ODOC", 100m);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.COR, RateMode.ALL, "USLAX", "AUSYD", "ODOC", 150m);
			Factory.Save();

			var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Sea, ContainerModes.LCL, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, NewClient.PK, null, "USLAX", "AUSYD", null, (50m, Constants.Customs.CusEntryFeeTypes.DutyAmount));
			declaration.JE_ShipmentIncoTerm = IncoTerms.ExWorks;
			declaration.JE_OH_ShippingLine = NewClient.PK;

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: false,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 100m },
				},
				autorateRevenue: false,
				assertMessage: "WHEN registry is disabled THEN should use forwarding rates"
			);

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 150m },
				},
				autorateRevenue: false,
				assertMessage: "WHEN registry is enabled THEN should use customs rates"
			);
		}

		void AssertStandAloneBrokerage(bool autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup, BaseJobDeclaration declaration, OrgHeader localClient, IEnumerable<AssertionCharge> expectedCharges, bool autorateRevenue = true, bool autorateCost = true, string assertMessage = default)
		{
			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup))
			{
				AutorateAndAssert
				(
					assertMessage,
					expected: expectedCharges,
					declaration,
					localClient,
					autorateRevenue: autorateRevenue,
					autorateCosts: autorateCost
				);
			}
		}

		public void TestAutoRatingStandAloneCustomsDeclaration_GivenRegistryIsEnabledThenShouldMatchBBK_BLK_ROR_BCNRates()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX", "BRKCHG", 100m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.BBK, "AUSYD", "USLAX", "BRKCHG", 200m);

			AssertAutoRatingStandAloneCustomsDeclaration(enableRegistry: false, expectedCharges: new[] { new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 100m } });
			AssertAutoRatingStandAloneCustomsDeclaration(enableRegistry: true, expectedCharges: new[] { new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 200m } });

			void AssertAutoRatingStandAloneCustomsDeclaration(bool enableRegistry, IEnumerable<AssertionCharge> expectedCharges)
			{
				using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableRegistry))
				{
					var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Sea, ContainerModes.BreakBulk, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, NewClient.PK, null, "AUSYD", "USLAX", null, (0, Constants.Customs.CusEntryFeeTypes.DutyAmount));
					declaration.JE_ShipmentIncoTerm = IncoTerms.ExWorks;
					declaration.JE_OH_ShippingLine = NewClient.PK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					Factory.Save();

					AssertStandAloneBrokerage(autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: false, declaration, NewClient, expectedCharges);
				}
			}
		}

		#endregion

		#region Sell Rate Priorities

		public void TestSellRatePriorities_ShipmentBrokerage()
		{
			var ratesPrioritiesCollection = new RatesPrioritiesCollection();
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL);
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.CNE, RatingJobTypes.ALL);
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.CNE, RatingJobTypes.CUS);
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.CUS);
			RatingDataRegistry.Instance.ImportCollectPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ratesPrioritiesCollection);

			var clientRate1 = Helper.NewClientRate(NewClient);
			clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 300m);
			clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CAI, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 350m);

			var clientRate2 = Helper.NewClientRate(Consignee);
			clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 400m);
			clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CAI, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 450m);
			Factory.Save();

			var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Air, ContainerModes.LCL, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, supplierPK: NewClient.PK, importerPK: Consignee.PK, "AUSYD", "USLAX", null, (50m, Constants.Customs.CusEntryFeeTypes.DutyAmount));

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: false,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 300m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is disabled THEN should use forwarding rates"
			);

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 450m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is enabled THEN should use customs rates and customs sell-rate-priorities"
			);
		}

		public void TestSellRatePriorities_StandAloneBrokerage()
		{
			var ratesPrioritiesCollection = new RatesPrioritiesCollection();
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL);
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.CNE, RatingJobTypes.ALL);
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.CNE, RatingJobTypes.CUS);
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.CUS);
			RatingDataRegistry.Instance.ImportCollectPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ratesPrioritiesCollection);

			var clientRate1 = Helper.NewClientRate(NewClient);
			clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 300m);
			clientRate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CAI, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 350m);

			var clientRate2 = Helper.NewClientRate(Consignee);
			clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 400m);
			clientRate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CAI, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 450m);
			Factory.Save();

			var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Air, ContainerModes.LCL, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, supplierPK: NewClient.PK, importerPK: Consignee.PK, "AUSYD", "USLAX", null, (50m, Constants.Customs.CusEntryFeeTypes.DutyAmount));

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: false,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 300m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is disabled THEN should use forwarding rates"
			);

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 450m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is enabled THEN should use customs rates and customs sell-rate-priorities"
			);
		}

		#endregion

		#region RatingHeaders Costing, Tariff, etc

		public void TestStandAloneBrokerage_AIR_Costing()
		{
			var costing = Helper.NewCosting(NewClient);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 300m);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CAI, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 350m);
			Factory.Save();

			var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Air, ContainerModes.LCL, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, NewClient.PK, null, "AUSYD", "USLAX", null, (50m, Constants.Customs.CusEntryFeeTypes.DutyAmount));
			declaration.JE_OH_ShippingLine = NewClient.PK;

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: false,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 300m }
				},
				autorateRevenue: false,
				assertMessage: "WHEN registry is disabled THEN should use forwarding rates"
			);

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true,
				declaration,
				NewClient,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 350m }
				},
				autorateRevenue: false,
				assertMessage: "WHEN registry is enabled THEN should use customs rates"
			);
		}

		public void TestStandAloneBrokerage_AIR_CompanyTariff()
		{
			var ratesPrioritiesCollection = new RatesPrioritiesCollection();
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.CUS);
			ratesPrioritiesCollection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL);
			RatingDataRegistry.Instance.ImportCollectPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ratesPrioritiesCollection);
			Factory.Save();

			var orgHeader = Helper.NewOrgHeader(1);
			Factory.Save();

			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 300m);
			companyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CAI, RateMode.LSE, "AUSYD", "USLAX", "BRKCHG", 350m);
			companyTariff.Factory.Save();

			var declaration = CreateJobDeclaration(CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import, TransportModes.Air, ContainerModes.LCL, CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code, PaymentPartyCodeDescriptionList.Codes.Broker, orgHeader.PK, null, "AUSYD", "USLAX", null, (50m, Constants.Customs.CusEntryFeeTypes.DutyAmount));

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: false,
				declaration,
				orgHeader,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 300m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is disabled THEN should use forwarding rates"
			);

			AssertStandAloneBrokerage
			(
				autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true,
				declaration,
				orgHeader,
				expectedCharges: new[]
				{
					new AssertionCharge { ChargeCode = "CUSDSB", JR_OSSellAmt = 50m, JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.", RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message." },
					new AssertionCharge { ChargeCode = "BRKCHG", JR_OSSellAmt = 350m }
				},
				autorateCost: false,
				assertMessage: "WHEN registry is enabled THEN should use customs rates"
			);
		}

		#endregion

		#endregion

		#region TestCustomsCharges

		public void TestCustomsChargeHasCorrectValueAfterRatingWithAdditionalJobs()
		{
			CreateRefCusRateCode(Constants.Customs.CusEntryFeeTypes.DutyAmount, Constants.Customs.CusEntryFeeTypes.DutyAmount);

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			#region Create Rate

			var rate = Helper.NewClientRate(localClient);
			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.New("TBK1", "Transport Booking 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var rateEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");

			var line2 = rateEntry2.AddRateLine("TBK1", FlatCalculator.Code);
			line2.GetCalculator<FlatCalculator>().BaseRate = 120;

			#endregion

			#region Create shipment

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "CLT";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			#endregion

			#region Create booking consolidation with booking

			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = "JS";

			var booking = bookingConsolidation.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = "EFPU";
			booking.KM_RatingFreightMode = "LSE";
			booking.KM_JobID = "TM00000001";

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsLooseRateable = true;
			fromInstruction.Address.OrganisationPK = localClient.PK;

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsLooseRateable = true;
			toInstruction.Address.OrganisationPK = consignee.PK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var box = CreatePackage(booking, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var pallet = CreatePackage(booking, 23, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var carton = CreatePackage(booking, 5, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction, box, pallet, carton);
			CreateInstructionPkgDivots(toInstruction, box, pallet, carton);

			#endregion

			#region Create declaration

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.LCL;
			declaration.JE_MessageType = CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Refund;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_ShipmentIncoTerm = "EXW";
			declaration.JE_PaymentMethod = "BRK";
			declaration.JE_JS = shipment.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "AAACW3RYN";
			cusEntryHeader.CH_BGMReference = "S00046074/1";
			cusEntryHeader.CH_TotalPaid = 1148;

			var customsCharge = cusEntryHeader.Charges.AddNew();
			customsCharge.C1_ChargeAmount = 1148;
			customsCharge.C1_ChargeType = Constants.Customs.CusEntryFeeTypes.DutyAmount;

			#endregion

			Factory.Save();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  15000m,
						},
					new AssertionCharge
						{
							ChargeCode = "TBK1",
							JR_OSSellAmt =  120m,
						},
					new AssertionCharge
						{
							ChargeCode = "CUSDSB",
							JR_OSSellAmt =  1148m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
		}

		public void TestCustomsChargeHasCorrectValueAfterRatingWithAdditionalJobsWithPreExistedCustomsCharges()
		{
			CreateRefCusRateCode(Constants.Customs.CusEntryFeeTypes.DutyAmount, Constants.Customs.CusEntryFeeTypes.DutyAmount);

			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			#region Create Rate

			var rate = Helper.NewClientRate(localClient);
			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var tbkChargeCode = Helper.ChargeCodes.New("TBK1", "Transport Booking 1", "", ChargeCodeGroupList.Codes.TransportBooking);
			var rateEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");

			var rateLine2 = rateEntry2.RateLines.AddNew();
			rateLine2.TL_AC = tbkChargeCode.PK;
			rateLine2.TL_RateCalculator = FlatCalculator.Code;
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 120;

			#endregion

			#region Create shipment

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			var query = new ZQuery(AccChargeCodeSchema.AC_Code, "CUSDSB");
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var customsChargeCode = Factory.LoadTop1<AccChargeCode>(query);

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = customsChargeCode.PK;
			charge.JR_OSCostAmt = 1148;
			charge.JR_OSSellAmt = 1148;

			#endregion

			#region Create booking consolidation with booking

			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = shipment.PK;
			bookingConsolidation.KB_ParentTableCode = "JS";

			var booking = bookingConsolidation.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = "EFPU";
			booking.KM_RatingFreightMode = "LSE";
			booking.KM_JobID = "TM00000001";

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsLooseRateable = true;
			fromInstruction.Address.OrganisationPK = localClient.PK;

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsLooseRateable = true;
			toInstruction.Address.OrganisationPK = consignee.PK;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";

			var box = CreatePackage(booking, 3, PkgUnit.Box, commodity, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var pallet = CreatePackage(booking, 23, PkgUnit.Pallet, commodity, 20m, Weight.Kilograms, 0.15m, Volume.CubicMetres);
			var carton = CreatePackage(booking, 5, PkgUnit.Carton, commodity, 50m, Weight.Kilograms, 0.30m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction, box, pallet, carton);
			CreateInstructionPkgDivots(toInstruction, box, pallet, carton);

			#endregion

			#region Create declaration

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.LCL;
			declaration.JE_MessageType = CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Refund;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_ShipmentIncoTerm = IncoTerms.ExWorks;
			declaration.JE_PaymentMethod = "BRK";
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_JS = shipment.PK;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "AAACW3RYN";
			cusEntryHeader.CH_BGMReference = "S00046074/1";
			cusEntryHeader.CH_TotalPaid = 1148;

			var customsCharge = cusEntryHeader.Charges.AddNew();
			customsCharge.C1_ChargeAmount = 1148;
			customsCharge.C1_ChargeType = Constants.Customs.CusEntryFeeTypes.DutyAmount;

			#endregion

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "FRT",
							JR_OSSellAmt =  15000m,
						},
					new AssertionCharge
						{
							ChargeCode = tbkChargeCode.AC_Code,
							JR_OSSellAmt =  120m,
						},
					new AssertionCharge
						{
							ChargeCode = customsChargeCode.AC_Code,
							JR_OSSellAmt =  1148m,
						}
				};

			AutorateAndAssert(expected, shipment, localClient);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain correct Customs Charge information", "  • CUSDSB charge from Customs Response message");
		}

		public void TestCustomChargesAreNotDuplicatedAfterAutoRatingWithDisbursementCalculator()
		{
			#region setup rates

			CreateRefCusRateCode(Constants.Customs.CusEntryFeeTypes.DutyAmount, Constants.Customs.CusEntryFeeTypes.DutyAmount);

			var cusDSB = Helper.ChargeCodes["CCLR"];

			var localClient = Helper.NewOrgHeader();
			localClient.OH_IsDebtor = true;
			var rate = Helper.NewClientRate(localClient);

			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine("FRT", DisbursementInterestCalculator.Code, "", CurrencyCodes.Australia);
			line1.TL_RateCalculator = DisbursementInterestCalculator.Code;
			line1.GetCalculator<DisbursementInterestCalculator>().AddApplyToItem(CalculatorConstants.Text.AllCharges);
			line1.GetCalculator<DisbursementInterestCalculator>().Uplift = 1;
			line1.GetCalculator<DisbursementInterestCalculator>().AdjustmentDays = 45;

			var line2 = entry.AddRateLine("CAF", PercentageCalculator.Code, "", CurrencyCodes.Australia);
			line2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = cusDSB.PK;
			line2.GetCalculator<PercentageCalculator>().Percent = 10m;

			Factory.Save();

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, localClient.PK, "AUSYD", "USLAX", 5000, 2);
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_INCO = "FOB";
			shipment.CustomsEntryNumberType = "T1";

			var consol = CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_UniqueConsignRef = "C00001111";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_PaymentMethod = "BRK";
			declaration.JE_EntryStatus = "DWC";
			declaration.JE_JS = shipment.PK;

			ServiceLocator.GetService<ICustomsCharges>(declaration);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges.AddNew(Constants.Customs.CusEntryFeeTypes.DutyAmount, 700m);

			var chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			var chargeTypeSettingDuty = chargeTypeSettings.AddNew();
			chargeTypeSettingDuty.ChargeType = Constants.Customs.CusEntryFeeTypes.DutyAmount;
			chargeTypeSettingDuty.AC_ChargeCode = cusDSB.PK;
			entryHeader.EntryChargeTypeList.RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeTypeSettings);

			Factory.Save();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			using (RatingDataRegistry.Instance.CurrentPrimeRate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 10M))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CAF",
						JR_OSSellAmt = 70m,
						RevenueCalculationDescription = "CAF: 10.00% of (AUD 700.00 (CCLR))"
					},
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 10.44m,
						RevenueCalculationDescription = "FRT: AUD 770.00 (All Charge Codes CAF 70.00 + CCLR 700.00) @ 11 % pa - 0 Days + 45 Adjustment Days Cash On Delivery (45 effective days) For Test Client #1"
					},
					new AssertionCharge
					{
						ChargeCode = "CCLR",
						JR_OSSellAmt = 700m
					}
				};

				AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
			}
		}

		public void TestCustomChargesAreNotDuplicatedAfterAutoRatingWithDisbursementCalculatorAndPreExistingJobCharge()
		{
			#region setup rates

			CreateRefCusRateCode(Constants.Customs.CusEntryFeeTypes.DutyAmount, Constants.Customs.CusEntryFeeTypes.DutyAmount);

			var cusDSB = Helper.ChargeCodes["CCLR"];

			var localClient = Helper.NewOrgHeader();
			localClient.OH_IsDebtor = true;
			var rate = Helper.NewClientRate(localClient);

			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine("FRT", DisbursementInterestCalculator.Code, "", CurrencyCodes.Australia);
			line1.TL_RateCalculator = DisbursementInterestCalculator.Code;
			line1.GetCalculator<DisbursementInterestCalculator>().AddApplyToItem(CalculatorConstants.Text.AllCharges);
			line1.GetCalculator<DisbursementInterestCalculator>().Uplift = 1;
			line1.GetCalculator<DisbursementInterestCalculator>().AdjustmentDays = 45;

			var line2 = entry.AddRateLine("CAF", PercentageCalculator.Code, "", CurrencyCodes.Australia);
			line2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = cusDSB.PK;
			line2.GetCalculator<PercentageCalculator>().Percent = 10m;

			Factory.Save();

			#endregion

			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, localClient.PK, "AUSYD", "USLAX", 5000, 2);
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_INCO = "FOB";
			shipment.CustomsEntryNumberType = "T1";

			var consol = CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_UniqueConsignRef = "C00001111";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_PaymentMethod = "BRK";
			declaration.JE_EntryStatus = "DWC";
			declaration.JE_JS = shipment.PK;

			ServiceLocator.GetService<ICustomsCharges>(declaration);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Charges.AddNew(Constants.Customs.CusEntryFeeTypes.DutyAmount, 700m);

			var chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			var chargeTypeSettingDuty = chargeTypeSettings.AddNew();
			chargeTypeSettingDuty.ChargeType = Constants.Customs.CusEntryFeeTypes.DutyAmount;
			chargeTypeSettingDuty.AC_ChargeCode = cusDSB.PK;
			entryHeader.EntryChargeTypeList.RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeTypeSettings);

			Factory.Save();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			using (RatingDataRegistry.Instance.CurrentPrimeRate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 10M))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			using (var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				shipmentJob.LocalChargesPK = localClient.PK;

				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "TAX1";
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.SetRateNumerator_ForTestOnly(5);

				var charge = shipmentJob.Charges.AddNew();
				charge.JR_AC = cusDSB.PK;
				charge.JR_Desc = "This is an existing Job Charge and already posted";
				charge.JR_OSSellAmt = 600m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_RX_NKCostCurrency = "AUD";
				charge.JR_OH_SellAccount = localClient.PK;
				charge.JR_SellRatingOverride = false;

				var transactionHeader = Factory.New<AccTransactionHeader>();
				transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
				var transactionLine = (ARInvoiceLine)Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
				transactionLine.AL_AH = transactionHeader.PK;
				transactionLine.AL_LineType = TransactionLineTypes.Revenue;
				transactionLine.AL_LineAmount = -600;
				transactionLine.AL_OSAmount = -600;
				transactionLine.AL_RX_NKTransactionCurrency = "AUD";
				transactionLine.AL_RevRecognitionType = "IMM";

				charge.JR_AL_ARLine = transactionLine.PK;
				charge.JR_AT_CostGSTRate = taxRate.PK;

				Assert(charge.IsRevenuePosted);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CAF",
						JR_OSSellAmt = 60m,
						RevenueCalculationDescription = "CAF: 10.00% of (AUD 600.00 (CCLR*))"
					},
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 8.95,
						RevenueCalculationDescription = "FRT: AUD 660.00 (All Charge Codes CAF 60.00 + CCLR* 600.00) @ 11 % pa - 0 Days + 45 Adjustment Days Cash On Delivery (45 effective days) For Test Client #1"
					},
					new AssertionCharge
					{
						ChargeCode = "CCLR",
						JR_OSSellAmt = 600m,
						JR_Desc = "This is an existing Job Charge and already posted"
					}
				};

				AutorateAndAssert(expected, shipment, localClient, job: shipmentJob, autorateCosts: false);
			}
		}

		public void TestDeferredCustomsChargeHasCorrectDescriptionAfterReAutoRating()
		{
			TestDeferredCustomsChargesHaveCorrectDescriptionAfterReAutorate(string.Empty);
		}

		public void TestDeferredCustomsChargeHasCorrectDescriptionAfterReAutoRating_WithLocalDescription()
		{
			TestDeferredCustomsChargesHaveCorrectDescriptionAfterReAutorate("Local language description goes here");
		}

		public void TestDeferredCustomsChargesHaveCorrectDescriptionAfterReAutorate(string localLanguageChargeCodeDescription)
		{
			CreateRefCusRateCode(Constants.Customs.CusEntryFeeTypes.GSTVATDeferred, Constants.Customs.CusEntryFeeTypes.GSTVATDeferred);
			var chargeCodeDeferred = CreateDeferredCustomsChargeCode("Customs Deferred Charge for information only", localLanguageChargeCodeDescription);

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RatingDataRegistry.Instance.CustomDeferredChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeDeferred.PK.ToGuid()))
			using (RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var localClient = Helper.NewOrgHeader();
				var consignee = Helper.NewOrgHeader();

				var shipment = CreateShipment("AUBNE", "USLAX", consignee, localClient);
				var declaration = CreateDeclaration(shipment);
				var customsCharge = CreateSingleCustomsCharge(declaration, Constants.Customs.CusEntryFeeTypes.GSTVATDeferred, 100);
				CreateSingleInvoiceLine(declaration, 1000m, 3);

				Factory.Save();

				using (var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly())
				{
					job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

					var expectedChargeCodeDescription = chargeCodeDeferred.AC_LocalLanguageDescription.IsEmpty ? chargeCodeDeferred.AC_Desc : chargeCodeDeferred.AC_LocalLanguageDescription;
					var expectedFirst = new[]
					{
						new AssertionCharge
						{
							ChargeCode = chargeCodeDeferred.AC_Code,
							JR_OSSellAmt =  0m,
							JR_Desc = $@"{expectedChargeCodeDescription}
  GST Deferred Amount                       100.00"
						}
					};
					declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
					AutorateAndAssert(expectedFirst, shipment, localClient, job: job);

					declaration.JE_OH_Importer = ZGuid.Empty;
					declaration.JE_OH_Supplier = ZGuid.Empty;
					customsCharge.C1_ChargeAmount = 300m;
					Factory.Save();

					var expectedSecond = new[]
					{
						new AssertionCharge
						{
							ChargeCode = chargeCodeDeferred.AC_Code,
							JR_OSSellAmt =  0m,
							JR_Desc = $@"{expectedChargeCodeDescription}
  GST Deferred Amount                       300.00"
						}
					};
					AutorateAndAssert(expectedSecond, shipment, localClient, job: job);
				}
			}
		}

		#endregion

		#region Client Contract Number

		public void TestPopulateShipmentClientContractNumber()
		{
			var chargeCode = Helper.ChargeCodes["CCLR"];

			// Three rates - one with contract# 111, one with contract# 222, and one blank
			var clientRateAtOrigin = Helper.NewClientRate(NewClient);
			var entry1 = clientRateAtOrigin.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "AUSYD", "NZAKL", chargeCode.AC_Code, 1100, CurrencyCodes.Australia);
			entry1.TI_ContractNumber = "111";
			var entry2 = clientRateAtOrigin.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "AUSYD", "NZAKL", chargeCode.AC_Code, 1150, CurrencyCodes.Australia);
			entry2.TI_ContractNumber = "222";
			var entryBlank = clientRateAtOrigin.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "AUSYD", "NZAKL", chargeCode.AC_Code, 1200, CurrencyCodes.Australia);
			entryBlank.TI_ContractNumber = "";

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "AUSYD", "NZAKL", 1000, 1);
			var declaration = CreateDeclaration(shipment);
			var jobHeader = new JobHeader.Loader(shipment).TryCreate();

			// Job contract# blank means all rates match.
			// User picks blank.
			var mockedDialogService = new Mock<IDialogService>();
			mockedDialogService
				.Setup(x => x.SelectSingleClientContractNumber(new[] { "", "111", "222" }))
				.Returns("");
			var testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), mockedDialogService.Object);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 1200
				}
			};
			AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false, ratingContext: testRatingContext);
			AssertEquals("", jobHeader.JH_ClientContractNumber);

			// User picks 222
			mockedDialogService = new Mock<IDialogService>();
			mockedDialogService
				.Setup(x => x.SelectSingleClientContractNumber(new[] { "", "111", "222" }))
				.Returns("222");
			testRatingContext = CreateRatingContextWithDialogService(new TestInteractor(), mockedDialogService.Object);

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalSellAmt = 1150
				}
			};

			AutorateAndAssert(expectedCharges, shipment, NewClient, autorateCosts: false, ratingContext: testRatingContext);
			AssertEquals("222", jobHeader.JH_ClientContractNumber);
		}

		#endregion

		#region TestAutoRatingDutyAndVATCharges

		public void TestAutoRatingDutyAndVATCharges_Germany()
		{
			var parentPK = CreateEUDataGrouping();
			CreateRefDataGrouping(CountryCodes.Germany, parentPK);
			CreateRefCusRateCode("A00", Constants.Customs.CusEntryFeeTypes.DutyAmount, "EUN");
			CreateRefCusRateCode("B00", "VAT", "EUN");
			var dutyChargeCode = CreateDutyCharge();
			var vatChargeCode = CreateVatCharge();
			var localClient = Helper.NewOrgHeader();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var declaration = CreateJobDeclarationForRatingTestForDE();
				Factory.Save();
				var collection = CreateChargeTypeCollection(dutyChargeCode, vatChargeCode);

				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);
				AssertAutoRatingWorksForDutyAndVat("Germany", declaration, localClient);
			}
		}

		/// <summary>
		/// This test is in conjunction with the above test but with bad data.
		/// </summary>
		public void TestAutoRatingDutyAndVATCharges_Germany_BadData_DSBChargeCodeFromAnotherCompany()
		{
			var parentPK = CreateEUDataGrouping();
			CreateRefDataGrouping(CountryCodes.Germany, parentPK);
			CreateRefCusRateCode("A00", Constants.Customs.CusEntryFeeTypes.DutyAmount, "EUN");
			CreateRefCusRateCode("B00", "VAT", "EUN");
			var dutyChargeCode = CreateDutyCharge();
			var vatChargeCode = CreateVatCharge();
			var localClient = Helper.NewOrgHeader();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var declaration = CreateJobDeclarationForRatingTestForDE();
				Factory.Save();
				var collection = CreateChargeTypeCollection(dutyChargeCode, vatChargeCode);

				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);

				var testObjectCreator = new TestObjectCreator(Factory);
				var newCompany = testObjectCreator.CreateCompanyAndBranch("AUSYD");
				dutyChargeCode.AC_GC = newCompany.PK;
				vatChargeCode.AC_GC = newCompany.PK;

				ErrorReporter.Clear();
				var notifications = UnitTestUserNotification.Instance;
				notifications.ClearMessages();

				AssertAutoRatingWorksForDutyAndVat("Germany", declaration, localClient, expectedChargesOverride: Array.Empty<AssertionCharge>());

				var expectedWarnings = new[]
				{
					"Customs charge filtered. Reason: the charge code 'DUTY' in 'AutoRating -> Charge Codes -> Customs -> Disbursement Charge Code Override' is not valid for autorating in the current login company.",
					"Customs charge filtered. Reason: the charge code 'VAT' in 'AutoRating -> Charge Codes -> Customs -> Disbursement Charge Code Override' is not valid for autorating in the current login company.",
				};
				AssertAutoratingAuditLogNoteContainsLines(declaration, "Log should say invalid charge code is filtered", expectedWarnings);

				const string message = "should not ErrorReport wrong company charge code in this case as it is identified and handled but show warning log instead.";
				AssertNullOrEmpty("ErrorReporter.LastMessageReported", ErrorReporter.LastMessageReported);

				AssertEquals("Two warnings should be reported.", 2, notifications.PreviousMessages.Length);

				var lastNotification = notifications.LastMessage.Text;
				AssertContainsInOrder(message, lastNotification, expectedWarnings);
			}
		}

		public void TestAutoRatingDutyAndVATCharges_Belgium()
		{
			var parentPK = CreateEUDataGrouping();
			CreateRefDataGrouping(CountryCodes.Belgium, parentPK);
			CreateRefCusRateCode("A00", Constants.Customs.CusEntryFeeTypes.DutyAmount, "EUN");
			CreateRefCusRateCode("B00", "VAT", "EUN");
			var dutyChargeCode = CreateDutyCharge();
			var vatChargeCode = CreateVatCharge();
			var localClient = Helper.NewOrgHeader();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Belgium))
			{
				var declaration = CreateJobDeclarationForRatingTest();
				Factory.Save();
				var collection = CreateChargeTypeCollection(dutyChargeCode, vatChargeCode);

				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);
				AssertAutoRatingWorksForDutyAndVat("Belgium", declaration, localClient);
			}
		}

		public void TestAutoRatingDutyAndVATCharges_Ireland()
		{
			var parentPK = CreateEUDataGrouping();
			CreateRefDataGrouping(CountryCodes.Ireland, parentPK);
			CreateRefCusRateCode("A00", Constants.Customs.CusEntryFeeTypes.DutyAmount, "EUN");
			CreateRefCusRateCode("B00", "VAT", "EUN");
			var dutyChargeCode = CreateDutyCharge();
			var vatChargeCode = CreateVatCharge();
			var localClient = Helper.NewOrgHeader();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var declaration = CreateJobDeclarationForRatingTest();
				Factory.Save();
				var collection = CreateChargeTypeCollection(dutyChargeCode, vatChargeCode);

				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);
				AssertAutoRatingWorksForDutyAndVat("Ireland", declaration, localClient);
			}
		}

		public void TestAutoRatingDutyAndVATCharges_Netherlands()
		{
			var parentPK = CreateEUDataGrouping();
			CreateRefDataGrouping(CountryCodes.Netherlands, parentPK);
			CreateRefCusRateCode("A00", Constants.Customs.CusEntryFeeTypes.DutyAmount, "EUN");
			CreateRefCusRateCode("B00", "VAT", "EUN");
			var dutyChargeCode = CreateDutyCharge();
			var vatChargeCode = CreateVatCharge();
			var localClient = Helper.NewOrgHeader();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Netherlands))
			{
				var declaration = CreateJobDeclarationForRatingTest();
				Factory.Save();
				var collection = CreateChargeTypeCollection(dutyChargeCode, vatChargeCode);

				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);
				AssertAutoRatingWorksForDutyAndVat("Netherlands", declaration, localClient);
			}
		}

		public void TestAutoRatingDutyAndVATCharges_France()
		{
			var parentPK = CreateEUDataGrouping();
			CreateRefDataGrouping(CountryCodes.France, parentPK);
			CreateRefCusRateCode("A00", Constants.Customs.CusEntryFeeTypes.DutyAmount, "EUN");
			CreateRefCusRateCode("B00", "VAT", "EUN");
			var dutyChargeCode = CreateDutyCharge();
			var vatChargeCode = CreateVatCharge();
			var localClient = Helper.NewOrgHeader();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var declaration = CreateJobDeclarationForRatingTestForFR();
				Factory.Save();
				var collection = CreateChargeTypeCollection(dutyChargeCode, vatChargeCode);

				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);
				AssertAutoRatingWorksForDutyAndVatForFR("France", declaration, localClient);
			}
		}

		ZString CreateEUDataGrouping()
		{
			var parentPK = ZString.Empty;
			var dynamicBOs = new DynamicBusinessObjectCollection(Factory);
			dynamicBOs.Load(
				FormattableString.Invariant(
					$@"SELECT TOP 1 {RefDataGroupingSchema.Constants.PK} FROM {RefDataGroupingSchema.Constants.TableName}
					WHERE {RefDataGroupingSchema.Constants.ZZZ_DataGrouping} = 'EUN'"));
			if (!(dynamicBOs.Count > 0))
			{
				parentPK = ZGuid.NewZGuid().ToString();
				var insertParentDataGroupingData = FormattableString.Invariant(
					$@"Insert into {RefDataGroupingSchema.Constants.TableName}
					({RefDataGroupingSchema.Constants.PK}, {RefDataGroupingSchema.Constants.ZZZ_DataGrouping}, {RefDataGroupingSchema.Constants.ZZZ_Description}, {RefDataGroupingSchema.Constants.ZZZ_ZZZ_Grouping})
			Values ('{parentPK}', 'EUN', 'EU Desc', null)");

				using (var command = CargoWise.Data.Db.Connection.Command(insertParentDataGroupingData))
				{
					command.ExecuteNonQuery();
				}
				Factory.Save();
			}
			else
			{
				parentPK = dynamicBOs.Select(x => new ZGuid(x[RefDataGroupingSchema.Constants.PK])).FirstOrDefault().ToString();
			}
			return parentPK;
		}

		BaseJobDeclaration CreateJobDeclarationForRatingTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var entryFee1 = entryLine.Fees.AddNew();
			entryFee1.CF_ChargeAmount = 0.45m;
			entryFee1.CF_ChargeType = "A00";

			var entryFee2 = entryLine.Fees.AddNew();
			entryFee2.CF_ChargeAmount = 619.06m;
			entryFee2.CF_ChargeType = "B00";

			return declaration;
		}

		BaseJobDeclaration CreateJobDeclarationForRatingTestForFR()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_PaymentMethod = "A";
			declaration.JE_DeclarantType = "IND";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var entryFee1 = entryLine.Fees.AddNew();
			entryFee1.CF_ChargeAmount = 0.55m;
			entryFee1.CF_ChargeType = "A00";
			entryFee1.CF_MethodOfPayment = "2";

			var entryFee2 = entryLine.Fees.AddNew();
			entryFee2.CF_ChargeAmount = 619.06m;
			entryFee2.CF_ChargeType = "B00";
			entryFee2.CF_MethodOfPayment = "2";

			return declaration;
		}

		BaseJobDeclaration CreateJobDeclarationForRatingTestForDE()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			var account = declarant.DefermentAccountNumberCollection.AddNew();
			account.CZ_Code = "10";
			account.CZ_Account = "1";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration["ZG_MethodOfPayment"] = "E";
			declaration.JE_DefermentAccountNumber = "1";
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.Branch.GB_OH_OrgProxy = declarant.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var entryFee1 = entryLine.Fees.AddNew();
			entryFee1.CF_ChargeAmount = 0.45m;
			entryFee1.CF_ChargeType = "A00";

			var entryFee2 = entryLine.Fees.AddNew();
			entryFee2.CF_ChargeAmount = 619.06m;
			entryFee2.CF_ChargeType = "B00";

			return declaration;
		}

		EntryChargeTypeSettingCollection CreateChargeTypeCollection(AccChargeCode dutyChargeCode, AccChargeCode vatChargeCode)
		{
			var collection = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

			var chargeType1 = collection.AddNew();
			chargeType1.AC_ChargeCode = dutyChargeCode.PK;
			chargeType1.ChargeType = Constants.Customs.CusEntryFeeTypes.DutyAmount;
			var chargeType2 = collection.AddNew();
			chargeType2.AC_ChargeCode = vatChargeCode.PK;
			chargeType2.ChargeType = "VAT";

			return collection;
		}

		AccChargeCode CreateDutyCharge()
		{
			var dutyChargeCode = Factory.New<AccChargeCode>();
			dutyChargeCode.AC_Code = "DUTY";
			dutyChargeCode.AC_Desc = "Customs duty on industrial products";
			dutyChargeCode.AC_ChargeType = ChargeType.Disbursement;
			dutyChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
			return dutyChargeCode;
		}

		AccChargeCode CreateVatCharge()
		{
			var vatChargeCode = Factory.New<AccChargeCode>();
			vatChargeCode.AC_Code = "VAT";
			vatChargeCode.AC_Desc = "VAT";
			vatChargeCode.AC_ChargeType = ChargeType.Disbursement;
			vatChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
			return vatChargeCode;
		}

		void AssertAutoRatingWorksForDutyAndVat(string company, BaseJobDeclaration declaration, OrgHeader localClient, AssertionCharge[] expectedChargesOverride = null)
		{
			var job = CreateJob(declaration, declaration.JE_DeclarationReference);
			job.PlugInData = declaration;
			job.JH_OA_AgentCollectAddr = localClient.MainAddress.PK;

			var expected = expectedChargesOverride ?? new[]
				{
					new AssertionCharge
					{
						ChargeCode = "DUTY",
						JR_OSSellAmt = 0.45m,
					},
					new AssertionCharge
					{
						ChargeCode = "VAT",
						JR_OSSellAmt = 619.06m,
					}
				};

			AutorateAndAssert("AutoRating should work for " + company, expected, declaration, localClient, null, job);
			job.Charges.RemoveAndDeleteAll();
		}

		void AssertAutoRatingWorksForDutyAndVatForFR(string company, BaseJobDeclaration declaration, OrgHeader localClient)
		{
			var job = CreateJob(declaration, declaration.JE_DeclarationReference);
			job.PlugInData = declaration;
			job.JH_OA_AgentCollectAddr = localClient.MainAddress.PK;

			var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "DUTY",
						JR_OSSellAmt = 1m,
					},
					new AssertionCharge
					{
						ChargeCode = "VAT",
						JR_OSSellAmt = 619m,
					}
				};

			AutorateAndAssert("AutoRating should work for " + company, expected, declaration, localClient, null, job);
			job.Charges.RemoveAndDeleteAll();
		}

		#endregion

		/// <summary>
		/// This test is to ensure that the invalid charge code (bad data) in registry: disbursement charge code from another company
		/// should not be used by autorating to create a new charge.
		/// </summary>
		public void TestAutorateDeclaration_BadData_DisbursementChargeCodeFromAnotherCompany() =>
			TestAutorateDeclaration_ChargeCodeFromAnotherCompany(isTestingDisbursementChargeCode: true);

		/// <summary>
		/// This test is to ensure that the invalid charge code (bad data) in registry: deferred charge code from another company
		/// should not be used by autorating to create a new charge.
		/// </summary>
		public void TestAutorateDeclaration_BadData_DeferredChargeCodeFromAnotherCompany() =>
			TestAutorateDeclaration_ChargeCodeFromAnotherCompany(isTestingDisbursementChargeCode: false);

		void TestAutorateDeclaration_ChargeCodeFromAnotherCompany(bool isTestingDisbursementChargeCode)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var chargeCode = testObjectCreator.DSBChargeCode;
			AssertEquals("Precondition: charge code is loaded for current login company", Env.CurrentCompanyPK, chargeCode.AC_GC);

			var newCompany = testObjectCreator.CreateCompanyAndBranch("AUSYD");
			chargeCode.AC_GC = newCompany.PK;

			var client = Helper.NewOrgHeader();
			client.OH_Code = "NTC3";
			client.OH_FullName = "New Test Client";
			client.OH_RL_NKClosestPort = "AUSYD";

			client.OH_IsDebtor = true;
			client.OH_IsConsignor = true;
			client.OH_IsConsignee = true;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.LCL;
			declaration.JE_EntryStatus = CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code;
			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			declaration.JE_OH_Supplier = client.PK;
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_TotalWeight = 20;
			declaration.JE_TotalWeightUnit = "KG";

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "AAACW3RYN";
			cusEntryHeader.CH_BGMReference = "S00046074/1";
			cusEntryHeader.CH_TotalPaid = 30m;

			var customsCharge = cusEntryHeader.Charges.AddNew();
			customsCharge.C1_ChargeAmount = 45m;

			var registryPath = string.Empty;
			if (isTestingDisbursementChargeCode)
			{
				// Registry values can be set for the scope of this test. Data rollback after the test will revert them automatically.
				RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
				customsCharge.C1_ChargeType = Constants.Customs.CusEntryFeeTypes.DutyAmount;
				registryPath = "AutoRating -> Charge Codes -> Customs -> Default Disbursement Charge Code";
			}
			else
			{
				RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
				RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
				customsCharge.C1_ChargeType = Constants.Customs.CusEntryFeeTypes.GSTVATDeferred;
				registryPath = "AutoRating -> Charge Codes -> Customs -> Deferred Charge Code";
			}

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			Factory.Save();

			ErrorReporter.Clear();
			var notifications = UnitTestUserNotification.Instance;
			notifications.ClearMessages();
			AutorateAndAssert("Should not create any charge and there is no error", Array.Empty<AssertionCharge>(), declaration, client, autorateCosts: false);

			var expectedWarning = $"Customs charge filtered. Reason: the charge code 'ZZDSB' in '{registryPath}' is not valid for autorating in the current login company.";
			AssertAutoratingAuditLogNoteContainsLines(declaration, "Log should say invalid charge code is filtered", expectedWarning);

			const string message = "should not ErrorReport wrong company charge code in this case as it is identified and handled but show warning log instead.";
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertEquals("Expected 2 notification messages", 2, notifications.PreviousMessages.Length);
			AssertContains(message, expectedWarning, notifications.LastMessage.Text);
		}

		public void TestAutorateDeclaration_CMBCalculatorWithUnitPK_ShouldNotThrowException()
		{
			var client = Helper.NewOrgHeader();
			client.OH_Code = "NTC3";
			client.OH_FullName = "New Test Client";
			client.OH_RL_NKClosestPort = "JPOSA";

			client.OH_IsDebtor = true;
			client.OH_IsConsignor = true;
			client.OH_IsConsignee = true;

			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntryWithCMBRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "JP", "AU", "CCLR", 1m, QuantityUnit.PK, "", "", "", ("-50", 5m), ("+50", 10m));

			var rateLine = rateEntry.RateLines[0];
			var minusRateLineItem = rateLine.RateLineItems.OfType<RateLineItem>()
				.Single(item => item.TM_Type == Calculator.Items.Operator.Minus);
			minusRateLineItem.TM_BreakWeightVolume = QuantityUnit.KG;

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = client.PK;
			declaration.JE_MessageType = CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.LCL;
			declaration.JE_ApplicationCode = "SMR";
			declaration.JE_RL_NKOrigin = "JPOSA";
			declaration.JE_RL_NKFinalDestination = "AUSYD";

			declaration.JE_TotalNoOfPacks = 20;
			declaration.JE_TotalNoOfPacksPackType = "PKG";
			declaration.JE_TotalVolume = 20;
			declaration.JE_TotalVolumeUnit = "M3";
			declaration.JE_TotalWeight = 20;
			declaration.JE_TotalWeightUnit = "KG";

			UnitTestUserNotification.Instance.AddYesAnswer();
			UnitTestUserNotification.Instance.AddYesAnswer();
			AutorateAndAssert("Should not throw NullReferenceException", Array.Empty<AssertionCharge>(), declaration, client, autorateCosts: false);
		}

		#region Implementation

		protected override void SetUp()
		{
			Helper.ChargeCodes.New("BRKCHG", "Rated Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.CustomsDuty);

			Factory.Save();
		}

		ForwardingShipment CreateShipment(string origin, string destination, OrgHeader consignee, OrgHeader consignor)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = "LCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "CLT";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.MainAddress.PK;

			return shipment;
		}

		BaseJobDeclaration CreateDeclaration(ForwardingShipment shipment)
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.LCL;
			declaration.JE_MessageType = CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Refund;
			declaration.JE_RL_NKOrigin = shipment.JS_RL_NKOrigin;
			declaration.JE_RL_NKFinalDestination = shipment.JS_RL_NKDestination;
			declaration.JE_ShipmentIncoTerm = "EXW";
			declaration.JE_PaymentMethod = "BRK";
			declaration.JE_JS = shipment.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			return declaration;
		}

		CusEntryHeaderCharges CreateSingleCustomsCharge(BaseJobDeclaration declaration, ZString chargeType, ZDecimal amount)
		{
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "AAACW3RYN";
			cusEntryHeader.CH_BGMReference = "S00046074/1";
			cusEntryHeader.CH_JE = declaration.PK;
			cusEntryHeader.CH_TotalPaid = amount;

			var custCharge = cusEntryHeader.Charges.AddNew();
			custCharge.C1_ChargeAmount = amount;
			custCharge.C1_ChargeType = chargeType;

			return custCharge;
		}

		BaseJobComInvoiceLine CreateSingleInvoiceLine(BaseJobDeclaration declaration, ZDecimal amount, int quantity)
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = amount;
			invoiceLine.JI_InvoiceQuantity = quantity;

			return invoiceLine;
		}

		AccChargeCode CreateDeferredCustomsChargeCode(string description, string localLanguageDescription = null)
		{
			localLanguageDescription = localLanguageDescription ?? string.Empty;

			var chargeCodeDeferred = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeDeferred.AC_ChargeType = ChargeType.Comment;
			chargeCodeDeferred.AC_Desc = description;
			chargeCodeDeferred.AC_LocalLanguageDescription = localLanguageDescription;
			chargeCodeDeferred.AC_MarginPercentage = 0m;
			chargeCodeDeferred.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCodeDeferred.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
			chargeCodeDeferred.SetGLAccountDataForTesting();
			chargeCodeDeferred.AC_Code = "ZZ-CUSDEF";
			Factory.Save();

			return chargeCodeDeferred;
		}

		BaseJobDeclaration CreateJobDeclaration(string messageType, string transportMode, string containerMode, string entryStatus, string paymentMethod, ZGuid supplierPK, ZGuid? importerPK, string origin, string destination, ZGuid? shipmentPK, params (decimal chargeAmount, string chargeType)[] charges)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_TransportMode = transportMode;
			declaration.JE_ContainerMode = containerMode;
			declaration.JE_EntryStatus = entryStatus;
			declaration.JE_PaymentMethod = paymentMethod;
			declaration.JE_OH_Supplier = supplierPK;

			if (importerPK != null)
			{
				declaration.JE_OH_Importer = importerPK.Value;
			}
			declaration.JE_RL_NKOrigin = origin;
			declaration.JE_RL_NKFinalDestination = destination;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			if (shipmentPK != null)
			{
				declaration.JE_JS = shipmentPK.Value;
			}

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			foreach (var charge in charges)
			{
				var customsCharge = cusEntryHeader.Charges.AddNew();
				customsCharge.C1_ChargeAmount = charge.chargeAmount;
				customsCharge.C1_ChargeType = charge.chargeType;
			}

			return declaration;
		}

		#endregion
	}
}
