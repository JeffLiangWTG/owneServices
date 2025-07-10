using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMRNsForReplacing()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "10";
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "20";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("MRN1", new ZDateTime(2016, 11, 14));
			entryHeader.CH_BGMReference = "LRN1";
			entryHeader.CH_CEI_Instruction = instruction1.PK;
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("MRN2", new ZDateTime(2016, 11, 14));
			entryHeader.CH_BGMReference = "LRN2";
			entryHeader.CH_CEI_Instruction = instruction2.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var mrnsForReplacing = instruction.Lookups.MRNsForReplacing;
			AssertEquals(2, mrnsForReplacing.Count);
			AssertEquals("CPC:10 LRN:LRN1", mrnsForReplacing.GetDescriptionFromCode("MRN1"));
			AssertEquals("CPC:20 LRN:LRN2", mrnsForReplacing.GetDescriptionFromCode("MRN2"));
		}

		public void TestCustomsOfficeList()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("CTN");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			AssertEquals("The lookup list is the same for JE_CustomsOffice and CEI_CustomsOfficeOverride", declaration.Lookups.CustomsOfficeList, instruction.Lookups.CustomsOfficeList);
			AssertEquals("Description", "CAPE TOWN", instruction.Lookups.CustomsOfficeList.GetDescriptionFromCode("CTN"));
		}

		public void TestCreditTerms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;

				AssertEquals("ZAINVDET: true - When Import job, Credit Terms should be empty", 0, instruction.Lookups.CreditTerms.Count);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;

				AssertEquals("ZAINVDET: true - When Export job, Credit Terms should contain 1001 items", 1001, instruction.Lookups.CreditTerms.Count);
				Assert("ZAINVDET: true - When Export job, Credit Terms should contain 'NEP' or 'ADV'", instruction.Lookups.CreditTerms.ContainsCode("NEP") && instruction.Lookups.CreditTerms.ContainsCode("ADV"));

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;

				AssertEquals("ZAINVDET: true - When ExBond job, Credit Terms should be empty", 0, instruction.Lookups.CreditTerms.Count);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: false))
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;

				AssertEquals("ZAINVDET: false - When Import job, Credit Terms should contain 1001 items", 1001, instruction.Lookups.CreditTerms.Count);
				Assert("ZAINVDET: false - When Import job, Credit Terms should contain 'NEP' or 'ADV'", instruction.Lookups.CreditTerms.ContainsCode("NEP") && instruction.Lookups.CreditTerms.ContainsCode("ADV"));

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;

				AssertEquals("ZAINVDET: false - When Export job, Credit Terms should contain 1001 items", 1001, instruction.Lookups.CreditTerms.Count);
				Assert("ZAINVDET: false - When Export job, Credit Terms should contain 'NEP' or 'ADV'", instruction.Lookups.CreditTerms.ContainsCode("NEP") && instruction.Lookups.CreditTerms.ContainsCode("ADV"));

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;

				AssertEquals("ZAINVDET: false - When Exbond job, Credit Terms should contain 1001 items", 1001, instruction.Lookups.CreditTerms.Count);
				Assert("ZAINVDET: false - When Exbond job, Credit Terms should contain 'NEP' or 'ADV'", instruction.Lookups.CreditTerms.ContainsCode("NEP") && instruction.Lookups.CreditTerms.ContainsCode("ADV"));
			}
		}
	}
}
