using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandedCostHeader))]
	sealed class LandedCostHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCustomsChargeLCItemSettings_DeclarationIsIntegrated()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var testDec = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
				testDec[JobDeclarationSchema.Constants.JE_ApplicationCode] = "ITF";
				var invoice = (BusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
				invoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = testDec.PK;
				invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount] = 1000m;
				invoice[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoice[JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm] = "FOB";

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var testDecLoaded = (BusinessObject)factory2.Load<Integration.Customs.IBaseJobDeclaration>(testDec.PK);
				var lCHeader = factory2.New<LandedCostHeader>();
				lCHeader.LT_ParentID = testDecLoaded.PK;
				lCHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
				lCHeader.SynchroniseAll();

				CombineAssertions(() =>
				{
					AssertEquals("Integrated", "TDT", lCHeader.CustomsChargeLCItemSettings.Single().CostType);

					testDecLoaded[JobDeclarationSchema.Constants.JE_ApplicationCode] = "";
					AssertEquals("!Integrated", true, lCHeader.CustomsChargeLCItemSettings.Any(x => x.CostType == "QUA"));
				});
			}
		}

		public void TestAutoRateWhenDefaultFromHost()
		{
			OrgHeader buyer = new TestHelper(new BusinessObjectFactory()).SetAutoratingData();

			BusinessObject order = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IOrder>());
			order[JobOrderHeaderSchema.JD_OA_BuyerAddress] = buyer.MainAddress.PK;
			order[JobOrderHeaderSchema.JD_IncoTerm] = Core.Constants.IncoTerms.FreeOnBoard;
			order[JobOrderHeaderSchema.JD_RL_NKPortOfDischarge] = "AUSYD";
			order[JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo] = "AUSYD";
			order[JobOrderHeaderSchema.JD_RL_NKPortOfLoading] = "NZAKL";
			order[JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt] = "NZAKL";
			order[JobOrderHeaderSchema.JD_RX_NKOrderCurrency] = Core.Constants.CurrencyCodes.Australia;
			order[JobOrderHeaderSchema.JD_TransportMode] = Core.Constants.TransportModes.Sea;
			order[JobOrderHeaderSchema.JD_ContainerMode] = Core.Constants.ContainerModes.LCL;
			order[JobOrderHeaderSchema.JD_RN_NKCountryOfSupply] = Core.Constants.CountryCodes.NewZealand;
			order[JobOrderHeaderSchema.JD_ActualWeight] = 500m;
			order[JobOrderHeaderSchema.JD_UnitOfWeight] = Core.Constants.Weight.Kilograms;
			order[JobOrderHeaderSchema.JD_ActualVolume] = 1500m;
			order[JobOrderHeaderSchema.JD_UnitOfVolume] = Core.Constants.Volume.CubicMetres;
			order[JobOrderHeaderSchema.JD_OrderNumber] = "J002342";
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired((IWorkflowProvider)order);
			((IWorkflowProvider)order).WorkflowItems.Milestones[ZArchitecture.Business.AutoEvents.Departure].SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2010, 1, 19)));
			((IWorkflowProvider)order).WorkflowItems.Milestones[ZArchitecture.Business.AutoEvents.Arrival].SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2010, 1, 25)));

			BusinessObject container = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IOrderContainer>());
			container[JobOrderContainerSchema.J1_ContainerCount] = new ZShort(2);
			container[JobOrderContainerSchema.J1_RC] = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			container[JobOrderContainerSchema.J1_ParentID] = order.PK;
			container[JobOrderContainerSchema.J1_ParentTableCode] = JobOrderHeaderSchema.Constants.Prefix;

			BusinessObject orderLine = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IOrderLine>());
			orderLine[JobOrderLineSchema.JO_JD] = order.PK;
			orderLine[JobOrderLineSchema.JO_LineNo] = new ZInt(1);
			orderLine[JobOrderLineSchema.JO_Quantity] = 10m;
			orderLine[JobOrderLineSchema.JO_F3_NKPackType] = "BOX";
			orderLine[JobOrderLineSchema.JO_ItemPrice] = 1000m;
			orderLine[JobOrderLineSchema.JO_LinePrice] = 10000m;
			orderLine[JobOrderLineSchema.JO_ActualWeight] = 200m;
			orderLine[JobOrderLineSchema.JO_UnitOfWeight] = Core.Constants.Weight.Kilograms;
			orderLine[JobOrderLineSchema.JO_ActualVolume] = 300m;
			orderLine[JobOrderLineSchema.JO_UnitOfVolume] = Core.Constants.Volume.CubicMetres;

			Factory.Save();

			LandedCostHeader lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.DefaultFromHost((ILandedCostHeader)order);
			lcHeader.SynchroniseAll();

			AssertEquals(1, lcHeader.CostInputs.Count);
			AssertEquals(103m, lcHeader.CostInputs[0].LI_CostAmount);
			AssertEquals("Distribute to is defaulted to Order", order, lcHeader.CostInputs[0].Parent);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, lcHeader.CostInputs[0].LI_RX_NKCostCurrency);
			AssertEquals("FRT: MIN ERN 103.00 (Job Minimum)", lcHeader.CostInputs[0].LI_ChargeDescription);
		}

		public void TestTotalCostWithMarkup1Applied()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			helper.Ultimate1.CostInLocalCurrencyExposed = 100m;
			helper.Ultimate2.CostInLocalCurrencyExposed = 200m;

			DutyTaxEntryFee dutyTaxEntryFee = new DutyTaxEntryFee();
			dutyTaxEntryFee["TDT"] = 25m;
			dutyTaxEntryFee["ST1"] = 45m;
			dutyTaxEntryFee["ST2"] = 65m;
			dutyTaxEntryFee["ST3"] = 85m;
			dutyTaxEntryFee["QUA"] = 10.25m;
			helper.DummyHeader.TotalDutyTaxEntryFeeItemsExposed = dutyTaxEntryFee;

			LandedCostHistory lCHistory1 = lCHeader.Histories.AddNew();
			lCHistory1.UltimateDistributee = helper.Ultimate1;
			lCHistory1.LH_LandedCostMarginPercent1 = 5.00;
			lCHistory1.SetLineValue(10.00m, "ENT");
			lCHistory1.SetLineValue(10.00m, "EXC");
			lCHistory1.LH_LandedCostGroup1 = 10.00m;
			lCHistory1.LH_LandedCostGroupMisc = 10.00m;
			lCHistory1.SetLineValue(10.00m, "ST1");

			LandedCostHistory lCHistory2 = lCHeader.Histories.AddNew();
			lCHistory2.UltimateDistributee = helper.Ultimate2;
			lCHistory2.LH_LandedCostMarginPercent1 = 10.00;
			lCHistory2.SetLineValue(10.00m, "ENT");
			lCHistory2.SetLineValue(10.00m, "EXC");
			lCHistory2.LH_LandedCostGroup1 = 10.00m;
			lCHistory2.LH_LandedCostGroupMisc = 10.00m;
			lCHistory2.SetLineValue(10.00m, "ST1");

			AssertEquals("TotalCostWithMarkUp1Applied", 432.50m, lCHeader.TotalCostWithMarkup1Applied);

			helper.DummyHeader.UniqueReferenceNumberExposed = "XYZ";
			AssertEquals("XYZ", lCHeader.UniqueReferenceNumber);

			helper.DummyHeader.JobNumberExposed = "ABC";
			AssertEquals("ABC", lCHeader.ReferenceNumber);
		}

		public void TestDisclaimer()
		{
			LandedCostHeader lcHeader = Factory.New<LandedCostHeader>();

			DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.LandedCostingClosingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "I do disclaim");

			AssertEquals("I do disclaim", lcHeader.Disclaimer);
		}

		public void TestDocManagerInfo()
		{
			var lcHeader = Factory.New<LandedCostHeader>();
			AssertEquals(Core.Constants.DocManagerCodes.LandedCostHeader, lcHeader.DocManagerInfo.DocManagerCode);
		}

		public void TestEndToEndForChargeImportsForOverseasFreight()
		{
			BusinessObject testDec = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			BusinessObject invoice = (BusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = testDec.PK;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount] = 1000m;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm] = "FOB";

			BusinessObject invoiceLine = (BusinessObject)Factory.New<Integration.Customs.IBaseJobComInvoiceLine>();
			invoiceLine[JobComInvoiceLineSchema.Constants.JI_JZ] = invoice.PK;
			invoiceLine[JobComInvoiceLineSchema.Constants.JI_LinePrice] = 1000m;

			BusinessObject charge = ((BusinessObjectCollection)invoiceLine["Charges"]).AddNew();
			charge[JobComInvHeaderChargeSchema.Constants.J7_ParentID] = invoiceLine.PK;
			charge[JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode] = JobComInvoiceLineSchema.Constants.Prefix;
			charge[JobComInvHeaderChargeSchema.Constants.J7_ChargeType] = "OFT";
			charge[JobComInvHeaderChargeSchema.Constants.J7_Amount] = 100m;
			charge[JobComInvHeaderChargeSchema.Constants.J7_RX_NKCurrency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var jobInvoicing = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobInvoicing[JobHeaderSchema.JH_ParentID.Name] = testDec.PK;

			JobCharge charge1 = Factory.New<JobCharge>();
			charge1.FillWithValidTestData();
			charge1.JR_JH = jobInvoicing.PK;
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			charge1.JR_LocalSellAmt = 150m;
			charge1.JR_OSSellAmt = 150m;
			charge1.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BusinessObject testDecLoaded = (BusinessObject)factory2.Load<Integration.Customs.IBaseJobDeclaration>(testDec.PK);
			LandedCostHeader lCHeader = factory2.New<LandedCostHeader>();
			lCHeader.LT_ParentID = testDecLoaded.PK;
			lCHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			lCHeader.SynchroniseAll();

			AssertEquals("There should be one LC input from Invoicing", 1, lCHeader.CostInputs.Count);
			AssertEquals("Amount", 150m, lCHeader.CostInputs[0].LI_CostAmount);
		}

		public void TestRefreshExchangeRatesWhenInvoiceIsDeleted()
		{
			var testDec = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			var invoice = (BusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = testDec.PK;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount] = 1000m;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm] = "FOB";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var testDecLoaded = (BusinessObject)factory2.Load<Integration.Customs.IBaseJobDeclaration>(testDec.PK);
			var lCHeader = factory2.New<LandedCostHeader>();
			lCHeader.LT_ParentID = testDecLoaded.PK;
			lCHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			lCHeader.SynchroniseAll();

			AssertEquals("PRecondition", 1, lCHeader.ExchangeRates.Count);

			var invoiceLoaded = (BusinessObject)factory2.Load<Integration.Customs.Shared.IBaseJobComInvoiceHeader>(invoice.PK);
			invoiceLoaded.Delete();
			AssertEquals(0, lCHeader.ExchangeRates.Count);
		}

		public void TestSynchroniseAllSetsDistributeLevelIfPossible()
		{
			DummyLandedCostHeader dummyHost = Factory.New<DummyLandedCostHeader>();

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = dummyHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			DummyLandedCostDistributeToAndLandCostChargeHolder dummyChargeHolder = Factory.New<DummyLandedCostDistributeToAndLandCostChargeHolder>();
			dummyHost.ChargeHoldersExposed = new ILandedCostChargeHolder[] { dummyChargeHolder };
			dummyHost.ExchangeRateHoldersExposed = Array.Empty<ILandedCostExchangeRateHolder>();
			dummyHost.CandidatesToDistributeCostToExposed = Array.Empty<ILandedCostDistributeTo>();

			DummyLandCostInput dummyCharge = new DummyLandCostInput();
			dummyCharge.IsValidToImportExposed = true;
			dummyCharge.AmountToDistributeExposed = new Money(100m, GlbCompany.CurrentCompany.LocalCurrency);
			dummyCharge.ChargeDescriptionExposed = "Blah";

			dummyChargeHolder.ChargesToImportForLandedCostingExposed = new IDefaultLandedCostInput[] { dummyCharge };
			lCHeader.SynchroniseAll();
			AssertEquals("LCHeader shouuhd have one cost input defaulted", 1, lCHeader.CostInputs.Count);

			LandCostInput costInput = lCHeader.CostInputs[0];
			AssertEquals("Cost input distribute level is set", dummyChargeHolder.PK, costInput.LI_ParentID);
			AssertEquals("Cost input distribute level is set", dummyChargeHolder.TableCode, costInput.LI_ParentTableCode);
		}

		public void TestExchangeRatesEditableChild()
		{
			DummyLandedCostHeader dummyHost = Factory.New<DummyLandedCostHeader>();
			dummyHost.ExchangeRateHoldersExposed = Array.Empty<ILandedCostExchangeRateHolder>();
			LandedCostHeader lCHeader = LandedCostHeader.New(dummyHost);
			AssertEquals("Exchange rates are an editable child", true, lCHeader.IsRegisteredEditableChildObject(lCHeader.ExchangeRates));
		}

		public void TestStaticConstructor()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();

			DummyLandedCostHeader dummyHost = Factory.New<DummyLandedCostHeader>();
			dummyHost.CompanyPK = company.PK;

			LandedCostHeader lCHeader = LandedCostHeader.New(dummyHost);
			AssertEquals("LCHeader ParentID set", dummyHost.PK, lCHeader.LT_ParentID);
			AssertEquals("LCHeader ParentTableCode Set", DummyBizoSchema.Constants.Prefix, lCHeader.LT_ParentTableCode);
			AssertEquals(company.PK, lCHeader.LT_GC);
		}

		public void TestIDocumentSupportable()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			AssertEquals("DocumentSupporter for declaration", typeof(LandedCostingDocumentDeclarationSupporter), ((IDocumentSupportable)lCHeader).DocumentSupporter.GetType());
		}

		public void TestTotals()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				TestHelper helper = new TestHelper(Factory);
				LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
				helper.Ultimate1.CostInLocalCurrencyExposed = 100m;
				helper.Ultimate2.CostInLocalCurrencyExposed = 200m;

				DutyTaxEntryFee dutyTaxEntryFee = new DutyTaxEntryFee();
				dutyTaxEntryFee["TDT"] = 25m;
				dutyTaxEntryFee["ST1"] = 45m;
				dutyTaxEntryFee["ST2"] = 65m;
				dutyTaxEntryFee["ST3"] = 85m;
				dutyTaxEntryFee["QUA"] = 10.25m;
				helper.DummyHeader.TotalDutyTaxEntryFeeItemsExposed = dutyTaxEntryFee;

				AssertEquals("TotalLanding Cost from CostInputs", 16000m, lCHeader.CostInputs.TotalLandingCost);
				AssertEquals("Total Customs Disbursement Charges", 230.25m, lCHeader.TotalCustomsDisbursementCharges);

				LandedCostHistory lCHistory1 = lCHeader.Histories.AddNew();
				lCHistory1.UltimateDistributee = helper.Ultimate1;
				LandedCostHistory lCHistory2 = lCHeader.Histories.AddNew();
				lCHistory2.UltimateDistributee = helper.Ultimate2;

				AssertEquals("Total Landing Cost", 16000m, lCHeader.TotalLandingCost);
				AssertEquals("Total Invoice cost", 300m, lCHeader.TotalInvoiceCost);

				AssertEquals("Total Cost", 16530.25m, lCHeader.TotalCost);
			}
		}

		public void TestTotalDutiesAndTaxes()
		{
			DummyLandedCostHeader dummyHost = Factory.New<DummyLandedCostHeader>();

			DutyTaxEntryFee total = new DutyTaxEntryFee();
			total["TDT"] = 1.1m;
			total["ENT"] = 2.2m;
			total["OTH"] = 3.3m;
			total["ST1"] = 4.4m;
			total["ST2"] = 5.5m;
			total["ST3"] = 6.6m;
			total["EXC"] = 7.7m;
			dummyHost.TotalDutyTaxEntryFeeItemsExposed = total;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = dummyHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			AssertEquals("Parent of LCHeader", dummyHost, lCHeader.Parent);
			AssertEquals("Total Duties AndTaxes", 30.8m, lCHeader.TotalCustomsDisbursementCharges);
		}

		public void TestExchangeRates()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			DummyExchangeRateHolder holder1 = Factory.New<DummyExchangeRateHolder>();
			holder1.LandedCostExchangeRateExposed = 0.5m;
			holder1.CurrencyCodeExposed = "USD";
			holder1.ReferenceNumberExposed = "1";

			DummyExchangeRateHolder holder2 = Factory.New<DummyExchangeRateHolder>();
			holder2.LandedCostExchangeRateExposed = 0.789m;
			holder2.CurrencyCodeExposed = "USD";
			holder2.ReferenceNumberExposed = "2";

			helper.DummyHeader.ExchangeRateHoldersExposed = new DummyExchangeRateHolder[] { holder1, holder2 };
			AssertEquals("Exchange rates are loaded", 2, lCHeader.ExchangeRates.Count);
		}

		public void TestExchangeRatesAreLoadedAgainWhenSynchronised()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			DummyExchangeRateHolder holder1 = Factory.New<DummyExchangeRateHolder>();

			helper.DummyHeader.ExchangeRateHoldersExposed = new DummyExchangeRateHolder[] { holder1 };
			AssertEquals("Exchange rates are loaded", 1, lCHeader.ExchangeRates.Count);

			DummyExchangeRateHolder holder2 = Factory.New<DummyExchangeRateHolder>();
			helper.DummyHeader.ExchangeRateHoldersExposed = new DummyExchangeRateHolder[] { holder1, holder2 };

			AssertEquals("Exchange rates still only have one loaded", 1, lCHeader.ExchangeRates.Count);
			lCHeader.SynchroniseAll();
			AssertEquals("Exchange rates are loaded", 2, lCHeader.ExchangeRates.Count);
		}

		public void TestCustomsLabelsConfigOrgProvider()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			DummyLandedCostHeader lCHost = Factory.New<DummyLandedCostHeader>();
			lCHost.ConsigneeExposed = consignee;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = lCHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			AssertEquals("ConfigOrg", consignee, ((ICustomLabelsConfigOrgProvider)lCHeader).ConfigOrg);
		}

		public void TestLabelsAndCostDistributionCode()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			DummyLandedCostHeader lCHost = Factory.New<DummyLandedCostHeader>();
			lCHeader.LT_ParentID = lCHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			OrgHeader consignee = Factory.New<OrgHeader>();
			OrgLandedCostingPrefs group1 = consignee.LandedCostingPreferences.AddNew();
			group1.O9_LandedCostGroup = 1;
			group1.O9_LandedCostGroupName = "GROUP1";
			group1.O9_DistributeCostBy = CostDistributionMechanismList.Codes.Actual;

			OrgLandedCostingPrefs group2 = consignee.LandedCostingPreferences.AddNew();
			group2.O9_LandedCostGroup = 2;
			group2.O9_LandedCostGroupName = "GROUP2";
			group2.O9_DistributeCostBy = CostDistributionMechanismList.Codes.ActualVolume;

			OrgLandedCostingPrefs group3 = consignee.LandedCostingPreferences.AddNew();
			group3.O9_LandedCostGroup = 3;
			group3.O9_LandedCostGroupName = "GROUP3";
			group3.O9_DistributeCostBy = CostDistributionMechanismList.Codes.ActualWeight;

			OrgLandedCostingPrefs group4 = consignee.LandedCostingPreferences.AddNew();
			group4.O9_LandedCostGroup = 4;
			group4.O9_LandedCostGroupName = "GROUP4";
			group4.O9_DistributeCostBy = CostDistributionMechanismList.Codes.Item;

			OrgLandedCostingPrefs group5 = consignee.LandedCostingPreferences.AddNew();
			group5.O9_LandedCostGroup = 5;
			group5.O9_LandedCostGroupName = "GROUP5";
			group5.O9_DistributeCostBy = CostDistributionMechanismList.Codes.LineValue;

			OrgLandedCostingPrefs group6 = consignee.LandedCostingPreferences.AddNew();
			group6.O9_LandedCostGroup = 6;
			group6.O9_LandedCostGroupName = "GROUP6";
			group6.O9_DistributeCostBy = CostDistributionMechanismList.Codes.Actual;

			OrgLandedCostingPrefs group7 = consignee.LandedCostingPreferences.AddNew();
			group7.O9_LandedCostGroup = 7;
			group7.O9_LandedCostGroupName = "GROUP7";

			lCHost.ConsigneeExposed = consignee;
			AssertEquals("Group1 Label", group1.O9_LandedCostGroupName, lCHeader.LandedCostGroup1Label);
			AssertEquals("Group2 Label", group2.O9_LandedCostGroupName, lCHeader.LandedCostGroup2Label);
			AssertEquals("Group3 Label", group3.O9_LandedCostGroupName, lCHeader.LandedCostGroup3Label);
			AssertEquals("Group4 Label", group4.O9_LandedCostGroupName, lCHeader.LandedCostGroup4Label);
			AssertEquals("Group5 Label", group5.O9_LandedCostGroupName, lCHeader.LandedCostGroup5Label);
			AssertEquals("Group6 Label", group6.O9_LandedCostGroupName, lCHeader.LandedCostGroup6Label);
			AssertEquals("GroupMisc Label", "Misc Charges", lCHeader.LandedCostGroupMiscLabel);

			AssertEquals("Group1 Cost Distribution Code", group1.O9_DistributeCostBy, lCHeader.LandedCostGroup1CostDistributionCode);
			AssertEquals("Group2 Cost Distribution Code", group2.O9_DistributeCostBy, lCHeader.LandedCostGroup2CostDistributionCode);
			AssertEquals("Group3 Cost Distribution Code", group3.O9_DistributeCostBy, lCHeader.LandedCostGroup3CostDistributionCode);
			AssertEquals("Group4 Cost Distribution Code", group4.O9_DistributeCostBy, lCHeader.LandedCostGroup4CostDistributionCode);
			AssertEquals("Group5 Cost Distribution Code", group5.O9_DistributeCostBy, lCHeader.LandedCostGroup5CostDistributionCode);
			AssertEquals("Group6 Cost Distribution Code", group6.O9_DistributeCostBy, lCHeader.LandedCostGroup6CostDistributionCode);
		}

		public void TestReadOnlyProperties()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			AssertEquals("LT_DateOfEntry is readonly", true, lCHeader.LT_DateOfEntryInfo.ReadOnly);
			AssertEquals("LT_DateOfProcessing is readonly", true, lCHeader.LT_DateOfProcessingInfo.ReadOnly);
		}

		public void TestParent()
		{
			DummyLandedCostHeader dummy1 = Factory.New<DummyLandedCostHeader>();
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = dummy1.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			AssertEquals("Parent", dummy1, lCHeader.Parent);

			DummyLandedCostHeader dummy2 = Factory.New<DummyLandedCostHeader>();
			lCHeader.LT_ParentID = dummy2.PK;
			AssertEquals("Parent Refreshed", dummy2, lCHeader.Parent);
		}

		public void TestHistories()
		{
			LandedCostHeader header = Factory.New<LandedCostHeader>();
			AssertNotNull("Histories", header.Histories);
			AssertEquals("IsRegistered as editable child", true, header.IsRegisteredEditableChildObject(header.Histories));
		}

		public void TestCostInputs()
		{
			LandedCostHeader header = Factory.New<LandedCostHeader>();
			AssertNotNull("Cost input", header.CostInputs);
			AssertEquals("CostInput is registered as an editible child", true, header.IsRegisteredEditableChildObject(header.CostInputs));
		}

		public void TestDefaultFromHost()
		{
			LandedCostHeader header = Factory.New<LandedCostHeader>();
			DummyLandedCostHeader headerHost = Factory.New<DummyLandedCostHeader>();
			headerHost.DateOfEntryExposed = new ZDate(2005, 10, 25);
			headerHost.LandedCostTypeExposed = "TTT";

			header.DefaultFromHost(headerHost);
			AssertEquals("DateOfEntry", header.LT_DateOfEntry, headerHost.DateOfEntryExposed);
			AssertEquals("PK", header.LT_ParentID, ((ILandedCostHeader)headerHost).PK);
			AssertEquals("TableCode", header.LT_ParentTableCode, ((ILandedCostHeader)headerHost).TableCode);
			AssertEquals("LC Type", header.LT_LandedCostType, ((ILandedCostHeader)headerHost).LandedCostType);

			header = Factory.New<LandedCostHeader>();
			header.LT_ParentID = headerHost.PK;
			header.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			header.DefaultFromHost();
			AssertEquals("DateOfEntry", header.LT_DateOfEntry, headerHost.DateOfEntryExposed);
			AssertEquals("LC Type", header.LT_LandedCostType, ((ILandedCostHeader)headerHost).LandedCostType);
		}

		public void TestSynchroniseAll()
		{
			DummyLandCostInput charge1;
			DummyLandCostInput charge2;
			DummyLandedCostHeader lCHost = SetUpPlugInHost(out charge1, out charge2);
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.SynchroniseAll(lCHost);

			AssertEquals("two input rows are created", 2, lCHeader.CostInputs.Count);
			AssertEquals("Cost Input1 Amount", charge1.AmountToDistribute.Amount, lCHeader.CostInputs[0].LI_CostAmount);
			AssertEquals("Cost input1 Currency", charge1.AmountToDistribute.Currency.Code, lCHeader.CostInputs[0].LI_RX_NKCostCurrency);
			AssertEquals("Cost Input1 Charge description", charge1.ChargeDescription, lCHeader.CostInputs[0].LI_ChargeDescription);
			AssertEquals("Cost input1 FK to ChargeCode", charge1.FKToChargeCode, lCHeader.CostInputs[0].LI_AC_ChargeCode);
			AssertEquals("IsUserEntered", false, lCHeader.CostInputs[0].LI_IsUserEntered);

			AssertEquals("Cost Input2 Amount", charge2.AmountToDistribute.Amount, lCHeader.CostInputs[1].LI_CostAmount);
			AssertEquals("Cost input2 Currency", charge2.AmountToDistribute.Currency.Code, lCHeader.CostInputs[1].LI_RX_NKCostCurrency);
			AssertEquals("Cost Input2 Charge description", charge2.ChargeDescription, lCHeader.CostInputs[1].LI_ChargeDescription);
			AssertEquals("Cost input2 FK to ChargeCode", charge2.FKToChargeCode, lCHeader.CostInputs[1].LI_AC_ChargeCode);
			AssertEquals("IsUserEntered", false, lCHeader.CostInputs[1].LI_IsUserEntered);
		}

		public void TestSynchroniseAll2()
		{
			DummyLandCostInput charge1;
			DummyLandCostInput charge2;
			DummyLandedCostHeader lCHost = SetUpPlugInHost(out charge1, out charge2);
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = lCHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			lCHeader.SynchroniseAll();

			AssertEquals("two input rows are created", 2, lCHeader.CostInputs.Count);
			AssertEquals("Cost Input1 Amount", charge1.AmountToDistribute.Amount, lCHeader.CostInputs[0].LI_CostAmount);
			AssertEquals("Cost input1 Currency", charge1.AmountToDistribute.Currency.Code, lCHeader.CostInputs[0].LI_RX_NKCostCurrency);
			AssertEquals("Cost Input1 Charge description", charge1.ChargeDescription, lCHeader.CostInputs[0].LI_ChargeDescription);
			AssertEquals("Cost input1 FK to ChargeCode", charge1.FKToChargeCode, lCHeader.CostInputs[0].LI_AC_ChargeCode);

			AssertEquals("Cost Input2 Amount", charge2.AmountToDistribute.Amount, lCHeader.CostInputs[1].LI_CostAmount);
			AssertEquals("Cost input2 Currency", charge2.AmountToDistribute.Currency.Code, lCHeader.CostInputs[1].LI_RX_NKCostCurrency);
			AssertEquals("Cost Input2 Charge description", charge2.ChargeDescription, lCHeader.CostInputs[1].LI_ChargeDescription);
			AssertEquals("Cost input2 FK to ChargeCode", charge2.FKToChargeCode, lCHeader.CostInputs[1].LI_AC_ChargeCode);
		}

		public void TestResetToOriginal()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_DateOfProcessing = new ZDateTime(2005, 10, 25);
			lCHeader.Histories.AddNew();

			lCHeader.ResetToOriginal();
			AssertEquals("Date of processing cleared", ZDateTime.Empty, lCHeader.LT_DateOfProcessing);
			AssertEquals("Histories are deleted", 0, lCHeader.Histories.Count);
		}

		public void TestLandedCostPercentageLabel()
		{
			var lCHeader = Factory.New<LandedCostHeader>();
			AssertEquals("Total Import Cost %", lCHeader.LandedCostPercentageLabel);
			AssertEquals("Total Cost %", lCHeader.LandedTotalCostPercentageLabel);
		}

		public void TestConsignee()
		{
			OrgHeader consginee = OrgHeader.New(Factory);

			DummyLandedCostHeader lcHost = Factory.New<DummyLandedCostHeader>();
			lcHost.ConsigneeExposed = consginee;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = lcHost.PK;
			lCHeader.LT_ParentTableCode = DummyLandedCostHeader.Schema.TablePrefix;

			AssertEquals(lCHeader.Consignee.PK, consginee.PK);
		}

		public void TestIHaveRequiredDocuments()
		{
			var buyer = new TestHelper(new BusinessObjectFactory()).SetAutoratingData();

			var order = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IOrder>());
			order[JobOrderHeaderSchema.JD_OrderNumber] = "Order1";
			order[JobOrderHeaderSchema.JD_OA_BuyerAddress] = buyer.MainAddress.PK;
			order[JobOrderHeaderSchema.JD_IncoTerm] = Core.Constants.IncoTerms.FreeOnBoard;
			order[JobOrderHeaderSchema.JD_RL_NKPortOfDischarge] = "AUSYD";
			order[JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo] = "AUSYD";
			order[JobOrderHeaderSchema.JD_RL_NKPortOfLoading] = "NZAKL";
			order[JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt] = "NZAKL";
			order[JobOrderHeaderSchema.JD_RX_NKOrderCurrency] = Core.Constants.CurrencyCodes.Australia;
			order[JobOrderHeaderSchema.JD_TransportMode] = Core.Constants.TransportModes.Sea;
			order[JobOrderHeaderSchema.JD_ContainerMode] = Core.Constants.ContainerModes.LCL;
			order[JobOrderHeaderSchema.JD_RN_NKCountryOfSupply] = Core.Constants.CountryCodes.NewZealand;
			order[JobOrderHeaderSchema.JD_ActualWeight] = 500m;
			order[JobOrderHeaderSchema.JD_UnitOfWeight] = Core.Constants.Weight.Kilograms;
			order[JobOrderHeaderSchema.JD_ActualVolume] = 1500m;
			order[JobOrderHeaderSchema.JD_UnitOfVolume] = Core.Constants.Volume.CubicMetres;

			var orderLine = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IOrderLine>());
			orderLine[JobOrderLineSchema.JO_JD] = order.PK;
			orderLine[JobOrderLineSchema.JO_LineNo] = new ZInt(1);
			orderLine[JobOrderLineSchema.JO_Quantity] = 10m;
			orderLine[JobOrderLineSchema.JO_F3_NKPackType] = "BOX";
			orderLine[JobOrderLineSchema.JO_ItemPrice] = 1000m;
			orderLine[JobOrderLineSchema.JO_LinePrice] = 10000m;
			orderLine[JobOrderLineSchema.JO_ActualWeight] = 200m;
			orderLine[JobOrderLineSchema.JO_UnitOfWeight] = Core.Constants.Weight.Kilograms;
			orderLine[JobOrderLineSchema.JO_ActualVolume] = 300m;
			orderLine[JobOrderLineSchema.JO_UnitOfVolume] = Core.Constants.Volume.CubicMetres;

			var lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.DefaultFromHost((ILandedCostHeader)order);
			lcHeader.SynchroniseAll();

			AssertNotNull(((IDocsAndCartageParent)lcHeader).RequiredDocumentsProvider.RequiredDocuments);
		}

		protected override BusinessObject GetNewBusinessObject() => new TestHelper(Factory).GetLCHeaderWithNoChargeRowsToDefault();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var testHelper = new TestHelper(factory);
			return testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();
		}

		DummyLandedCostHeader SetUpPlugInHost(out DummyLandCostInput charge1, out DummyLandCostInput charge2)
		{
			DummyLandedCostHeader lCHost = Factory.New<DummyLandedCostHeader>();
			lCHost.ExchangeRateHoldersExposed = Array.Empty<ILandedCostExchangeRateHolder>();

			charge1 = new DummyLandCostInput();
			charge1.AmountToDistributeExposed = new Money(500, GlbCompany.CurrentCompany.LocalCurrency);
			charge1.ChargeDescriptionExposed = "Charge1";
			charge1.ExchangeRateExposed = 1m;
			charge1.FKToChargeCodeExposed = Factory.LoadTop1(typeof(AccChargeCode), new ZQuery()).PK;

			charge2 = new DummyLandCostInput();
			charge2.AmountToDistributeExposed = new Money(400, GlbCompany.CurrentCompany.LocalCurrency);
			charge2.ChargeDescriptionExposed = "Charge2";
			charge2.ExchangeRateExposed = 0.5m;
			charge2.FKToChargeCodeExposed = ZGuid.Empty;

			DummyLandCostChargeHolder chargeHolder = new DummyLandCostChargeHolder();
			chargeHolder.ChargesToImportForLandedCostingExposed = new IDefaultLandedCostInput[] { charge1, charge2 };
			lCHost.ChargeHoldersExposed = new ILandedCostChargeHolder[] { chargeHolder };

			DummyLandedCostDistributeTo distributeTo = Factory.New<DummyLandedCostDistributeTo>();
			lCHost.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { distributeTo };

			return lCHost;
		}
	}

	[TestedType(typeof(LandedCostHeader))]
	sealed class LandedCostHeaderClusterKeyMasterMandatoryTest : ClusterKeyMasterMandatoryTest
	{
		#region Overrides of ClusterKeyEntityTest

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var testHelper = new TestHelper(Factory);

			return testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();
		}

		#endregion
	}

	[TestedType(typeof(LandedCostHeader))]
	sealed class LandedCostHeaderClusterKeyWorkerMandatoryTest : ClusterKeyWorkerMandatoryTest
	{
		#region Overrides of ClusterKeyEntityTest

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var jobDec = NewParentObject();
			var lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = jobDec.PK;
			lCHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			lCHeader.SynchroniseAll();
			return lCHeader;
		}

		#endregion

		#region Overrides of ClusterKeyWorkerMandatoryTest

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var lcHeader = ((LandedCostHeader)ClusterKeyEntityToTest);
			var jobDec = (EnterpriseBusinessObject)lcHeader.Parent;
			var invHeader = (EnterpriseBusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invHeader[JobComInvoiceHeaderSchema.JZ_JE] = jobDec.PK;
			var invLine = (EnterpriseBusinessObject)Factory.New<Integration.Customs.IBaseJobComInvoiceLine>();
			invLine[JobComInvoiceLineSchema.JI_JZ] = invHeader.PK;

			var lcInput = lcHeader.CostInputs.AddNew();
			lcInput.LI_ParentID = invLine.PK;
			lcInput.LI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			var lcHistory = lcHeader.Histories.AddNew();
			lcHistory.LH_ParentID = invLine.PK;
			lcHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			return new IClusterKeyWorker[] { lcInput, lcHistory };
		}

		protected override EnterpriseBusinessObject NewParentObject() => (EnterpriseBusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();

		protected override ZGuid? DefaultParentFk => ZGuid.Empty;

		protected override bool IsParentMandatoryButFkParentPtyOptional => true;

		#endregion
	}
}
