using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AddInfoCusEntryInstructionValidation(AddInfoCusEntryInstruction parent) : EU.Business.Declaration.AddInfoCusEntryInstructionValidation(parent)
{
	protected new AddInfoCusEntryInstruction Parent => (AddInfoCusEntryInstruction)base.Parent;

	CusEntryInstruction ParentInstruction => Parent?.Parent;

	#region TemporaryLocation

	protected override void CheckZG_TemporaryLocationCodeType()
	{
		base.CheckZG_TemporaryLocationCodeType();
		if (Parent.ZG_ExportManifest)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_TemporaryLocationCodeTypeInfo);
		}
	}

	protected override void CheckZG_TemporaryLocation()
	{
		if (Parent.ZG_ExportManifest &&
			!Parent.ZG_TemporaryLocationCodeType.IsEmpty &&
			Parent.ZG_TemporaryLocation.IsEmpty)
		{
			Parent.ZG_TemporaryLocationInfo.AddMessageError(Res.GetString("16CAA65A-7260-4FDE-96A4-5B8CB4897B04", "Number is required but is empty."));
		}
	}

	#endregion

	#region ZG_EADPrintOut

	protected override void CheckZG_EADPrintOut()
	{
		base.CheckZG_EADPrintOut();

		if (ParentInstruction is { } instruction
			&& instruction.IsExitSummary)
		{
			return;
		}

		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_EADPrintOutInfo);
	}

	#endregion

	#region ZG_OfficeOfExitArrivalTimeLimit

	protected override void CheckZG_OfficeOfExitArrivalTimeLimit()
	{
		base.CheckZG_OfficeOfExitArrivalTimeLimit();
		var parentDeclaration = ParentInstruction?.JobDeclaration;
		if (parentDeclaration != null)
		{
			if (!parentDeclaration.JE_LocationOfGoods.IsEmpty &&
				parentDeclaration.JE_LocationQualifier == GoodsLocationTypeList.Codes.GLC &&
				LinkedInvoiceLineHasAuthorisationForCentralisedClearance())
			{
				var sourceValue = Parent.ZG_OfficeOfExitArrivalTimeLimit;
				if (sourceValue.IsEmpty || sourceValue <= ZDateTime.UtcNow)
				{
					Parent.ZG_OfficeOfExitArrivalTimeLimitInfo.AddMessageError(Res.GetString("14345029-2DDD-4846-9530-80AFD1510422", "Office Of Exit Arrival Time Limit must be in the future."));
				}
			}
		}
	}

	bool LinkedInvoiceLineHasAuthorisationForCentralisedClearance()
	{
		var parentInstruction = Parent.Parent;
		var result = false;
		result = parentInstruction.InvoiceLines.OfType<ISupportingDocumentsProvider>().Any(invLine => HasAuthorisationForCentralisedClearance(invLine));
		result = result || parentInstruction.InvoiceLines.Select(l => l.InvoiceHeader).Distinct().OfType<ISupportingDocumentsProvider>().Any(invHeader => HasAuthorisationForCentralisedClearance(invHeader));
		return result;
	}

	bool HasAuthorisationForCentralisedClearance(ISupportingDocumentsProvider provider)
	{
		return provider?.SupportingDocuments.OfType<SupportingDocument>().Any(sup => sup.CSI_Code == SupportingDocumentCodes.C513 || sup.CSI_Code == SupportingDocumentCodes.C514) ?? false;
	}

	#endregion
}
