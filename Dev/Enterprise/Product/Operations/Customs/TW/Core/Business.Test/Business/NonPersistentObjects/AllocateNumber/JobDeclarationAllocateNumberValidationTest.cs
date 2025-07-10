using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobDeclarationAllocateNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPart2Number()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CusEntryInstruction;
			var warningMessage = "The 3rd-4th character of entry number does not match the captured entry number key component values. Ignore this validation when confirmed.";
			var allocateNumber = new JobDeclarationAllocateNumber(declaration, false);
			var targetInfo = allocateNumber.Part2NumberInfo;
			CombineAssertions("Category A", () =>
			{
				allocateNumber.Part2Number = "AA";
				AssertHasWarning(targetInfo, warningMessage);
				allocateNumber.Part2Number = "BF";
				AssertNoWarning(targetInfo, warningMessage);
			});

			entryInstruction.CEI_Style = "F2";
			CombineAssertions("Category C", () =>
			{
				allocateNumber.Part2Number = "F1";
				AssertHasWarning(targetInfo, warningMessage);
				allocateNumber.Part2Number = "F2";
				AssertNoWarning(targetInfo, warningMessage);
			});

			entryInstruction.CEI_Style = "B1";
			CombineAssertions("Category B", () =>
			{
				allocateNumber.Part2Number = "B2";
				AssertHasWarning(targetInfo, warningMessage);
				allocateNumber.Part2Number = "B1";
				AssertNoWarning(targetInfo, warningMessage);
			});
		}

		public void TestCheckPart4Number()
		{
			var declaration = CreateDeclaration();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_BoxNumber = "123";
			var warningMessage = "The entered Box Number does not match the captured entry number key component values. Ignore this validation when confirmed.";
			var allocateNumber = new JobDeclarationAllocateNumber(declaration, false);
			var targetInfo = allocateNumber.Part4NumberInfo;
			CombineAssertions("Category A", () =>
			{
				allocateNumber.Part4Number = "456";
				AssertHasWarning(targetInfo, warningMessage);
				allocateNumber.Part4Number = "123";
				AssertNoWarning(targetInfo, warningMessage);
			});

			entryInstruction.CEI_Style = "F2";
			CombineAssertions("Category C", () =>
			{
				allocateNumber.Part4Number = "789";
				AssertHasWarning(targetInfo, warningMessage);
				allocateNumber.Part4Number = "123";
				AssertNoWarning(targetInfo, warningMessage);
			});

			entryInstruction.CEI_Style = "B1";
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
			supplierDocumentaryAddress.CBPCode = "DT0001";
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
			importerDocumentaryAddress.CBPCode = "DB0002";
			CombineAssertions("Category B", () =>
			{
				allocateNumber.Part4Number = "4567";
				AssertHasWarning(targetInfo, warningMessage);
				allocateNumber.Part4Number = "002B";
				AssertNoWarning(targetInfo, warningMessage);
			});
		}

		JobDeclaration CreateDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsOffice = "BF";
			declaration.JE_CustomsProfile = "123-3";
			return declaration;
		}
	}
}
