using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public static class CopyHelper
{
	public static string[] TempImportBeforeReexportProcedureCodes => new string[] { "40", "41", "50", "51", "52", "57", "71" };
	public static string[] TempImportBeforeFinalProcedureCodes => new string[] { "50", "51", "52", "57" };

	public static TemplateCopyDeclarationResult CreateReexportCopyOf(JobDeclaration declaration)
	{
		var procedureCodes = declaration.CustomsEntryInstructions.Select(instruction => (string)instruction.CEI_Procedure.SubstringSafe(0, 2)).ToArray();
		var (isCancelledOrNotInDatabase, errorMessage) = IsCancelledOrNotInDatabase(declaration);
		if (isCancelledOrNotInDatabase)
		{
			return new TemplateCopyDeclarationResult(errorMessage, ReexportCopyCaption);
		}
		if (!procedureCodes.Intersect(CopyHelper.TempImportBeforeReexportProcedureCodes).Any())
		{
			return new TemplateCopyDeclarationResult(CannotCopyDeclarationWithThisCode, ReexportCopyCaption);
		}

		var copy = declaration.TemplateCopy() as JobDeclaration;
		copy.JE_MessageType = JobMessageTypeList.Codes.Export;
		copy.JE_CopyStatus = NODeclarationCopyStatus.Codes.ReExport;
		copy.JE_MessageSubType = declaration.JE_MessageSubType.ToString() switch
		{
			ShipmentTypeImport.Codes.ImportOfGoodsFromAnEuEeaOrEftaMemberState => ShipmentTypeExport.Codes.ExportOfGoodsToAnEuEeaOrEftaMemberState,
			ShipmentTypeImport.Codes.ImportOfGoodsFromAllOtherNotCoveredByEu => ShipmentTypeExport.Codes.ExportOfGoodsToAllOtherNotCoveredByEu,
			_ => copy.JE_MessageSubType,
		};

		// Switch origins and destinations for reexport:
		copy.JE_OH_Supplier = declaration.JE_OH_Importer;
		copy.JE_OH_Importer = declaration.JE_OH_Supplier;
		copy.JE_CustomsLoadPort = declaration.JE_CustomsDischargePort;
		copy.JE_CustomsDischargePort = declaration.JE_CustomsLoadPort;
		copy.JE_RL_NKOrigin = declaration.JE_RL_NKFinalDestination;
		copy.JE_RL_NKFinalDestination = declaration.JE_RL_NKOrigin;

		foreach (var entryInstruction in copy.CustomsEntryInstructions)
		{
			var procedureCode = entryInstruction.CEI_Procedure.SubstringSafe(0, 2);
			if (TempImportBeforeReexportProcedureCodes.Contains((string)procedureCode))
			{
				entryInstruction.CEI_Style = ReexportCodeGroup;
				entryInstruction.CEI_Procedure = $"{ReexportProcedureCode}{procedureCode}";
			}
		}
		return new TemplateCopyDeclarationResult(copy);
	}

	public static TemplateCopyDeclarationResult CreateFinalImportCopyOf(JobDeclaration declaration)
	{
		var procedureCodes = declaration.CustomsEntryInstructions.Select(instruction => (string)instruction.CEI_Procedure.SubstringSafe(0, 2)).ToArray();
		var (isCancelledOrNotInDatabase, errorMessage) = IsCancelledOrNotInDatabase(declaration);
		if (isCancelledOrNotInDatabase)
		{
			return new TemplateCopyDeclarationResult(errorMessage, FinalImportCopyCaption);
		}
		if (!procedureCodes.Intersect(CopyHelper.TempImportBeforeFinalProcedureCodes).Any())
		{
			return new TemplateCopyDeclarationResult(CannotCopyDeclarationWithThisCode, FinalImportCopyCaption);
		}

		var copy = declaration.TemplateCopy() as JobDeclaration;
		copy.JE_CopyStatus = NODeclarationCopyStatus.Codes.FinalImport;

		foreach (var entryInstruction in copy.CustomsEntryInstructions)
		{
			var procedureCode = entryInstruction.CEI_Procedure.SubstringSafe(0, 2);
			if (TempImportBeforeFinalProcedureCodes.Contains((string)procedureCode))
			{
				entryInstruction.CEI_Style = FinalImportCodeGroup;
				entryInstruction.CEI_Procedure = $"{FinalImportProcedureCode}{procedureCode}";
			}
		}
		return new TemplateCopyDeclarationResult(copy);
	}

	public static TemplateCopyDeclarationResult CreateRecalculationCopyOf(JobDeclaration declaration)
	{
		var (isCancelledOrNotInDatabase, errorMessage) = IsCancelledOrNotInDatabase(declaration);
		if (isCancelledOrNotInDatabase)
		{
			return new TemplateCopyDeclarationResult(errorMessage, RecalculationCopyCaption);
		}
		var copy = declaration.TemplateCopy() as JobDeclaration;
		copy.JE_CopyStatus = NODeclarationCopyStatus.Codes.Recalculation;
		return new TemplateCopyDeclarationResult(copy);
	}

	static (bool, ZString) IsCancelledOrNotInDatabase(JobDeclaration declaration)
	{
		if (declaration.IsCancelled)
		{
			return (true, CannotCopyCancelledDeclaration);
		}
		if (!declaration.IsInDatabase)
		{
			return (true, ParentDeclarationNotSaved);
		}
		return (false, ZString.Empty);
	}

	public static void LinkDeclarationCopyToParent(JobDeclaration declarationParent, JobDeclaration declarationCopy)
	{
		declarationParent.RelatedDeclarations.Add(declarationCopy);
		ZExceptionReporting.ProcessWithSaveExceptionHandling(declarationParent.Factory.Save, null, reportErrorsOnly: true);
	}

	const string ReexportCodeGroup = "3";
	const string ReexportProcedureCode = "30";
	const string FinalImportCodeGroup = "4";
	const string FinalImportProcedureCode = "40";

	public static string RecalculationCopyCaption => Res.GetString("b9736c66-2954-4be3-8f37-60a3b80e09ca", "Copy to new recalculation declaration");
	public static string ReexportCopyCaption => Res.GetString("906F4CA4-D26D-41C4-99C6-C51ABA168187", "Copy to an export declaration following temporary import");
	public static string FinalImportCopyCaption => Res.GetString("736AC236-0EC2-4914-9E2E-B98B94E253E5", "Copy to a final import declaration following temporary import");
	public static string CannotCopyCancelledDeclaration => Res.GetString("e3d80a4a-ed33-47a9-a27b-b0e14d378392", "Cannot copy a declaration which has been canceled.");
	public static string ParentDeclarationNotSaved => Res.GetString("103f6a63-65c7-42cd-9b1b-2e5d42d60dc2", "Please save the declaration before attempting to create a related declaration.");
	public static string CannotCopyDeclarationWithThisCode => Res.GetString("69C7ADFC-C0F7-460D-8E10-BA73E05AFD34", "The selected type of copy is not valid for the selected declaration procedure.");

	public class TemplateCopyDeclarationResult
	{
		public bool IsValid { get; }
		public JobDeclaration Copy { get; }
		public string CopyStatusCode { get; }
		public string ErrorMessage { get; }

		public TemplateCopyDeclarationResult(JobDeclaration copy)
		{
			Copy = copy;
			IsValid = true;
		}
		public TemplateCopyDeclarationResult(string errorMessage, string copyStatusCode)
		{
			IsValid = false;
			CopyStatusCode = CopyStatusCode;
			ErrorMessage = errorMessage;
		}
	}
}
