using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class CPTPPDataObjectWriter : Base.CertificateOfOriginDataObjectWriter<CPTPP, CPTPPLineItem>
	{
		protected override string DocumentType => "CPTPP";

		public CPTPPDataObjectWriter(IDataWritingManager writeManager, DocumentVisualizer.Core.IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.CPTPPCertificateOfOrigin)
		{
		}
	}
}
