using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class TAFTA : CertificateOfOriginDocDataObject<TAFTALineItem>
	{
		public TAFTA(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}

		#region BuyerAddress

		public Address BuyerAddress
		{
			get => buyerAddress;
			set => buyerAddress = SetChild(buyerAddress, value);
		}

		Address buyerAddress;

		#endregion
	}
}
