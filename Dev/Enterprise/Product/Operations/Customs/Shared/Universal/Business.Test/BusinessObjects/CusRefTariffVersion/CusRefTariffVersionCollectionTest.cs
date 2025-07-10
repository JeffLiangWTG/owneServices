using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTariffVersionCollection))]
	class CusRefTariffVersionCollectionTest : ActiveBusinessObjectCollectionTestCase<CusRefTariffVersionCollection>
	{
		public void TestFilteredByCountryCode()
		{
			var version1 = Factory.New<CusRefTariffVersion>();
			version1.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var version2 = Factory.New<CusRefTariffVersion>();
			version2.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			CombineAssertions(() =>
			{
				var collection = new CusRefTariffVersionCollection(Factory, Core.Constants.CountryCodes.Australia);
				AssertEquals("match", true, version1.MatchesFilter(collection.CompleteFilter));
				AssertEquals("un-match", false, version2.MatchesFilter(collection.CompleteFilter));
			}

			);
		}

		protected override CusRefTariffVersionCollection GetCollectionToTest() => new CusRefTariffVersionCollection(Factory);
	}
}
