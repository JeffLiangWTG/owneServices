using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class KAFTA : CertificateOfOriginDocDataObject<KAFTALineItem>
	{
		public KAFTA(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}
	}
}
