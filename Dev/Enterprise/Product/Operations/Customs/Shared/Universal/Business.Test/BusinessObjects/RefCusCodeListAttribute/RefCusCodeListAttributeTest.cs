using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeListAttribute))]
	[CountrySpecificTest(Core.Constants.CountryCodes.Australia)]
	internal class RefCusCodeListAttributeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTriggerException_InvalidName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			Factory.Save();
			helper.CreateNewOrGetExistingDataGrouping("XA", "Some Country");
			helper.CreateCusCodeType("TYPE", "Type");
			var refCusCodeList = helper.CreateCusCodeList("XA", "TYPE", "One", "Code Type One", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Name1", "Desc.", "TYPE", "XA", "TYPE");
			Factory.Save();
			helper.CreateCusCodeListAttribute(refCusCodeList.PK, "Name1", "111", createAttributeName: false);
			Factory.Save();
			helper.CreateCusCodeListAttribute(refCusCodeList.PK, "Name2", "222", createAttributeName: false);
			AssertExceptionThrown<ZSaveException>("Invalid name", () => Factory.Save());
		}

		public void TestTriggerException_InvalidCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			Factory.Save();
			helper.CreateNewOrGetExistingDataGrouping("XA", "Some Country");
			helper.CreateNewOrGetExistingDataGrouping("XB", "Some Other Country");
			helper.CreateCusCodeType("TYPE", "Type", "XA");
			helper.CreateCusCodeType("TYP2", "Typ2", "XB");
			helper.CreateCusCodeType("TYP2", "Typ2", "XA");
			var refCusCodeList = helper.CreateCusCodeList("XA", "TYPE", "One", "Code Type One", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateCusCodeList("XB", "TYP2", "Two", "Code Type Two", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Name1", "111", "TYPE", "XA", "TYPE");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Name2", "111", "TYP2", "XA", "TYP2");
			Factory.Save();
			helper.CreateCusCodeListAttribute(refCusCodeList.PK, "Name1", "111", createAttributeName: false);
			Factory.Save();
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, "Name1", "222", createAttributeName: false);
			AssertExceptionThrown<ZSaveException>("Invalid name", () => Factory.Save());
		}

		public void TestTriggerException_InvalidCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			Factory.Save();
			helper.CreateNewOrGetExistingDataGrouping("XA", "Some Country");
			helper.CreateNewOrGetExistingDataGrouping("XB", "Some Other Country");
			helper.CreateCusCodeType("TYPE", "Type", "XA");
			helper.CreateCusCodeType("TYP2", "Typ2", "XB");
			var refCusCodeList = helper.CreateCusCodeList("XA", "TYPE", "One", "Code Type One", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateCusCodeList("XB", "TYP2", "Two", "Code Type Two", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Name1", "111", "TYPE", "XA", "TYPE");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Name2", "111", "TYP2", "XB", "TYP2");
			Factory.Save();
			helper.CreateCusCodeListAttribute(refCusCodeList.PK, "Name1", "111", createAttributeName: false);
			Factory.Save();
			helper.CreateCusCodeListAttribute(refCusCodeList.PK, "Name2", "222", createAttributeName: false);
			AssertExceptionThrown<ZSaveException>("Invalid name", () => Factory.Save());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica, "South Africa");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var cusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "A", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Name", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			var attribute = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodeList.PK, attributeName.ZXE_Name, "Value");
			return attribute;
		}
	}
}
