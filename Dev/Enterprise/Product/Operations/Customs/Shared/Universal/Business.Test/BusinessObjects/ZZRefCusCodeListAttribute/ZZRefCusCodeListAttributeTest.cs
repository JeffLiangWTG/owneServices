using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusCodeListAttribute))]
	internal class ZZRefCusCodeListAttributeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var codeList = Factory.NewWithValidTestData<ZZRefCusCodeList>();
			Factory.Save();
			var attribute = factory.New<ZZRefCusCodeListAttribute>();
			attribute.ZZE_Value = "VWG";
			attribute.ZZE_ZZD_CodeList = codeList.PK;
			attribute.ZZE_ZXE_NKName = "A";
			return attribute;
		}
	}
}
