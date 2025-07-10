using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVItem))]
	internal class CusUSLVItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<CusUSLVClearance>()
				.CusUSLVConsignments
				.AddNew()
				.CusUSLVItems
				.AddNew();
		}

		public void TestULI_TariffFormattedMaxLength()
		{
			var cusUSLVItem = Factory.NewWithValidTestData<CusUSLVItem>();
			AssertEquals(12, cusUSLVItem.ULI_TariffFormattedInfo.MaxLength);
		}

		public void TestULI_RX_NKCurrEXRate()
		{
			var cnyCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");
			cnyCurrency.SetCustomsRate(ZDateTime.Now, ZDateTime.Now, 1m);

			var cadCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CAD");
			cadCurrency.SetCustomsRate(ZDateTime.Now, ZDateTime.Now, 7m);
			cadCurrency.SetCustomsRate(ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(-1), 6m);

			var cusUSLVClearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var cusUSLVConsignment = cusUSLVClearance.CusUSLVConsignments.AddNew();
			cusUSLVConsignment.ULB_HouseBillIssuerSCAC = "SCA1";
			var cusUSLVItem = cusUSLVConsignment.CusUSLVItems.AddNew();
			cusUSLVItem.ULI_Tariff = "Tif1";
			AssertEquals("Rate is 1 due to default Currency is USD", 1m, cusUSLVItem.ULI_RX_NKCurrEXRate);

			cusUSLVItem.ULI_RX_NKCurrency = "CNY";
			AssertEquals("rate is 0 due to ArrivalDate is empty", 0m, cusUSLVItem.ULI_RX_NKCurrEXRate);

			cusUSLVClearance.ULH_DepartureDate = ZDate.Today;
			AssertEquals("CNY's Rate", 1m, cusUSLVItem.ULI_RX_NKCurrEXRate);

			cusUSLVItem.ULI_RX_NKCurrency = "CAD";
			AssertEquals("CAD's Rate", 7m, cusUSLVItem.ULI_RX_NKCurrEXRate);

			cusUSLVClearance.ULH_DepartureDate = ZDate.Today.AddDays(-1);
			AssertEquals("CAD's Rate from yesterday", 6m, cusUSLVItem.ULI_RX_NKCurrEXRate);
		}

		public void TestPropertiesDecimalPlaces()
		{
			var cusUSLVItem = Factory.NewWithValidTestData<CusUSLVItem>();

			cusUSLVItem.ULI_RX_NKCurrency = "CNY";
			AssertEquals(2, cusUSLVItem.GoodsValueDecimals);

			cusUSLVItem.ULI_RX_NKCurrency = "TWD";
			AssertEquals(0, cusUSLVItem.GoodsValueDecimals);

			cusUSLVItem.ULI_RX_NKCurrency = "ABC";
			AssertEquals(2, cusUSLVItem.GoodsValueDecimals);

			var tester = new DecimalPlacesAttributeTester(cusUSLVItem);
			tester.CheckSetter(new List<string>() { nameof(cusUSLVItem.ULI_GoodsValue) }, nameof(cusUSLVItem.GoodsValueDecimals));
			tester.CheckConstant(new List<string>() { nameof(cusUSLVItem.ULI_RX_NKCurrEXRate) }, nameof(cusUSLVItem.ExchangeRateDecimals), 6);
		}

		public void TestSetULI_Tariff()
		{
			var cusUSLVItem = Factory.NewWithValidTestData<CusUSLVItem>();
			cusUSLVItem.ULI_Tariff = "1234.5678.9";
			Factory.Save();

			cusUSLVItem.ReloadSafe();
			AssertEquals("123456789", cusUSLVItem.ULI_Tariff);
			AssertEquals("1234.56.789", cusUSLVItem.ULI_TariffFormatted);

			cusUSLVItem.ULI_TariffFormatted = "9.8765.4321";
			Factory.Save();

			cusUSLVItem.ReloadSafe();
			AssertEquals("987654321", cusUSLVItem.ULI_Tariff);
			AssertEquals("9876.54.321", cusUSLVItem.ULI_TariffFormatted);
		}

		[TestDate(2019, 10, 10)]
		public void TestOGARequirementCalculator()
		{
			const string tariffNum = "8923894890";
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNum;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "OM1";

			Item.ULI_Tariff = tariffNum;
			var calculator = Item.OGARequirementCalculator;

			var type = calculator.GetType();

			var getImportTariff = type.GetField("getImportTariff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(calculator) as Func<USCTariff>;
			var getImportSupTariff = type.GetField("getImportSupTariff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(calculator) as Func<USCTariff>;
			var getDutyDate = type.GetField("getDutyDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(calculator) as Func<ZDateTime>;
			CombineAssertions(() =>
			{
				AssertEquals(tariffNum, getImportTariff().UE_Tariff);
				AssertEquals(null, getImportSupTariff());
				AssertEquals(new DateTime(2019, 10, 10), getDutyDate());
			});
		}

		[TestDate(2019, 10, 10)]
		public void TestPGARequirementIndicator()
		{
			const string tariffNum = "8923894890";
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNum;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "OM1";

			Item.ULI_Tariff = tariffNum;
			var indicator = Item.PGARequirementIndicator;

			var type = indicator.GetType();

			var getTariff = type.GetField("getTariff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(indicator) as Func<USCTariff>;
			var getSupTariff = type.GetField("getSupTariff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(indicator) as Func<USCTariff>;
			var getEffectiveDate = type.GetField("getEffectiveDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(indicator) as Func<ZDateTime>;
			var hasApplicableEntryType = type.GetField("hasApplicableEntryType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(indicator) as Func<ZString, ZBool>;
			CombineAssertions(() =>
			{
				AssertEquals(tariffNum, getTariff().UE_Tariff);
				AssertEquals(null, getSupTariff());
				AssertEquals(new DateTime(2019, 10, 10), getEffectiveDate());
				AssertEquals(true, hasApplicableEntryType(""));
			});
		}

		public void TestSetULI_PartNo()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			Item.Consignment.ConsigneeOrgPK = importer.PK;

			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.RelatedOrganisations.AddOwner(importer);

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1234567890";
			pivot.CD_UC_NKCountryOfOrigin = "AU";
			pivot.CD_ADDApplicable = false;
			pivot.CD_CVDApplicable = false;

			AssertNull("Should default to null", Item.Product);

			AssertNullOrEmpty(Item.ULI_GoodsDescription);
			AssertNullOrEmpty(Item.ULI_Tariff);
			AssertNullOrEmpty(Item.ULI_RN_NKCountryOfOrigin);
			Assert(Item.ULI_AntiDumping);
			Assert(Item.ULI_Countervailing);

			Item.ULI_PartNo = part.OP_PartNum;

			AssertEquals("Should find the matching part", part.PK, Item.Product.PK);
			AssertEquals(part.OP_Desc, Item.ULI_GoodsDescription);

			AssertEquals(pivot.CI_TariffNum, Item.ULI_Tariff);
			AssertEquals(pivot.CD_UC_NKCountryOfOrigin, Item.ULI_RN_NKCountryOfOrigin);
			AssertEquals(pivot.CD_ADDApplicable, Item.ULI_AntiDumping);
			AssertEquals(pivot.CD_CVDApplicable, Item.ULI_Countervailing);
		}

		public void TestSetPartNo_UpdatesPGADisclaimReasons()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			Item.Consignment.ConsigneeOrgPK = importer.PK;

			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.RelatedOrganisations.AddOwner(importer);

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1234567890";
			pivot.CD_ACEFDADisclaimReason = "A";
			pivot.CD_AMSDisclaimReason = "B";
			pivot.CD_NOPDisclaimReason = "R";
			pivot.CD_APHISDisclaimReason = "C";
			pivot.CD_CPSCDisclaimReason = "D";
			pivot.CD_DEADisclaimReason = "E";
			pivot.CD_FWSDisclaimReason = "F";
			pivot.CD_LaceyActDisclaimReason = "G";
			pivot.CD_NHTSADisclaimReason = "H";
			pivot.CD_NMFS370DisclaimReason = "I";
			pivot.CD_NMFSAMRDisclaimReason = "J";
			pivot.CD_NMFSHMSDisclaimReason = "K";
			pivot.CD_ODSDisclaimReason = "L";
			pivot.CD_OMCDisclaimReason = "M";
			pivot.CD_PSTDisclaimReason = "N";
			pivot.CD_TSCADisclaimReason = "O";
			pivot.CD_TTBDisclaimReason = "P";
			pivot.CD_VNEDisclaimReason = "Q";

			Item.ULI_PartNo = part.OP_PartNum;

			AssertEquals("Should find the matching part", part.PK, Item.Product.PK);

			CombineAssertions("PGA Disclaim Reasons should be copied from Pivot to Item", () =>
			{
				AssertEquals("ACEDFA", pivot.CD_ACEFDADisclaimReason, item.ACEFDAWrapper.DisclaimReason);
				AssertEquals("AMS", pivot.CD_AMSDisclaimReason, item.AMSWrapper.DisclaimReason);
				AssertEquals("NOP", pivot.CD_NOPDisclaimReason, item.NOPWrapper.DisclaimReason);
				AssertEquals("APHIS", pivot.CD_APHISDisclaimReason, item.APHISWrapper.DisclaimReason);
				AssertEquals("CPSC", pivot.CD_CPSCDisclaimReason, item.CPSCWrapper.DisclaimReason);
				AssertEquals("DEA", pivot.CD_DEADisclaimReason, item.DEAWrapper.DisclaimReason);
				AssertEquals("FWS", pivot.CD_FWSDisclaimReason, item.FWSWrapper.DisclaimReason);
				AssertEquals("Lacey", pivot.CD_LaceyActDisclaimReason, item.LaceyActWrapper.DisclaimReason);
				AssertEquals("NHTSA", pivot.CD_NHTSADisclaimReason, item.NHTSAWrapper.DisclaimReason);
				AssertEquals("NMFS370", pivot.CD_NMFS370DisclaimReason, item.NMFS370Wrapper.DisclaimReason);
				AssertEquals("NMFSAMR", pivot.CD_NMFSAMRDisclaimReason, item.NMFSAMRWrapper.DisclaimReason);
				AssertEquals("NMFSHMS", pivot.CD_NMFSHMSDisclaimReason, item.NMFSHMSWrapper.DisclaimReason);
				AssertEquals("ODS", pivot.CD_ODSDisclaimReason, item.ODSWrapper.DisclaimReason);
				AssertEquals("OMC", pivot.CD_OMCDisclaimReason, item.OMCWrapper.DisclaimReason);
				AssertEquals("PST", pivot.CD_PSTDisclaimReason, item.PSTWrapper.DisclaimReason);
				AssertEquals("TSCA", pivot.CD_TSCADisclaimReason, item.TSCAWrapper.DisclaimReason);
				AssertEquals("TTB", pivot.CD_TTBDisclaimReason, item.TTBWrapper.DisclaimReason);
				AssertEquals("VNE", pivot.CD_VNEDisclaimReason, item.VNEWrapper.DisclaimReason);
			});
		}

		public void TestClearUpPGAsWhenTariffChanged()
		{
			var pga1 = Item.CusUSLVItemPGAs.AddNew();
			pga1.ULP_Agency = "NMF";
			pga1.ULP_AgencyProgram = "370";
			pga1.ULP_Indicator = "C";
			pga1.ULP_DisclaimReason = "B";
			var pga2 = Item.CusUSLVItemPGAs.AddNew();
			pga2.ULP_Agency = "OMC";
			pga2.ULP_AgencyProgram = "OMC";
			pga2.ULP_Indicator = "C";
			pga2.ULP_DisclaimReason = "A";
			Item.ULI_Tariff = "8564456451";

			AssertEquals(2, Item.CusUSLVItemPGAs.Count);
			AssertEquals(2, Item.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().Count(p => !string.IsNullOrEmpty(p.DisclaimReason)));

			Item.ULI_Tariff = "5234456651";
			AssertEquals(0, Item.CusUSLVItemPGAs.Count);
			AssertEquals(0, Item.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().Count(p => !string.IsNullOrEmpty(p.DisclaimReason)));
		}

		public void TestDelete()
		{
			Item.PartSyncManager.Enabled = true;
			Item.Delete();
			Assert(!Item.PartSyncManager.Enabled);
		}

		#region ICusEntryLine

		public void TestCountryOfOrigin()
		{
			Item.ULI_RN_NKCountryOfOrigin = "FR";
			AssertEquals("Country of Origin", "FR", ((US.Business.MessageBuilders.ICusEntryLine)Item).CountryOfOrigin);
		}

		public void TestCL_CustomsValueWhenLineCurrencyIsUSD()
		{
			Item.ULI_GoodsValue = 0.0m;
			AssertEquals("Actual Customs Value", 0m, item.ULI_GoodsValueInUSD);
			AssertEquals("Customs Value", 0m, (item as US.Business.MessageBuilders.ICusEntryLine).CL_CustomsValue);

			Item.ULI_GoodsValue = 0.3m;
			AssertEquals("Actual Customs Value", 0.3m, item.ULI_GoodsValueInUSD);
			AssertEquals("Customs Value", 1m, (item as US.Business.MessageBuilders.ICusEntryLine).CL_CustomsValue);

			Item.ULI_GoodsValue = 0.6m;
			AssertEquals("Actual Customs Value", 0.6m, item.ULI_GoodsValueInUSD);
			AssertEquals("Customs Value", 1m, (item as US.Business.MessageBuilders.ICusEntryLine).CL_CustomsValue);

			Item.ULI_GoodsValue = 1.2m;
			AssertEquals("Actual Customs Value", 1.2m, item.ULI_GoodsValueInUSD);
			AssertEquals("Customs Value", 1m, (item as US.Business.MessageBuilders.ICusEntryLine).CL_CustomsValue);
		}

		public void TestCL_CustomsValueWhenLineCurrencyIsNotUSD()
		{
			var cnyCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");
			cnyCurrency.SetCustomsRate(ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), 3m);

			var eurCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");
			eurCurrency.SetCustomsRate(ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), 0.5m);

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();

			clearance.ULH_DepartureDate = ZDate.Today;

			item.ULI_GoodsValue = 0.0m;

			item.ULI_RX_NKCurrency = "CNY";
			AssertEquals("Actual Customs Value", 0m, item.ULI_GoodsValueInUSD);
			AssertEquals("Customs Value", 0m, (item as US.Business.MessageBuilders.ICusEntryLine).CL_CustomsValue);

			item.ULI_RX_NKCurrency = "EUR";
			AssertEquals("Actual Customs Value", 0m, item.ULI_GoodsValueInUSD);
			AssertEquals("Customs Value", 0m, (item as US.Business.MessageBuilders.ICusEntryLine).CL_CustomsValue);

			item.ULI_GoodsValue = 0.3m;

			item.ULI_RX_NKCurrency = "CNY";
			AssertEquals("Actual Customs Value", 0.1m, item.ULI_GoodsValueInUSD);
			AssertEquals("Customs Value", 1m, (item as US.Business.MessageBuilders.ICusEntryLine).CL_CustomsValue);

			item.ULI_RX_NKCurrency = "EUR";
			AssertEquals("Actual Customs Value", 0.6m, item.ULI_GoodsValueInUSD);
			AssertEquals("Customs Value", 1m, (item as US.Business.MessageBuilders.ICusEntryLine).CL_CustomsValue);

			item.ULI_GoodsValue = 0.6m;

			item.ULI_RX_NKCurrency = "CNY";
			AssertEquals("Actual Customs Value", 0.2m, item.ULI_GoodsValueInUSD);
			AssertEquals("Customs Value", 1m, (item as US.Business.MessageBuilders.ICusEntryLine).CL_CustomsValue);

			item.ULI_RX_NKCurrency = "EUR";
			AssertEquals("Actual Customs Value", 1.2m, item.ULI_GoodsValueInUSD);
			AssertEquals("Customs Value", 1m, (item as US.Business.MessageBuilders.ICusEntryLine).CL_CustomsValue);
		}

		public void TestCL_LineNumber()
		{
			Item.LineNumber = 1;
			AssertEquals("Line Number", (short)1, ((US.Business.MessageBuilders.ICusEntryLine)Item).CL_LineNumber);
		}

		public void TestTariff()
		{
			Item.ULI_Tariff = "123456789";
			AssertEquals("Tariff", "123456789", ((US.Business.MessageBuilders.ICusEntryLine)Item).Tariff);
		}

		public void TestDescription()
		{
			Item.ULI_GoodsDescription = "Stuff";
			AssertEquals("Goods Description", "Stuff", ((US.Business.MessageBuilders.ICusEntryLine)Item).Description);
		}

		#endregion

		#region IGovernmentAgencies

		public void TestCommercialDescription()
		{
			Item.ULI_GoodsDescription = "Stuff";

			AssertEquals("Commercial Description", "Stuff", ((US.Business.MessageBuilders.ICusEntryLine)Item).CommercialDescription);
		}

		public void TestShouldIncludePGAInMessage_WhenPgaCodeIsNotLacey_ReturnsTrue()
		{
			var pgaCode = "TTB";
			var isCertified = true;

			AssertEquals(true, ((US.Business.MessageBuilders.ICusEntryLine)Item).ShouldIncludePGAInMessage(isCertified, pgaCode));
		}

		public void TestShouldIncludePGAInMessage_WhenPgaCodeIsLacey_ReturnsFalse()
		{
			var pgaCode = "Lacey";
			var isCertified = true;

			AssertEquals(false, ((US.Business.MessageBuilders.ICusEntryLine)Item).ShouldIncludePGAInMessage(isCertified, pgaCode));
		}

		public void TestNMFSLine()
		{
			AssertEquals(((IGovernmentAgencies)Item).NMFS370Lines.Count(), 0);
			AssertEquals(((IGovernmentAgencies)Item).NMFS370Lines.GetType(), typeof(List<INMFSLine>));
			AssertEquals(((IGovernmentAgencies)Item).NMFSAMRLines.Count(), 0);
			AssertEquals(((IGovernmentAgencies)Item).NMFSAMRLines.GetType(), typeof(List<INMFSLine>));
			AssertEquals(((IGovernmentAgencies)Item).NMFSCOALines.Count(), 0);
			AssertEquals(((IGovernmentAgencies)Item).NMFSCOALines.GetType(), typeof(List<INMFSLine>));
			AssertEquals(((IGovernmentAgencies)Item).NMFSHMSLines.Count(), 0);
			AssertEquals(((IGovernmentAgencies)Item).NMFSHMSLines.GetType(), typeof(List<INMFSLine>));
			AssertEquals(((IGovernmentAgencies)Item).NMFSSIMLines.Count(), 0);
			AssertEquals(((IGovernmentAgencies)Item).NMFSSIMLines.GetType(), typeof(List<INMFSLine>));
		}

		#endregion

		#region IGovernmentAgenciesIndicators

		public void TestODS()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.ODS).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.ODS).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).ODSDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).ODSIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).ODSDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).ODSIndicator);
		}

		public void TestFSIS()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FSIS).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FSIS).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).FSISDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).FSISIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).FSISDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).FSISIndicator);
		}

		public void TestVNE()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.VNE).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.VNE).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).VNEDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).VNEIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).VNEDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).VNEIndicator);
		}

		public void TestPST()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.PST).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.PST).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim program not empty", "PS1", ((IGovernmentAgenciesIndicators)Item).PSTDisclaimProgram);
			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).PSTDisclaimReason);
			AssertEquals("Disclaim indicator empty", "", ((IGovernmentAgenciesIndicators)Item).PSTIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim program not empty", "PS1", ((IGovernmentAgenciesIndicators)Item).PSTDisclaimProgram);
			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).PSTDisclaimReason);
			AssertEquals("Disclaim indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).PSTIndicator);
		}

		public void TestNMFS370()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes._370).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes._370).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).NMFS370DisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).NMFS370Indicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).NMFS370DisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).NMFS370Indicator);
		}

		public void TestNMFSAMR()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.AMR).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.AMR).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).NMFSAMRDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).NMFSAMRIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).NMFSAMRDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).NMFSAMRIndicator);
		}

		public void TestNMFSHMS()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.HMS).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.HMS).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).NMFSHMSDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).NMFSHMSIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).NMFSHMSDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).NMFSHMSIndicator);
		}

		public void TestACEFDA()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FDA).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FDA).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).ACEFDADisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).ACEFDAIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).ACEFDADisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).ACEFDAIndicator);
		}

		public void TestTSCA()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.TSCA).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.TSCA).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).TSCADisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).TSCAIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).TSCADisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).TSCAIndicator);
		}

		public void TestAMS()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.AMS).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.AMS).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).AMSDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).AMSIndicator);
			AssertEquals("Agency Program", "MO8", ((IGovernmentAgenciesIndicators)Item).AMSDisclaimProgram);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).AMSDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).AMSIndicator);
		}

		public void TestNOP()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.NOP).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.NOP).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).NOPDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).NOPIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).NOPDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).NOPIndicator);
		}

		public void TestNHTSA()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.NHTSA).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.NHTSA).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).NHTSADisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).NHTSAIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).NHTSADisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).NHTSAIndicator);
		}

		public void TestTTB()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.TTB).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.TTB).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).TTBDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).TTBIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).TTBDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).TTBIndicator);
		}

		public void TestOMC()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.OMC).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.OMC).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).OMCDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).OMCIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).OMCDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).OMCIndicator);
		}

		public void TestLaceyAct()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.Lacey).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.Lacey).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).LaceyActDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).LaceyActIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).LaceyActDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).LaceyActIndicator);
		}

		public void TestAPHIS()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.APHIS).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.APHIS).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).APHISDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).APHISIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).APHISDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).APHISIndicator);
		}

		public void TestFWS()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FWS).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FWS).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).FWSDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).FWSIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).FWSDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).FWSIndicator);
		}

		public void TestCPSC()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.CPSC).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.CPSC).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).CPSCDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).CPSCIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).CPSCDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).CPSCIndicator);
		}

		public void TestDEA()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.DEA).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.DEA).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).DEADisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).DEAIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).DEADisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).DEAIndicator);
		}

		public void TestHFC()
		{
			var pga = Item.CusUSLVItemPGAs.AddNew();

			pga.ULP_Agency = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.HFC).Agency;
			pga.ULP_AgencyProgram = Item.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.HFC).Program;
			pga.ULP_ULI = Item.PK;

			AssertEquals("Disclaim reason empty", "", ((IGovernmentAgenciesIndicators)Item).HFCDisclaimReason);
			AssertEquals("Indicator empty", "", ((IGovernmentAgenciesIndicators)Item).HFCIndicator);

			pga.ULP_Indicator = "C";
			pga.ULP_DisclaimReason = "A";

			AssertEquals("Disclaim reason not empty", "A", ((IGovernmentAgenciesIndicators)Item).HFCDisclaimReason);
			AssertEquals("Indicator not empty", "C", ((IGovernmentAgenciesIndicators)Item).HFCIndicator);
		}

		#endregion

		#region IInvoiceLinePartDetails

		public void TestIInvoiceLinePartDetailsMembers()
		{
			var cusUSLVItem = Factory.New<CusUSLVItem>();
			IInvoiceLinePartDetails partDetails = cusUSLVItem;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, partDetails.CustomsCountryCode);
			AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
		}

		public void TestRefreshPartSyncManagerActiveDeciderPK()
		{
			var cusUSLVItem = Factory.New<CusUSLVItem>();
			AssertEquals(ZGuid.Empty, ((IInvoiceLinePartDetails)cusUSLVItem).PartSyncManagerActiveDeciderPK);

			var cusUSLVConsignment = Factory.New<CusUSLVConsignment>();
			cusUSLVItem.ULI_ULB = cusUSLVConsignment.PK;
			AssertEquals(ZGuid.Empty, ((IInvoiceLinePartDetails)cusUSLVItem).PartSyncManagerActiveDeciderPK);

			var cusUSLVClearance = Factory.New<CusUSLVClearance>();
			cusUSLVConsignment.ULB_ULH = cusUSLVClearance.PK;
			AssertEquals(cusUSLVClearance.PK, ((IInvoiceLinePartDetails)cusUSLVItem).PartSyncManagerActiveDeciderPK);
		}

		#endregion IInvoiceLinePartDetails

		public void TestItemReadonlyWhenConsignmentEntryLineReferenceHasValue()
		{
			var item = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVItem;
			var consignment = item.Consignment;
			consignment.CE_EntryLineReference = "NOTEMPTYANYMORE";

			var propertyInfos = item.GetType().GetProperties()
								.Where(p => p.PropertyType == typeof(ZPropertyInfo))
								.Select(info => (ZPropertyInfo)info.GetValue(item));

			CombineAssertions(() =>
			{
				propertyInfos.ForEach(info => AssertEquals($"{info.Name} ReadOnly:", true, info.ReadOnly));
			});

			AssertEquals("NOTEMPTYANYMORE", consignment.CE_EntryLineReference);
		}

		#region SetUp

		CusUSLVItem Item => item ?? (item = (CusUSLVItem)GetNewBusinessObjectForDeleteTest(Factory));
		CusUSLVItem item;

		#endregion
	}
}
