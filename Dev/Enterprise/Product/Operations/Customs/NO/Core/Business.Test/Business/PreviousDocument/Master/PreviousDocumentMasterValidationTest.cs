using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(PreviousDocumentMasterValidation))]
sealed class PreviousDocumentMasterValidationTest : TestCaseWithFactory
{
	public void TestAutoValidateType()
	{
		AssertEquals(typeof(PreviousDocumentMasterValidation), documentMaster.Validation.AutoValidationType);
	}

	public void TestCheckCSI_Procedure_ListValidation()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(documentMaster.CSI_ProcedureInfo, "ABC", "71A");
	}

	public void TestCheckCSI_Procedure_ValidateAgainstGoodsLocationAndPreviousProcedureCodeCombinedValue()
	{
		CreateRefCusProcedureCodeDataForTest();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		declaration.JE_LocationOfGoods = "A8";
		entryInstruction.CEI_Procedure = "0471";

		var previousDocumentMaster = entryInstruction.PreviousDocumentMaster;
		var previousDocumentMasterValidation = previousDocumentMaster.Validation;

		CombineAssertions("When Procedure 0471 having OutOfProcedure=Y", () =>
		{
			previousDocumentMaster.CSI_Procedure = "71A";
			previousDocumentMasterValidation.ValidateCSI_Procedure();
			AssertHasMessageErrorContaining("CSI_Procedure value not matching the expected value", previousDocumentMaster.CSI_ProcedureInfo, GetExpectedMessage("71A8"));

			previousDocumentMaster.CSI_Procedure = "71A8";
			previousDocumentMasterValidation.ValidateCSI_Procedure();
			AssertNoMessageErrorContaining("CSI_Procedure value matching the expected value", previousDocumentMaster.CSI_ProcedureInfo, GetExpectedMessage("71A8"));
		});

		entryInstruction.CEI_Procedure = "5710";
		previousDocumentMaster.CSI_Procedure = "71A";
		AssertNoNotifications("When Procedure 5710 having OutOfProcedure=N and CSI_Procedure value is not matching the expected value", previousDocumentMaster.CSI_ProcedureInfo);

		string GetExpectedMessage(string expectedValue)
			=> FormattableString.Invariant($"The combination of Procedure on Entry Instruction and Goods Location on Declaration Level requires Previous Procedure to be '{expectedValue}'.");
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		documentMaster = entryInstruction.PreviousDocumentMaster;
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	PreviousDocumentMaster documentMaster;

	void CreateRefCusProcedureCodeDataForTest()
	{
		var referenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
		_ = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "04",
			previousProcedureCode: "71",
			concession: ZString.Empty,
			description: "Description1",
			shipmentType: "IMP",
			outOfWarehouse: true);

		_ = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "57",
			previousProcedureCode: "10",
			concession: ZString.Empty,
			description: "Description 2",
			shipmentType: "IMP",
			outOfWarehouse: false);
	}
}
