using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Supporters
{
	public interface ISupportingDocDataObject
	{
		AgreementInfo AgreementInfo { get; }
		DocSendingBusinessObjectCollection DocSendingCollection { get; }
	}
}
