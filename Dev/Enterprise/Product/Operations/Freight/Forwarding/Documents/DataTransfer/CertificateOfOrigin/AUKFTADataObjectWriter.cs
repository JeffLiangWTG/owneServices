using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class AUKFTADataObjectWriter : Base.CertificateOfOriginDataObjectWriter<AUKFTA, AUKFTALineItem>
	{
		protected override string DocumentType => "AUKFTA";

		public AUKFTADataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.AUKFTACertificateOfOrigin)
		{
		}

		protected override IEnumerable<AddInfo> CreateAddInfosAdditional(AUKFTA aukfta)
		{
			if (aukfta.DeclarationCompletedBy != null && !string.IsNullOrEmpty(aukfta.DeclarationCompletedBy.Value))
			{
				yield return CreateAddInfo(AddinfoTypes.DeclarationCompletedBy, aukfta.DeclarationCompletedBy.Value);
			}
		}
	}
}
