using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using static Enterprise.Customs.NL.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NL.Business.Declaration;

public class PreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
{
	public PreviousDocumentValidation(PreviousDocument parent) : base(parent)
	{
	}

	protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		CheckRule_UC0849();
		var parent = Parent;
		if (parent.Parent is CusEntryInstruction && parent.CSI_Code == PreviousDocumentType.N705)
		{
			parent.CSI_CodeInfo.AddMessageError(Res.GetString("D7F64902-183E-456C-BE37-C5F3E76A0D46", "[NR9015] Previous document type N705 can only be used at 'Item/Line' level."));
		}
		CheckRuleBG9006();
	}

	protected override void CheckCSI_LineNo()
	{
		base.CheckCSI_LineNo();
		CheckRule_UC0849();
	}

	protected override bool IsSubTypeMandatory => false;

	void CheckRule_UC0849()
	{
		var parent = Parent;
		var errorMessage = Res.GetString("4A25B671-760D-48AC-B85D-84A3FF579A1C", "[C0849] If the 'Type' is C651, C658 or N705, then 'Line No.' is required.");
		parent.RemoveRowMessageError(errorMessage);

		if (parent.IsInExportInvoiceLinePreviousDocuments
			&& parent.CSI_Code.In(new ZString[] { PreviousDocumentType.C651,PreviousDocumentType.C658, PreviousDocumentType.N705 })
			&& parent.CSI_LineNo.IsEmpty)
		{
			parent.AddRowMessageError(errorMessage);
		}
	}

	void CheckRuleBG9006()
	{
		var parent = Parent;
		var errorMessage = Res.GetString("0781D972-DACB-4E21-8845-E2E95C6F9A1F", "[G9006] If previous document type = N705 then only one occurrence is allowed for previous document");
		parent.RemoveRowMessageError(errorMessage);

		var typeCode = parent.CSI_Code;
		if (parent.Parent is JobComInvoiceLine invoiceLine && !typeCode.IsEmpty && invoiceLine.PreviousDocuments.Cast<PreviousDocument>().Any(doc => doc.CSI_Code == PreviousDocumentType.N705) && invoiceLine.PreviousDocuments.Count > 1)
		{
			parent.AddRowMessageError(errorMessage);
		}
	}
}
