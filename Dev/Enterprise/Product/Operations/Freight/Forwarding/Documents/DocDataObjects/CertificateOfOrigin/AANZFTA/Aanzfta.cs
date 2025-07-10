using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class Aanzfta : CertificateOfOriginDocDataObject<AanzftaLineItem>
	{
		public Aanzfta(ZString sourceType, ZString sourceID) : base(sourceType, sourceID) { }
	}
}
