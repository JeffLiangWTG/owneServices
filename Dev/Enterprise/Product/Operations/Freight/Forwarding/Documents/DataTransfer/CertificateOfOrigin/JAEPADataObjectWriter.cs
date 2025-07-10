using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class JAEPADataObjectWriter : Base.CertificateOfOriginDataObjectWriter<JAEPA, JAEPALineItem>
	{
		protected override string DocumentType => "JAEPA";

		public JAEPADataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.JAEPACertificateOfOrigin)
		{
		}
	}
}
