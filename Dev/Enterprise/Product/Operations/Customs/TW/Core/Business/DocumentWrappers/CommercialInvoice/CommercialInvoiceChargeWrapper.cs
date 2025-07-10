using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CommercialInvoiceChargeWrapper : DocumentWrapper
	{
		readonly InvoiceCharge invoiceChargeBO;

		CommercialInvoiceChargeWrapper(InvoiceCharge invoiceCharge, BusinessObjectFactory factory)
			: base(invoiceCharge, factory)
		{
			invoiceChargeBO = invoiceCharge ?? factory.GetNull<InvoiceCharge>();
		}

		public static CommercialInvoiceChargeWrapper New(InvoiceCharge invoiceCharge, BusinessObjectFactory factoryToWrap)
		{
			return new CommercialInvoiceChargeWrapper(invoiceCharge, factoryToWrap);
		}

		public ZString ChargeType => invoiceChargeBO.J7_ChargeType;

		public ZString ChargeDescription
		{
			get
			{
				var result = invoiceChargeBO.J7_ChargeDescription;
				if (result.IsEmpty)
				{
					result = NewCustomsChargeTypeList.GetMultilingualDescriptionFromCode(ChargeType)?.ToString(Core.SharedConstants.Languages.English) ?? ZString.Empty;
				}
				return result;
			}
		}

		CustomsChargeTypeList NewCustomsChargeTypeList => Factory.GetCachedValue<CustomsChargeTypeList>();

		public ZDecimal Amount => invoiceChargeBO.J7_Amount;

		public ZString Currency => invoiceChargeBO.J7_RX_NKCurrency;

		public MoneyWrapper Price
		{
			get
			{
				Money chargeMoney;
				var invoiceCurrency = invoiceChargeBO.Invoice?.Invoice_Currency;
				if (invoiceCurrency != null && invoiceChargeBO.J7_RX_NKCurrency != invoiceCurrency.Code)
				{
					chargeMoney = invoiceChargeBO.CurrencyConverter.ConvertExact(invoiceChargeBO.MoneyInLocalCurrency, invoiceCurrency);
				}
				else
				{
					chargeMoney = invoiceChargeBO.Money;
				}
				return new MoneyWrapper(chargeMoney, Factory);
			}
		}
	}
}
