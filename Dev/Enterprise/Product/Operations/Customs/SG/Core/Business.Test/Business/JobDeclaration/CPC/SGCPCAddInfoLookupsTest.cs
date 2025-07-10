using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.Testing
{
	internal class SGCPCAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCPCCodeList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "", "11", "11", "111", "One", "IPT", group: "GTR");
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "", "22", "22", "222", "Two", "INP", group: "APS");
			var procedure3 = helper.CreateRefCusProcedure("AU", "", "33", "33", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure(currentCountry, "", "44", "44", "444", "Four", "IPT", group: "GTR");
			var procedure5 = helper.CreateRefCusProcedure(currentCountry, "", "420", "44", "4100", "Five", "OUT", group: "DRT");
			var procedure6 = helper.CreateRefCusProcedure(currentCountry, "", "540", "55", "6000", "Six", "OUT", group: "DRT");
			var procedure6Attribute1 = helper.CreateRefCusProcedureAttribute(procedure6.PK, AttributeNames.Codes.ISCOO, "Y");
			var procedure7 = helper.CreateRefCusProcedure(currentCountry, "", "771", "77", "4000", "Seven", "TNP", group: "TTF");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GTR;
			var cpc = declaration.CPCs.AddNew();
			SGCPCAddInfo cpcAddInfo = cpc.Data;
			var lookups = new SGCPCAddInfoLookups(cpcAddInfo);
			var cPCCodeList = lookups.CPCCodeList;
			AssertEquals("CPC List should have procedure codes filtered by MessageType & MessageSubType", 2, cPCCodeList.Count);
			Assert("CPC List", cPCCodeList.ContainsCode("11111"));
			Assert("CPC List", cPCCodeList.ContainsCode("44444"));
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			cPCCodeList = lookups.CPCCodeList;
			AssertEquals("CPC List should have procedure codes filtered by MessageType & MessageSubType", 1, cPCCodeList.Count);
			Assert("CPC List", cPCCodeList.ContainsCode("22222"));
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			cPCCodeList = lookups.CPCCodeList;
			AssertEquals("CPC List for OUT/DRT should only have the non CofO codes", 1, cPCCodeList.Count);
			Assert("CPC List", cPCCodeList.ContainsCode("4204100"));
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			declaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			cPCCodeList = lookups.CPCCodeList;
			AssertEquals("CPC List for OUT/DRT with COO should only have the CofO codes", 1, cPCCodeList.Count);
			Assert("CPC List", cPCCodeList.ContainsCode("5406000"));
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TTF;
			cPCCodeList = lookups.CPCCodeList;
			AssertEquals("CPC List for TNP/TTF should pick up code > 600", 1, cPCCodeList.Count);
			Assert("CPC List", cPCCodeList.ContainsCode("7714000"));
		}
	}
}
