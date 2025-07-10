using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class PAFTADataObjectWriter : Base.CertificateOfOriginDataObjectWriter<PAFTA, PAFTALineItem>
	{
		protected override string DocumentType => "PAFTA";

		public PAFTADataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.PAFTACertificateOfOrigin)
		{
		}
	}
}
