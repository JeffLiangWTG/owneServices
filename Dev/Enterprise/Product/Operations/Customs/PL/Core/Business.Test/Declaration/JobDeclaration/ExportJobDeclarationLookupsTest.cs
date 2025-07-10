using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using EURefCusCodeListTypes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestGoodsOrigin()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, parent: grouping);

		helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EX15, "Origin country/territory for entry style EX");
		helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_CO15, "Origin country/territory for entry style CO");
		helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EU15, "Origin country/territory for entry style EU");

		helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, EURefCusCodeListTypes.Code_EX15, "AA", "Test AA", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, EURefCusCodeListTypes.Code_CO15, "BB", "Test BB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, EURefCusCodeListTypes.Code_CO15, "CC", "Test CC", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, EURefCusCodeListTypes.Code_EU15, "DD", "Test DD", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, EURefCusCodeListTypes.Code_EU15, "EE", "Test EE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

		Factory.Save();

		CombineAssertions(() =>
		{
			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			AssertEquals("EX Entry Style", "AA", ((CodeDescriptionPairList)lookups.GoodsOrigin).CodesAsString);

			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			AssertEquals("CO Entry Style", "BB", ((CodeDescriptionPairList)lookups.GoodsOrigin).CodesAsString);

			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
			AssertEquals("EU Entry Style", "DD", ((CodeDescriptionPairList)lookups.GoodsOrigin).CodesAsString);
		});
	}

	public void TestGoodsDestination()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, parent: grouping);

		helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EX17, "Destination country/territory for entry style EX");
		helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_CO17, "Destination country/territory for entry style CO");
		helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EU17, "Destination country/territory for entry style EU");

		helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, EURefCusCodeListTypes.Code_EX17, "AA", "Test AA", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, EURefCusCodeListTypes.Code_CO17, "BB", "Test BB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, EURefCusCodeListTypes.Code_CO17, "CC", "Test CC", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, EURefCusCodeListTypes.Code_EU17, "DD", "Test DD", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, EURefCusCodeListTypes.Code_EU17, "EE", "Test EE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

		Factory.Save();

		CombineAssertions(() =>
		{
			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			AssertEquals("EX Entry Style", "AA", ((CodeDescriptionPairList)lookups.GoodsDestination).CodesAsString);

			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			AssertEquals("CO Entry Style", "BB", ((CodeDescriptionPairList)lookups.GoodsDestination).CodesAsString);

			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
			AssertEquals("EU Entry Style", "DD", ((CodeDescriptionPairList)lookups.GoodsDestination).CodesAsString);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		lookups = jobDeclaration.Lookups;
	}

	JobDeclaration jobDeclaration;
	JobDeclarationLookups lookups;
}
