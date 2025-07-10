using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.TW.Business
{
	[CodeAlive("The new method is used to create wrapper")]
	public class CommercialInvoiceWrapper : DocumentWrapper
	{
		protected CommercialInvoiceWrapper(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory)
			: base(invoiceHeader, factory)
		{
			invoiceHeaderBO = invoiceHeader ?? factory.GetNull<JobComInvoiceHeader>();
		}

		public static CommercialInvoiceWrapper New(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factoryToWrap)
		{
			return new CommercialInvoiceWrapper(invoiceHeader, factoryToWrap);
		}

		protected JobComInvoiceHeader invoiceHeaderBO;

		public ZDecimal DecimalPlaces => 2;

		public ZString InvoiceNumber => invoiceHeaderBO.JZ_InvoiceNumber;

		public ZString InvoiceDate => invoiceHeaderBO.JZ_InvoiceDate.Date.ToISO8601ShortDateString();

		public ZString InvoiceCurrency => invoiceHeaderBO.JZ_RX_NKInvoice_Currency;

		public ZString IncoTermCode => invoiceHeaderBO.JZ_IncoTerm.ToUpper();

		public ZString IncoTermPlace => invoiceHeaderBO.JZ_IncoTermPlace.ToUpper();

		public ZString MarksAndNumbers => invoiceHeaderBO.TW_MarksAndNumbers;

		public ZString Description => invoiceHeaderBO.JZ_Description;

		public ZString Remarks => invoiceHeaderBO.JZ_Remarks;

		public ZInt UnitPriceDecimalPlaces => Factory.GetValue(ref unitPriceDecimalPlacesCached, () =>
		{
			ZInt maxDecimalPlaces = 0;
			var invoiceLines = invoiceHeaderBO.JobComInvoiceLines;
			if (invoiceLines.Count > 0)
			{
				maxDecimalPlaces = invoiceLines.Cast<JobComInvoiceLine>().Select(x => (ZInt)BitConverter.GetBytes(decimal.GetBits(x.AddInfoChild.TWL_DocumentaryUnitPrice.Normalize())[3])[2]).Max();
			}
			return maxDecimalPlaces;
		});
		CachedProperty<ZInt> unitPriceDecimalPlacesCached;

		public CommercialInvoiceLineWrapperCollection InvoiceLines
		{
			get
			{
				if (invoiceLines == null)
				{
					invoiceLines = new CommercialInvoiceLineWrapperCollection(Factory);
					invoiceLines.AddLinesFrom(invoiceHeaderBO.JobComInvoiceLines.Cast<JobComInvoiceLine>().OrderBy(c => c.JI_LineNo));
				}
				return invoiceLines;
			}
		}
		CommercialInvoiceLineWrapperCollection invoiceLines;

		public CommercialInvoiceChargeWrapperCollection AdditionCharges
		{
			get
			{
				if (additionCharges == null)
				{
					additionCharges = new CommercialInvoiceChargeWrapperCollection(Factory);
					additionCharges.AddChargesFrom(invoiceHeaderBO.Charges.AdditionCharges().OrderBy(c => c.J7_ChargeType));
				}
				return additionCharges;
			}
		}
		CommercialInvoiceChargeWrapperCollection additionCharges;

		public CommercialInvoiceChargeWrapperCollection DeductionCharges
		{
			get
			{
				if (deductionCharges == null)
				{
					deductionCharges = new CommercialInvoiceChargeWrapperCollection(Factory);
					deductionCharges.AddChargesFrom(invoiceHeaderBO.Charges.DeductionCharges());
				}
				return deductionCharges;
			}
		}
		CommercialInvoiceChargeWrapperCollection deductionCharges;

		public CommercialInvoiceChargeWrapperCollection AdditionAndDeductionCharges
		{
			get
			{
				if (additionAndDeductionCharges == null)
				{
					additionAndDeductionCharges = new CommercialInvoiceChargeWrapperCollection(Factory);
					additionAndDeductionCharges.AddRange(AdditionCharges);
					additionAndDeductionCharges.AddRange(DeductionCharges);
				}
				return additionAndDeductionCharges;
			}
		}
		CommercialInvoiceChargeWrapperCollection additionAndDeductionCharges;

		#region Total Amount
		public ZDecimal InvoiceLineTotalAmount => Factory.GetValue(ref invoiceLineTotalAmountCached, () => InvoiceLines.Cast<CommercialInvoiceLineWrapper>().Sum(c => c.LinePrice));
		CachedProperty<ZDecimal> invoiceLineTotalAmountCached;
		public ZDecimal AdditionChargesTotalAmount => Factory.GetValue(ref additionChargesTotalAmountCached, () =>
		{
			var result = Money.Empty;
			foreach (var charge in invoiceHeaderBO.Charges.AdditionCharges())
			{
				var chargeMoney = IsCurrencySameAsInvoiceCurrency(charge.J7_RX_NKCurrency) ? charge.Money : charge.CurrencyConverter.ConvertExact(charge.MoneyInLocalCurrency, invoiceHeaderBO.Invoice_Currency);
				result = invoiceHeaderBO.CurrencyConverter.Add(result, chargeMoney);
			}
			return result.Amount;
		});
		CachedProperty<ZDecimal> additionChargesTotalAmountCached;
		public ZDecimal DeductionChargesTotalAmount => Factory.GetValue(ref deductionChargesTotalAmountCached, () =>
		{
			var result = Money.Empty;
			foreach (var charge in invoiceHeaderBO.Charges.DeductionCharges())
			{
				var chargeMoney = IsCurrencySameAsInvoiceCurrency(charge.J7_RX_NKCurrency) ? charge.Money : charge.CurrencyConverter.ConvertExact(charge.MoneyInLocalCurrency, invoiceHeaderBO.Invoice_Currency);
				result = invoiceHeaderBO.CurrencyConverter.Add(result, chargeMoney);
			}
			return result.Amount;
		});
		CachedProperty<ZDecimal> deductionChargesTotalAmountCached;

		public ZDecimal SayTotalAmount => InvoiceLineTotalAmount + AdditionChargesTotalAmount - DeductionChargesTotalAmount;

		public ZString SayTotalDescription => DocumentWrapperHelper.GetSayTotalDescription(invoiceHeaderBO.Invoice_Currency, SayTotalAmount);
		#endregion

		ZBool IsCurrencySameAsInvoiceCurrency(ZString currencyCode)
		{
			return !currencyCode.IsEmpty && InvoiceCurrency == currencyCode;
		}

		#region Invoice Total Quantity String

		List<ZString> InvoiceLinesGroupByQuantityStringList
		{
			get
			{
				if (invoiceTotalQuantityString == null)
				{
					invoiceTotalQuantityString = new List<ZString>();
					var invoiceGroupList = InvoiceLines.Cast<CommercialInvoiceLineWrapper>().
						GroupBy(c => c.LineQuantity.Unit.Code).
						Select(c => new { Quantity = c.Sum(a => a.LineQuantity.Value), Code = c.Key }).
						OrderBy(c => c.Code);

					var decimalPlaces = invoiceHeaderBO?.GetTWInvoiceLineMaxDecimalPlaces(JobTWComInvoiceLineSchema.TWL_DocumentaryQty.Name) ?? 2;
					foreach (var invoiceGroup in invoiceGroupList)
					{
						invoiceTotalQuantityString.Add(Utilities.FormatNumberWithGroupSeparators(invoiceGroup.Quantity, decimalPlaces, CultureInfo.InvariantCulture) + " " + invoiceGroup.Code);
					}
				}
				return invoiceTotalQuantityString;
			}
		}
		List<ZString> invoiceTotalQuantityString;

		public ZDecimal InvoiceLinesGroupByQuantityStringListCount => InvoiceLinesGroupByQuantityStringList.Count;

		public ZString InvoiceLinesGroupByQuantity
		{
			get
			{
				if (invoiceLinesGroupByQuantity.IsEmpty)
				{
					var result = new ZStringBuilder();
					InvoiceLinesGroupByQuantityStringList.ForEach(x => result.AppendLine(x));
					invoiceLinesGroupByQuantity = result.ToString().Trim();
				}
				return invoiceLinesGroupByQuantity;
			}
		}
		ZString invoiceLinesGroupByQuantity;
		#endregion

		public DeclarationWrapper Declaration => declaration ??= DeclarationWrapper.New(invoiceHeaderBO.JobDeclaration, Factory);
		DeclarationWrapper declaration;

		TWJobDocAddress SupplierDocumentaryAddress
		{
			get
			{
				if (supplierDocumentaryAddress == null)
				{
					supplierDocumentaryAddress = invoiceHeaderBO.SupplierDocumentaryAddress;
					if ((supplierDocumentaryAddress == null || supplierDocumentaryAddress.IsEmpty) && invoiceHeaderBO.JobDeclaration?.SupplierDocumentaryAddress is TWJobDocAddress decDocAddress && !decDocAddress.IsEmpty)
					{
						supplierDocumentaryAddress = decDocAddress;
					}
				}
				return supplierDocumentaryAddress;
			}
		}
		TWJobDocAddress supplierDocumentaryAddress;

		TWJobDocAddress BuyerDocumentaryAddress
		{
			get
			{
				if (buyerDocumentaryAddress == null)
				{
					buyerDocumentaryAddress = invoiceHeaderBO.BuyerDocumentaryAddress;
					if ((buyerDocumentaryAddress == null || buyerDocumentaryAddress.IsEmpty) && invoiceHeaderBO.JobDeclaration?.ImporterDocumentaryAddress is TWJobDocAddress decDocAddress && !decDocAddress.IsEmpty)
					{
						buyerDocumentaryAddress = decDocAddress;
					}
				}
				return buyerDocumentaryAddress;
			}
		}
		TWJobDocAddress buyerDocumentaryAddress;

		public DocumentaryAddressDetailsWrapper SellerAddressData
		{
			get
			{
				if (sellerAddressData == null)
				{
					var supplierDocumentaryAddress = SupplierDocumentaryAddress;
					if (supplierDocumentaryAddress?.Address != null)
					{
						sellerAddressData = new ExportExporterDocumentaryAddressDetailsWrapper(supplierDocumentaryAddress.Organisation, supplierDocumentaryAddress);
					}
				}
				return sellerAddressData;
			}
		}

		DocumentaryAddressDetailsWrapper sellerAddressData;

		public DocumentaryAddressDetailsWrapper BuyerAddressData
		{
			get
			{
				if (buyerAddressData == null)
				{
					var buyerDocumentaryAddress = BuyerDocumentaryAddress;
					if (buyerDocumentaryAddress?.Address != null)
					{
						buyerAddressData = new ExportBuyerDocumentaryAddressDetailsWrapper(buyerDocumentaryAddress.Organisation, buyerDocumentaryAddress);
					}
				}
				return buyerAddressData;
			}
		}

		DocumentaryAddressDetailsWrapper buyerAddressData;

		public DocJobDocAddress SellerJobDocAddress => sellerJobDocAddress ??= DocJobDocAddress.New(SupplierDocumentaryAddress, Factory);
		DocJobDocAddress sellerJobDocAddress;

		public DocJobDocAddress BuyerJobDocAddress => buyerJobDocAddress ??= DocJobDocAddress.New(BuyerDocumentaryAddress, Factory);
		DocJobDocAddress buyerJobDocAddress;

		public ZString Transportation => Factory.GetValue(ref transportationCached, GetTransportationCore);
		CachedProperty<ZString> transportationCached;

		protected virtual ZString GetTransportationCore()
		{
			return Declaration.Transportation;
		}

		public ZString PortOfOriginName => Factory.GetValue(ref portOfOriginNameCached, GetPortOfOriginNameCore);
		CachedProperty<ZString> portOfOriginNameCached;

		protected virtual ZString GetPortOfOriginNameCore()
		{
			return Declaration.PortOfOriginName;
		}

		public virtual ZString FinalDestinationName => Factory.GetValue(ref finalDestinationNameCached, GetFinalDestinationNameCore);
		CachedProperty<ZString> finalDestinationNameCached;

		protected virtual ZString GetFinalDestinationNameCore()
		{
			return Declaration.FinalDestinationName;
		}
	}
}
