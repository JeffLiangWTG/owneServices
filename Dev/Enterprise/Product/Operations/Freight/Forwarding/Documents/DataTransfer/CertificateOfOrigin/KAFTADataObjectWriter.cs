using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class KAFTADataObjectWriter : Base.CertificateOfOriginDataObjectWriter<KAFTA, KAFTALineItem>
	{
		protected override string DocumentType => "KAFTA";

		public KAFTADataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.KAFTACertificateOfOrigin)
		{
		}
	}
}
