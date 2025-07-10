using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public class SupportingDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
{
	public SupportingDocumentValidation(SupportingDocument parent) : base(parent)
	{
	}

	protected override void CheckDuplicatedDocuments(ZPropertyInfo info)
	{
		CheckRuleR9004(info);
	}

	protected override void CheckCSI_RX_NKCurrency()
	{
		base.CheckCSI_RX_NKCurrency();
		var parent = Parent;

		if (!parent.CSI_Value.IsEmpty && parent.CSI_RX_NKCurrency.IsEmpty)
		{
			parent.CSI_RX_NKCurrencyInfo.AddMessageError(Res.GetString("F9924BD0-422B-4262-A03F-7F26D12D785A", "If Value isn't empty, Currency is required."));
		}
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();
		var parent = Parent;
		if (parent.CSI_Quantity > 0)
		{
			if (parent.CSI_UnitOfQuantity.IsEmpty)
			{
				parent.CSI_UnitOfQuantityInfo.AddMessageError(Res.GetString("1E0EB4CD-EDD1-4307-BA37-609C5A49F2D4", "If Quantity isn't empty, Unit of Quantity is required."));
			}
			if (IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.CSI_UnitOfQuantityInfo);
			}
		}
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		CheckWhetherQuantityAndValueExistsMeanwhile(Parent.CSI_QuantityInfo);
	}

	protected override void CheckCSI_Value()
	{
		base.CheckCSI_Value();
		CheckWhetherQuantityAndValueExistsMeanwhile(Parent.CSI_ValueInfo);
	}

	void CheckWhetherQuantityAndValueExistsMeanwhile(ZPropertyInfo info)
	{
		if (!Parent.CSI_Quantity.IsEmpty && !Parent.CSI_Value.IsEmpty)
		{
			info.AddMessageError(Res.GetString("C50EC76C-4380-4D6C-9A66-E428E093FF89", "If Qty is completed value details should be disabled and vice versa"));
		}
	}

	protected override void CheckCSI_UnitOfQuantity2()
	{
		base.CheckCSI_UnitOfQuantity2();
		if (IsImport && Parent.CSI_Quantity2 > 0)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_UnitOfQuantity2Info);
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantity2Info);
		}
	}

	bool IsImport => Parent?.ImportExportParent?.IsImport ?? false;

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		CheckRuleR9004(Parent.CSI_ReferenceNumberInfo);
	}

	void CheckRuleR9004(ZPropertyInfo info)
	{
		var parent = Parent;
		var errorMessage = Res.GetString("4D5698C1-41DE-485E-95F2-7C6039ADAD04", "[R9004] Combination of Type and Reference must be unique.");
		if (parent.Parent is JobComInvoiceLine invoiceLine)
		{
			var entryInstructionHasValidSubStyle = invoiceLine.EntryInstruction != null && invoiceLine.EntryInstruction.CEI_SubStyle.In(
				new ZString[] { EntrySubStyleList.Codes.IncompleteDeclaration, EntrySubStyleList.Codes.SimplifiedDeclaration,
					EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC }) && parent.CSI_ReferenceNumber.IsEmpty;
			if (!entryInstructionHasValidSubStyle && HasDuplicateSupportingDocuments(invoiceLine.SupportingDocuments.OfType<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument>().ToList()))
			{
				info.AddMessageError(errorMessage);
			}
		}
	}
}
