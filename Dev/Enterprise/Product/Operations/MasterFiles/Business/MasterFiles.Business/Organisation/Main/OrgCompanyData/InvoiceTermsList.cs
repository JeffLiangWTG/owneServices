using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public abstract class InvoiceTermsList : CodeDescriptionPairList
	{
		public InvoiceTermsList()
			: base()
		{
			Add(CashOnDelivery);
			Add(FromCustomsClearanceDate);
			Add(FromInvoiceDate);
			Add(FromMonthEnd);
			Add(FromPeriodEnd);
			Add(FromShipmentDate);
			Add(PaymentInAdvance);
		}

		public static CodeDescriptionPair CashOnDelivery { get { return new CodeDescriptionPair(Constants.InvoiceTerms.CashOnDelivery, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|CashOnDelivery", "Cash On Delivery")); } }
		public static CodeDescriptionPair PaymentInAdvance { get { return new CodeDescriptionPair(Constants.InvoiceTerms.PaymentInAdvance, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|PaymentInAdvance", "Payment in Advance")); } }
		public static CodeDescriptionPair FromCustomsClearanceDate { get { return new CodeDescriptionPair(Constants.InvoiceTerms.FromCustomsClearanceDate, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|FromCustomsClearanceDate", "From Customs Clearance Date")); } }
		public static CodeDescriptionPair FromInvoiceDate { get { return new CodeDescriptionPair(Constants.InvoiceTerms.FromInvoiceDate, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|FromInvoiceDate", "From Date of Invoice")); } }
		public static CodeDescriptionPair FromMonthEnd { get { return new CodeDescriptionPair(Constants.InvoiceTerms.FromMonthEnd, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|FromMonthEnd", "From End of Month")); } }
		public static CodeDescriptionPair FromWeekEnd { get { return new CodeDescriptionPair(Constants.InvoiceTerms.FromWeekEnd, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|FromWeekEnd", "From End of Week")); } }
		public static CodeDescriptionPair FromPeriodEnd { get { return new CodeDescriptionPair(Constants.InvoiceTerms.FromPeriodEnd, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|FromPeriodEnd", "From End of Period")); } }
		public static CodeDescriptionPair FromShipmentDate { get { return new CodeDescriptionPair(Constants.InvoiceTerms.FromShipmentDate, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|FromShipmentDate", "From Date of Shipment")); } }
		public static CodeDescriptionPair LaterOfShipmentOrInvoiceDate { get { return new CodeDescriptionPair(Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|LaterOfShipmentOrInvoiceDate", "Later of Shipment or Invoice Date")); } }
		public static CodeDescriptionPair FromDeliveryOrPickupDate { get { return new CodeDescriptionPair(Constants.InvoiceTerms.FromDeliveryOrPickupDate, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|FromDeliveryOrPickupDate", "From Delivery or Pickup Date")); } }
		public static CodeDescriptionPair MonthsFromInvoiceCycleDate { get { return new CodeDescriptionPair(Constants.InvoiceTerms.MonthsFromInvoiceCycleDate, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|MonthsFromInvoiceCycleDate", "Months From Invoice Cycle Date")); } }
		public static CodeDescriptionPair TermDaysAndDebtorPaymentCycle { get { return new CodeDescriptionPair(Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle, ResString.GetMultilingualString("MasterFiles|InvoiceTerms|TermDaysAndDebtorPaymentCycle", "Debtor Payment Cycle")); } }
	}

	public class APInvoiceTermsList : InvoiceTermsList
	{
		public APInvoiceTermsList()
			: base()
		{
		}
	}

	public class ARInvoiceTermsList : InvoiceTermsList
	{
		public ARInvoiceTermsList()
			: base()
		{
			Insert(IndexOf(PaymentInAdvance), FromDeliveryOrPickupDate);
			Insert(IndexOf(FromDeliveryOrPickupDate), LaterOfShipmentOrInvoiceDate);
			Add(MonthsFromInvoiceCycleDate);
			Add(TermDaysAndDebtorPaymentCycle);
			Add(FromWeekEnd);
		}
	}
}
