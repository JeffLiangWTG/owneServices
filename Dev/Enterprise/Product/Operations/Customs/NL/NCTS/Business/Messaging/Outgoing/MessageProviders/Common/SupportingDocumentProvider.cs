using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class SupportingDocumentProvider : DocumentProvider, INCTSSupportingDocument
{
	public SupportingDocumentProvider(CusSupportingInfo cusSupportingInfo) : base(cusSupportingInfo)
	{
	}

	public string ComplementOfInformation => document.CSI_ReferenceNumber2;

	public int DocumentLineItemNumber => document.CSI_ItemNumber;
}
