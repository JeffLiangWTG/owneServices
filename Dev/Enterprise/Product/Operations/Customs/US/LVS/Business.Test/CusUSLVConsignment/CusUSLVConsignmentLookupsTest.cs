using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	internal class CusUSLVConsignmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConsigneePostCodes()
		{
			var country = Factory.New<RefCountry>();
			country.Code = "ZZ";
			country.RN_IsActive = true;
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "1000";
			postCode.RK_RN_NKCountry = country.Code;
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2000";
			postCode2.RK_RN_NKCountry = country.Code;
			var postCode3 = Factory.New<RefPostCode>();
			postCode3.RK_CityTownPostCode = "3000";
			postCode3.RK_RN_NKCountry = "AU";
			Factory.Save();

			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.ULB_RN_NKConsigneeCountry = "ZZ";

			AssertArrayEqualsByElements(new[] { postCode.RK_CityTownPostCode, postCode2.RK_CityTownPostCode },
				consignment.Lookups.ConsigneePostCodes.Select(p => p.RK_CityTownPostCode).ToArray());
		}

		public void TestSellerPostCodes()
		{
			var country = Factory.New<RefCountry>();
			country.Code = "ZZ";
			country.RN_IsActive = true;
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "1000";
			postCode.RK_RN_NKCountry = country.Code;
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2000";
			postCode2.RK_RN_NKCountry = country.Code;
			var postCode3 = Factory.New<RefPostCode>();
			postCode3.RK_CityTownPostCode = "3000";
			postCode3.RK_RN_NKCountry = "AU";
			Factory.Save();

			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.ULB_RN_NKSellerCountry = "ZZ";

			AssertArrayEqualsByElements(new[] { postCode.RK_CityTownPostCode, postCode2.RK_CityTownPostCode },
				consignment.Lookups.SellerPostCodes.Select(p => p.RK_CityTownPostCode).ToArray());
		}

		public void TestLowValueEntryTypeList()
		{
			var shipment = Factory.New<CusUSLVConsignment>();
			var list = shipment.Lookups.LowValueEntryTypeList;
			var codes = EntryTypeList.GetLowValueDeclarationEntryTypeList();

			CombineAssertions("LowValueEntryTypeList contains all pairs in GetLowValueDeclarationEntryTypeList", () =>
			{
				AssertEquals(2, list.Count);
				AssertEquals(EntryTypeList.Codes.LowValue, codes[0].Code);
				AssertEquals(EntryTypeList.Descriptions.LowValue, codes[0].Description);
				AssertEquals(EntryTypeList.Codes.InformalFreeDutiable, codes[1].Code);
				AssertEquals(EntryTypeList.Descriptions.InformalFreeDutiable, codes[1].Description);
			});
		}
	}
}
