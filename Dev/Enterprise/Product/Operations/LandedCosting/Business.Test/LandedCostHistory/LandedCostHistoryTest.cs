using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandedCostHistory))]
	sealed class LandedCostHistoryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSupportsNoCostApportionmentItem()
		{
			var header = Factory.New<DummyLandedCostHeader>();
			header.SupportsNoCostApportionmentItem = true;
			var lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.DefaultFromHost(header);

			var landedCostHistory = lcHeader.Histories.AddNew();
			AssertEquals(true, landedCostHistory.SupportsNoCostApportionmentItem);

			landedCostHistory = Factory.New<LandedCostHistory>();
			AssertEquals(false, landedCostHistory.SupportsNoCostApportionmentItem);
		}

		public void TestLH_LandedCostHistoryLineType_Readonly()
		{
			var header = Factory.New<DummyLandedCostHeader>();
			header.SupportsNoCostApportionmentItem = true;
			var lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.DefaultFromHost(header);

			var landedCostHistory = lcHeader.Histories.AddNew();
			AssertEquals(false, landedCostHistory.LH_LandedCostHistoryLineType_Readonly);

			landedCostHistory = Factory.New<LandedCostHistory>();
			AssertEquals(true, landedCostHistory.LH_LandedCostHistoryLineType_Readonly);
		}

		public void TestIsNoCostApportionmentItem()
		{
			var landedCostHistory = Factory.New<LandedCostHistory>();
			landedCostHistory.LH_LandedCostHistoryLineType = LandedCostType.NoCostApportionmentItem;
			AssertEquals(true, landedCostHistory.IsNoCostApportionmentItem);
		}

		public void TestLH_OP()
		{
			var landedCostHistory = Factory.New<LandedCostHistory>();
			Assert("LandedCostHistory.LH_OP should be readonly to make the framework validation shut up and allow us to run and save LC for inactive products.", landedCostHistory.LH_OPInfo.ReadOnly);
		}

		public void TestLandedLineCostItems()
		{
			var landedCostHistory = Factory.New<LandedCostHistory>();
			AssertEquals("Count of items should be 0 and getter for LandedLineCostItems should not barf or be null", 0, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.LH_LandedCostGroup1);
			landedCostHistory.LH_LandedCostGroup1 = 69m;
			AssertEquals(69m, landedCostHistory.LH_LandedCostGroup1);
			AssertEquals(1, landedCostHistory.LandedLineCostItems.Count);
			AssertEquals(69m, landedCostHistory.LandedLineCostItems[LandedLineCostType.Codes.LandedCostGroup1].LZ_CostAmount);

			AssertEquals(0m, landedCostHistory.LH_LandedCostGroup2);
			landedCostHistory.LH_LandedCostGroup2 = 68m;
			AssertEquals(68m, landedCostHistory.LH_LandedCostGroup2);
			AssertEquals(2, landedCostHistory.LandedLineCostItems.Count);
			AssertEquals(68m, landedCostHistory.LandedLineCostItems[LandedLineCostType.Codes.LandedCostGroup2].LZ_CostAmount);

			AssertEquals(0m, landedCostHistory.LH_LandedCostGroup3);
			landedCostHistory.LH_LandedCostGroup3 = 67m;
			AssertEquals(67m, landedCostHistory.LH_LandedCostGroup3);
			AssertEquals(3, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.LH_LandedCostGroup4);
			landedCostHistory.LH_LandedCostGroup4 = 66m;
			AssertEquals(66m, landedCostHistory.LH_LandedCostGroup4);
			AssertEquals(4, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.LH_LandedCostGroup5);
			landedCostHistory.LH_LandedCostGroup5 = 65m;
			AssertEquals(65m, landedCostHistory.LH_LandedCostGroup5);
			AssertEquals(5, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.LH_LandedCostGroup6);
			landedCostHistory.LH_LandedCostGroup6 = 69m;
			AssertEquals(69m, landedCostHistory.LH_LandedCostGroup6);
			AssertEquals(6, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.LH_LandedCostGroupMisc);
			landedCostHistory.LH_LandedCostGroupMisc = 64m;
			AssertEquals(64m, landedCostHistory.LH_LandedCostGroupMisc);
			AssertEquals(7, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.GetLineValue("TDT"));
			landedCostHistory.SetLineValue(63m, "TDT");
			AssertEquals(63m, landedCostHistory.GetLineValue("TDT"));
			AssertEquals(8, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.GetLineValue("EXC"));
			landedCostHistory.SetLineValue(69m, "EXC");
			AssertEquals(69m, landedCostHistory.GetLineValue("EXC"));
			AssertEquals(9, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.GetLineValue("OTH"));
			landedCostHistory.SetLineValue(62m, "OTH");
			AssertEquals(62m, landedCostHistory.GetLineValue("OTH"));
			AssertEquals(10, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.GetLineValue("ST1"));
			landedCostHistory.SetLineValue(61m, "ST1");
			AssertEquals(61m, landedCostHistory.GetLineValue("ST1"));
			AssertEquals(11, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.GetLineValue("ST2"));
			landedCostHistory.SetLineValue(60m, "ST2");
			AssertEquals(60m, landedCostHistory.GetLineValue("ST2"));
			AssertEquals(12, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.GetLineValue("ST3"));
			landedCostHistory.SetLineValue(69m, "ST3");
			AssertEquals(69m, landedCostHistory.GetLineValue("ST3"));
			AssertEquals(13, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.GetLineValue("ENT"));
			landedCostHistory.SetLineValue(59m, "ENT");
			AssertEquals(59m, landedCostHistory.GetLineValue("ENT"));
			AssertEquals(14, landedCostHistory.LandedLineCostItems.Count);

			AssertEquals(0m, landedCostHistory.GetLineValue("QUA"));
			landedCostHistory.SetLineValue(58m, "QUA");
			AssertEquals(58m, landedCostHistory.GetLineValue("QUA"));
			AssertEquals(15, landedCostHistory.LandedLineCostItems.Count);
		}

		public void TestDeleteCostItemsWhenDeleteHistory()
		{
			var testHelper = new TestHelper(Factory);
			var header = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();
			var history = testHelper.GetHistory(header);
			history.SetLineValue(100m, "ENT");
			history.Delete();

			AssertEquals(0, history.LandedLineCostItems.Count);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestProxyPropertiesOfUltimateDistributee()
		{
			LandedCostHeader header = Factory.New<LandedCostHeader>();
			LandedCostHistory history = header.Histories.AddNew();
			DummyIUltimateDistributee dummy = Factory.New<DummyIUltimateDistributee>();
			history.UltimateDistributee = dummy;

			dummy.InvoiceNumberExposed = "HUKAHKJ";
			AssertEquals(dummy.InvoiceNumberExposed, history.InvoiceNumber);

			dummy.SupplierNameExposed = "ur907ter";
			AssertEquals(dummy.SupplierNameExposed, history.SupplierName);

			dummy.InvoiceCurrencyCodeExposed = "USD";
			AssertEquals(dummy.InvoiceCurrencyCodeExposed, history.InvoiceCurrencyCode);

			dummy.OrderNumberExposed = "789534789";
			AssertEquals(dummy.OrderNumberExposed, history.OrderNumber);

			dummy.OrderLineNumberExposed = 345;
			AssertEquals(dummy.OrderLineNumberExposed, history.OrderLineNumber);

			dummy.LineDescriptionExposed = "yuire7894";
			AssertEquals(dummy.LineDescriptionExposed, history.LineDescription);

			AssertEquals("HUKAHKJ_ur907ter", history.InvoiceNumberSupplierNameGroupByString);

			dummy.LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrencyExposed = 0.5m;
			AssertEquals(dummy.LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrency, history.LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrency);

			dummy.InvoiceUQExposed = "KG";
			AssertEquals(dummy.InvoiceUQExposed, history.InvoiceUQ);

			dummy.CustomsQuantityExposed = 798534;
			AssertEquals(dummy.CustomsQuantityExposed, history.CustomsQuantity);

			dummy.CustomsUQExposed = "NO";
			AssertEquals(dummy.CustomsUQExposed, history.CustomsUQ);

			dummy.TariffNumberExposed = "0000";
			AssertEquals(dummy.TariffNumberExposed, history.TariffNumber);

			dummy.CountryOfOriginCodeExposed = "XO";
			AssertEquals(dummy.CountryOfOriginCodeExposed, history.CountryOfOriginCode);

			dummy.DutyRateDescriptionExposed = "10% + 4/KG";
			AssertEquals(dummy.DutyRateDescriptionExposed, history.DutyRateDescription);

			dummy.WeightExposed = 53;
			AssertEquals(dummy.WeightExposed, history.Weight);

			dummy.WeightUQExposed = "T";
			AssertEquals(dummy.WeightUQExposed, history.WeightUQ);

			dummy.VolumeExposed = 54;
			AssertEquals(dummy.VolumeExposed, history.Volume);

			dummy.VolumeUQExposed = "L";
			AssertEquals(dummy.VolumeUQExposed, history.VolumeUQ);

			dummy.LinePriceInInvoiceCurrencyExposed = 547;
			AssertEquals(dummy.LinePriceInInvoiceCurrency, history.LinePriceInInvoiceCurrency);

			dummy.CostInLocalCurrencyExposed = 550;
			AssertEquals(dummy.CostInLocalCurrencyExposed, history.LinePriceInLocalCurrency);

			dummy.GSTVATAmountExposed = 660;
			AssertEquals(dummy.GSTVATAmount, history.EntryGSTVATAmount);
		}

		public void TestCustomsValue()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();

			DummyIUltimateDistributee dummy = Factory.New<DummyIUltimateDistributee>();
			dummy.CustomsValueExposed = 1500m;

			lCHistory.UltimateDistributee = dummy;
			AssertEquals("Customs Value", 1500m, lCHistory.CustomsValue);
		}

		public void TestRoundedFigures()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
				LandedCostHistory lCHistory = lCHeader.Histories.AddNew();
				lCHistory.LH_LandedCostGroup1 = 10.024m;
				lCHistory.LH_LandedCostGroup2 = 10.025m;
				lCHistory.LH_LandedCostGroup3 = 11.024m;
				lCHistory.LH_LandedCostGroup4 = 11.025m;
				lCHistory.LH_LandedCostGroup5 = 12.024m;
				lCHistory.LH_LandedCostGroup6 = 12.025m;
				lCHistory.LH_LandedCostGroupMisc = 13.024m;
				lCHistory.SetLineValue(10.454m, "ST1");
				lCHistory.SetLineValue(11.455m, "ST2");
				lCHistory.SetLineValue(12.454m, "ST3");
				lCHistory.SetLineValue(13.456m, "TDT");
				lCHistory.SetLineValue(14.254m, "OTH");
				lCHistory.SetLineValue(12.245m, "ENT");
				lCHistory.SetLineValue(13.999m, "QUA");

				AssertEquals("RoundedSpecialTax1", 10.45m, lCHistory.GetRoundedLineValue("ST1"));
				AssertEquals("RoundedSpecialTax2", 11.46m, lCHistory.GetRoundedLineValue("ST2"));
				AssertEquals("RoundedSpecialTax3", 12.45m, lCHistory.GetRoundedLineValue("ST3"));
				AssertEquals("RoundedOtherOrFlatDuty", 14.25m, lCHistory.GetRoundedLineValue("OTH"));
				AssertEquals("RoundedTotalDuty", 13.46m, lCHistory.GetRoundedLineValue("TDT"));
				AssertEquals("Rounded Entry Fees", 12.25m, lCHistory.GetRoundedLineValue("ENT"));
				AssertEquals("Rounded Quarantinee fees", 14m, lCHistory.GetRoundedLineValue("QUA"));

				AssertEquals("RoundedGroup1", 10.02m, lCHistory.RoundedGroup1);
				AssertEquals("RoundedGroup2", 10.03m, lCHistory.RoundedGroup2);
				AssertEquals("RoundedGroup3", 11.02m, lCHistory.RoundedGroup3);
				AssertEquals("RoundedGroup4", 11.03m, lCHistory.RoundedGroup4);
				AssertEquals("RoundedGroup5", 12.02m, lCHistory.RoundedGroup5);
				AssertEquals("RoundedGroup6", 12.03m, lCHistory.RoundedGroup6);
				AssertEquals("RoundedGroupMisc", 13.02m, lCHistory.RoundedGroupMisc);

				AssertEquals("Customs Disbursement Charges", 88.32m, lCHistory.CustomsDisbursementCharges);

				AssertEquals("Total Landing Cost", 79.17m, lCHistory.TotalLandingCost);
				AssertEquals("Total Cost", 167.49m, lCHistory.TotalCost);
			}
		}

		public void TestRoundedPerUnitFigures()
		{
			var mock = Factory.NewMoq<LandedCostHistory>();
			DummyIUltimateDistributee dummyDistributee = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee.ItemCountExposed = 17m;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = mock.Object;
			lCHistory.UltimateDistributee = dummyDistributee;
			lCHistory.LH_LT = lCHeader.PK;
			mock.Setup(m => m.TotalCost).Returns(new ZDecimal(100.00));
			AssertEquals("RoundedPerUnitTotalCost", 5.8824m, lCHistory.RoundedPerUnitTotalCost);
			mock.Setup(m => m.TotalLandingCost).Returns(new ZDecimal(250));
			AssertEquals("RoundedPerUnitLandingCost", 14.7059m, lCHistory.RoundedPerUnitLandingCost);
			mock.Setup(m => m.CustomsDisbursementCharges).Returns(new ZDecimal(300));
			AssertEquals("RoundedPerUnitCustomsDisbursementCharges", 17.6471m, lCHistory.RoundedPerUnitCustomsDisbursementCharges);
			mock.VerifyAll();
		}

		public void TestVariableGstRate()
		{
			var dummyDistributee = Factory.New<DummyUltimateDistributeeWithVariableGst>();
			dummyDistributee.ItemCountExposed = 17m;

			var lCHeader = Factory.New<LandedCostHeader>();
			var lCHistory = Factory.New<LandedCostHistory>();
			lCHistory.UltimateDistributee = dummyDistributee;
			lCHistory.LH_LT = lCHeader.PK;
			AssertEquals(true, dummyDistributee.IsCapableOfCalculatingOwnGstVatRate);
			AssertEquals("GSTVATRate", 0.69m, lCHistory.GSTVATRate);
		}

		public void TestExGSTSellPrices()
		{
			var mock = Factory.NewMoq<LandedCostHistory>();
			DummyIUltimateDistributee dummyDistributee = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee.ItemCountExposed = 17m;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = mock.Object;
			lCHistory.UltimateDistributee = dummyDistributee;
			lCHistory.LH_LT = lCHeader.PK;
			lCHistory.LH_LandedCostMarginPercent1 = 10m;
			lCHistory.LH_LandedCostMarginPercent2 = 20m;
			lCHistory.LH_LandedCostMarginPercent3 = 30m;
			mock.Setup(m => m.RoundedPerUnitTotalCost).Returns(new ZDecimal(12.9999m));
			AssertEquals("ExGSTSellPrice1", 14.29989m, lCHistory.SellPrice1ExGST);
			AssertEquals("RoundedSellPrice1ExGST", 14.30m, lCHistory.RoundedSellPrice1ExGST);

			AssertEquals("ExGSTSellPrice2", 15.59988m, lCHistory.SellPrice2ExGST);
			AssertEquals("RoundedSellPrice2ExGST", 15.60m, lCHistory.RoundedSellPrice2ExGST);

			AssertEquals("ExGSTSellPrice3", 16.89987m, lCHistory.SellPrice3ExGST);
			AssertEquals("RoundedSellPrice3ExGST", 16.90m, lCHistory.RoundedSellPrice3ExGST);
			mock.VerifyAll();
		}

		public void TestIncGSTSellPrices()
		{
			var mock = Factory.NewMoq<LandedCostHistory>();
			DummyIUltimateDistributee dummyDistributee = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee.ItemCountExposed = 17m;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = mock.Object;
			lCHistory.UltimateDistributee = dummyDistributee;
			lCHistory.LH_LT = lCHeader.PK;
			lCHistory.LH_LandedCostMarginPercent1 = 10m;
			lCHistory.LH_LandedCostMarginPercent2 = 20m;
			lCHistory.LH_LandedCostMarginPercent3 = 30m;
			mock.Setup(m => m.SellPrice1ExGST).Returns(new ZDecimal(12.135m));
			mock.Setup(m => m.SellPrice2ExGST).Returns(new ZDecimal(12.244m));
			mock.Setup(m => m.SellPrice3ExGST).Returns(new ZDecimal(12.356m));
			mock.Setup(m => m.GSTVATRate).Returns(new ZDecimal(0.1m));

			AssertEquals("PreCondition:RoundedSellPrice1ExGST", 12.14m, lCHistory.RoundedSellPrice1ExGST);
			AssertEquals("PreCondition:RoundedSellPrice2ExGST", 12.24m, lCHistory.RoundedSellPrice2ExGST);
			AssertEquals("PreCondition:RoundedSellPrice3ExGST", 12.36m, lCHistory.RoundedSellPrice3ExGST);

			AssertEquals("SellPrice1IncGST", 13.354m, lCHistory.SellPrice1IncGST);
			AssertEquals("SellPrice2IncGST", 13.464m, lCHistory.SellPrice2IncGST);
			AssertEquals("SellPrice3IncGST", 13.596m, lCHistory.SellPrice3IncGST);
			AssertEquals("RoundedSellPrice1IncGST", 13.35m, lCHistory.RoundedSellPrice1IncGST);
			AssertEquals("RoundedSellPrice2IncGST", 13.46m, lCHistory.RoundedSellPrice2IncGST);
			AssertEquals("RoundedSellPrice3IncGST", 13.60m, lCHistory.RoundedSellPrice3IncGST);
			mock.VerifyAll();
		}

		public void TestHumanReadableUltimateDistributeeCode()
		{
			DummyIUltimateDistributee dummyDistributee = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee.HumanReadableCodeExposed = "MeMeMeMeMe";

			LandedCostHistory lCHistory = Factory.New<LandedCostHistory>();
			AssertEquals("HumanReadableUltimateDistributeeCode", "", lCHistory.HumanReadableUltimateDistributeeCode);

			lCHistory.UltimateDistributee = dummyDistributee;
			AssertEquals("HumanReadableUltimateDistributeeCode", dummyDistributee.HumanReadableCode, lCHistory.HumanReadableUltimateDistributeeCode);
		}

		public void TestEffectiveMarkUpPercentages()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.MiscServ.OM_LandedCostMarginPercent1 = 10m;
			consignee.MiscServ.OM_LandedCostMarginPercent2 = 15m;
			consignee.MiscServ.OM_LandedCostMarginPercent3 = 20m;

			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			helper.DummyHeader.ConsigneeExposed = consignee;

			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();
			AssertEquals("Effective Margin1", 10m, lCHistory.EffectiveMarkUpPercentage1);
			AssertEquals("Effective Margin2", 15m, lCHistory.EffectiveMarkUpPercentage2);
			AssertEquals("Effective Margin3", 20m, lCHistory.EffectiveMarkUpPercentage3);

			lCHistory.LH_LandedCostMarginPercent1 = 20m;
			lCHistory.LH_LandedCostMarginPercent2 = 25m;

			AssertEquals("Effective Margin1", 20m, lCHistory.EffectiveMarkUpPercentage1);
			AssertEquals("Effective Margin2", 25m, lCHistory.EffectiveMarkUpPercentage2);
			AssertEquals("Effective Margin3", 0m, lCHistory.EffectiveMarkUpPercentage3);
		}

		public void TestDateOfProcessing()
		{
			LandedCostHistory lCHistory = Factory.New<LandedCostHistory>();
			AssertEquals("Date of processing", ZDateTime.Empty, lCHistory.DateOfProcessing);

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHistory.LH_LT = lCHeader.PK;
			lCHeader.LT_DateOfProcessing = new ZDateTime(2005, 10, 25);
			AssertEquals("Date of processing", lCHeader.LT_DateOfProcessing, lCHistory.DateOfProcessing);
		}

		public void TestUniqueReferenceNumber()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();

			AssertEquals("Unique Reference number", "", lCHistory.UniqueReferenceNumber);

			DummyLandedCostHeader lCHost = Factory.New<DummyLandedCostHeader>();
			lCHost.UniqueReferenceNumberExposed = "Xcvcvcv";
			lCHeader.LT_ParentID = lCHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			AssertEquals("Unique Reference number", lCHost.UniqueReferenceNumber, lCHistory.UniqueReferenceNumber);
		}

		public void TestUltimateDistributee()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();

			BusinessObject invoiceLine = (BusinessObject)Factory.New<Integration.Customs.IBaseJobComInvoiceLine>();
			lCHistory.LH_ParentID = invoiceLine.PK;
			lCHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			AssertEquals("UltimateDistributee", invoiceLine, lCHistory.UltimateDistributee);

			LandedCostHistory lCHistory2 = lCHeader.Histories.AddNew();
			BusinessObject orderLine = (BusinessObject)Factory.New<Freight.Integration.Forwarding.IOrderLine>();
			lCHistory2.LH_ParentID = orderLine.PK;
			lCHistory2.LH_ParentTableCode = JobOrderLineSchema.Constants.Prefix;
			AssertEquals("OrderLine is loaded", orderLine, lCHistory2.UltimateDistributee);
		}

		public void TestLCHeader()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = Factory.New<LandedCostHistory>();
			lCHistory.LH_LT = lCHeader.PK;
			AssertEquals("LC Header", lCHeader, lCHistory.LCHeader);
		}

		public void TestDefaultValues()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Afghanistan;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_GC = company.PK;
			lCHeader.LT_LandedCostType = LandedCostType.Actual;
			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();

			AssertEquals("LC History Country of Entry", Core.Constants.CountryCodes.Afghanistan, lCHistory.LH_RN_NKCountryOfEntry);
		}

		public void TestDutiesAndTaxesAndPerUnitDutiesAndTaxes()
		{
			DummyIUltimateDistributee distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.ItemCountExposed = 333;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();
			lCHistory.UltimateDistributee = distributee;

			lCHistory.SetLineValue(1500m, "TDT");
			lCHistory.SetLineValue(200m, "ST1");
			lCHistory.SetLineValue(200m, "ST2");
			lCHistory.SetLineValue(100m, "ST3");
			lCHistory.SetLineValue(500m, "OTH");
			AssertEquals("Customs Disbursement Charges", 2500m, lCHistory.CustomsDisbursementCharges);
			AssertEquals("PerUnitCustomsDisbursementCharges", 7.5075m, lCHistory.PerUnitCustomsDisbursementCharges, 0.00001m);
			AssertEquals("RoundedPerUnitCustomsDisbursementCharges", 7.5075m, lCHistory.RoundedPerUnitCustomsDisbursementCharges);
		}

		public void TestTotalImportCostAndPerUnitImportCost()
		{
			DummyIUltimateDistributee distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.ItemCountExposed = 33;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();
			lCHistory.UltimateDistributee = distributee;

			lCHistory.LH_LandedCostGroup1 = 100m;
			lCHistory.LH_LandedCostGroup2 = 200m;
			lCHistory.LH_LandedCostGroup3 = 300m;
			lCHistory.LH_LandedCostGroup4 = 400m;
			lCHistory.LH_LandedCostGroup5 = 500m;
			AssertEquals("TotalLandingCost", 1500m, lCHistory.TotalLandingCost);
			AssertEquals("PerUnitCost", 45.454545m, lCHistory.PerUnitLandingCost, 0.000001m);
			AssertEquals("RoundedPerUnitCost", 45.4545m, lCHistory.RoundedPerUnitLandingCost);
		}

		public void TestCostAndPerUnitTotalCost()
		{
			DummyIUltimateDistributee distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.ItemCountExposed = 37;
			distributee.CostInLocalCurrencyExposed = 2000m;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();
			lCHistory.UltimateDistributee = distributee;

			lCHistory.SetLineValue(1500m, "TDT");
			lCHistory.SetLineValue(200m, "ST1");
			lCHistory.LH_LandedCostGroup1 = 100m;
			lCHistory.LH_LandedCostGroup2 = 200m;
			lCHistory.LH_LandedCostMarginPercent1 = 5.00;
			AssertEquals("TotalCost", 4000m, lCHistory.TotalCost);
			AssertEquals("TotalCostWithMarkup1Applied", 4200m, lCHistory.TotalCostWithMarkup1Applied);
			AssertEquals("PerUnitTotalCost", 108.1081m, lCHistory.PerUnitTotalCost, 0.00001m);
			AssertEquals("RoundedPerUnitTotalCost", 108.1081m, lCHistory.RoundedPerUnitTotalCost);
		}

		public void TestCostsFromInvoiceLine()
		{
			DummyIUltimateDistributee distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.ItemCountExposed = 100;
			distributee.CostInLocalCurrencyExposed = 4000m;
			distributee.UnitPriceInInvoiceCurrencyExposed = 20m;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();
			lCHistory.UltimateDistributee = distributee;

			AssertEquals("InvoiceCostInLocalCurrency", 4000m, lCHistory.InvoiceCostInLocalCurrency);
			AssertEquals("UnitPriceInLocalCurrency", 40m, lCHistory.UnitPriceInLocalCurrency);
			AssertEquals("UnitPriceInInvoiceCurrency", 20m, lCHistory.UnitPriceInInvoiceCurrency);
		}

		public void TestDutiesAndTaxesIncludeEntryFee()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();
			DummyIUltimateDistributee dummyDistributee = Factory.New<DummyIUltimateDistributee>();

			lCHistory.SetLineValue(12.544m, "ENT");
			AssertEquals("Customs Disbursement Charges", 12.54m, lCHistory.CustomsDisbursementCharges);

			lCHistory.SetLineValue(12.545m, "ENT");
			AssertEquals("Customs Disbursement Charges", 12.55m, lCHistory.CustomsDisbursementCharges);
		}

		public void TestLandedCostPercentage()
		{
			var distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.ItemCountExposed = 100;
			distributee.CostInLocalCurrencyExposed = 4000m;

			var lCHeader = Factory.New<LandedCostHeader>();
			var lCHistory = lCHeader.Histories.AddNew();
			lCHistory.UltimateDistributee = distributee;

			lCHistory.LH_LandedCostGroup1 = 2000m;
			lCHistory.LH_LandedCostGroup2 = 1200;
			lCHistory.LH_LandedCostGroup3 = 0m;
			lCHistory.LH_LandedCostGroup4 = 0m;
			lCHistory.LH_LandedCostGroup5 = 0m;
			lCHistory.LH_LandedCostGroup6 = 0m;

			AssertEquals("InvoiceCostInLocalCurrency", 4000m, lCHistory.InvoiceCostInLocalCurrency);
			AssertEquals("TotalCost", 3200m, lCHistory.TotalLandingCost);
			AssertEquals("LandedCostPercentage " + System.Environment.NewLine + lCHistory.TotalLandingCost + System.Environment.NewLine + lCHistory.InvoiceCostInLocalCurrency, 80.0m, lCHistory.LandedCostPercentage);

			lCHistory.SetLineValue(45m, "ENT");
			lCHistory.SetLineValue(160m, "TDT");
			AssertEquals("LandedTotalCostPercentage " + System.Environment.NewLine + lCHistory.TotalLandingCost + System.Environment.NewLine + lCHistory.InvoiceCostInLocalCurrency, 85.125m, lCHistory.LandedTotalCostPercentage);
		}

		public void TestIfInvoiceCostInLocalCurrencyIsZeroLandedCostPercentageReturnsZero()
		{
			DummyIUltimateDistributee distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.ItemCountExposed = 100;
			distributee.InvoiceCurrencyCodeExposed = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			distributee.LinePriceInInvoiceCurrencyExposed = 0m;
			distributee.LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrencyExposed = 0.5m;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory = lCHeader.Histories.AddNew();
			lCHistory.UltimateDistributee = distributee;

			lCHistory.LH_LandedCostGroup1 = 1000m;
			lCHistory.LH_LandedCostGroup2 = 0m;
			lCHistory.LH_LandedCostGroup3 = 0m;
			lCHistory.LH_LandedCostGroup4 = 0m;
			lCHistory.LH_LandedCostGroup5 = 0m;
			lCHistory.LH_LandedCostGroup6 = 0m;

			AssertEquals("InvoiceCostInLocalCurrency", 0m, lCHistory.InvoiceCostInLocalCurrency);
			AssertEquals("TotalCost", 1000m, lCHistory.TotalLandingCost);
			AssertEquals("LandedCostPercentage", true, lCHistory.LandedCostPercentage.IsEmpty);
		}

		public void TestGstRateforOrderLine_GB()
		{
			var accTaxRate = Factory.New<AccTaxRate>();
			accTaxRate.AT_Code = "GST";
			accTaxRate.AT_IsActive = true;
			accTaxRate.AT_Type = AccTaxRate.Types.Rated;
			accTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			accTaxRate.SetRateNumerator_ForTestOnly(20);

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (AccountingConfigurationRegistry.Instance.MainGSTTaxID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, accTaxRate.PK.ToGuid()))
			{
				var lcHeader = Factory.New<LandedCostHeader>();
				lcHeader.LT_GC = company.PK;
				var lcHistory = lcHeader.Histories.AddNew();
				var orderLine = (BusinessObject)Factory.New<Freight.Integration.Forwarding.IOrderLine>();
				lcHistory.LH_ParentID = orderLine.PK;
				lcHistory.LH_ParentTableCode = JobOrderLineSchema.Constants.Prefix;

				AssertEquals("GSTVATRate", 0.2m, lcHistory.GSTVATRate);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new TestHelper(factory);
			var header = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			return helper.GetHistory(header);
		}
	}

	[TestedType(typeof(LandedCostHistory))]
	sealed class LandedCostHistoryClusterKeyWorkerMandatoryTest : ClusterKeyWorkerMandatoryTest
	{
		#region Overrides of ClusterKeyEntityTest

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var parent = (LandedCostHeader)NewParentObject();
			var jobDec = (EnterpriseBusinessObject)parent.Parent;
			var invHeader = (EnterpriseBusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invHeader[JobComInvoiceHeaderSchema.JZ_JE] = jobDec.PK;
			var invLine = (EnterpriseBusinessObject)Factory.New<Integration.Customs.IBaseJobComInvoiceLine>();
			invLine[JobComInvoiceLineSchema.JI_JZ] = invHeader.PK;

			var lcHistory = parent.Histories.AddNew();
			lcHistory.LH_ParentID = invLine.PK;
			lcHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			return lcHistory;
		}

		#endregion

		#region Overrides of ClusterKeyWorkerMandatoryTest

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var landedLineCostItem = ((LandedCostHistory)ClusterKeyEntityToTest).LandedLineCostItems.AddNew();
			landedLineCostItem.LZ_CostType = "ABC";
			landedLineCostItem.LZ_CostAmount = 69m;

			return new IClusterKeyWorker[]
			{
				landedLineCostItem,
			};
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var jobDec = (EnterpriseBusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			var lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = jobDec.PK;
			lCHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			lCHeader.SynchroniseAll();
			return lCHeader;
		}

		#endregion
	}
}
