using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class JAEPA : CertificateOfOriginDocDataObject<JAEPALineItem>
	{
		public JAEPA(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}
	}
}
