using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class CPTPP : CertificateOfOriginDocDataObject<CPTPPLineItem>
	{
		public CPTPP(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}
	}
}
