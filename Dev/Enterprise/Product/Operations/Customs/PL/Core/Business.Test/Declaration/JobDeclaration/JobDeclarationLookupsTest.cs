using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

public class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals(declaration.Lookups.Declaration, declaration);
	}

	public void TestPLRepresentationTypeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.Lookups.PLRepresentationTypeList.AssertContainsExactCodes("Poland should contain specified list from PLRepresentationTypeList",
			new [] { "1", "2", "3", "4", "5" });
	}

	public void TestGoodsLocationTypeList_Import()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.Lookups.GoodsLocationTypeList.AssertContainsExactCodes("Poland should contain specified list from GoodsLocationTypeList",
			new [] { "CUS", "GLC", "OTH" });
	}

	public void TestGoodsLocationTypeList_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ZG_ExportManifest = false;

		var list = declaration.Lookups.GoodsLocationTypeList;
		CombineAssertions(() =>
		{
			list.AssertContainsExactCodes("Codes - All", new [] { "U", "V", "W", "X", "Y", "Z" });

			entryInstruction.ZG_ExportManifest = true;
			list = declaration.Lookups.GoodsLocationTypeList;
			list.AssertContainsExactCodes("Codes - Export Manifest is true", new [] { "Y", "Z" });
		});
	}

	public void TestPLCustomsChargeTypeList()
	{
		var declaration = Factory.New<JobDeclaration>();

		var chargeTypeList = declaration.Lookups.PLCustomsChargeTypeList;
		CombineAssertions(() =>
		{
			chargeTypeList.AssertContainsExactCodes("Poland should contain specified list from PLCustomsChargeTypeList",
				new [] { "071", "072", "073", "074", "080", "1ST", "2ST", "AB", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AN", "BA", "BB", "BC", "BD", "BE", "BF", "BG" });
			AssertEquals("Cached", chargeTypeList, declaration.Lookups.PLCustomsChargeTypeList);
		});
	}

	public void TestEUCountryCodes()
	{
		var declaration = Factory.New<JobDeclaration>();
		var expectedList = new ZString[] { "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "EL", "ES", "FI", "FR", "GR", "HU", "IE", "IT", "LT", "LU", "LV", "MT", "NT", "PL", "PT", "QR", "QV", "RO", "SE", "SI", "SK", "XI" };

		AssertEquals("expectedList must not contains GB", false, expectedList.Contains("GB"));
		AssertContainsExactElementsInAnyOrder("Expected elements", expectedList, declaration.Lookups.EUCountryCodes);
	}

	public void TestPLEntryStatusList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var list = declaration.Lookups.EntryStatusList;
		CombineAssertions(() =>
		{
			list.AssertContainsExactCodes("Codes", new [] { "REQ", "CAN", "ECE", "ECO", "ECX", "ERE", "EXP", "ICO", "IMA", "IMF", "MRN", "NPP", "REL", "UPO" });
			AssertEquals("Cached", list, declaration.Lookups.EntryStatusList);
		});
	}

	public void TestTypeOfLocationList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var list = declaration.Lookups.TypeOfLocationList;
		CombineAssertions(() =>
		{
			list.AssertContainsExactCodes("Codes", new [] { "A", "B", "C", "D" });
			AssertEquals("Cached", list, declaration.Lookups.TypeOfLocationList);
		});
	}

	public void TestPaymentPartyList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var list = declaration.Lookups.PaymentPartyList;
		CombineAssertions(() =>
		{
			list.AssertContainsExactCodes("Codes", new [] { "A", "B", "C", "D", "E", "G", "H", "J", "K", "L", "O", "P", "R", "S", "T", "U", "V", "Z" });
			AssertEquals("Cached", list, declaration.Lookups.PaymentPartyList);
		});
	}

	public void TestGetEntryStyleList_EXS()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		var list = declaration.Lookups.EntryStyleList;
		CombineAssertions(() =>
		{
			list.AssertContainsExactCodes("Codes", ["A1", "A2"]);
			AssertEquals("Cached", list, declaration.Lookups.EntryStyleList);
		});
	}

	public void TestAdditionalCustomsOffices_IsRelevantToIsLocalCountryOnlyProperty()
	{
		PrepareCustomsOffices();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var mainOfficeRequirement = declaration.CustomsOfficeRequirementHelper.AdditionalOffice;

		mainOfficeRequirement.IsLocalCountryOnly = true;
		var customsOffice = declaration.Lookups.AdditionalCustomsOffices;
		customsOffice.Load();
		AssertContainsExactElementsInAnyOrder(new[] { "PL EXT", "PL EIN" }, customsOffice.Select(x => x.ZZD_Code));

		mainOfficeRequirement.IsLocalCountryOnly = false;
		customsOffice = declaration.Lookups.AdditionalCustomsOffices;
		customsOffice.Load();
		AssertContainsExactElementsInAnyOrder(new[] { "PL EXT", "DE EXT", "PL EIN", "DE EIN" }, customsOffice.Select(x => x.ZZD_Code));
	}

	public void TestAdditionalCustomsOffices_IsForeignOnly()
	{
		PrepareCustomsOffices();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var additionalOfficeRequirement = declaration.CustomsOfficeRequirementHelper.AdditionalOffice;
		additionalOfficeRequirement.IsForeignCountryOnly = true;
		additionalOfficeRequirement.IsLocalCountryOnly = false;
		var customsOffice = declaration.Lookups.AdditionalCustomsOffices;
		customsOffice.Load();
		AssertContainsExactElementsInAnyOrder(new[] { "DE EXT", "DE EIN" }, customsOffice.Select(x => x.ZZD_Code));
	}

	public void TestEntryStyleList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var list = declaration.Lookups.EntryStyleList;
		CombineAssertions("Export", () =>
		{
			list.AssertContainsExactCodes("Codes", new [] { "CO", "EX" });
			AssertEquals("Cached", list, declaration.Lookups.EntryStyleList);
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		list = declaration.Lookups.EntryStyleList;
		CombineAssertions("Import", () =>
		{
			list.AssertContainsExactCodes("Codes", new [] { "EU", "CO", "IM" });
			AssertEquals("Cached", list, declaration.Lookups.EntryStyleList);
		});
	}

	public void TestGetVesselByVesselName()
	{
		var vessel = RefVessel.New(Factory);
		vessel.RV_Code = "abc123";

		var vessel2 = RefVessel.New(Factory);
		vessel2.RV_LloydsNumber = "abc123";

		var lookups = Factory.New<JobDeclaration>().Lookups;

		CombineAssertions(() =>
		{
			var lookupVessel = lookups.GetVesselByVesselName("asd123");
			AssertNull("Vessel does not exist in DB", lookupVessel);

			lookupVessel = lookups.GetVesselByVesselName("abc123");
			AssertEquals("Vessel exists in DB", "abc123", lookupVessel.RV_Code);
			AssertEquals("Vessel exists but have not specified LloydNumber", ZString.Empty, lookupVessel.RV_LloydsNumber);
		});
	}

	public void TestGetVesselByLloydsNumber()
	{
		var vessel = RefVessel.New(Factory);
		vessel.RV_Code = "abc123";

		var vessel2 = RefVessel.New(Factory);
		vessel2.RV_LloydsNumber = "abc123";

		var lookups = Factory.New<JobDeclaration>().Lookups;

		CombineAssertions(() =>
		{
			var lookupVessel = lookups.GetVesselByLloydsNumber("asd123");
			AssertNull("Vessel does not exist in DB", lookupVessel);

			lookupVessel = lookups.GetVesselByLloydsNumber("abc123");
			AssertEquals("Vessel exists in DB", "abc123", lookupVessel.RV_LloydsNumber);
			AssertEquals("Vessel exists but have not specified Code", ZString.Empty, lookupVessel.RV_Code);
		});
	}

	void PrepareCustomsOffices()
	{
		var yesterday = ZDateTime.Today.AddDays(-1);
		var tomorrow = ZDateTime.Today.AddDays(1);

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland", eun);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE EXT", "DE EXT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE EIN", "DE EIN", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BOX", "BOX desc", yesterday, tomorrow);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "PL", "PL desc", yesterday, tomorrow);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "PL EXT", "PL EXT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "PL ENT", "PL ENT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "PL EIN", "PL EIN", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);

		Factory.Save();
	}
}
