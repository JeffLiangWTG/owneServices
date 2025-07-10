using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC044CDocumentProvider : DocumentProvider
{
	public CC044CDocumentProvider(CusSupportingInfo document) : base(document)
	{
	}

	public override string Type => StatusIsNew ? document.CSI_Code : null;

	public override string ReferenceNumber => StatusIsNew ? document.CSI_ReferenceNumber : null;

	protected bool StatusIsNew => document.CSI_Status == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
}
