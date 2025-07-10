using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeListAttribute.Loader))]
	class RefCusCodeListAttributeLoaderTest : LoaderTestCase
	{
		public void TestLoad_HavingAttributeName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice", Core.Constants.CountryCodes.Eritrea);
			Factory.Save();
			var cusCodeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BOB", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var cusCodeList1Attribute = helper.CreateCusCodeListAttribute(cusCodeList1.PK, "HELLO", "1");
			var cusCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BOB", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 5, 1));
			var cusCodeList2Attribute = helper.CreateCusCodeListAttribute(cusCodeList2.PK, "HELLO", "2");
			var cusCodeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BOB", new ZDateTime(2016, 7, 1), new ZDateTime(2016, 12, 1));
			var cusCodeList3Attribute = helper.CreateCusCodeListAttribute(cusCodeList3.PK, "HELLO", "3");
			var cusCodeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BOB", new ZDateTime(2016, 6, 1), new ZDateTime(2016, 12, 1));
			var cusCodeList4Attribute = helper.CreateCusCodeListAttribute(cusCodeList4.PK, "HELLO", "4");
			var cusCodeList5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BOB", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var cusCodeList5Attribute = helper.CreateCusCodeListAttribute(cusCodeList5.PK, "HELLO", "5");
			var cusCodeList6 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "JOE", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var cusCodeList6Attribute = helper.CreateCusCodeListAttribute(cusCodeList6.PK, "HELLO", "6");
			var cusCodeList7 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BOB", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 7, 1));
			var cusCodeList7Attribute = helper.CreateCusCodeListAttribute(cusCodeList7.PK, "HI", "7");
			var cusCodeListAttributes = new RefCusCodeListAttribute.Loader(Factory).Load(Core.Constants.CountryCodes.Eritrea, new ZDateTime(2016, 6, 10), Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BOB", "HELLO");
			AssertEquals(2, cusCodeListAttributes.Length);
			AssertCollectionContains(cusCodeList1Attribute, cusCodeListAttributes);
			AssertCollectionContains(cusCodeList4Attribute, cusCodeListAttributes);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusCodeListAttribute.Loader(Factory);
		}
	}
}
