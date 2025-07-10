using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC044CSupportingDocumentProvider : CC044CDocumentProvider, INCTSSupportingDocument
{
	public CC044CSupportingDocumentProvider(CusSupportingInfo document) : base(document)
	{
	}

	public int DocumentLineItemNumber => 0;

	public string ComplementOfInformation => StatusIsNew ? document.CSI_ReferenceNumber2 : null;
}
