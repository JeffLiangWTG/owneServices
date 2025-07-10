using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusAttributeFilter))]
	sealed class CusAttributeFilterTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var attrib = pivot.Attributes1.AddNew();
			AssertEquals(typeof(CusAttributeFilterValidation), attrib.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var part = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var attributeFilter = pivot.Attributes1.AddNew();
			return attributeFilter;
		}
	}
}
