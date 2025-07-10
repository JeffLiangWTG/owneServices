using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.GUI.ConvertToLocalCurrencyControl;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class ConvertToLocalCurrencyControlTest : TestCaseWithFactory
	{
		public void TestSetCurrencyCodeFindBoxReadOnly()
		{
			using (var control = new ConvertToLocalCurrencyControl())
			{
				control.SetCurrencyCodeFindBoxReadOnly(false);
				AssertEquals("ReadOnly should be false", false, control.Controls["UnitFindBox"].GetReadOnly());

				control.SetCurrencyCodeFindBoxReadOnly(true);
				AssertEquals("ReadOnly should be true", true, control.Controls["UnitFindBox"].GetReadOnly());
			}
		}

		public void TestCurrencyConverterFromDelegate()
		{
			using (ConvertToLocalCurrencyControl control = new ConvertToLocalCurrencyControl())
			{
				control.CurrencyConverterGetterMethod = new CurrencyConverterGetter(TestHelperCurrencyConverterMethod);
				AssertNotNull("CurrencyConverter", control.CurrencyConverterGetterMethod);
			}
		}

		public void TestCurrencyConverterFromNullDelegate()
		{
			using (ConvertToLocalCurrencyControl control = new ConvertToLocalCurrencyControl())
			{
				control.CurrencyConverterGetterMethod = null;
				AssertNull("CurrencyConverter", control.CurrencyConverterGetterMethod);
			}
		}

		CurrencyConverter TestHelperCurrencyConverterMethod() => CurrencyConverter.New(Factory);
	}
}
