using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class CusBondDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGuaranteeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var organization1 = Factory.NewWithValidTestData<OrgHeader>();
				organization1.OH_Code = "Org1";
				var organization2 = Factory.NewWithValidTestData<OrgHeader>();
				organization2.OH_Code = "Org2";

				var permitHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				permitHeader1.CPH_Number = "PERMIT1";
				permitHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				permitHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
				permitHeader1.CPH_OH_PermitHolder = organization1.PK;

				var permitHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				permitHeader2.CPH_Number = "PERMIT2";
				permitHeader2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				permitHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
				permitHeader2.CPH_OH_PermitHolder = organization2.PK;

				var permitHeader3 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				permitHeader3.CPH_Number = "PERMIT3";
				permitHeader3.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				permitHeader3.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				permitHeader3.CPH_OH_PermitHolder = organization1.PK;

				var permitHeader4 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				permitHeader4.CPH_Number = "PERMIT4";
				permitHeader4.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;
				permitHeader4.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
				permitHeader4.CPH_OH_PermitHolder = organization1.PK;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = organization1.PK;
				var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
				var cusBondDetail = instruction.Guarantee;
				AssertEquals("permitHeader1", 1, cusBondDetail.Lookups.GuaranteeList.Count);
				AssertEquals("CPH_Number", "PERMIT1", cusBondDetail.Lookups.GuaranteeList[0].CPH_Number);
			}
		}

		public void TestBondTypeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var organization1 = Factory.NewWithValidTestData<OrgHeader>();
				organization1.OH_Code = "Org1";

				var permitHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				permitHeader1.CPH_Type = "BANKA";
				permitHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				permitHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
				permitHeader1.CPH_OH_PermitHolder = organization1.PK;

				var permitHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				permitHeader2.CPH_Type = "TEST";
				permitHeader2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				permitHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
				permitHeader2.CPH_OH_PermitHolder = organization1.PK;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = organization1.PK;
				var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
				var cusBondDetail = instruction.Guarantee;
				var bondTypeList = cusBondDetail.Lookups.BondTypeList;
				AssertEquals("CodesAsString", "BANKA, DAC, DACR2, DIGER, GAR, GDS, GLB, GTR1, GTR2, GTRAN, NAKIT, RODER, UND, TEST", bondTypeList.CodesAsString);
				var bankas = bondTypeList.ToArray().Where(x => x.Code == "BANKA");
				AssertEquals("No duplicate codes from importer", 1, bankas.Count());
				AssertEquals("From Importer guarantee type", "From Importer", bondTypeList.GetDescriptionFromCode("TEST"));
			}
		}
	}
}
