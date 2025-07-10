using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsSupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeCodeList()
		{
			var trCountryCode = Core.Constants.CountryCodes.Turkey;
			var supDocTypeCode = "DC44S";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(trCountryCode, "Turkey");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");
			helper.CreateNewOrGetExistingCusCodeType(supDocTypeCode, "Supporting Document Of NCTS");
			helper.CreateNewOrGetExistingCusCodeType("DC000", "Invalid Type");
			Factory.Save();

			var refCusCodeList1 = helper.CreateCusCodeList(trCountryCode, supDocTypeCode, "SD01", "SD01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateCusCodeList(trCountryCode, supDocTypeCode, "SD02", "SD02 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, supDocTypeCode, "INV01", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList4 = helper.CreateCusCodeList(trCountryCode, "DC000", "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList5 = helper.CreateCusCodeList(trCountryCode, supDocTypeCode, "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList6 = helper.CreateCusCodeList(trCountryCode, supDocTypeCode, "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var goodsItem = departureMovement.GoodsItems.AddNew();
			var supportingDocument = goodsItem.SupportingDocuments.AddNew();
			var lookups = supportingDocument.Lookups;
			var typeCodesList = lookups.TypeCodeList;
			var completeFilter = typeCodesList.CompleteFilter;

			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("List Type", typeCodesList);
				AssertSame("Cached", typeCodesList, supportingDocument.Lookups.TypeCodeList);
				AssertEquals("Matched TR", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));

				typeCodesList.Load();
				AssertEquals(2, typeCodesList.Count);
				Assert(typeCodesList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "SD01"));
				Assert(typeCodesList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "SD02"));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
	}
}
