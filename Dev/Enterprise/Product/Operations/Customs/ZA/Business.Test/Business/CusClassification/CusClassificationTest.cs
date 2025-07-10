using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusClassification))]
	sealed class CusClassificationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFormatterIsAttached()
		{
			testClassification.CC_TariffNum = "3902.10.3";
			AssertEquals("3902103", testClassification.CC_TariffNum);
		}

		public void TestDefaultValues()
		{
			AssertEquals("Country is set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, testClassification.CC_RN_NKCountryCode);
			AssertEquals("Type is set", CusClassification.ClassificationType.Both, testClassification.CC_ClassificationType);
		}

		CusClassification testClassification;
		protected override void SetUp()
		{
			base.SetUp();
			testClassification = Factory.New<CusClassification>();
		}
	}
}
