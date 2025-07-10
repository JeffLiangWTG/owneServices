using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Universal.Testing
{
	class RefCusMapTypeListTest : TestCaseWithFactory
	{
		public void TestGetList()
		{
			Factory.Load<RefCusMapType>(new ZQuery()).DeleteAll();
			var codeType1 = CreateMapType("CSTA", "INW", "Customs Status", ZBool.False);
			var codeType2 = CreateMapType("REL", "BTH", "Related Party Indicator", ZBool.True);
			var codeType3 = CreateMapType("ZADOC", "OUT", "ZA Supporting Document Types", ZBool.False);
			var codeType4 = CreateMapType("EXPST", "OUT", "Export State Mapping", ZBool.True);
			var codeType5 = CreateMapType("IMPST", "OUT", "Import State Mapping", ZBool.True);
			var codeType6 = CreateMapType("HAFEE", "OUT", "Harbour Fee", ZBool.True);
			var codeType7 = CreateMapType("TWI2C", "OUT", "Taiwan Invoice to Customs Invoice Unit Mapping", ZBool.True);
			_ = CreateMapType("RCODE", "OUT", "Tax Revenue Code", ZBool.True);
			_ = CreateMapType("RATET", "OUT", "BR Rate Types", ZBool.True);
			var list1 = RefCusMapTypeList.GetList(Factory);
			var list2 = RefCusMapTypeList.GetList(Factory);
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 9, list1.Count);
			AssertEquals("description", "Customs Status", list1.GetDescriptionFromCode("CSTA"));
			AssertEquals("description", "Related Party Indicator", list1.GetDescriptionFromCode("REL"));
			AssertEquals("description", "ZA Supporting Document Types", list1.GetDescriptionFromCode("ZADOC"));
			AssertEquals("description", "Export State Mapping", list1.GetDescriptionFromCode("EXPST"));
			AssertEquals("description", "Import State Mapping", list1.GetDescriptionFromCode("IMPST"));
			AssertEquals("description", "Harbour Fee", list1.GetDescriptionFromCode("HAFEE"));
			AssertEquals("description", "Taiwan Invoice to Customs Invoice Unit Mapping", list1.GetDescriptionFromCode("TWI2C"));
			AssertEquals("description", "Tax Revenue Code", list1.GetDescriptionFromCode("RCODE"));
			AssertEquals("description", "BR Rate Types", list1.GetDescriptionFromCode("RATET"));
		}

		public void TestGetListOfEditableTypes()
		{
			Factory.Load<RefCusMapType>(new ZQuery()).DeleteAll();
			var codeType1 = CreateMapType("CSTA", "INW", "Customs Status", ZBool.False);
			var codeType2 = CreateMapType("REL", "BTH", "Related Party Indicator", ZBool.True);
			var codeType3 = CreateMapType("ZADOC", "OUT", "ZA Supporting Document Types", ZBool.False);
			var list1 = RefCusMapTypeList.GetListOfEditableTypes(Factory);
			var list2 = RefCusMapTypeList.GetListOfEditableTypes(Factory);
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 2, list1.Count);
			AssertEquals("description", "Customs Status", list1.GetDescriptionFromCode("CSTA"));
			AssertEquals("description", null, list1.GetDescriptionFromCode("REL"));
			AssertEquals("description", "ZA Supporting Document Types", list1.GetDescriptionFromCode("ZADOC"));
		}

		RefCusMapType CreateMapType(ZString type, ZString direction, ZString desc, ZBool isReadonly)
		{
			var mapType = Factory.New<RefCusMapType>();
			mapType.ZZP_MapType = type;
			mapType.ZZP_Direction = direction;
			mapType.ZZP_Description = desc;
			mapType.ZZP_IsReadonly = isReadonly;
			return mapType;
		}
	}
}
