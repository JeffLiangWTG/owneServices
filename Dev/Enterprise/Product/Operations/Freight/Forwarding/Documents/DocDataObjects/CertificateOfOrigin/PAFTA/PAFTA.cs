using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class PAFTA : CertificateOfOriginDocDataObject<PAFTALineItem>
	{
		public PAFTA(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}
	}
}
