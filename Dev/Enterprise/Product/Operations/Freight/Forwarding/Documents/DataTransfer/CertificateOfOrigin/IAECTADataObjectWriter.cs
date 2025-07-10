using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class IAECTADataObjectWriter : Base.CertificateOfOriginDataObjectWriter<IAECTA, IAECTALineItem>
	{
		protected override string DocumentType => "IAECTA";

		public IAECTADataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.IAECTACertificateOfOrigin)
		{
		}

		protected override IEnumerable<AddInfo> CreateAddInfosAdditional(IAECTA iaecta)
		{
			if (!string.IsNullOrEmpty(iaecta.ExportDocumentNumber))
			{
				yield return CreateAddInfo(AddinfoTypes.ExportDocumentNumber, iaecta.ExportDocumentNumber);
			}
		}
	}
}
