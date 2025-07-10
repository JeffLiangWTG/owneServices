using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class Chafta : CertificateOfOriginDocDataObject<ChaftaLineItem>
	{
		public Chafta(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}

		#region PlaceOfDelivery

		public IUnloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}

		IUnloco placeOfDelivery;

		#endregion

		#region PlaceOfIssue

		public IUnloco PlaceOfIssue
		{
			get => placeOfIssue;
			set => placeOfIssue = SetChild(placeOfIssue, value);
		}

		IUnloco placeOfIssue;

		#endregion

		#region PlaceOfReceipt

		public IUnloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}

		IUnloco placeOfReceipt;

		#endregion
	}
}
