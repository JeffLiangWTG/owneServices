using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccTaxRateController))]
	sealed class AccTaxRateControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccTaxRate;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = Factory.LoadTop1<GlbCompany>(new ZQuery()).GC_RN_NKCountryCode;
			Factory.Save();
			return taxRate;
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
			AssertEquals("You are not allowed to add a new tax code", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
