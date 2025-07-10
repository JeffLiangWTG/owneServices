using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaTaxValidation))]
	sealed class AsycudaTaxValidationTest : BusinessObjectLookupsTestCase
	{
		AsycudaManifestHeader header;
		AsycudaManifestHeader Header => header ?? (header = Factory.NewWithValidTestData<AsycudaManifestHeader>());

		AsycudaTax tax;
		AsycudaTax Tax => tax ?? (tax = Header.Bills.AddNew().AsycudaTaxes.AddNew());

		AsycudaTaxValidation Validation => (AsycudaTaxValidation)Tax.Validation;

		public void TestCheckAET_RateOverrideReasonCode()
		{
			Tax.AET_RateOverrideReasonCode = "UNK";
			Validation.ValidateAET_RateOverrideReasonCode();
			const string expectedMessage = "'UNK' is invalid";
			AssertHasMessageError(tax.AET_RateOverrideReasonCodeInfo, expectedMessage);

			Tax.AET_RateOverrideReasonCode = RateOverrideReasonCodeList.Codes.Override;
			Validation.ValidateAET_RateOverrideReasonCode();
			AssertNoMessageErrors(tax.AET_RateOverrideReasonCodeInfo);
		}

		public void TestCheckAET_MethodOfPayment()
		{
			Tax.AET_MethodOfPayment = "UNK";
			Validation.ValidateAET_MethodOfPayment();
			const string expectedMessage = "'UNK' is invalid";
			AssertHasMessageError(tax.AET_MethodOfPaymentInfo, expectedMessage);

			Tax.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyLevied;
			Validation.ValidateAET_MethodOfPayment();
			AssertNoMessageErrors(tax.AET_MethodOfPaymentInfo);
		}

		public void TestCheckAET_ChargeType()
		{
			Tax.AET_ChargeType = "UNK";
			Validation.ValidateAET_ChargeType();
			const string expectedMessage = "'UNK' is invalid";
			AssertHasMessageError(tax.AET_ChargeTypeInfo, expectedMessage);

			Tax.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyLevied;
			Tax.AET_ChargeType = ChargeTypeCASList.Codes.LatePaymentFee;
			Validation.ValidateAET_ChargeType();
			AssertNoMessageErrors(tax.AET_ChargeTypeInfo);
		}

		public void TestCheckAET_MethodOfCalculation()
		{
			Tax.Validation.ValidateAET_MethodOfCalculation();
			AssertEquals("Should not call base.CheckAET_MethodOfCalculation", false, Tax.AET_MethodOfCalculationInfo.HasNotifications());
		}
	}
}
