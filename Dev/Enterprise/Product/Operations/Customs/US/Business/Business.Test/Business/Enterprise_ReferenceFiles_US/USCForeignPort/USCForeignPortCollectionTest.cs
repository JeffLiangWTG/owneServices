using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCForeignPortCollection))]
	public class USCForeignPortCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(USCForeignPortCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new USCForeignPortCollection(Factory);
		}

		public void TestInitialiseFilterDefaults()
		{
			var collection = new USCForeignPortCollection(Factory);
			collection.Load();
			AssertEquals(3, collection.Count);

			collection = new USCForeignPortCollection(Factory, USCForeignPortWrapper.Type.Common);
			collection.Load();
			AssertEquals(1, collection.Count);

			collection = new USCForeignPortCollection(Factory, USCForeignPortWrapper.Type.AES);
			collection.Load();
			AssertEquals(2, collection.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var codeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "5860", "DUMMY 1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.AES);

			var codeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "89999", "DUMMY 2", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList2.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);

			var codeList3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "98554", "DUMMY 3", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList3.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);

			helper.CreateNewOrGetExistingCusCodeType("Test", "Test");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, "Test", "5860", "DUMMY 1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Cambodia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "5860", "DUMMY 1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}
	}
}
