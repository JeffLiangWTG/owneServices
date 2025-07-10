using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	sealed class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		public void TestTypeDecider()
		{
			var entryLine = Factory.New<CusEntryLine>();
			AssertType(typeof(CusEntryLineFee), entryLine.Fees.AddNew());
		}

		public void TestChargeAmountDecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryLineFee), nameof(CusEntryLineFee.CF_ChargeAmount), false, x => x.DecimalPlaces == 2);
		}

		public void TestBaseValueDecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryLineFee), nameof(CusEntryLineFee.CF_BaseValue), false, x => x.DecimalPlaces == 2);
		}

		public void TestRateDecimalPlaces()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(CusEntryLineFee), nameof(CusEntryLineFee.CF_Rate), false, x => x.DecimalPlaces == 2);
		}

		public void TestChargeAmountRefresher()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			AssertType<ChargeAmountRefresher>("ChargeAmountRefresher Type", lineFee.ChargeAmountRefresher);
		}

		public void TestNationalFeeTypeCode()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.NationalFeeTypeCode = DeclarationHelper.NationalVatType.Code;

			CombineAssertions(() =>
			{
				AssertEquals("NationalType", DeclarationHelper.NationalVatType.Code, lineFee.NationalFeeTypeCode);
				AssertEquals("NationalType Should be readonly", false, lineFee.NationalFeeTypeCodeInfo.ReadOnly);
				AssertEquals("Duty Type", DataBoundResourceStrings.GetDataForProperty(lineFee.NationalFeeTypeCodeInfo).Caption);
			});
		}

		public void TestCF_ChargeType()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.NationalFeeTypeCode = DeclarationHelper.NationalVatType.Code;

			CombineAssertions(() =>
			{
				AssertEquals("NationalType", DeclarationHelper.NationalVatType.Code, lineFee.NationalFeeTypeCode);
				AssertEquals("CF_ChargeType", FeeTypeList.Codes.B00, lineFee.CF_ChargeType);
				AssertEquals("NationalType", "40", lineFee.NationalFeeTypeCode);

				lineFee.NationalFeeTypeCode = "61";
				AssertEquals("NationalType", "61", lineFee.CF_ChargeType);
			});
		}
	}
}
