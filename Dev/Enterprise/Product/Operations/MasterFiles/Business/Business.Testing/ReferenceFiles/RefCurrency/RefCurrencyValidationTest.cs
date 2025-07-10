using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCurrencyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRX_Code()
		{
			Currency.RX_Code = ZString.Empty;
			Assert("Expecting RX_Code to be empty and have errors.", Currency.RX_CodeInfo.HasErrors());
			Currency.RX_Code = "A";
			Assert("Expecting RX_Code to have errors because it is not 3 characters long.", Currency.RX_CodeInfo.HasErrors());
			Currency.RX_Code = "wer";
			Assert("RX_Code should be correct, not expecting errors.", !Currency.RX_CodeInfo.HasNotifications());
		}

		public void TestValidateRX_Symbol()
		{
			Currency.RX_Symbol = ZString.Empty;
			Assert("Expecting RX_Symbol to be empty and have errors.", Currency.RX_SymbolInfo.HasErrors());
			Currency.RX_Symbol = "$";
			Assert("RX_Symbol should be correct, not expecting errors.", !Currency.RX_SymbolInfo.HasNotifications());
		}

		public void TestValidateRX_UnitName()
		{
			Currency.RX_UnitName = ZString.Empty;
			Assert("Expecting RX_UnitName to be empty and have errors.", Currency.RX_UnitNameInfo.HasErrors());
			Currency.RX_UnitName = "Dollars";
			Assert("RX_UnitName should be correct, not expecting errors.", !Currency.RX_UnitNameInfo.HasNotifications());
		}

		public void TestValidateRX_SubUnitName()
		{
			Currency.RX_SubUnitName = ZString.Empty;
			AssertNoErrors("Expecting RX_SubUnitName to be empty but not have errors as it's not mandatory.", Currency.RX_SubUnitNameInfo);
			Currency.RX_SubUnitName = "Cents";
			AssertNoErrors("RX_SubUnitName should be correct, and still not have errors.", Currency.RX_SubUnitNameInfo);
		}

		public void TestValidateRX_SubUnitRatio()
		{
			Currency.RX_SubUnitRatio = 0;
			Assert("Expecting RX_SubUnitRatio to have errors, because value is 0.", Currency.RX_SubUnitRatioInfo.HasErrors());

			Currency.RX_SubUnitRatio = 23;
			Assert("Expecting RX_SubUnitRatio to have errors, because value is not 1, 10, 100 or 1000.", Currency.RX_SubUnitRatioInfo.HasErrors());

			Currency.RX_SubUnitRatio = 1;
			Assert("RX_SubUnitRatio should be correct, not expecting errors.", !Currency.RX_SubUnitRatioInfo.HasNotifications());
		}

		public void TestValidateRX_ISOSubUnitRatio()
		{
			Currency.RX_ISOSubUnitRatio = 0;
			Assert("Expecting RX_ISOSubUnitRatio to have errors, because value is 0.", Currency.RX_ISOSubUnitRatioInfo.HasErrors());

			Currency.RX_ISOSubUnitRatio = 60;
			Assert("Expecting RX_ISOSubUnitRatio to have errors, because value is not 1, 10, 100 or 1000.", Currency.RX_ISOSubUnitRatioInfo.HasErrors());

			Currency.RX_ISOSubUnitRatio = 1;
			Assert("RX_ISOSubUnitRatio should be correct, not expecting errors.", !Currency.RX_ISOSubUnitRatioInfo.HasNotifications());

			Currency.RX_ISOSubUnitRatio = 10;
			Assert("RX_ISOSubUnitRatio should be correct, not expecting errors.", !Currency.RX_ISOSubUnitRatioInfo.HasNotifications());

			Currency.RX_ISOSubUnitRatio = 100;
			Assert("RX_ISOSubUnitRatio should be correct, not expecting errors.", !Currency.RX_ISOSubUnitRatioInfo.HasNotifications());
		}

		public void TestValidateRX_Desc()
		{
			Currency.RX_Desc = ZString.Empty;
			Assert("Expecting RX_Desc to be empty and have errors.", Currency.RX_DescInfo.HasErrors());
			Currency.RX_Desc = "Australia, Dollars";
			Assert("RX_Desc should be correct, not expecting errors.", !Currency.RX_DescInfo.HasNotifications());
		}

		#region Implementation

		RefCurrency Currency;

		protected override void SetUp()
		{
			base.SetUp();
			Currency = Factory.New<RefCurrency>();
		}

		#endregion
	}
}
