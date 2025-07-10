using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX5105GoodsShipment_GovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem
	{
		public NX5105GoodsShipment_GovernmentAgencyGoodsItem(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine)
		{
			EntryLine = Argument.NotNull(cusEntryLine, "cusEntryLine");
			InvoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
		}

		public CusEntryLine EntryLine { get; }

		internal JobComInvoiceLine InvoiceLine { get; }

		public ZInt SequenceNumeric => SequenceNumericCore;

		protected virtual ZInt SequenceNumericCore => EntryLine.CL_LineNumber;

		public ICommodity Commodity => commodity ?? (commodity = GetCommodityCore());
		ICommodity commodity;

		protected virtual ICommodity GetCommodityCore()
		{
			return new NX5105GovernmentAgencyGoodsItem_Commodity(EntryLine, InvoiceLine);
		}

		IEnumerable<IAdditionalDocument> IGovernmentAgencyGoodsItem.AdditionalDocuments
		{
			get
			{
				var invoiceLines = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>();
				var permitDocuments = invoiceLines.SelectMany(invoiceLine => invoiceLine.PermitCusSupportingCollection.Cast<PermitCusSupporting>().
					Where(x => !x.CSI_ReferenceNumber.IsEmpty).GroupBy(x => new { x.CSI_ReferenceNumber, x.CSI_LineNo }).Select(x => x.First()).Select(x => new AdditionalDocumentWrapper(x.CSI_ReferenceNumber, x.CSI_LineNo)));
				var exemptionOfControllingAgenciesDocuments = invoiceLines.SelectMany(invoiceLine => invoiceLine.ExemptionOfControllingAgenciesCusSupportings.Cast<ExemptionOfControllingAgenciesCusSupporting>().
					Where(x => !x.CSI_ReferenceNumber.IsEmpty).GroupBy(x => new { x.CSI_ReferenceNumber }).Select(x => x.First()).Select(x => new AdditionalDocumentWrapper(x.CSI_ReferenceNumber, 0)));
				return permitDocuments.Concat(exemptionOfControllingAgenciesDocuments).Take(5);
			}
		}

		IGoodsMeasure IGovernmentAgencyGoodsItem.GoodsMeasure => GetGoodsMeasure();

		protected virtual IGoodsMeasure GetGoodsMeasure()
		{
			return new NX5105GovernmentAgencyGoodsItem_GoodsMeasure(EntryLine);
		}

		IOrigin IGovernmentAgencyGoodsItem.Origin => new NX5105GovernmentAgencyGoodsItem_Origin(InvoiceLine);

		IPreviousDocument IGovernmentAgencyGoodsItem.PreviousDocument => InvoiceLine.GetPreviousDocument();

		public virtual IGoodsStatisticalMeasure GoodsStatisticalMeasure
		{
			get
			{
				IGoodsStatisticalMeasure goodsStatisticalMeasure = null;
				var customsSecondQty = EntryLine.CL_Calc_CustomsSecondQuantity;
				var customsSecondUnitQty = EntryLine.CL_CustomsSecondUnitQty;
				if (!customsSecondQty.IsEmpty && !customsSecondUnitQty.IsEmpty)
				{
					goodsStatisticalMeasure = new GoodsStatisticalMeasureWrapper(customsSecondQty, customsSecondUnitQty);
				}
				return goodsStatisticalMeasure;
			}
		}

		IPreviousDocument IGovernmentAgencyGoodsItem.PreBondedDocument => InvoiceLine.GetPreBondedDocument();

		IPartyDetails IGovernmentAgencyGoodsItem.Manufacturer
		{
			get
			{
				IPartyDetails result = null;
				var manufacturerAddress = InvoiceLine.ManufacturerDocAddress;
				var countryCode = manufacturerAddress.E2_RN_NKCountryCode;
				if (!countryCode.IsEmpty && countryCode != Core.Constants.CountryCodes.Taiwan)
				{
					result = GetNewManufacturerWrapperCore(manufacturerAddress);
				}
				return result;
			}
		}

		protected virtual IPartyDetails GetNewManufacturerWrapperCore(TWJobDocAddress address)
		{
			return new NX5105ManufacturerWrapper(address, InvoiceLine);
		}

		IEnumerable<IAdditionalInformation> IGovernmentAgencyGoodsItem.AdditionalInformations => InvoiceLine.GetAdditionalInformations();

		IGoodsLicensingStatisticalMeasure IGovernmentAgencyGoodsItem.GoodsLicensingStatisticalMeasure => GoodsLicensingStatisticalMeasureCore;

		protected virtual IGoodsLicensingStatisticalMeasure GoodsLicensingStatisticalMeasureCore => null;

		ILPCODetail IGovernmentAgencyGoodsItem.MedicalInstrument => GetMedicalInstrumentCore();
		protected virtual ILPCODetail GetMedicalInstrumentCore() => null;

		#region Not Applicable

		IPackaging IGovernmentAgencyGoodsItem.Packaging => InvoiceLine.JI_PackagingQTY.IsEmpty ? null : new PackingWarpper(ZDate.Empty, InvoiceLine.JI_PackagingUQ, InvoiceLine.JI_PackagingQTY);

		ILPCODetail IGovernmentAgencyGoodsItem.ApprovalDocument => GetApprovalDocumentCore;
		protected virtual ILPCODetail GetApprovalDocumentCore => null;

		ICommoditySpecification IGovernmentAgencyGoodsItem.CommoditySpecification => null;

		IEnumerable<IShippingIdentification> IGovernmentAgencyGoodsItem.ShippingIdentifications => GetShippingIdentificationsCore;
		protected virtual IEnumerable<IShippingIdentification> GetShippingIdentificationsCore => null;

		IGovernmentProcedure IGovernmentAgencyGoodsItem.GovernmentProcedure => null;

		ZDateTime IGovernmentAgencyGoodsItem.ControlInspectionStartDateTime => ZDateTime.Empty;

		ZString IGovernmentAgencyGoodsItem.ExaminationPlace => ZString.Empty;

		IEnumerable<ITransportEquipment> IGovernmentAgencyGoodsItem.TransportEquipments => null;

		IAdditionalDeclaration IGovernmentAgencyGoodsItem.AdditionalDeclaration => null;

		#endregion

		public virtual ZString ExtraInfoForClassification => EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => !x.JI_ExtraInfoForClassification.IsEmpty)?.JI_ExtraInfoForClassification ?? ZString.Empty;

		#region Properties For Documents

		public virtual ZString EntryLineGroupForDocument => EntryLine?.CL_Calc_EntryLineGroup ?? ZString.Empty;
		public virtual ZString CommodityDescriptionForDocument => EntryLine?.CL_Calc_GoodsDescriptionWithoutGrouping ?? ZString.Empty;

		#endregion

		public ZString CriteriaCode => null;

		public ZString PreferentialCriteria => null;

		public ZString ProducerCode => null;

		public ZString OtherCriteria => null;
	}
}
