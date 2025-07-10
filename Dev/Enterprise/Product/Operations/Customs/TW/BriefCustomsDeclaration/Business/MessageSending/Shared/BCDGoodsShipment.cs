using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class BCDGoodsShipment : IBCDGoodsShipment
	{
		public BCDGoodsShipment(AsycudaBill bill, IAdditionalSupportingDocument addtionalDoc)
		{
			HouseBill = Argument.NotNull(bill, nameof(bill));
			AddtionalDoc = addtionalDoc;
		}
		protected AsycudaBill HouseBill { get; }

		protected IAdditionalSupportingDocument AddtionalDoc { get; }

		ZInt IBCDGoodsShipment.SequenceNumeric => HouseBill.ABL_SequenceNumber;

		ZDecimal IBCDGoodsShipment.TotalGrossMassMeasure => HouseBill.MassInKilos;

		IBCDConsignment IBCDGoodsShipment.Consignment => new BCDGoodsShipmentConsignment(HouseBill);

		IConsignment IGoodsShipment.Consignment => null;

		IEnumerable<IAdditionalDocument> IGoodsShipment.AdditionalDocuments => GetAdditionalDocumentsCore();

		protected virtual IEnumerable<IAdditionalDocument> GetAdditionalDocumentsCore()
		{
			var billNumber = HouseBill.ABL_BillNumber;
			var documents = billNumber.IsEmpty ? Array.Empty<SupportingDocument>()
				: AddtionalDoc.GetSupportingDocuments().Where(document => document.BillNumber == billNumber);
			return new AdditionalDocumentWrapper().GetDocuments(documents, AddtionalDoc.GetAllEDocs());
		}

		IEnumerable<IGovernmentAgencyGoodsItem> IGoodsShipment.GovernmentAgencyGoodsItems => HouseBill.PackedItems.Cast<AsycudaPackedItem>().OrderBy(x => x.API_LineNo).Select((p, index) => new GovernmentAgencyGoodsItem(p, index + 1));

		ZDateTime IGoodsShipment.ExitDateTime => ZDate.Empty;

		ZDecimal IGoodsShipment.ItemChargeAmount => HouseBill.ABL_CustomsValue;

		ZDecimal IGoodsShipment.TotalCIFAmount => ZDecimal.Zero;

		IPartyDetails IGoodsShipment.Consignee => new BCDConsignee(HouseBill);

		IPartyDetails IGoodsShipment.Consignor => null;

		ICustomsValuation IGoodsShipment.CustomsValuation => null;

		ZString IGoodsShipment.DeliveryDestinationName => HouseBill.ABL_LocationInformation;

		IEnumerable<IGoodsShipmentDutyTaxFee> IGoodsShipment.DutyTaxFees => null;

		IPartyDetails IGoodsShipment.NotifyParty => null;

		IPartyDetails IGoodsShipment.Seller => null;

		ZString IGoodsShipment.TradeTermsConditionCode => HouseBill.ABL_Incoterm;

		ZString IGoodsShipment.UCR => ZString.Empty;

		IPartyDetails IGoodsShipment.Buyer => new BCDConsignee(HouseBill);

		IPartyDetails IGoodsShipment.Exporter => new BCDExporter(HouseBill);

		IEnumerable<IGoodsMeasure> IGoodsShipment.GoodsMeasures => null;

		IEnumerable<IAdditionalInformation> IGoodsShipment.AdditionalInformations => null;

		IEnumerable<IAdditionalDeclaration> IGoodsShipment.AdditionalDeclarations => null;
	}
}
