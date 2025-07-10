using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class InvoiceTermWithShortDescription : CodeDescriptionPair
	{
		InvoiceTermWithShortDescription(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public static InvoiceTermWithShortDescription CashOnDelivery { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.CashOnDelivery, ResString.GetMultilingualString("1f9e9241-7c8c-4a47-a60f-c8f540448f68", "Cash on Delivery")); } }
		public static InvoiceTermWithShortDescription PaymentInAdvance { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.PaymentInAdvance, ResString.GetMultilingualString("35f827e0-2858-40c3-9442-3c964d236120", "Payment in Advance")); } }
		public static InvoiceTermWithShortDescription FromCustomsClearanceDate { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.FromCustomsClearanceDate, ResString.GetMultilingualString("c4233539-e81a-47f5-80a4-cfbfc4255d1e", "{0} days from Customs Clear.", "{0}")); } }
		public static InvoiceTermWithShortDescription FromInvoiceDate { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.FromInvoiceDate, ResString.GetMultilingualString("e85745b3-b439-4f5a-8e58-64d9a7fb9851", "{0} days from Inv. Date", "{0}")); } }
		public static InvoiceTermWithShortDescription FromMonthEnd { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.FromMonthEnd, ResString.GetMultilingualString("9855909f-ab89-4b1a-a580-616344373d51", "{0} days from EOM", "{0}")); } }
		public static InvoiceTermWithShortDescription FromWeekEnd { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.FromWeekEnd, ResString.GetMultilingualString("BFE44509-B8DE-491B-B340-B6597ED38C73", "{0} days from EWK", "{0}")); } }
		public static InvoiceTermWithShortDescription FromPeriodEnd { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.FromPeriodEnd, ResString.GetMultilingualString("c90acf31-83c2-4455-a835-7ec36504c437", "{0} days from EOP", "{0}")); } }
		public static InvoiceTermWithShortDescription FromShipmentDate { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.FromShipmentDate, ResString.GetMultilingualString("2816c918-b587-40b8-8128-2a9ef1781504", "{0} days from shipment", "{0}")); } }
		public static InvoiceTermWithShortDescription MonthsFromInvoiceCycleDate { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.MonthsFromInvoiceCycleDate, ResString.GetMultilingualString("1200C55A-700A-4E18-B92A-8F8F8D9D801C", "{0} months from inv. cycle", "{0}")); } }
		public static InvoiceTermWithShortDescription TermDaysAndDebtorPaymentCycle { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle, ResString.GetMultilingualString("C80CD33F-09F5-4A3C-B631-F39ACCC0BC49", "{0} days and payment cycle", "{0}")); } }
		public static InvoiceTermWithShortDescription LaterOfShipmentOrInvoiceDate { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate, ResString.GetMultilingualString("9d6f63ee-abc4-4e2b-ae31-e8b59f88ad3a", "{0} days from later shipment/invoice date", "{0}")); } }
		public static InvoiceTermWithShortDescription MultipleInstallments { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.MultipleInstallments, ResString.GetMultilingualString("20E586BC-762D-4609-BF37-1052E5E27037", "Multiple Installments")); } }
		public static InvoiceTermWithShortDescription FromDeliveryOrPickupDate { get { return new InvoiceTermWithShortDescription(Constants.InvoiceTerms.FromDeliveryOrPickupDate, ResString.GetMultilingualString("05614f80-d3ff-3708-a53e-ce5c2e13825c", "{0} days from delivery or pickup date", "{0}")); } }
	}
}
