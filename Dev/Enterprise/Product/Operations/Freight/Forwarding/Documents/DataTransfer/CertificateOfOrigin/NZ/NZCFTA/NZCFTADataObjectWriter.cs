using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin.NZ
{
	sealed class NZCFTADataObjectWriter : Base.CertificateOfOriginDataObjectWriter<NZCFTA, NZCFTALineItem>
	{
		protected override string DocumentType => "NZCFTA";

		public NZCFTADataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.NZCFTACertificateOfOrigin)
		{
		}

		protected override IEnumerable<AddInfo> CreatePackingAddInfoAdditional(NZCFTALineItem lineItem)
			=> PopulateQuantityValues(lineItem);

		protected override IEnumerable<AddInfo> CreateCommercialInvoiceAddInfoAdditional(NZCFTALineItem lineItem)
			=> PopulateQuantityValues(lineItem);

		IEnumerable<AddInfo> PopulateQuantityValues(NZCFTALineItem lineItem)
		{
			if (lineItem.QuantityUnit is not null)
			{
				yield return CreateAddInfo(nameof(lineItem.QuantityUnit), lineItem.QuantityUnit.Code);
			}
			else
			{
				yield return CreateAddInfo(nameof(lineItem.QuantityUnit), string.Empty);
			}

			yield return CreateAddInfo(nameof(lineItem.QuantityNumber), lineItem.QuantityNumber);
		}
	}
}
