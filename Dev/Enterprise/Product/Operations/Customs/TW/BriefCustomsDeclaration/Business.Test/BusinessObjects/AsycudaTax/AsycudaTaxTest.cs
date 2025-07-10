using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaTax))]
	sealed class AsycudaTaxTest : EnterpriseBusinessObjectTestCase
	{
		public void TestChargeTypeReadOnly()
		{
			var tax = Tax;
			tax.AET_MethodOfPayment = ZString.Empty;
			CombineAssertions(() =>
			{
				Assert("ChargeTypeReadOnly should be true when AET_MethodOfPayment is empty", tax.AET_ChargeTypeReadOnly);
				Assert("ChargeType should be readonly when AET_MethodOfPayment is empty", tax.AET_ChargeTypeInfo.ReadOnly);
			});

			tax.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyLevied;
			CombineAssertions(() =>
			{
				Assert("ChargeTypeReadOnly should be false when AET_MethodOfPayment is not empty", !tax.AET_ChargeTypeReadOnly);
				Assert("ChargeType should not be readonly when AET_MethodOfPayment is empty", !tax.AET_ChargeTypeInfo.ReadOnly);
			});
		}

		public void TestAmountReadOnly()
		{
			var tax = Tax;
			tax.AET_ChargeType = ZString.Empty;
			CombineAssertions(() =>
			{
				Assert("AmountReadOnly should be true when AET_ChargeType is empty", tax.AET_ChargeAmountReadOnly);
				Assert("Amount should be readonly when AET_ChargeType is empty", tax.AET_ChargeAmountInfo.ReadOnly);
			});

			tax.AET_ChargeType = MethodOfPaymentList.Codes.DutyLevied;
			CombineAssertions(() =>
			{
				Assert("AmountReadOnly should be false when AET_ChargeType is not empty", !tax.AET_ChargeAmountReadOnly);
				Assert("Amount should not be readonly when AET_ChargeType is empty", !tax.AET_ChargeAmountInfo.ReadOnly);
			});
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("AET_MethodOfCalculation", UniversalReferenceConstants.MethodOfCalculation.Percentage, Tax.AET_MethodOfCalculation);
		}

		[ExpectNoExceptions]
		public void TestPropertyCaptions()
		{
			var tax = Tax;
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptions(tax.AET_RateOverrideReasonCodeInfo, "Action");
				BusinessObjectCaptionTestHelper.AssertCaptions(tax.AET_MethodOfPaymentInfo, "Payment Method");
				BusinessObjectCaptionTestHelper.AssertCaptions(tax.AET_ChargeTypeInfo, "Type");
				BusinessObjectCaptionTestHelper.AssertCaptions(tax.ChargeTypeDescriptionInfo, "Description");
				BusinessObjectCaptionTestHelper.AssertCaptions(tax.AET_ChargeAmountInfo, "Amount");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_BaseValueInfo, "Base Amount", "Base Amount", "Base Amount", "Base Amount for duties, taxes and fees.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_MethodOfCalculationInfo, "Method of Calculation", "Method of Calculation", "Method of Calculation", "Calculation Method for duties, taxes and fees.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(tax.AET_RateInfo, "Rate", "Rate", "Rate", "Rate of duties, taxes and fees.");
			});
		}

		public void TestAET_MethodOfCalculationLookups()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaTax), nameof(AsycudaTax.AET_MethodOfCalculation), false, a => a.ListDataSourceMember == "Lookups.MethodOfCalculationList");
		}

		public void TestCalculateManualTotal()
		{
			var tax = Tax;
			tax.AET_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
			tax.AET_MethodOfCalculation = "CAS";
			tax.AET_BaseValue = 100m;
			tax.AET_Rate = 0.3m;
			AssertEquals(30m, tax.AET_ChargeAmount);

			tax.AET_Rate = 0.4m;
			AssertEquals(40m, tax.AET_ChargeAmount);

			tax.AET_ChargeAmount = 0m;
			tax.AET_BaseValue = 200m;
			AssertEquals(80m, tax.AET_ChargeAmount);

			tax.AET_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Additional;
			tax.AET_ChargeAmount = 0m;
			tax.AET_Rate = 0.1m;
			AssertEquals(20m, tax.AET_ChargeAmount);

			tax.AET_ChargeAmount = 0m;
			tax.AET_BaseValue = 500m;
			AssertEquals(50m, tax.AET_ChargeAmount);

			tax.AET_MethodOfCalculation = UniversalReferenceConstants.MethodOfCalculation.Percentage;
			tax.AET_ChargeAmount = 0m;
			tax.AET_BaseValue = 600m;
			AssertEquals(60m, tax.AET_ChargeAmount);

			tax.AET_ChargeAmount = 0m;
			tax.AET_BaseValue = 66m;
			AssertEquals(6m, tax.AET_ChargeAmount);
		}

		public void TestCalculateManualTotalOnRateChanged()
		{
			var tax = Tax;
			tax.AET_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Override;
			tax.AET_MethodOfCalculation = "CAS";
			tax.AET_BaseValue = 100m;
			tax.AET_Rate = 0.3m;
			AssertEquals(30m, tax.AET_ChargeAmount);

			tax.AET_Rate = 0.4m;
			AssertEquals(40m, tax.AET_ChargeAmount);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var bO = GetNewBusinessObjectForDeleteTest(Factory);
			Factory.Save();
			AssertEquals("BO Is Deleted For ReCalculate Tax", true, bO.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var asycudaTax = header.Bills.AddNew().AsycudaTaxes.AddNew();
			return asycudaTax;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues() => new List<ZString>() { AsycudaTax.Schema.AET_ABL };

		AsycudaTax tax;
		AsycudaTax Tax => tax ?? (tax = GetNewBusinessObject() as AsycudaTax);
	}
}
