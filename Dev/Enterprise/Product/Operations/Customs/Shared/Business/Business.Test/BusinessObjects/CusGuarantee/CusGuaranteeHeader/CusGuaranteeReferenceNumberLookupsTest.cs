using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusGuaranteeReferenceNumberLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var cusGuarantee = Factory.New<BaseCusGuaranteeHeader>();
			var cusGuaranteeReferenceNumber = cusGuarantee.AdditionalGuaranteeReferences.AddNew();
			AssertEquals(0, ((CusGuaranteeReferenceNumberLookups)cusGuaranteeReferenceNumber.Lookups).CY_CodeList.Count);
		}
	}
}
