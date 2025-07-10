using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class RCEP : CertificateOfOriginDocDataObject<RCEPLineItem>
	{
		public RCEP(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}
	}
}
