using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class RCEPLineItem : CertificateOfOriginLineItemDocDataObject
	{
		public RCEPLineItem(object id) : base(id)
		{
		}

		#region FOB

		public Money FOB
		{
			get => fob;
			set => fob = SetChild(fob, value);
		}

		Money fob;

		#endregion
	}
}
