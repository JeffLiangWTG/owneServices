using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CertificateOfOrigin
{
	sealed class RCEPDataObjectWriter : Base.CertificateOfOriginDataObjectWriter<RCEP, RCEPLineItem>
	{
		protected override string DocumentType => (NoResString)"RCEP";

		public RCEPDataObjectWriter(IDataWritingManager writeManager, IDocument document)
			: base(writeManager, document, ShipmentDocumentNames.RCEPCertificateOfOrigin)
		{
		}

		protected override IEnumerable<AddInfo> CreatePackingAddInfoAdditional(RCEPLineItem lineItem)
		{
			if (lineItem.OriginCriterion?.Code == (ZString?)OriginCriterionListRCEP.Codes.RVC)
			{
				yield return CreateAddInfo(AddinfoTypes.FOBCurrency, lineItem.FOB.Currency.Code);
				yield return CreateAddInfo(AddinfoTypes.FOBAmount, lineItem.FOB.Amount);
			}
		}
	}
}
