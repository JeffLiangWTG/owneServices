using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefLanguageType))]
	class RefLanguageTypeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCodeProperty()
		{
			var languageType = Factory.New<RefLanguageType>();
			languageType.ZX6_Language = "ZHS";
			AssertEquals("ZHS", CodePropertyAttribute.CodeFromBusinessObject(languageType));
		}
	}
}
