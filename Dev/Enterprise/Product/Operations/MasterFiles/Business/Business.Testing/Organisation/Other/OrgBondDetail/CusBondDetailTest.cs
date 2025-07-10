using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CusBondDetail))]
	sealed class CusBondDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidationAndLookups()
		{
			var bondData = Factory.New<CusBondDetail>();
			AssertEquals(typeof(CusBondDetailValidation), bondData.Validation.GetType());
			AssertEquals(typeof(CusBondDetailLookups), bondData.Lookups.GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var org = factory.New<OrgHeader>();
			org.OH_Code = "CBD-TEST";

			var result = factory.New<CusBondDetail>();
			result.Parent = org;
			return result;
		}
	}
}
