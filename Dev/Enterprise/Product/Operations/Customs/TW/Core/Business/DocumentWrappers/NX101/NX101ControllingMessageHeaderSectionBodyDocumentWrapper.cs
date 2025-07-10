using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public class NX101ControllingMessageHeaderSectionBodyDocumentWrapper : DocumentWrapper
	{
		public NX101ControllingMessageHeaderSectionBodyDocumentWrapper(NX101ControllingMessageHeaderDocumentWrapper parent, NX101GoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem, ZString certificateType, BusinessObjectFactory factory)
			: base(governmentAgencyGoodsItem, factory)
		{
			CertificateType = certificateType;
			this.governmentAgencyGoodsItem = governmentAgencyGoodsItem;
			invoiceLine = governmentAgencyGoodsItem.InvoiceLine;
			this.parent = parent;
			isCertificateType01 = CertificateType == CertificateTypeList.Codes.Code1;
		}

		readonly string quantityFormat = "#,#.##";
		readonly bool isCertificateType01;

		ZString CertificateType { get; }

		readonly IGovernmentAgencyGoodsItem governmentAgencyGoodsItem;
		readonly JobComInvoiceLine invoiceLine;
		readonly NX101ControllingMessageHeaderDocumentWrapper parent;

		public ZString Grouping => grouping ??= invoiceLine.JI_Group;
		string grouping;

		public ZString TariffQuantity => Quantity.ToString(quantityFormat, CultureInfo.InvariantCulture.NumberFormat);

		ICommodity GovernmentAgencyGoodsItemCommodity => governmentAgencyGoodsItemCommodity ??= governmentAgencyGoodsItem.Commodity;
		ICommodity governmentAgencyGoodsItemCommodity;

		ZString PrintingTariffCode => GovernmentAgencyGoodsItemCommodity?.PrintingTariffCode ?? ZString.Empty;

		IEnumerable<IClassification> Classifications => classifications ??= GovernmentAgencyGoodsItemCommodity?.Classifications?.Cast<IClassification>();
		IEnumerable<IClassification> classifications;

		ZString HSTariff => Classifications?.FirstOrDefault(c => c.IdentificationTypeCode == MessageConstants.IdentificationTypeCodes.HS)?.ID ?? ZString.Empty;

		ZString ZZZTariff => Classifications?.FirstOrDefault(c => c.IdentificationTypeCode == MessageConstants.IdentificationTypeCodes.ZZZ)?.ID ?? ZString.Empty;

		public ZString EightDigitsHSTariff => HSTariff.Left(8);

		public ZString EightDigitsZZZTariff => ZZZTariff.Left(8);

		public ZString Tariffformat => tariffformat ??= new TaiwanTariffFormatter().DottedFormatWithPrintLength(HSTariff, PrintingTariffCode);
		string tariffformat;

		public ZString GoodsDescription => goodsDescription ??= GovernmentAgencyGoodsItemCommodity?.Description ?? ZString.Empty;
		string goodsDescription;

		public ZString Brand => brand ??= (isCertificateType01 ? ZString.Empty : GovernmentAgencyGoodsItemCommodity?.Name ?? ZString.Empty);
		string brand;

		public ZString Model => model ??= (isCertificateType01 ? ZString.Empty : GovernmentAgencyGoodsItemCommodity?.CommercialCategorizationID ?? ZString.Empty);
		string model;

		public ZString Specification => specification ??= (isCertificateType01 ? ZString.Empty : GovernmentAgencyGoodsItemCommodity?.CommodityRelatedPackaging?.Specification ?? ZString.Empty);
		string specification;

		public ZString ShippingMarks => shippingMarks ??= governmentAgencyGoodsItem.Packaging?.MarksNumbers ?? ZString.Empty;
		string shippingMarks;

		public ZString DescriptionOfGoods
		{
			get
			{
				var descriptionOfGoods = new ZStringBuilder();
				descriptionOfGoods.AppendIfNotEmpty(GoodsDescription);
				descriptionOfGoods.AppendIfNotEmpty(Specification);
				return descriptionOfGoods.ToStringWithDelimiterBetweenAppends("\r\n");
			}
		}

		IGoodsMeasure governmentAgencyGoodsItemGoodsMeasure => governmentAgencyGoodsItem.GoodsMeasure;

		internal decimal Quantity => governmentAgencyGoodsItemGoodsMeasure?.TariffQuantity ?? ZDecimal.Zero;

		public ZString Unit
		{
			get
			{
				var result = ZString.Empty;
				if (governmentAgencyGoodsItemGoodsMeasure != null)
				{
					var customUnitCode = governmentAgencyGoodsItemGoodsMeasure.CustomUnitCode;
					switch (CertificateType)
					{
						case CertificateTypeList.Codes.Code15:
							var permitUQ = invoiceLine.JI_PermitUQ;
							if (!permitUQ.IsEmpty)
							{
								result = PermitChinaDescriptionUnit;
							}
							break;
						case CertificateTypeList.Codes.Code18:
							result = customUnitCode.IsEmpty ? governmentAgencyGoodsItemGoodsMeasure.UnitCode : customUnitCode;
							break;
						default:
							result = customUnitCode;
							break;
					}
				}
				return result;
			}
		}

		ZString PermitChinaDescriptionUnit => Factory.GetValue(ref permitCnDescription, () =>
		{
			var result = ZString.Empty;
			var permitUQ = invoiceLine.JI_PermitUQ;
			if (!permitUQ.IsEmpty)
			{
				result = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, invoiceLine.JI_PermitUQ, Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today)?.GetValue(RefCusCodeListSchema.ZZD_Description) ?? ZString.Empty;
			}
			return result;
		});
		CachedProperty<ZString> permitCnDescription;

		public ZString QuantityAndUnit => $"{TariffQuantity} {Unit}";

		public ZString CriteriaCode => governmentAgencyGoodsItem.CriteriaCode;

		public ZString PreferentialCriteria => governmentAgencyGoodsItem.PreferentialCriteria;

		public ZString OtherCriteria => governmentAgencyGoodsItem.OtherCriteria;

		public ZString ProducerCode => governmentAgencyGoodsItem.ProducerCode;

		IInvoiceLine GovernmentAgencyGoodsItemCommodityInvoiceLine => governmentAgencyGoodsItemCommodityInvoiceLine ??= GovernmentAgencyGoodsItemCommodity?.InvoiceLine;
		IInvoiceLine governmentAgencyGoodsItemCommodityInvoiceLine;

		public ZString InvoiceCurrency => GovernmentAgencyGoodsItemCommodityInvoiceLine?.CurrencyTypeCode ?? ZString.Empty;

		internal ZDecimal InvoicePriceInternal => GovernmentAgencyGoodsItemCommodityInvoiceLine?.ItemChargeAmount ?? ZDecimal.Zero;

		public ZString InvoicePrice => InvoicePriceInternal.ToString(parent.InvoicePriceDecimalPlaces);

		IInvoice GovernmentAgencyGoodsItemCommodityInvoice => governmentAgencyGoodsItemCommodityInvoice ??= GovernmentAgencyGoodsItemCommodity?.Invoice;
		IInvoice governmentAgencyGoodsItemCommodityInvoice;

		public ZString InvoiceID => GovernmentAgencyGoodsItemCommodityInvoice?.ID ?? ZString.Empty;

		public ZDate InvoiceDate => GovernmentAgencyGoodsItemCommodityInvoice?.IssueDateTime ?? ZDate.Empty;

		#region MarksNumbers

		public ZString AlignGroupingMarksNumbers { get; private set; }

		public ZString AlignTariffformatMarksNumbers { get; private set; }

		public ZString AlignGoodsDescriptionMarksNumbers { get; private set; }

		public ZString AlignBrandMarksNumbers { get; private set; }

		public ZString AlignModelMarksNumbers { get; private set; }

		public ZString AlignSpecificationMarksNumbers { get; private set; }

		public ZString AlignShippingMarksMarksNumbers { get; private set; }

		internal void AssignMarksNumbers(IEnumerator<ZString> marksNumbersTotalRows, ref bool hasNext)
		{
			AlignGroupingMarksNumbers = marksNumbersTotalRows.GetMarksNumbersByAlignedFields(Grouping, ref hasNext);
			if (hasNext)
			{
				AlignTariffformatMarksNumbers = marksNumbersTotalRows.GetMarksNumbersByAlignedFields(Tariffformat, ref hasNext);
			}
			if (hasNext)
			{
				AlignGoodsDescriptionMarksNumbers = marksNumbersTotalRows.GetMarksNumbersByAlignedFields(GoodsDescription, ref hasNext);
			}
			if (hasNext)
			{
				AlignBrandMarksNumbers = marksNumbersTotalRows.GetMarksNumbersByAlignedFields(Brand, ref hasNext);
			}
			if (hasNext)
			{
				AlignModelMarksNumbers = marksNumbersTotalRows.GetMarksNumbersByAlignedFields(Model, ref hasNext);
			}
			if (hasNext)
			{
				AlignSpecificationMarksNumbers = marksNumbersTotalRows.GetMarksNumbersByAlignedFields(Specification, ref hasNext);
			}
			if (hasNext)
			{
				AlignShippingMarksMarksNumbers = marksNumbersTotalRows.GetMarksNumbersByAlignedFields(ShippingMarks, ref hasNext);
			}
		}

		#endregion
	}
}
