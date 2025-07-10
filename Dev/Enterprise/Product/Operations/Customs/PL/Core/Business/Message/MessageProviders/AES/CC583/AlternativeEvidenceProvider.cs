using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

class AlternativeEvidenceProvider(AlternativeEvidence alternativeEvidence, int index) : IAlternativeEvidence
{
	readonly AlternativeEvidence alternativeEvidence = alternativeEvidence;

	public int SequenceNumber => index;

	public string Type => alternativeEvidence.EvidenceType;

	public IReadOnlyCollection<ITransportDocument> TransportDocuments =>
		transportDocuments ??= CheckRuleC0684() ? [new TransportDocumentProvider(1, alternativeEvidence.DocType, alternativeEvidence.Reference)] : [];
	IReadOnlyCollection<ITransportDocument> transportDocuments;

	readonly string[] ruleC0684EvidenceTypes = ["11", "14", "15", "17"];

	bool CheckRuleC0684() => ruleC0684EvidenceTypes.Contains(alternativeEvidence.EvidenceType.ToString());
}
