using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(USCForeignPortFilterStripBusinessObject))]
	public class USCForeignPortFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestValidForTypeQuery()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var codeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "5860", "DUMMY 1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.AES);

			var codeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "89999", "DUMMY 2", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList2.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);

			var codeList3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "98554", "DUMMY 3", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList3.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();

			var port1 = Factory.Load<ZZRefCusCodeListCombined>(codeList1.PK);
			var port2 = Factory.Load<ZZRefCusCodeListCombined>(codeList2.PK);
			var port3 = Factory.Load<ZZRefCusCodeListCombined>(codeList3.PK);

			var bizObj = new USCForeignPortFilterStripBusinessObject();
			var filter = (ModuleTextFilter)bizObj["Valid For Type"];
			filter.IsActive = true;
			filter.Property = ForeignPortTypeList.Codes.AES;
			AssertEquals("port1", true, port1.MatchesFilter(bizObj.Filter));
			AssertEquals("port2", false, port2.MatchesFilter(bizObj.Filter));
			AssertEquals("port3", true, port3.MatchesFilter(bizObj.Filter));
			filter.Property = ForeignPortTypeList.Codes.InBond;
			AssertEquals("port1", false, port1.MatchesFilter(bizObj.Filter));
			AssertEquals("port2", true, port2.MatchesFilter(bizObj.Filter));
			AssertEquals("port3", true, port3.MatchesFilter(bizObj.Filter));
			filter.Property = "COM";
			AssertEquals("port1", false, port1.MatchesFilter(bizObj.Filter));
			AssertEquals("port2", false, port2.MatchesFilter(bizObj.Filter));
			AssertEquals("port3", true, port3.MatchesFilter(bizObj.Filter));
		}

		public void TestFilters()
		{
			var filter = new USCForeignPortFilterStripBusinessObject();
			AssertNotNull(filter["Code"]);
			AssertNotNull(filter["Name"]);
			AssertNotNull(filter["Valid For Type"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCForeignPortFilterStripBusinessObject();
	}
}
