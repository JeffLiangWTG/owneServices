using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CopyHelper))]
sealed class CopyHelperTest : TestCaseWithFactory
{
	public void TestCreateReexportCopy_ShouldSetReexport()
	{
		entryInstruction.CEI_Procedure = "4100";
		Factory.Save();
		CopyAndLinkToParent(declaration, CopyHelper.CreateReexportCopyOf);
		var declarationCopy = declaration.RelatedDeclarations.SingleOrDefault() as JobDeclaration;
		AssertEquals(
			"Should set JE_CopyStatus to re-export",
			expected: NODeclarationCopyStatus.Codes.ReExport,
			declarationCopy?.JE_CopyStatus);
	}

	public void TestCreateReexportCopy_ShouldSetLinkToParent()
	{
		entryInstruction.CEI_Procedure = "4100";
		AssertCopySetsUpLinkWithParent(CopyHelper.CreateReexportCopyOf);
	}

	public void TestCreateReexportEU_CopyRules()
	{
		SetUpForReexport();
		declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.Denmark;
		declaration.JE_MessageSubType = ShipmentTypeImport.Codes.ImportOfGoodsFromAnEuEeaOrEftaMemberState;
		Factory.Save();

		CombineAssertions(() =>
		{
			CopyAndLinkToParent(declaration, CopyHelper.CreateReexportCopyOf);
			Factory.Save();

			var declarationCopy = (JobDeclaration)declaration.RelatedDeclarations.SingleOrDefault();
			AssertReexportRules(declarationCopy);

			AssertEquals(
				"The copied EU declaration should be also 'EU'",
				ShipmentTypeExport.Codes.ExportOfGoodsToAnEuEeaOrEftaMemberState,
				declarationCopy.JE_MessageSubType);
		});
	}

	public void TestCreateReexportNonEU_CopyRules()
	{
		SetUpForReexport();
		declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
		declaration.JE_MessageSubType = ShipmentTypeImport.Codes.ImportOfGoodsFromAllOtherNotCoveredByEu;
		Factory.Save();

		CombineAssertions(() =>
		{
			CopyAndLinkToParent(declaration, CopyHelper.CreateReexportCopyOf);
			Factory.Save();

			var declarationCopy = (JobDeclaration)declaration.RelatedDeclarations.SingleOrDefault();
			AssertReexportRules(declarationCopy);

			AssertEquals(
				"The copied IM declaration should become 'EX'",
				ShipmentTypeExport.Codes.ExportOfGoodsToAllOtherNotCoveredByEu,
				declarationCopy.JE_MessageSubType);
		});
	}

	public void TestCreateFinalImportCopy_ShouldSetFinalImport()
	{
		entryInstruction.CEI_Procedure = "5100";
		Factory.Save();
		CopyAndLinkToParent(declaration, CopyHelper.CreateFinalImportCopyOf);
		var declarationCopy = declaration.RelatedDeclarations.SingleOrDefault() as JobDeclaration;
		AssertEquals(
			"Should set JE_CopyStatus to final import",
			expected: NODeclarationCopyStatus.Codes.FinalImport,
			declarationCopy?.JE_CopyStatus);
	}

	public void TestCreateFinalImportCopy_ShouldSetLinkToParent()
	{
		entryInstruction.CEI_Procedure = "5100";
		AssertCopySetsUpLinkWithParent(CopyHelper.CreateFinalImportCopyOf);
	}

	public void TestCreateFinalImport_CopyRules()
	{
		entryInstruction.CEI_Procedure = "5110";
		Factory.Save();

		CombineAssertions(() =>
		{
			CopyAndLinkToParent(declaration, CopyHelper.CreateFinalImportCopyOf);
			Factory.Save();

			var declarationCopy = declaration.RelatedDeclarations.SingleOrDefault();
			var copyEntryInstruction = declarationCopy.CustomsEntryInstructions[0];

			AssertEquals("Declaration number should be the same as parent's", declaration.DeclarationNumber, declarationCopy.DeclarationNumber);
			AssertEquals("The copied declaration's code group should be '4'", FinalImportCodeGroup, copyEntryInstruction.CEI_Style);
			AssertEquals("Procedure code should be '40' + first 2 digits of parent's code.", "4051", copyEntryInstruction.CEI_Procedure);
		});
	}

	public void TestCreateRecalculationCopy_CopyRules()
	{
		declaration.JE_GoodsNumber = "123";
		declaration.JE_Position = "456";
		entryInstruction.CEI_SubPosition = "111";
		Factory.Save();
		CopyAndLinkToParent(declaration, CopyHelper.CreateRecalculationCopyOf);
		var declarationCopy = declaration.RelatedDeclarations.SingleOrDefault() as JobDeclaration;

		AssertEquals("[PRE-CONDITION] Parent Declaration.RelatedDeclarations count", 1, declaration.RelatedDeclarations.Count);
		var entryInstructionCopy = declarationCopy.CustomsEntryInstructions[0];
		CombineAssertions(() =>
		{
			AssertEquals(nameof(JobDeclaration.JE_GoodsNumber), declaration.JE_GoodsNumber, declarationCopy.JE_GoodsNumber);
			AssertEquals(nameof(JobDeclaration.JE_Position), declaration.JE_Position, declarationCopy.JE_Position);
			AssertEquals(nameof(CusEntryInstruction.CEI_SubPosition), entryInstruction.CEI_SubPosition, entryInstructionCopy.CEI_SubPosition);
		});
	}

