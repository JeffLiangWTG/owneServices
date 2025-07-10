using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class IACEPADataObjectWriter : Base.CertificateOfOriginDataObjectWriter<IACEPA, IACEPALineItem>
	{
		protected override string DocumentType => "IACEPA";

		public IACEPADataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.IACEPACertificateOfOrigin)
		{
		}

		protected override IEnumerable<AddInfo> CreateAddInfosAdditional(IACEPA iacepa)
		{
			if (iacepa.IsExhibition && !string.IsNullOrEmpty(iacepa.ExhibitionDetail))
			{
				yield return CreateAddInfo(AddinfoTypes.ExhibitionDetail, iacepa.ExhibitionDetail);
			}
		}
	}
}
