using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(QuotationLine))]
	internal sealed class QuotationLineTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestConditionsApply()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = UnitCalculator.Code;
			((UnitCalculator)line.Calculator).PerUnit = 5m;

			AssertNull(QuotationLine.ConditionsApply(line));

			line.TL_Condition = RateLineConditions.UserDefined;
			line.TL_ConditionalExpression = "MOD=FSA";
			AssertEquals("*conditions apply|||", QuotationLine.ConditionsApply(line).ToString());

			line.TL_Condition = RateLineConditions.ForwardingAndBrokerage;
			AssertEquals("*conditions apply|||", QuotationLine.ConditionsApply(line).ToString());
		}

		public void TestContractNumber()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry("FCL");
			var line = entry.AddRateLine("FRT", FlatCalculator.Code);

			AssertNull("contract number not entered", QuotationLine.ContractNumber(line));

			entry.TI_ContractNumber = "Blaticus";
			AssertEquals("contract number entered", "Contract Number: Blaticus|||", QuotationLine.ContractNumber(line).ToString());
		}

		public void TestNoteText()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry("FCL");
			var line = entry.AddRateLine("FRT", FlatCalculator.Code);

			AssertNull("note text not entered", QuotationLine.RateNote(line));

			line.ChargeInformationNoteText = "Blaticus";
			AssertEquals("note text entered", "Note: Blaticus|||", QuotationLine.RateNote(line).ToString());
		}

		#region Local Language Description

		public void TestLocalLanguageDescription()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Desc = "Charge Code 1";
			chargeCode1.AC_LocalLanguageDescription = "Gujarati Description 1";

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Desc = "Charge Code 2";
			chargeCode2.AC_LocalLanguageDescription = "Gujarati Description 2";

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_RL_NKClosestPort = "AUSYD";
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry("ORG", "FCL", "AUSYD", "");
			quote.TH_OH = client.PK;
			var rateLine = entry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode1.PK;

			var quotationLine = QuotationLine.New(rateLine, QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
			AssertContains("Charge Code 1", quotationLine.ToString());

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.EnableLocalChargeCodeDescriptionDefault).Returns(true);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(true);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);
			mock.Setup(m => m.APAccountGroup).Returns(Factory.NewWithValidTestData<OrgCreditorGroup>().PK.ToGuid());
			mock.Setup(m => m.ARAccountGroup).Returns(Factory.NewWithValidTestData<OrgDebtorGroup>().PK.ToGuid());

			using (ObjectFactory.Substitute(mock.Object))
			{
				quotationLine = QuotationLine.New(rateLine, QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
				AssertContains("Gujarati Description 1", quotationLine.ToString());

				rateLine.TL_RateDescLocal = "Changed";
				quotationLine = QuotationLine.New(rateLine, QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
				AssertContains("Changed", quotationLine.ToString());

				rateLine = entry.RateLines.AddNew();
				rateLine.TL_AC = chargeCode2.PK;
				quotationLine = QuotationLine.New(rateLine, QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
				AssertContains("Gujarati Description 2", quotationLine.ToString());
			}
		}

		public void TestLocalLanguageDescriptionNotApplyIfClientFromOtherCountry()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Germany);
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.OH_RL_NKClosestPort = "DEFRA";
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.OH_RL_NKClosestPort = "AUMEL";

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Origin Local Charge";
			chargeCode.AC_LocalLanguageDescription = "Herkunft Gebühr";

			var quote1 = Factory.New<Quote>();
			var entry1 = quote1.AddRateEntry("ORG", "FCL", "AUSYD", "");
			quote1.TH_OH = client1.PK;
			var rateLine1 = entry1.RateLines.AddNew();
			rateLine1.TL_AC = chargeCode.PK;

			var quotationLine1 = QuotationLine.New(rateLine1, QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
			AssertContains("Origin Local Charge", quotationLine1.ToString());

			var quote2 = Factory.New<Quote>();
			var entry2 = quote2.AddRateEntry("ORG", "FCL", "AUSYD", "");
			quote2.TH_OH = client2.PK;
			var rateLine2 = entry2.RateLines.AddNew();
			rateLine2.TL_AC = chargeCode.PK;

			var quotationLine2 = QuotationLine.New(rateLine2, QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
			AssertContains("Origin Local Charge", quotationLine2.ToString());

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.EnableLocalChargeCodeDescriptionDefault).Returns(true);
			mock.Setup(m => m.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors).Returns(false);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(true);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);
			mock.Setup(m => m.APAccountGroup).Returns(Factory.NewWithValidTestData<OrgCreditorGroup>().PK.ToGuid());
			mock.Setup(m => m.ARAccountGroup).Returns(Factory.NewWithValidTestData<OrgDebtorGroup>().PK.ToGuid());

			using (ObjectFactory.Substitute(mock.Object))
			{
				quotationLine1 = QuotationLine.New(rateLine1, QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
				AssertContains("Herkunft Gebühr", quotationLine1.ToString());

				quotationLine2 = QuotationLine.New(rateLine2, QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
				AssertContains("Origin Local Charge", quotationLine2.ToString());
			}
		}

		public void TestLocalLanguageAmount()
		{
			var info = typeof(RawDataRegistry).GetProperty("NotChargedText", BindingFlags.GetProperty | BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			var registryItem = info.GetValue(Env.Registry.RawRegistry, null) as MultilingualStringRegistryItem;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Not Charged (English)");

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Code AAA";
			chargeCode.AC_ShowOnQuotation = true;
			chargeCode.AC_SuppressOnQuoteIfZero = false;

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)rateLine.Calculator).BaseRate = ZDecimal.Zero;

			var quotationLine = QuotationLine.NewWithValue(rateLine, QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription, rateLine.RateLineItems[0].TM_RelevantValue, "", (NoResString)"");
			AssertEquals("Charge Code AAA||Not Charged (English)|", quotationLine.ToString());

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockChs = Res.UseMockData())
			{
				var key = ((ResourceString)registryItem.Value).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "Not Charged (Chinese - Simplified)"));

				AssertEquals("Charge Code AAA||Not Charged (Chinese - Simplified)|", quotationLine.ToString());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900327. Used to test string translates so cannot hardcode.")]
		public void TestLocalLanguageUnit()
		{
			MultilingualString str = ResString.GetMultilingualString("pu", "Per Unit (English)");

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Code BBB";
			chargeCode.AC_ShowOnQuotation = true;

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RX_NKCurrency = string.Empty;
			rateLine.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)rateLine.Calculator).BaseRate = 120m;

			var quotationLine = QuotationLine.NewWithValue(rateLine, QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription, rateLine.RateLineItems[0].TM_RelevantValue, "", str);
			AssertEquals("Charge Code BBB||120.00|Per Unit (English)", quotationLine.ToString());

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockChs = Res.UseMockData())
			{
				mockChs.Put("pu", new ResourceStringData("pu", "Per Unit (Chinese - Simplified)"));
				AssertEquals("Charge Code BBB||120.00|Per Unit (Chinese - Simplified)", quotationLine.ToString());
			}
		}

		#endregion

		public void TestGetDescription()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "BOB";
			chargeCode.AC_Desc = "charge desc";
			chargeCode.AC_LocalLanguageDescription = "local charge code";

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateDesc = "rate desc";

			var cline = QuotationLine.New(rateLine, QuotationLineType.Mandatory | QuotationLineType.UseChargeDescription, "", (NoResString)"", (NoResString)"");
			var rline = QuotationLine.New(rateLine, QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");

			CombineAssertions(delegate
			{
				AssertEquals("use charge description", "charge desc", cline.GetDescription(""));
				AssertEquals("use rate description", "rate desc", rline.GetDescription(""));
			});

			var enableLocalChargeCodeDescription = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			var applyLocalChargeCodeDescriptionDefaultToForeignDebtors = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors");

			using (enableLocalChargeCodeDescription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (applyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(delegate
				{
					AssertEquals("use charge description", "local charge code", cline.GetDescription(""));
					AssertEquals("use rate description", "local charge code", rline.GetDescription(""));
				});
			}
		}

		#region Multilingual

		public void TestDescription_Multilingual_EnableLocalChargeCodeDescriptionDefault()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");
			AssertDescription_Multilingual
			(
				chargeCode,
				enableLocalChargeCodeDescriptionDefaultValue: true,
				chargeCodeMultiLingualDescription: "Mein Testgebührencode",
				expectedDescription: "Charge Local Description 1"
			);
		}

		public void TestDescription_Multilingual_DisableLocalChargeCodeDescriptionDefault()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");
			AssertDescription_Multilingual
			(
				chargeCode,
				enableLocalChargeCodeDescriptionDefaultValue: false,
				chargeCodeMultiLingualDescription: "Mein Testgebührencode",
				expectedDescription: "Mein Testgebührencode"
			);
		}

		void AssertDescription_Multilingual(AccChargeCode chargeCode, bool enableLocalChargeCodeDescriptionDefaultValue, string chargeCodeMultiLingualDescription, string expectedDescription)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var ratingHeader = Helper.NewQuote(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableLocalChargeCodeDescriptionDefaultValue))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, chargeCodeMultiLingualDescription);

				var rline = QuotationLine.New(rateLine, QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
				AssertEquals
				(
					$"GIVEN enableLocalChargeCodeDescriptionDefaultValue={enableLocalChargeCodeDescriptionDefaultValue} THEN description",
					expectedDescription,
					rline.GetDescription("")
				);
			}
		}

		static void SetChargeCodeDescriptionMultilingual(AccChargeCode chargeCode, IMockResourceStringCache mockRes, string multilingualDescription)
		{
			var resKey = chargeCode.AC_DescInfo.CustomizableDataResourceStrings.GetMultilingualString(chargeCode, chargeCode.AC_Desc).ResourceKey;
			mockRes.Put(resKey, new ResourceStringData(resKey, multilingualDescription));
		}

		public void TestDescription_Multilingual_AddTextToDescription()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");

			var ratingHeader = Helper.NewQuote(Helper.NewOrgHeader());
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, "Mein Testgebührencode");

				rateLine.TL_RateDesc += " (updated)";

				var rline = QuotationLine.New(rateLine, QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
				AssertEquals
				(
					"WHEN text is added at the end of description, THEN should show multilingual description with the added text",
					"Mein Testgebührencode (updated)",
					rline.GetDescription("")
				);
			}
		}

		public void TestDescription_Multilingual_UpdatingDescription()
		{
			var chargeCode = Helper.ChargeCodes.New("CHARGE1", "Charge Description 1", FlatCalculator.Code, localLanguageDescription: "Charge Local Description 1");

			var ratingHeader = Helper.NewQuote(Helper.NewOrgHeader());
			var rateEntry = ratingHeader.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				SetChargeCodeDescriptionMultilingual(chargeCode, mockRes, "Mein Testgebührencode");

				rateLine.TL_RateDesc = "User updated description";

				var rline = QuotationLine.New(rateLine, QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription, "", (NoResString)"", (NoResString)"");
				AssertEquals
				(
					"WHEN rateLine description is updated, THEN should show updated text",
					"User updated description",
					rline.GetDescription("")
				);
			}
		}

		#endregion

		public void TestNewWithValue_Decimals()
		{
			AssertNewWithValueDecimals(100m, "100.00");
			AssertNewWithValueDecimals(1.23m, "1.23");
			AssertNewWithValueDecimals(1.2m, "1.20");
			AssertNewWithValueDecimals(1.1234m, "1.1234");
		}

		void AssertNewWithValueDecimals(decimal inputDecimal, string expectedOutput)
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			var line = QuotationLine.NewWithValue(rateLine, QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription, inputDecimal, "Hello", (NoResString)"M3");
			AssertContains("|" + expectedOutput + "|", line.ToString());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry("FCL");
			var line = entry.AddRateLine("FRT", FlatCalculator.Code);

			return QuotationLine.New(line, "text");
		}

		TestHelper Helper => helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		#endregion
	}
}
