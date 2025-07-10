using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	sealed class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		[ExpectNoExceptions]
		public void TestRateDutyReadOnly()
		{
			Declaration.JE_MessageType = "IMP";
			var fee = EntryLine.Fees.AddNew();
			fee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
			NUnit.Framework.Assert.That(!fee.RateDutyReadOnly, NUnit.Framework.Is.True, "Should be editable when Import.");

			fee.CF_RateOverrideReasonCode = ZString.Empty;
			NUnit.Framework.Assert.That(fee.RateDutyReadOnly, NUnit.Framework.Is.True, "Should be readonly when CF_RateOverrideReasonCode is empty.");

			Declaration.JE_MessageType = "EXP";
			fee = EntryLine.Fees.AddNew();
			fee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
			NUnit.Framework.Assert.That(fee.RateDutyReadOnly, NUnit.Framework.Is.True, "Should be readonly when Export.");

			fee.CF_RateOverrideReasonCode = ZString.Empty;
			NUnit.Framework.Assert.That(fee.RateDutyReadOnly, NUnit.Framework.Is.True, "Should be readonly when Export.");
		}

		public void TW_MethodOfPaymentValueShouldAlwaysBeCASWhenExport()
		{
			Declaration.JE_MessageType = "EXP";
			var fee = EntryLine.Fees.AddNew();
			NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

			Declaration.JE_MessageType = "IMP";
			fee = EntryLine.Fees.AddNew();
			NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		public void TW_RateDutyValueShouldAlwaysBePercentageWhenExport()
		{
			Declaration.JE_MessageType = "EXP";
			var fee = EntryLine.Fees.AddNew();
			NUnit.Framework.Assert.That(fee.TW_RateDuty, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));

			Declaration.JE_MessageType = "IMP";
			fee = EntryLine.Fees.AddNew();
			NUnit.Framework.Assert.That(fee.TW_RateDuty, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestChargeTypeShouldAlwayBeTPFWhenExport()
		{
			Declaration.JE_MessageType = "EXP";
			var fee = EntryLine.Fees.AddNew();
			NUnit.Framework.Assert.That(fee.TW_Type, NUnit.Framework.Is.EqualTo("TPF").Using(CustomComparers.TypeComparison));

			Declaration.JE_MessageType = "IMP";
			fee = EntryLine.Fees.AddNew();
			NUnit.Framework.Assert.That(fee.TW_Type, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestMethodOfPaymentReadOnly()
		{
			Declaration.JE_MessageType = "IMP";
			var fee = EntryLine.Fees.AddNew();
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ADD;
			NUnit.Framework.Assert.That(fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be readonly.");

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTA;
			NUnit.Framework.Assert.That(!fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be editable.");

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.RTD;
			NUnit.Framework.Assert.That(fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be readonly.");

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTS;
			NUnit.Framework.Assert.That(!fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be editable.");

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CVD;
			NUnit.Framework.Assert.That(fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be readonly.");

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.HWS;
			NUnit.Framework.Assert.That(!fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be editable.");

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ADT;
			NUnit.Framework.Assert.That(fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be readonly.");

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.TAT;
			NUnit.Framework.Assert.That(!fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be editable.");

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			NUnit.Framework.Assert.That(!fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be editable.");

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			NUnit.Framework.Assert.That(!fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be editable.");

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.SSG;
			NUnit.Framework.Assert.That(!fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be editable.");

			fee.CF_ChargeType = RefCusTaxOrFeeCodes.TPF;
			NUnit.Framework.Assert.That(!fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be editable.");

			fee.CF_ChargeType = RefCusTaxOrFeeCodes.VAT;
			NUnit.Framework.Assert.That(!fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be editable.");

			fee.TW_RateOverride = "";
			NUnit.Framework.Assert.That(fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be readonly.");
			fee.TW_RateOverride = "ADD";
			NUnit.Framework.Assert.That(!fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be editable.");

			Declaration.JE_MessageType = "EXP";
			fee = EntryLine.Fees.AddNew();
			NUnit.Framework.Assert.That(fee.MethodOfPaymentReadOnly, NUnit.Framework.Is.True, "Should be readonly when Export.");
		}

		[ExpectNoExceptions]
		public void TestSetMethodOfPaymentIfChargeTypeIsCashOnly()
		{
			Declaration.JE_MessageType = "IMP";
			var fee = EntryLine.Fees.AddNew();
			CombineAssertions("CF_RateOverrideReasonCode is Additional", () =>
			{
				fee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Additional;
				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ADD;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTA;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.RTD;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTS;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CVD;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.HWS;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ADT;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.TAT;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.SSG;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = RefCusTaxOrFeeCodes.TPF;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = RefCusTaxOrFeeCodes.VAT;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));
			});

			CombineAssertions("CF_RateOverrideReasonCode is Override", () =>
			{
				fee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ADD;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTA;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.RTD;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTS;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CVD;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.HWS;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ADT;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo("CAS").Using(CustomComparers.TypeComparison));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.TAT;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.SSG;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = RefCusTaxOrFeeCodes.TPF;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = RefCusTaxOrFeeCodes.VAT;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));
			});

			CombineAssertions("CF_RateOverrideReasonCode is Empty", () =>
			{
				fee.CF_RateOverrideReasonCode = ZString.Empty;
				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ADD;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTA;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.RTD;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTS;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CVD;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.HWS;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ADT;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.TAT;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.SSG;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = RefCusTaxOrFeeCodes.TPF;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));

				fee.CF_ChargeType = RefCusTaxOrFeeCodes.VAT;
				NUnit.Framework.Assert.That(fee.TW_MethodOfPayment, NUnit.Framework.Is.EqualTo(ZString.Empty));
			});
		}

		[ExpectNoExceptions]
		public void TestDefaultValue()
		{
			var fee = Factory.New<CusEntryLineFee>();
			NUnit.Framework.Assert.That(fee.CF_RateOverrideReasonCode, NUnit.Framework.Is.EqualTo(TWRateOverrideReasonList.Codes.Additional).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalAmountReadOnly()
		{
			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Additional;
			NUnit.Framework.Assert.That(!fee.TotalAmountReadOnly, NUnit.Framework.Is.True, "Should be false.");

			fee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
			NUnit.Framework.Assert.That(!fee.TotalAmountReadOnly, NUnit.Framework.Is.True, "Should be false.");

			fee.CF_RateOverrideReasonCode = ZString.Empty;
			NUnit.Framework.Assert.That(fee.TotalAmountReadOnly, NUnit.Framework.Is.True, "Should be true.");
		}

		[ExpectNoExceptions]
		public void TestCalculateManualTotal()
		{
			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
			fee.CF_MethodOfCalculation = "CAS";
			fee.CF_BaseValue = 100m;
			fee.CF_Rate = 0.3m;
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(30m).Using(CustomComparers.TypeComparison));

			fee.CF_Rate = 0.4m;
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(40m).Using(CustomComparers.TypeComparison));

			fee.CF_ChargeAmount = 0m;
			fee.CF_BaseValue = 200m;
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(80m).Using(CustomComparers.TypeComparison));

			fee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Additional;
			fee.CF_ChargeAmount = 0m;
			fee.CF_Rate = 0.1m;
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(20m).Using(CustomComparers.TypeComparison));

			fee.CF_ChargeAmount = 0m;
			fee.CF_BaseValue = 500m;
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(50m).Using(CustomComparers.TypeComparison));

			fee.CF_MethodOfCalculation = UniversalReferenceConstants.MethodOfCalculation.Percentage;
			fee.CF_ChargeAmount = 0m;
			fee.CF_BaseValue = 600m;
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(60m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCalculateManualTotalOnRateChanged()
		{
			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
			fee.CF_MethodOfCalculation = "CAS";
			fee.CF_BaseValue = 100m;
			fee.CF_Rate = 0.3m;
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(30m).Using(CustomComparers.TypeComparison));

			fee.CF_Rate = 0.4m;
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(40m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeDecider()
		{
			NUnit.Framework.Assert.That(Factory.New(typeof(Customs.Business.CusEntryLineFee)).GetType(), NUnit.Framework.Is.EqualTo(GetExpectedBusinessObjectType()), "Update Customs.Business.CusEntryLineFee to include a decider for this class");
		}

		[ExpectNoExceptions]
		public void TestTypeDescription()
		{
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ZHT", "ChineseTraditional");
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM");
			helper.LoadOrCreateNewCusRateCode(Factory, "CTA", rateType.PK, true, false, "Commodity Tax (Ad-Valorem)");
			var rateCodeLan = helper.LoadOrCreateNewCusRateCodeLanguage(Factory, "CTA", "ZHT");
			rateCodeLan.ZXC_Description = "貨物稅(從價)";

			helper.CreateRefCusTaxOrFeeType("VAT", "Value Added Tax");
			helper.CreateRefCusTaxOrFeeType("OTH", "Other");
			var vatTaxOrFee = helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, 0, 0, "VAT");
			vatTaxOrFee.ZZF_Description = "Business tax";
			helper.CreateTaxOrFeeLanguage(vatTaxOrFee, "ZHT", "營業稅");

			var tpfTaxOrFee = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, 0, 0, "OTH");
			tpfTaxOrFee.ZZF_Description = "Promotion trade service fee";
			helper.CreateTaxOrFeeLanguage(tpfTaxOrFee, "ZHT", "推廣貿易服務費");
			Factory.Save();

			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTA;
			NUnit.Framework.Assert.That(fee.TW_TypeDescription, NUnit.Framework.Is.EqualTo("Commodity Tax (Ad-Valorem)").Using(CustomComparers.TypeComparison));

			fee.CF_ChargeType = RefCusTaxOrFeeCodes.VAT;
			NUnit.Framework.Assert.That(fee.TW_TypeDescription, NUnit.Framework.Is.EqualTo("Business tax").Using(CustomComparers.TypeComparison));

			fee.CF_ChargeType = RefCusTaxOrFeeCodes.TPF;
			NUnit.Framework.Assert.That(fee.TW_TypeDescription, NUnit.Framework.Is.EqualTo("Promotion trade service fee").Using(CustomComparers.TypeComparison));

			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseTraditional;
			Factory.Save();
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CTA;
			NUnit.Framework.Assert.That(fee.TW_TypeDescription, NUnit.Framework.Is.EqualTo("貨物稅(從價)").Using(CustomComparers.TypeComparison));

			fee.CF_ChargeType = RefCusTaxOrFeeCodes.VAT;
			NUnit.Framework.Assert.That(fee.TW_TypeDescription, NUnit.Framework.Is.EqualTo("營業稅").Using(CustomComparers.TypeComparison));

			fee.CF_ChargeType = RefCusTaxOrFeeCodes.TPF;
			NUnit.Framework.Assert.That(fee.TW_TypeDescription, NUnit.Framework.Is.EqualTo("推廣貿易服務費").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestShouldDeleteIfChargeAmountIsZero()
		{
			var fee = Factory.New<CusEntryLineFeeForTest>();
			fee.CF_ChargeAmount = 0M;
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			NUnit.Framework.Assert.That(!fee.ShouldDeleteIfChargeAmountIsZeroForTest, NUnit.Framework.Is.True);
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			NUnit.Framework.Assert.That(!fee.ShouldDeleteIfChargeAmountIsZeroForTest, NUnit.Framework.Is.True);
			fee.CF_ChargeType = SpecialDutyRateCodeList.Codes.AntiDumpingDuty;
			NUnit.Framework.Assert.That(fee.ShouldDeleteIfChargeAmountIsZeroForTest, NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestIsSavedByFactory()
		{
			EntryLine.Fees.RemoveAndDeleteAll();
			var fee = EntryLine.Fees.AddNew();
			fee.CF_ChargeAmount = 0M;
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var entyLine = newFactory.Load<CusEntryLine>(EntryLine.PK);
			fee = entyLine.Fees[0];
			fee.CF_ChargeAmount = 0M;

			NUnit.Framework.Assert.That(fee.IsSavedByFactory, NUnit.Framework.Is.True);

			newFactory.Save();
			newFactory = new BusinessObjectFactory();
			fee = newFactory.Load<CusEntryLineFee>(fee.PK);
			NUnit.Framework.Assert.That(fee.CF_ChargeAmount, NUnit.Framework.Is.EqualTo(0M).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTW_Type_Attribute()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_Type", false, attrib => attrib.Caption == "Type"));
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_Type", false, attrib => attrib.FullDescription == "The code of the duties, taxes and fees."));
			});
		}

		[ExpectNoExceptions]
		public void TestTW_TypeDescription_Attribute()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_TypeDescription", false, attrib => attrib.Caption == "Description"));
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_TypeDescription", false, attrib => attrib.FullDescription == "The description of the duties, taxes and fees."));
			});
		}

		[ExpectNoExceptions]
		public void TestTW_MethodOfPayment_Attribute()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_MethodOfPayment", false, attrib => attrib.Caption == "Method of Payment"));
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_MethodOfPayment", false, attrib => attrib.FullDescription == "The payment method of the duties, taxes and fees."));
			});
		}

		[ExpectNoExceptions]
		public void TestTW_RateOverride_Attribute()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_RateOverride", false, attrib => attrib.Caption == "Action"));
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_RateOverride", false, attrib => attrib.FullDescription == "Action Type."));
			});
		}

		[ExpectNoExceptions]
		public void TestTW_BaseAmount_Attribute()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_BaseAmount", false, attrib => attrib.Caption == "Base Amount"));
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_BaseAmount", false, attrib => attrib.FullDescription == "Base Amount for duties, taxes and Fees."));
			});
		}

		[ExpectNoExceptions]
		public void TestTW_RateDuty_Attribute()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_RateDuty", false, attrib => attrib.Caption == "Method of Calculation"));
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_RateDuty", false, attrib => attrib.FullDescription == "Calculation Method for duties, taxes and Fees."));
			});
		}

		[ExpectNoExceptions]
		public void TestTW_RateSuspension_Attribute()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_RateSuspension", false, attrib => attrib.Caption == "Tax Rate"));
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_RateSuspension", false, attrib => attrib.FullDescription == "Rate of duties, taxes and Fees."));
			});
		}

		[ExpectNoExceptions]
		public void TestTW_Amount_Attribute()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_Amount", false, attrib => attrib.Caption == "Total Amount"));
				NUnit.Framework.Assert.That(typeof(CusEntryLineFee), CustomConstraints.HasCustomAttribute<ResourceStringDataAttribute>("TW_Amount", false, attrib => attrib.FullDescription == "The amount of the duties, taxes and fees."));
			});
		}

		CusEntryLine EntryLine
		{
			get
			{
				if (fEntryLine == null)
				{
					var invoiceHeader = Declaration.Invoices.AddNew();
					var entryInstruction = Declaration.CusEntryInstruction;
					entryInstruction.CEI_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
					var invoiceLine = Declaration.InvoiceLines.AddNew();
					invoiceLine.JI_JZ = invoiceHeader.PK;
					invoiceLine.JI_CEI = entryInstruction.PK;
					invoiceLine.JI_PartNo = "XXX";
					invoiceLine.JI_Tariff = "A";
					invoiceLine.JI_Description = "";

					Declaration.DoMerge();
					fEntryLine = Declaration.CustomsEntryHeaders[0].MergedLines[0];
				}
				return fEntryLine;
			}
		}

		CusEntryLine fEntryLine;

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					fDeclaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
					fDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
					fDeclaration.CustomsEntryHeaders.AddNew();
				}
				return fDeclaration;
			}
		}

		JobDeclaration fDeclaration;

		class CusEntryLineFeeForTest : CusEntryLineFee
		{
			public CusEntryLineFeeForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public bool ShouldDeleteIfChargeAmountIsZeroForTest => ShouldDeleteIfChargeAmountIsZero;
		}
	}
}
