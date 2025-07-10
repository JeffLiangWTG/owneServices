using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.TW.Business
{
	static class ValidationHelper
	{
		internal static void CheckGroupingPlusDeclarationGoodsDescriptionLength(JobComInvoiceLine invoiceLine, ZPropertyInfo targetInfo)
		{
			if (!targetInfo.Value.IsEmpty && invoiceLine.JI_DeclarationGoodsDescription.Length + invoiceLine.JI_Group.Length > 512)
			{
				targetInfo.AddWarning(Res.GetString("79392189-F5CC-4631-A2A5-2EA31C57D144", "Grouping plus Declaration Goods Description Only the first 512 characters will be sent to the customs."));
			}
		}

		internal static void CheckDeclarationGoodsDescriptionLengthWhenReportingAircraftParts(JobComInvoiceLine invoiceLine, ZPropertyInfo targetInfo)
		{
			if (invoiceLine.JI_DeclarationGoodsDescription.Length > 512 && invoiceLine.AddInfoChild.ReportAircraftPartsIsNeeded)
			{
				targetInfo.AddMessageError(Res.GetString("9fbbc9a2-eb63-4023-a973-4f7adae1ef7e", "Declaration Goods Description must be equal to or less than 512 characters when reporting aircraft parts."));
			}
		}

		internal static void CheckNoOfPacksBalance(JobDeclaration declaration, ZPropertyInfo noOfPacksInfo)
		{
			if (declaration != null)
			{
				var totalNoOfPacks = new ZDecimal(declaration.JE_TotalNoOfPacks);
				var totalNoOfPacksForInvoices = declaration.Invoices.TotalNoOfPacks;
				if (totalNoOfPacks != totalNoOfPacksForInvoices)
				{
					noOfPacksInfo.AddWarning(ValidationConstants.InvoiceHeader.GetWarningDeclarationAndInvoicesIsUnbalanceForPackageNumber(declaration.JE_TotalNoOfPacksPackType, totalNoOfPacks, totalNoOfPacksForInvoices));
				}
			}
		}

		internal static void CheckPermitNumbersPlusSpecialCodeNumber(JobComInvoiceLine invoiceLine, ZPropertyInfo targetInfo)
		{
			if (invoiceLine != null)
			{
				if (invoiceLine.ExemptionOfControllingAgenciesCusSupportings.Count + invoiceLine.PermitCusSupportingCollection.Count > 5)
				{
					targetInfo.AddMessageError(Res.GetString("D6829D53-16DB-48F3-B8AF-255C2348CC89", "The sum of the number of Import/Export Permit plus the number of Special Code for Exemption of Controlling Agencies should not exceed 5."));
				}
			}
		}

		internal static void CheckOrganisationExistVAT(OrgHeader header, Action addMessageError)
		{
			if (header != null && !OrgHeaderHelper.CheckHasVatInTW(header))
			{
				addMessageError.Invoke();
			}
		}

		internal static string GetOriginInvalidCodeMessage(JobDeclaration declaration)
		{
			string message;
			if (IsOriginWithForeignPortIsRequired(declaration))
			{
				message = ValidationConstants.Declaration.EnterForeignPortCodeMessage;
			}
			else if (IsOriginWithTWPortCodeIsRequired(declaration))
			{
				message = ValidationConstants.Declaration.EnterTWPortCodeMessage;
			}
			else
			{
				message = Customs.Business.BaseJobDeclarationValidation.MessageErrorPortCodeInvalid;
			}
			return message;
		}

		internal static string GetFinalDestinationInvalidCodeMessage(JobDeclaration declaration)
		{
			string message;
			if (IsFinalDestinationWithForeignPortIsRequired(declaration))
			{
				message = ValidationConstants.Declaration.EnterForeignPortCodeMessage;
			}
			else if (IsFinalDestinationWithTWPortCodeIsRequired(declaration))
			{
				message = ValidationConstants.Declaration.EnterTWPortCodeMessage;
			}
			else
			{
				message = JobDeclarationValidation.MessageErrorPortCodeInvalid;
			}
			return message;
		}

		[ThreadSafe]
		static readonly ImmutableArray<ZString> importDeclarationTypeWithForeignPortIsRequired = ImmutableArray.Create<ZString>
		(
			Constants.DeclarationTypes.Import.G1,
			Constants.DeclarationTypes.Import.G7,
			Constants.DeclarationTypes.Import.F1
		);

		[ThreadSafe]
		static readonly ImmutableArray<ZString> importDeclarationTypeWithTWPortCodeIsRequired = ImmutableArray.Create<ZString>
		(
			Constants.DeclarationTypes.Import.G2,
			Constants.DeclarationTypes.Import.D2,
			Constants.DeclarationTypes.Import.D7,
			Constants.DeclarationTypes.Import.F2,
			Constants.DeclarationTypes.Import.F3
		);

		[ThreadSafe]
		static readonly ImmutableArray<ZString> importDeclarationTypeWithAllPortCodeIsRequired = ImmutableArray.Create<ZString>
		(
			Constants.DeclarationTypes.Import.D8,
			Constants.DeclarationTypes.Import.B6
		);

		[ThreadSafe]
		static readonly ImmutableArray<ZString> exportDeclarationTypeWithForeignPortIsRequired = ImmutableArray.Create<ZString>
		(
			Constants.DeclarationTypes.Export.G3,
			Constants.DeclarationTypes.Export.G5,
			Constants.DeclarationTypes.Export.F5
		);

		[ThreadSafe]
		static readonly ImmutableArray<ZString> exportDeclarationTypeWithTWPortCodeIsRequired = ImmutableArray.Create<ZString>
		(
			Constants.DeclarationTypes.Export.D1,
			Constants.DeclarationTypes.Export.B1,
			Constants.DeclarationTypes.Export.B2,
			Constants.DeclarationTypes.Export.F4
		);

		[ThreadSafe]
		static readonly ImmutableArray<ZString> exportDeclarationTypeWithAllPortCodeIsRequired = ImmutableArray.Create<ZString>
		(
			Constants.DeclarationTypes.Export.D5,
			Constants.DeclarationTypes.Export.B8,
			Constants.DeclarationTypes.Export.B9
		);

		internal static bool IsImportWarehouse2(JobDeclaration declaration) => HasEntryInstructions(declaration) && declaration.IsImport && !declaration.CusEntryInstruction.CEI_OA_Warehouse2.IsEmpty;

		internal static bool IsExportWarehouse(JobDeclaration declaration) => HasEntryInstructions(declaration) && declaration.IsExport && !declaration.CusEntryInstruction.CEI_OA_Warehouse.IsEmpty;

		internal static bool IsImportTransportMode(JobDeclaration declaration) => declaration.IsImport && !declaration.JE_TransportMode.IsEmpty;

		static bool HasEntryInstructions(JobDeclaration declaration) => declaration.CustomsEntryInstructionProvider?.CustomsEntryInstructions?.Any<CusEntryInstruction>() ?? false;

		internal static bool IsOriginWithForeignPortIsRequired(JobDeclaration declaration) => HasEntryInstructions(declaration) && declaration.IsImport && importDeclarationTypeWithForeignPortIsRequired.Contains(declaration.CusEntryInstruction.CEI_Style);

		internal static bool IsOriginWithTWPortCodeIsRequired(JobDeclaration declaration) => HasEntryInstructions(declaration) && declaration.IsImport && importDeclarationTypeWithTWPortCodeIsRequired.Contains(declaration.CusEntryInstruction.CEI_Style);

		internal static bool IsOriginWithAllPortCodeIsRequired(JobDeclaration declaration) => HasEntryInstructions(declaration) && declaration.IsImport && importDeclarationTypeWithAllPortCodeIsRequired.Contains(declaration.CusEntryInstruction.CEI_Style);

		internal static bool IsFinalDestinationWithForeignPortIsRequired(JobDeclaration declaration) => HasEntryInstructions(declaration) && declaration.IsExport && exportDeclarationTypeWithForeignPortIsRequired.Contains(declaration.CusEntryInstruction.CEI_Style);

		internal static bool IsFinalDestinationWithTWPortCodeIsRequired(JobDeclaration declaration) => HasEntryInstructions(declaration) && declaration.IsExport && exportDeclarationTypeWithTWPortCodeIsRequired.Contains(declaration.CusEntryInstruction.CEI_Style);

		internal static bool IsFinalDestinationWithAllPortCodeIsRequired(JobDeclaration declaration) => HasEntryInstructions(declaration) && declaration.IsExport && exportDeclarationTypeWithAllPortCodeIsRequired.Contains(declaration.CusEntryInstruction.CEI_Style);

		internal static bool IsDifferentInvoiceLineValueLinkedToSameControllingMsgHeader(JobComInvoiceLine invoiceLine, string messageType, Func<JobComInvoiceLine, ZString> propertySelectorFunc)
		{
			var linkControllingMsgHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.IsLinkedCMHeader && x.MessageType == messageType);
			return linkControllingMsgHeader?.ControllingMessageHeader?.ControllingMessageHeaderLinkInvoiceLines?.Cast<ControllingMessageHeaderLinkInvoiceLine>().Any(i => i.Link && i.Invoiceline != null && propertySelectorFunc(i.Invoiceline) != propertySelectorFunc(invoiceLine)) ?? false;
		}
	}
}
