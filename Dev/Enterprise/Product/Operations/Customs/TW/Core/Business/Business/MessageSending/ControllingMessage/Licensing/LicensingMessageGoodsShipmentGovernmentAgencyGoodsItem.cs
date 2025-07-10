using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem :
		IGovernmentAgencyGoodsItem,
		IGoodsMeasure,
		IOrigin,
		ILPCODetail,
		ICommoditySpecification,
		IGoodsLicensingStatisticalMeasure
	{
		protected CusTWControllingMessageHeader Header { get; }
		protected JobComInvoiceLine InvoiceLine { get; }

		public LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			InvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
			GoodsItemSequenceNumeric = sequenceNumeric;
			Header = header;
		}

		ZInt GoodsItemSequenceNumeric { get; }

		ZInt IGovernmentAgencyGoodsItem.SequenceNumeric => GetSequenceNumericCore();

		protected virtual ZInt GetSequenceNumericCore() => GoodsItemSequenceNumeric;

		ICommodity IGovernmentAgencyGoodsItem.Commodity => GetCommodityCore();

		protected virtual ICommodity GetCommodityCore() => new LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(Header, InvoiceLine);

		IEnumerable<IAdditionalDocument> IGovernmentAgencyGoodsItem.AdditionalDocuments => new[] { new AdditionalDocumentWrapper(ZString.Empty, GoodsItemSequenceNumeric) };

		public IEnumerable<IAdditionalInformation> AdditionalInformations => InvoiceLine.ReservedFields.Cast<JobComInvoiceLineReservedField>().Select(y => new AdditionalInformationWrapper(y.CY_Code, y.CY_Data));

		public IGoodsMeasure GoodsMeasure => this;

		IPartyDetails IGovernmentAgencyGoodsItem.Manufacturer => InvoiceLine.ManufacturerDocAddress is TWJobDocAddress manufacturerDocAddress ? new LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemManufacturer(manufacturerDocAddress) : null;

		public IOrigin Origin => this;

		IPackaging IGovernmentAgencyGoodsItem.Packaging => GetPackagingCore();

		protected virtual IPackaging GetPackagingCore() => new PackingWarpper(ZDate.Empty, InvoiceLine.JI_PackagingUQ, InvoiceLine.JI_PackagingQTY);

		IPreviousDocument IGovernmentAgencyGoodsItem.PreviousDocument => null;

		ILPCODetail IGovernmentAgencyGoodsItem.ApprovalDocument => this;

		ICommoditySpecification IGovernmentAgencyGoodsItem.CommoditySpecification => this;

		public IGoodsLicensingStatisticalMeasure GoodsLicensingStatisticalMeasure => this;

		IGoodsStatisticalMeasure IGovernmentAgencyGoodsItem.GoodsStatisticalMeasure
		{
			get
			{
				IGoodsStatisticalMeasure goodsStatisticalMeasure = null;
				var customsSecondQty = InvoiceLine.JI_CustomsSecondQuantity;
				var customsSecondUnitQty = InvoiceLine.JI_CustomsSecondUnitQty;
				if (!customsSecondQty.IsEmpty && !customsSecondUnitQty.IsEmpty)
				{
					goodsStatisticalMeasure = new GoodsStatisticalMeasureWrapper(customsSecondQty, customsSecondUnitQty);
				}
				return goodsStatisticalMeasure;
			}
		}

		ILPCODetail IGovernmentAgencyGoodsItem.MedicalInstrument => new LPCODetailWrapper(InvoiceLine.CertificateNo, InvoiceLine.AuthorizedPerson, InvoiceLine.PartyIdentifier);

		IPreviousDocument IGovernmentAgencyGoodsItem.PreBondedDocument => new PreviousDocumentWrapper(InvoiceLine.PreviousBondedEntryNumber, InvoiceLine.PreviousBondedEntryLineNumber);

		IEnumerable<IShippingIdentification> IGovernmentAgencyGoodsItem.ShippingIdentifications => InvoiceLine.ShippingIdentificationDataCollection.Cast<ShippingIdentificationData>().Select(x => new ShippingIdentificationWrapper(x.TW_ManufacturedLotNo, x.TW_ExpirationDate, x.TW_ProductLotNoAmount, x.TW_ManufacturedDate));

		IGovernmentProcedure IGovernmentAgencyGoodsItem.GovernmentProcedure => null;

		ZDateTime IGovernmentAgencyGoodsItem.ControlInspectionStartDateTime => ZDateTime.Empty;

		ZString IGovernmentAgencyGoodsItem.ExaminationPlace => null;

		IEnumerable<ITransportEquipment> IGovernmentAgencyGoodsItem.TransportEquipments => null;

		IAdditionalDeclaration IGovernmentAgencyGoodsItem.AdditionalDeclaration => null;

		ZString IGovernmentAgencyGoodsItem.CriteriaCode => null;

		ZString IGovernmentAgencyGoodsItem.PreferentialCriteria => null;

		ZString IGovernmentAgencyGoodsItem.ProducerCode => null;

		public ZString OtherCriteria => null;

		#region ICommoditySpecification members
		ZString ICommoditySpecification.CharacteristicQualifierCode => default;

		ZString ICommoditySpecification.ElementDescription => InvoiceLine.JI_Compositions;
		#endregion
		#region ILPCODetail members
		ZString ILPCODetail.LPCOExemptionCode => InvoiceLine.ExemptionCode;

		ZString ILPCODetail.LPCOID => InvoiceLine.TypeApprovalCertificateNo;

		ILPCOAuthorizedParty ILPCODetail.LPCOAuthorizedParty => new LPCOAuthorizedPartyWrapper(InvoiceLine.TypeApprovalAuthorizedParty, InvoiceLine.TypeApprovalPartyIdentifier);
		#endregion
		#region IGoodsMeasure
		ZDecimal IGoodsMeasure.NetWeightMeasure => InvoiceLine.NetWeightInKG;

		ZDecimal IGoodsMeasure.TariffQuantity
		{
			get
			{
				var quantity = InvoiceLine.JI_CustomsThirdQuantity;
				var unitQty = InvoiceLine.JI_CustomsThirdUnitQty;
				if (quantity.IsEmpty || unitQty.IsEmpty)
				{
					quantity = InvoiceLine.JI_InvoiceQuantity;
				}
				return quantity;
			}
		}

		ZString IGoodsMeasure.UnitCode
		{
			get
			{
				var quantity = InvoiceLine.JI_CustomsThirdQuantity;
				var unitQty = InvoiceLine.JI_CustomsThirdUnitQty;
				if (quantity.IsEmpty || unitQty.IsEmpty)
				{
					unitQty = InvoiceLine.JI_InvoiceUQ;
				}
				return unitQty;
			}
		}

		ZString IGoodsMeasure.CustomUnitCode => ZString.Empty;
		#endregion
		#region IOrigin
		ZString IOrigin.CountryCode => GetCountryCodeCore();

		protected virtual ZString GetCountryCodeCore() => InvoiceLine.JI_CountryOfOrigin;

		IAdditionalDocument IOrigin.AdditionalDocument => null;
		#endregion
		#region IGoodsLicensingStatisticalMeasure
		ZDecimal IGoodsLicensingStatisticalMeasure.LicensingQuantity => InvoiceLine.JI_CustomsThirdQuantity;

		ZString IGoodsLicensingStatisticalMeasure.StatisticalUnitCode => InvoiceLine.JI_CustomsThirdUnitQty;

		ZString IGoodsLicensingStatisticalMeasure.ResponsibleGovernmentAgency => Header.TW1_ControllingAgency;
		#endregion
	}
}
