using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefPremisesGateCode))]
	sealed class RefPremiseGateCodeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCheckConstraintsForR5_DataProvider()
		{
			var premiseGateCodeDataProviderList = new PremiseGateCodeDataProviderList();
			premiseGateCodeDataProviderList.AddPairIfNotExist(string.Empty, string.Empty);

			foreach (CodeDescriptionPair item in premiseGateCodeDataProviderList)
			{
				var gateCode = Factory.NewWithValidTestData<RefPremisesGateCode>();
				gateCode.R5_DataProvider = item.Code;
				AssertNoExceptionThrown(item.Code, () => Factory.Save());
			}
		}

		public void TestCheckConstraintsForR5_OrgRegCodeType()
		{
			var premiseGateCodeDataProviderList = new PremiseGateCodeDataProviderList();
			premiseGateCodeDataProviderList.AddPairIfNotExist(string.Empty, string.Empty);

			foreach (CodeDescriptionPair item in premiseGateCodeDataProviderList)
			{
				var gateCode = Factory.NewWithValidTestData<RefPremisesGateCode>();
				gateCode.R5_OrgRegCodeType = item.Code;
				AssertNoExceptionThrown(item.Code, () => Factory.Save());
			}
		}

		public void TestHumanReadableNameCore()
		{
			var premisesGateCode = (RefPremisesGateCode)GetNewBusinessObject();
			premisesGateCode.R5_PremisesGateCode = "ABC";
			premisesGateCode.R5_PremisesGateDescription = "DEF";
			premisesGateCode.R5_DataProvider = "123";

			AssertEquals("Premises Codes - ABC - DEF(Provider: 123)", premisesGateCode.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<RefPremisesGateCode>();
		}
	}
}
