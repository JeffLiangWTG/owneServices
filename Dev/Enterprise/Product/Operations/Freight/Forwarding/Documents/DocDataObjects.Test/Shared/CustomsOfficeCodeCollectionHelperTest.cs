using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	public class CustomsOfficeCodeCollectionHelperTest : TestCaseWithFactory
	{
		[TestDate(2024, 09, 19)]
		public void TestGetEuropeanUnionECICSList()
		{
			var europeanUnionECICSList = CustomsOfficeCodeCollectionHelper.GetEuropeanUnionECICSList(Factory);

			AssertNotNull(europeanUnionECICSList);
			AssertEquals(3, europeanUnionECICSList.FilterBusinessObjectDefaults.Count);

			AssertEquals(DocDataConstants.RefCusCodeListType.Code_ECICS, europeanUnionECICSList.FilterBusinessObjectDefaults[Customs.Universal.Constants.ZZRefCusCodeListFilters.ListType + ":Property"].Value);
			Assert(!europeanUnionECICSList.FilterBusinessObjectDefaults[Customs.Universal.Constants.ZZRefCusCodeListFilters.ListType + ":Property"].IsRemovable);

			AssertEquals(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, europeanUnionECICSList.FilterBusinessObjectDefaults[Customs.Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping + ":Property"].Value);
			Assert(!europeanUnionECICSList.FilterBusinessObjectDefaults[Customs.Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping + ":Property"].IsRemovable);

			AssertEquals(new ZDateTime(2024, 09, 19), europeanUnionECICSList.FilterBusinessObjectDefaults[Customs.Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate + ":Property1"].Value);
			Assert(europeanUnionECICSList.FilterBusinessObjectDefaults[Customs.Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate + ":Property1"].IsRemovable);
		}
	}
}
