using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusGuaranteeHeaderCollection))]
	sealed class CusGuaranteeHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusGuaranteeHeaderCollection>
	{
		protected override CusGuaranteeHeaderCollection GetCollectionToTest()
		{
			return new CusGuaranteeHeaderCollection(Factory);
		}

		public void TestDefaultFilter()
		{
			var gua1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			Factory.NewWithValidTestData<BaseCusPermitHeader>();

			Factory.Save();

			var collection = new CusGuaranteeHeaderCollection(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { gua1 }, collection);
		}

		public void TestFilter_CountryCodeAndTypesAndReferences()
		{
			var gua1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			gua1.CPH_Type = "TRA";
			gua1.AdditionalGuaranteeReferences.AddNew("D1");

			var gua2 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			gua2.CPH_Type = "GEN";

			var gua3 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			gua3.CPH_Type = "TRA";
			gua3.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;

			var permit1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			permit1.CPH_Type = "TRA";

			Factory.Save();

			var collection = new CusGuaranteeHeaderCollection(Factory, new ZString[] { Core.Constants.CountryCodes.Eritrea }, new ZString[] { "TRA" }, new ZString[] { "D1", "D2" });
			AssertContainsExactElementsInAnyOrder(new[] { gua1 }, collection);
		}
	}
}
