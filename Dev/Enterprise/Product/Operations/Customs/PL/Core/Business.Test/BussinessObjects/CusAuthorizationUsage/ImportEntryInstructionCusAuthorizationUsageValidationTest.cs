using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ImportEntryInstructionCusAuthorizationUsageValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAGC_OH_Owner()
	{
		Factory.AddCodeToCusMap_EUNAU(CusAuthorizationUsageType.C506);
		var (_, _, authorisation) = GetNewCusAuthorizationUsage();
		CombineAssertions(() =>
		{
			authorisation.AGC_Code = CusAuthorizationUsageType.C506;
			authorisation.Validation.ValidateAGC_OH_Owner();
			AssertNoMessageErrorContaining("AGC_Code C506", authorisation.AGC_OH_OwnerInfo, MandatoryValidation.YouHaveNotEntered);

			authorisation.AGC_Code = "C505";
			authorisation.Validation.ValidateAGC_OH_Owner();
			AssertHasMessageErrorContaining("AGC_Code is not C506", authorisation.AGC_OH_OwnerInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	(JobDeclaration, CusEntryInstruction, CusAuthorizationUsage) GetNewCusAuthorizationUsage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		return (declaration, entryInstruction, entryInstruction.CusAuthorizationUsages.AddNew());
	}
}
