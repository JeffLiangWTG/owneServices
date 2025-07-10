using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class CusEntryLineFeeValidationTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestCheckCF_MethodOfCalculation()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var entryLineFee = (CusEntryLineFee)entryLine.Fees.AddNew();
			entryLineFee.CF_MethodOfCalculation = ZString.Empty;
			AssertNotificationsForEmptyMethodOfCalculation(entryLineFee);

			entryLineFee.CF_MethodOfCalculation = "ABC";
			AssertHasMessageErrorContaining(entryLineFee.CF_MethodOfCalculationInfo, "The code you have selected is not in the list.");
		}

		protected virtual void AssertNotificationsForEmptyMethodOfCalculation(CusEntryLineFee entryLineFee) => AssertNoNotifications(entryLineFee.CF_MethodOfCalculationInfo);

		public void TestCheckCF_MethodOfPayment()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var entryLineFee = (CusEntryLineFee)entryLine.Fees.AddNew();
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			AssertNotificationsForEmptyMethodOfPayment(entryLineFee);

			entryLineFee.CF_MethodOfPayment = "ABC";
			AssertHasMessageErrorContaining(entryLineFee.CF_MethodOfPaymentInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCF_ChargeTypeDuplicateTaxCode()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var entryLineFee1 = (CusEntryLineFee)entryLine.Fees.AddNew();
			entryLineFee1.CF_ChargeType = "10";
			entryLineFee1.NationalFeeTypeCode = "10";
			entryLineFee1.CF_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Additional;
			entryLineFee1.CF_Source = Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CW1;
			entryLineFee1.CF_MethodOfCalculation = MethodOfCalculationList.Codes.CW1;
			entryLineFee1.CF_MethodOfPayment = "P";
			entryLineFee1.CF_BaseValue = 1000m;
			entryLineFee1.CF_Rate = 10m;
			entryLineFee1.CF_ChargeAmount = 100m;
			entryLineFee1.Validation.ValidateCF_ChargeType();
			AssertNoErrorContaining("When Duplicate Charge Type", entryLineFee1.CF_ChargeTypeInfo, "Duplicate Charge Type");

			var entryLineFee2 = (CusEntryLineFee)entryLine.Fees.AddNew();
			entryLineFee2.CF_ChargeType = "B00";
			entryLineFee2.NationalFeeTypeCode = "40";
			entryLineFee2.CF_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Additional;
			entryLineFee2.CF_Source = Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CW1;
			entryLineFee2.CF_MethodOfCalculation = MethodOfCalculationList.Codes.CW1;
			entryLineFee2.CF_MethodOfPayment = "P";
			entryLineFee2.CF_BaseValue = 1000m;
			entryLineFee2.CF_Rate = 10m;
			entryLineFee2.CF_ChargeAmount = 100m;
			entryLineFee2.Validation.ValidateCF_ChargeType();
			AssertNoErrorContaining("When Duplicate Charge Type", entryLineFee2.CF_ChargeTypeInfo, "Duplicate Charge Type");

			var entryLineFee3 = (CusEntryLineFee)entryLine.Fees.AddNew();
			entryLineFee3.CF_ChargeType = "B00";
			entryLineFee3.NationalFeeTypeCode = "40";
			entryLineFee3.CF_RateOverrideReasonCode = EU.Business.RateOverrideReasonList.Codes.Additional;
			entryLineFee3.CF_Source = Customs.Business.CusEntryLineFeeSourceCodeList.Codes.CW1;
			entryLineFee3.CF_MethodOfCalculation = MethodOfCalculationList.Codes.CW1;
			entryLineFee3.CF_MethodOfPayment = "P";
			entryLineFee3.CF_BaseValue = 1000m;
			entryLineFee3.CF_Rate = 10m;
			entryLineFee3.CF_ChargeAmount = 100m;
			entryLineFee3.Validation.ValidateCF_ChargeType();
			AssertHasErrorContaining("When Duplicate Charge Type", entryLineFee3.CF_ChargeTypeInfo, "Duplicate Charge Type");

			entryLineFee3.CF_ChargeType = "89";
			entryLineFee3.NationalFeeTypeCode = "89";
			entryLineFee3.Validation.ValidateCF_ChargeType();
			AssertNoErrorContaining("When Duplicate Charge Type", entryLineFee3.CF_ChargeTypeInfo, "Duplicate Charge Type");
		}

		protected virtual void AssertNotificationsForEmptyMethodOfPayment(CusEntryLineFee entryLineFee) => AssertNoNotifications(entryLineFee.CF_MethodOfPaymentInfo);
	}
}
