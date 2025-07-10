using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class TAFTADataObjectWriter : Base.CertificateOfOriginDataObjectWriter<TAFTA, TAFTALineItem>
	{
		protected override string DocumentType => "TAFTA";

		public TAFTADataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.TAFTACertificateOfOrigin)
		{
		}

		protected override OrganizationAddress GetBuyerAddress(TAFTA tafta)
		{
			if (tafta.BuyerAddress.IsEmpty())
			{
				return null;
			}

			return tafta.BuyerAddress.ToUXmlOrganizationAddress(nameof(tafta.BuyerAddress), writeManager.WriterStrategy);
		}
	}
}
