using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCarrierAttributeCombined))]
	class ZZRefCarrierAttributeCombinedTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnly()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			ZZRefCarrierAttributeCombined attribute = carrier.Attributes.AddNew();
			carrier.ZZ4_IsSystem = false;
			AssertEquals(false, attribute.ReadOnly);
			attribute.ReadOnly = true;
			AssertEquals(true, attribute.ReadOnly);
			carrier.ZZ4_IsSystem = true;
			AssertEquals(true, attribute.ReadOnly);
			attribute.ReadOnly = false;
			AssertEquals(true, attribute.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var carrier = factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "BOB";
			carrier.ZZ4_Description = "BOB BOAT";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Estonia;
			return carrier.Attributes.AddNew("BOBAttribute", "SHORT");
		}
	}
}
