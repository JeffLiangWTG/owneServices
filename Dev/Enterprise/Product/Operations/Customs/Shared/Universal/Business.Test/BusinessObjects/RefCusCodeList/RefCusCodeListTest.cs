using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeList))]
	class RefCusCodeListTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDelete()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusCodeType = helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Cambodia, C.RefCusCodeListTypes.Codes.CustomsOffice, "B0B", "BOB THE BUILDER", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("BOBAttribute", "Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Cambodia, C.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("BOBAttribute1", "Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Cambodia, C.RefCusCodeListTypes.Codes.CustomsOffice);
			var attribute1 = cusCodeList.Attributes.AddNew("BOBAttribute", "SHORT");
			var attribute2 = cusCodeList.Attributes.AddNew("BOBAttribute1", "SHORT");
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cusCodeListInDiffFactory = newFactory.Load<RefCusCodeList>(cusCodeList.PK);
			cusCodeListInDiffFactory.Delete();
			newFactory.Save();
			AssertEquals("cusCodeList.IsDeleted", true, cusCodeList.IsDeleted);
			AssertEquals("attribute1.IsDeleted", true, attribute1.IsDeleted);
			AssertEquals("attribute2.IsDeleted", true, attribute1.IsDeleted);
		}

		public void TestCusCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var type1 = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities", Core.Constants.CountryCodes.Eritrea);
			var type2 = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			type2.ZZK_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Ethiopia;
			Factory.Save();
			var cusCodeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "ABC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			AssertEquals(type2.PK, cusCodeList1.CusCodeType.PK);
			cusCodeList1.ZZD_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Eritrea;
			AssertEquals(type1.PK, cusCodeList1.CusCodeType.PK);
			cusCodeList1.ZZD_ZZK_NKCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ;
			AssertNull(cusCodeList1.CusCodeType);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			return helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "A", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}
	}
}
