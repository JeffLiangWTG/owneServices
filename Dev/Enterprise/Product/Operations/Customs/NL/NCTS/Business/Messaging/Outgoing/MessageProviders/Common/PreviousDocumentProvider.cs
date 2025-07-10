using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class PreviousDocumentProvider : DocumentProvider, INCTSPreviousDocument
{
	public PreviousDocumentProvider(CusSupportingInfo document) : base(document)
	{
	}

	public string ComplementOfInformation => document.CSI_ReferenceNumber2;
}
