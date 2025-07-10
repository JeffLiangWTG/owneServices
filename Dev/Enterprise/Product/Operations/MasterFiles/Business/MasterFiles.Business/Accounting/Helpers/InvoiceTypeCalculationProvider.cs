using System;
using System.Linq;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class InvoiceTypeCalculationProvider
	{
		public static string[] DisbursementInvoiceTypes
		{
			get { return AccTransactionHeader.DisbursementInvoiceTypes; }
		}

		public static bool IsDisbursementInvoiceType(string invoiceType)
		{
			return AccTransactionHeader.IsDisbursementInvoiceType(invoiceType);
		}

		public static string[] DeferredInvoiceTypes
		{
			get
			{
				return new string[]
				{
					InvoiceTypesList.Codes.DestinationChargesInvoice_Batching,
					InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching,
					InvoiceTypesList.Codes.DisbursementInvoice_Batching,
					InvoiceTypesList.Codes.FinalInvoice_Batching,
					InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching,
					InvoiceTypesList.Codes.FreightInvoice_Batching,
					InvoiceTypesList.Codes.InvoicePerTaxCode_Batching,
					InvoiceTypesList.Codes.SelfBillingInvoice_Batching,
					AgencyInvoiceTypesList.Codes.ForeignCollect_Batching,
					AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching,
					AgencyInvoiceTypesList.Codes.LocalCollect_Batching,
					AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching,
					AgencyInvoiceTypesList.Codes.Misc_Batching
				};
			}
		}

		public static bool IsDeferredInvoiceType(string invoiceType)
		{
			return Array.IndexOf(DeferredInvoiceTypes, invoiceType) > -1;
		}

		public static string[] NonDeferredInvoiceTypes
		{
			get
			{
				return new string[]
				{
					InvoiceTypesList.Codes.DestinationChargesInvoice,
					InvoiceTypesList.Codes.DisbursementInForeignCurrency,
					InvoiceTypesList.Codes.DisbursementInvoice,
					InvoiceTypesList.Codes.FinalInvoice,
					InvoiceTypesList.Codes.ForeignCurrencyInvoice,
					InvoiceTypesList.Codes.FreightInvoice,
					InvoiceTypesList.Codes.InvoicePerTaxCode,
					InvoiceTypesList.Codes.SelfBillingInvoice,
					AgencyInvoiceTypesList.Codes.ForeignCollect,
					AgencyInvoiceTypesList.Codes.ForeignPrePaid,
					AgencyInvoiceTypesList.Codes.LocalCollect,
					AgencyInvoiceTypesList.Codes.LocalPrePaid,
					AgencyInvoiceTypesList.Codes.Misc
				};
			}
		}

		public static bool IsNonDeferredInvoiceType(string invoiceType)
		{
			return Array.IndexOf(NonDeferredInvoiceTypes, invoiceType) > -1;
		}

		public static string[] DisbursementNonDeferredInvoiceTypes
		{
			get { return (from string invType in DisbursementInvoiceTypes where IsNonDeferredInvoiceType(invType) select invType).ToArray(); }
		}

		public static string ConvertNonDeferredInvoiceTypeToDeferredOne(string invoiceType)
		{
			int invoiceTypeIndex = Array.IndexOf(NonDeferredInvoiceTypes, invoiceType);
			return invoiceTypeIndex > -1 ? DeferredInvoiceTypes[invoiceTypeIndex] : invoiceType;
		}

		public static string ConvertDeferredInvoiceTypeToNonDeferredOne(string invoiceType)
		{
			int invoiceTypeIndex = Array.IndexOf(DeferredInvoiceTypes, invoiceType);
			return invoiceTypeIndex > -1 ? NonDeferredInvoiceTypes[invoiceTypeIndex] : invoiceType;
		}

		public static bool BillInLocalCurrency(string invoiceType)
		{
			bool billInLocal = true;

			string adaptedInvoiceType = ConvertDeferredInvoiceTypeToNonDeferredOne(invoiceType);

			if (adaptedInvoiceType == InvoiceTypesList.Codes.FreightInvoice ||
				adaptedInvoiceType == InvoiceTypesList.Codes.ForeignCurrencyInvoice ||
				adaptedInvoiceType == InvoiceTypesList.Codes.DisbursementInForeignCurrency ||
				adaptedInvoiceType == AgencyInvoiceTypesList.Codes.ForeignCollect ||
				adaptedInvoiceType == AgencyInvoiceTypesList.Codes.ForeignPrePaid ||
				adaptedInvoiceType == InvoiceTypesList.Codes.SelfBillingInvoice)
			{
				billInLocal = false;
			}

			return billInLocal;
		}
	}
}
