using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class InvoiceTermsListWithShortDescription : CodeDescriptionPairList
	{
		public InvoiceTermsListWithShortDescription()
			: base()
		{
			Add(InvoiceTermWithShortDescription.CashOnDelivery);
			Add(InvoiceTermWithShortDescription.FromCustomsClearanceDate);
			Add(InvoiceTermWithShortDescription.FromInvoiceDate);
			Add(InvoiceTermWithShortDescription.FromMonthEnd);
			Add(InvoiceTermWithShortDescription.FromWeekEnd);
			Add(InvoiceTermWithShortDescription.FromPeriodEnd);
			Add(InvoiceTermWithShortDescription.FromShipmentDate);
			Add(InvoiceTermWithShortDescription.PaymentInAdvance);
			Add(InvoiceTermWithShortDescription.MonthsFromInvoiceCycleDate);
			Add(InvoiceTermWithShortDescription.TermDaysAndDebtorPaymentCycle);
			Add(InvoiceTermWithShortDescription.LaterOfShipmentOrInvoiceDate);
			Add(InvoiceTermWithShortDescription.MultipleInstallments);
			Add(InvoiceTermWithShortDescription.FromDeliveryOrPickupDate);
		}
	}
}
