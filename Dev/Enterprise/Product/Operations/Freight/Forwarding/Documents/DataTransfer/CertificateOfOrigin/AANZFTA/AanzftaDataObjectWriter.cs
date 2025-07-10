using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class AanzftaDataObjectWriter : Base.CertificateOfOriginDataObjectWriter<Aanzfta, AanzftaLineItem>
	{
		public AanzftaDataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.AANZFTACertificateOfOrigin)
		{
		}

		protected override string DocumentType => "AANZFTA";

		protected override IEnumerable<AddInfo> CreateAddInfosAdditional(Aanzfta aanzfta)
		{
			if (aanzfta.IsSubjectOfThirdPartyInvoice &&
				!string.IsNullOrEmpty(aanzfta.ThirdPartyInvoiceIssuer))
			{
				yield return CreateAddInfo(AddinfoTypes.ThirdPartyInvoiceIssuer, aanzfta.ThirdPartyInvoiceIssuer);
			}
		}
	}
}
