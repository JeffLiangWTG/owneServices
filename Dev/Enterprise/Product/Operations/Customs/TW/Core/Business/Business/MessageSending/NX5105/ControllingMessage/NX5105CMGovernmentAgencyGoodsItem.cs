using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX5105CMGovernmentAgencyGoodsItem : NX5105GoodsShipment_GovernmentAgencyGoodsItem
	{
		public NX5105CMGovernmentAgencyGoodsItem(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine) : base(cusEntryLine, invoiceLine)
		{
		}

		protected override ICommodity GetCommodityCore()
		{
			return new NX5105CMCommodity(EntryLine, InvoiceLine);
		}

		protected override IGoodsLicensingStatisticalMeasure GoodsLicensingStatisticalMeasureCore
		{
			get
			{
				IGoodsLicensingStatisticalMeasure result = null;
				var licensingQuantity = InvoiceLine.JI_CustomsThirdQuantity;
				var statisticalUnitCode = InvoiceLine.JI_CustomsThirdUnitQty;
				if (!licensingQuantity.IsEmpty || !statisticalUnitCode.IsEmpty)
				{
					result = new GoodsLicensingStatisticalMeasureWrapper(licensingQuantity, statisticalUnitCode);
				}
				return result;
			}
		}

		protected override IEnumerable<IShippingIdentification> GetShippingIdentificationsCore =>
			InvoiceLine.ShippingIdentificationDataCollection.Cast<ShippingIdentificationData>().Select(x => new ShippingIdentificationWrapper(x.TW_ManufacturedLotNo, x.TW_ExpirationDate, x.TW_ProductLotNoAmount, x.TW_ManufacturedDate));

		protected override ILPCODetail GetApprovalDocumentCore => new ApprovalDocumentWrapper(InvoiceLine.ExemptionCode, InvoiceLine.TypeApprovalCertificateNo, InvoiceLine.TypeApprovalAuthorizedParty, InvoiceLine.TypeApprovalPartyIdentifier);

		protected override ILPCODetail GetMedicalInstrumentCore() => new LPCODetailWrapper(InvoiceLine.CertificateNo, InvoiceLine.AuthorizedPerson, InvoiceLine.PartyIdentifier);

		protected override IPartyDetails GetNewManufacturerWrapperCore(TWJobDocAddress address)
		{
			return new NX5105CMManufacturerWrapper(address, InvoiceLine);
		}
	}
}
