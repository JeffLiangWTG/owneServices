using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CusRefRateCodeControlTest : TestCaseWithFactory
	{
		public void TestControl()
		{
			var testItem = Factory.NewWithValidTestData<CusRefRateCode>();
			using (var testControl = new CusRefRateCodeControl())
			using (var testForm = new ZForm(testItem))
			{
				testForm.Show();
				AssertEquals("CountryCodeFindBox", "CR7_RN_NKCountryCode", GetBindingMemberByName(testControl, "CountryCodeFindBox"));
				AssertEquals("RateCodeTextBox", "CR7_RateCode", GetBindingMemberByName(testControl, "RateCodeTextBox"));
				AssertEquals("DescriptionTextBox", "CR7_Description", GetBindingMemberByName(testControl, "DescriptionTextBox"));
				AssertEquals("RateTypeGropEdit", "CR7_RateType", GetBindingMemberByName(testControl, "RateTypeGropEdit"));
			}
		}

		string GetBindingMemberByName(CusRefRateCodeControl parentControl, string subControlName)
		{
			var subControl = parentControl.Controls.Find(subControlName, true)[0];
			var bindingMember = parentControl.BindingSource.GetBindingMember(subControl);
			return bindingMember;
		}
	}
}
