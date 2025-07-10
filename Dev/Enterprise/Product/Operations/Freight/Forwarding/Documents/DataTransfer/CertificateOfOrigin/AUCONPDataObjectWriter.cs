using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class AUCONPDataObjectWriter : Base.CertificateOfOriginDataObjectWriter<AUCONP, AUCONPLineItem>
	{
		protected override string DocumentType => "AUCONP";

		public AUCONPDataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.AUCONPCertificateOfOrigin)
		{
		}

		protected override IEnumerable<AddInfo> CreateAddInfosAdditional(AUCONP auconp)
		{
			if (!string.IsNullOrEmpty(auconp.ExporterReference))
			{
				yield return CreateAddInfo(AddinfoTypes.ExporterReference, auconp.ExporterReference);
			}
		}
	}
}
