using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class COONZDataObjectWriter : Base.CertificateOfOriginDataObjectWriter<COONZ, COONZLineItem>
	{
		protected override string DocumentType => "COONZ";

		public COONZDataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.NZCertificateOfOrigin)
		{
		}

		protected override void PopulateGoodsDetailsPackingLine(COONZLineItem lineItem, UniversalDataBuss.DataObjects.Universal.PackingLine universalPackline)
		{
			universalPackline.CountryOfOrigin = new Country
			{
				Code = lineItem.OriginCode,
				Name = lineItem.Origin
			};
		}

		protected override void PopulateGoodsDetailsInvoiceLine(COONZLineItem lineItem, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceLine universalInvoiceLine)
		{
			universalInvoiceLine.CountryOfOrigin = new Country
			{
				Code = lineItem.OriginCode,
				Name = lineItem.Origin
			};
		}
	}
}
