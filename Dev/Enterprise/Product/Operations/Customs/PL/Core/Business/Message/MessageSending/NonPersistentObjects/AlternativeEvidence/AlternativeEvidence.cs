using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business;

public class AlternativeEvidence(BaseMessageSendingObjectParent messageSendingObjectParent) : JobDeclarationAlternativeEvidence(messageSendingObjectParent.Factory)
{
	public BaseMessageSendingObjectParent Parent => messageSendingObjectParent;

	[ResourceStringData("E89A6375-DFAD-4E1D-8FA5-7A0EC3AB3724", Caption = "Alternative Evidence Type")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationAlternativeEvidenceLookups.AlternativeEvidenceTypeList))]
	public override ZString EvidenceType
	{
		get => base.EvidenceType;
		set => base.EvidenceType = value;
	}

	[ResourceStringData("F4AFCC76-E517-4585-85EB-576F008D3042", Caption = "Transport Document Type")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationAlternativeEvidenceLookups.TransportDocumentTypeList))]
	public override ZString DocType
	{
		get => base.DocType;
		set => base.DocType = value;
	}

	protected override bool DocType_ReadOnly => !(EvidenceType.ToString() is "11" or "14" or "15" or "17");
}
