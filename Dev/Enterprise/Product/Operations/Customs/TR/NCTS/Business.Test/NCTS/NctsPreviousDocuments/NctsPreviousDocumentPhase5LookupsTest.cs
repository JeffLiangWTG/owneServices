using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentPhase5LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWeightUQList()
		{
			var document = Factory.New<NctsPreviousDocument>();

			AssertSame(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), document.Lookups.WeightUQList);
		}

		public void TestIncotermList()
		{
			var document = Factory.New<NctsPreviousDocument>();

			AssertSame(Factory.GetCachedValue<IncotermCodeList>(), document.Lookups.IncotermList);
		}

		public void TestSubTypeList()
		{
			var document = Factory.New<NctsPreviousDocument>();

			AssertSame(Factory.GetCachedValue<PaymentTypeList>(), document.Lookups.SubTypeList);
		}

		public void TestNatureOfBusinessList()
		{
			var yesterday = ZDateTime.Now.AddDays(-1);
			var tomorrow = ZDateTime.Now.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var natureOfBusinessType = "TRNOB";
			helper.CreateCusCodeType(natureOfBusinessType, natureOfBusinessType);
			helper.CreateCusCodeList("TR", natureOfBusinessType, "99", "Diger Ticari Islemler", yesterday, tomorrow);
			helper.CreateCusCodeList("TR", natureOfBusinessType, "22", "Iade Edilen Esyanin Degistirilmesi", yesterday, tomorrow);
			Factory.Save();

			var document = Factory.New<NctsPreviousDocument>();

			AssertContainsExactElementsInAnyOrder("Turkey's previous document types", new string[] { "99", "22" }, document.Lookups.NatureOfBusinessList.GetAllCodes());
		}

		public void TestLookups()
		{
			var document = Factory.New<NctsPreviousDocument>();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var bills = nctsHeader.MovementHeader.Header.Bills.AddNew();
			var goodsitems = bills.GoodsItems.AddNew();
			var previousDocument2 = goodsitems.PreviousDocuments.AddNew();

			AssertEquals("Ncts Previous Document Lookups", typeof(NctsPreviousDocumentPhase5Lookups), previousDocument2.Lookups.GetType());
		}

		public void TestPreviousDocumentsCodeList()
		{
			var trCountryCode = Core.Constants.CountryCodes.Turkey;
			var preDocTypeCode = "DC44P";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(trCountryCode, "Turkey");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");
			helper.CreateNewOrGetExistingCusCodeType(preDocTypeCode, "Previous Document Of NCTS");
			helper.CreateNewOrGetExistingCusCodeType("DC000", "Invalid Type");
			Factory.Save();

			var refCusCodeList1 = helper.CreateCusCodeList(trCountryCode, preDocTypeCode, "PD01", "PD01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateCusCodeList(trCountryCode, preDocTypeCode, "PD02", "PD02 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, preDocTypeCode, "INV01", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList4 = helper.CreateCusCodeList(trCountryCode, "DC000", "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList5 = helper.CreateCusCodeList(trCountryCode, preDocTypeCode, "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList6 = helper.CreateCusCodeList(trCountryCode, preDocTypeCode, "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var bills = nctsHeader.MovementHeader.Header.Bills.AddNew();
			var goodsitems = bills.GoodsItems.AddNew();
			var previousDocument = goodsitems.PreviousDocuments.AddNew();

			var lookups = previousDocument.Lookups;
			var previousDocumentsCodeList = lookups.PreviousDocumentsCodeList;
			var completeFilter = previousDocumentsCodeList.CompleteFilter;

			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("List Type", previousDocumentsCodeList);
				AssertSame("Cached", previousDocumentsCodeList, previousDocumentsCodeList);
				AssertEquals("Matched TR", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));

				previousDocumentsCodeList.Load();
				AssertEquals(2, previousDocumentsCodeList.Count);
				Assert(previousDocumentsCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "PD01"));
				Assert(previousDocumentsCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "PD02"));
			});
		}
	}
}
