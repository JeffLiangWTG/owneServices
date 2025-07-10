using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.Business.Testing
{
	[TestedType(typeof(CusClassification))]
	class CusClassificationTest : Customs.Business.Testing.BaseCusClassificationTest
	{
		public void TestDefaultValues()
		{
			AssertEquals("Country is set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Classification.CC_RN_NKCountryCode);
			AssertEquals("Type is set", CusClassification.ClassificationType.Both, Classification.CC_ClassificationType);
		}

		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<CusClassification>("Update Customs.Business.BaseCusClassification to include a decider for this class.", Factory.New(typeof(Customs.Business.BaseCusClassification)));
		}

		new CusClassification Classification => (CusClassification)base.Classification;
	}
}