	public void TestLinkDeclarationCopyToParent_ShouldSetRecalculation()
	{
		Factory.Save();
		CopyAndLinkToParent(declaration, CopyHelper.CreateRecalculationCopyOf);
		var declarationCopy = declaration.RelatedDeclarations.SingleOrDefault() as JobDeclaration;
		AssertEquals(
			"Should set JE_CopyStatus to recalculation",
			expected: NODeclarationCopyStatus.Codes.Recalculation,
			declarationCopy?.JE_CopyStatus);
	}

	public void TestLinkDeclarationCopyToParent_ShouldSetupLinkBetweenCopyAndParent()
	{
		AssertCopySetsUpLinkWithParent(CopyHelper.CreateRecalculationCopyOf);
	}

	#region Helpers

	void AssertReexportRules(JobDeclaration declarationCopy)
	{
		var copyEntryInstruction = declarationCopy.CustomsEntryInstructions[0];

		AssertEquals("The supplier should be parent's importer", declaration.JE_OH_Importer, declarationCopy.JE_OH_Supplier);
		AssertEquals("The importer should be parent's supplier", declaration.JE_OH_Supplier, declarationCopy.JE_OH_Importer);
		AssertEquals("The loading port should be parent's discharge port", declaration.JE_CustomsDischargePort, declarationCopy.JE_CustomsLoadPort);
		AssertEquals("The discharge port should be parent's loading port", declaration.JE_CustomsLoadPort, declarationCopy.JE_CustomsDischargePort);
		AssertEquals("Loading date should be reset", ZDateTime.Empty, declarationCopy.JE_ETDOfLoading);
		AssertEquals("Discharge date should be reset", ZDateTime.Empty, declarationCopy.JE_ETAOfDischarge);
		AssertEquals("The port of origin should be parent's destination port", declaration.JE_RL_NKFinalDestination, declarationCopy.JE_RL_NKOrigin);
		AssertEquals("The destination port should be parent's port of origin", declaration.JE_RL_NKOrigin, declarationCopy.JE_RL_NKFinalDestination);
		AssertEquals("Date at final destination should be reset", ZDateTime.Empty, declarationCopy.JE_DateAtFinalDestination);
		AssertEquals("Date at origin should be reset", ZDateTime.Empty, declarationCopy.JE_DateAtOrigin);
		AssertEquals("The copied declaration should be for export", Enterprise.Core.Constants.FreightShipmentDirection.Code.Export, declarationCopy.JE_MessageType);
		AssertEquals("The copied declaration's code group should be '3'", ReexportCodeGroup, copyEntryInstruction.CEI_Style);
		AssertEquals("The copied declaration's code should be 30 + first 2 digits of the parent declaration", "3041", copyEntryInstruction.CEI_Procedure);
	}

	void AssertCopySetsUpLinkWithParent(Func<JobDeclaration, CopyHelper.TemplateCopyDeclarationResult> createCopyFunc)
	{
		Factory.Save();
		CombineAssertions(() =>
		{
			CopyAndLinkToParent(declaration, createCopyFunc);
			Factory.Save();
			var declarationCopy = declaration.RelatedDeclarations.SingleOrDefault();
			AssertNotNull("Should setup link between original and copy as a related declaration", declarationCopy);
			AssertSame("Should setup link between copy and original as it's parent", declaration, declarationCopy?.ParentRelatedDeclaration);
		});
	}

	#endregion

	void CopyAndLinkToParent(JobDeclaration declaration, Func<JobDeclaration, CopyHelper.TemplateCopyDeclarationResult> createCopyFunc)
	{
		var declarationCopy = createCopyFunc(declaration);
		CopyHelper.LinkDeclarationCopyToParent(declaration, declarationCopy.Copy);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Enterprise.Core.Constants.FreightShipmentDirection.Code.Import;

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	void SetUpForReexport()
	{
		var declarationSupplier = Factory.New<OrgHeader>();
		declarationSupplier.OH_Code = "SUPPLIER";
		declarationSupplier.OH_IsConsignor = true;
		declarationSupplier.MainAddress.OA_Address1 = "Add1";
		declaration.JE_OH_Supplier = declarationSupplier.PK;

		var declarationImporter = Factory.New<OrgHeader>();
		declarationImporter.OH_Code = "IMPORTER";
		declarationImporter.OH_IsConsignee = true;
		declarationImporter.MainAddress.OA_Address1 = "Add1";
		declaration.JE_OH_Importer = declarationImporter.PK;

		declaration.JE_RL_NKFinalDestination = Core.Constants.CountryCodes.Norway;

		entryInstruction.CEI_Procedure = "4100";
		declaration.JE_CustomsLoadPort = "Loading";
		declaration.JE_CustomsDischargePort = "Discharge";
		declaration.JE_DateAtOrigin = new ZDateTime(2023, 10, 9);
		declaration.JE_DateAtFinalDestination = new ZDateTime(2023, 11, 9);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;

	const string ReexportCodeGroup = "3";
	const string FinalImportCodeGroup = "4";
}
