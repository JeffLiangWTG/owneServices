using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public class SupportingDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
{
	public SupportingDocumentValidation(SupportingDocument parent)
		: base(parent)
	{
	}

	protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

	protected IEnumerable<CusEntryInstruction> GetRelatedEntryInstructions()
	{
		var supportingDocumentParent = Parent.Parent;

		switch (supportingDocumentParent)
		{
			case JobComInvoiceLine invoiceLine:
				return new[] { invoiceLine.EntryInstruction };
			case JobComInvoiceHeader invoiceHeader:
				return invoiceHeader.CusEntryInstructions.Cast<CusEntryInstruction>();
			case JobDeclaration declaration:
				return declaration.CustomsEntryInstructions;
			default:
				return Enumerable.Empty<CusEntryInstruction>();
		}
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();
		if (Parent.CSI_Quantity > 0)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_UnitOfQuantityInfo);
		}
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantityInfo);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		if (!Parent.CSI_UnitOfQuantity.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfIsZero(Parent.CSI_QuantityInfo);
		}
	}

	protected override void CheckCSI_ReferenceNumber2() { }

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
	}
}
