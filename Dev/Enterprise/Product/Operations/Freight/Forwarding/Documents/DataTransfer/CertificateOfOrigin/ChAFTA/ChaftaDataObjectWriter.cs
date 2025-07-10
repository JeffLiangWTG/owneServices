using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class ChaftaDataObjectWriter : Base.CertificateOfOriginDataObjectWriter<Chafta, ChaftaLineItem>
	{
		protected override string DocumentType => "CHAFTA";

		public ChaftaDataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.ChaftaCertificateOfOrigin)
		{
		}

		protected override UniversalShipment PopulateDataObject(Chafta certificate)
		{
			var shipment = base.PopulateDataObject(certificate);
			shipment.PlaceOfDelivery = certificate.PlaceOfDelivery.ToUXmlUnloco();
			shipment.PlaceOfIssue = certificate.PlaceOfIssue.ToUXmlUnloco();
			shipment.PlaceOfReceipt = certificate.PlaceOfReceipt.ToUXmlUnloco();
			return shipment;
		}
	}
}
