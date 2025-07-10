using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusPreference))]
	internal class RefCusPreferenceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping("EUN");
			var bo = (RefCusPreference)base.GetNewBusinessObjectForDeleteTest(factory);
			bo.ZZS_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			return bo;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var bo = base.GetBusinessObjectForFetchForLoad();
			((RefCusPreference)bo).ZZS_ZZZ_NKDataGrouping = "EUN";
			return bo;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateOrGetLanguage("ENG", "English");
			helper.CreateOrGetLanguage("CHS", "Chinese");
		}
	}
}
