using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class DocumentProvider : IDocument
{
	public DocumentProvider(CusSupportingInfo document)
	{
		this.document = Argument.NotNull(document, nameof(document));
	}
	protected readonly CusSupportingInfo document;

	public int SequenceNumeric => document.CSI_LineNo;

	public virtual string Type => document.CSI_Code;

	public virtual string ReferenceNumber => document.CSI_ReferenceNumber;
}
