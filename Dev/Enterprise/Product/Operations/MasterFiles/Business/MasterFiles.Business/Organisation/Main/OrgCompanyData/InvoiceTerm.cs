using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	public struct InvoiceTerm
	{
		public InvoiceTerm(OrgARTerms arTerm)
			: this(arTerm.PY_InvoiceTerm, arTerm.Lookups.InvoiceTermListWithoutDefaultValue.GetDescriptionFromCode(arTerm.PY_InvoiceTerm), arTerm.PY_InvoiceDays)
		{
			invoiceClass_cachedValue = arTerm.PY_InvoiceClass;
			if (arTerm.PY_InvoiceTerm == Constants.InvoiceTerms.MonthsFromInvoiceCycleDate ||
				arTerm.PY_InvoiceTerm == Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle)
			{
				arTerm_cachedValue = arTerm;
			}
		}

		public InvoiceTerm(ZString term, ZString termDescription, ZByte days)
		{
			term_cachedValue = term;
			termDescription_cachedValue = termDescription;
			days_cachedValue = days;
			invoiceClass_cachedValue = ZString.Empty;
			arTerm_cachedValue = null;
		}

		readonly ZString invoiceClass_cachedValue;
		readonly ZString term_cachedValue;
		readonly ZString termDescription_cachedValue;
		readonly ZByte days_cachedValue;
		readonly OrgARTerms arTerm_cachedValue;

		public ZString InvoiceClass
		{
			get { return invoiceClass_cachedValue; }
		}

		public ZString Term
		{
			get { return term_cachedValue; }
		}

		public ZString TermDescription
		{
			get { return termDescription_cachedValue; }
		}

		public ZByte Days
		{
			get { return days_cachedValue; }
		}

		public ZDateTime GetARTermsCycleDueDate(ZDateTime invoiceDate, int termMonths)
		{
			ZDateTime result = ZDateTime.Empty;
			if (arTerm_cachedValue != null && !arTerm_cachedValue.IsDeleted)
			{
				result = arTerm_cachedValue.GetARTermsCycleDueDate(invoiceDate, termMonths);
			}

			return result;
		}

		public ZDateTime GetARPaymentCycleDueDate(ZDateTime invoiceDate, int termDays)
		{
			ZDateTime result = ZDateTime.Empty;
			if (arTerm_cachedValue != null && !arTerm_cachedValue.IsDeleted)
			{
				result = arTerm_cachedValue.GetARPaymentCycleDueDate(invoiceDate, termDays);
			}

			return result;
		}

		public bool IsTermWithoutDays
		{
			get { return GetIsTermWithoutDays(Term); }
		}

		public bool IsTermWithMonths
		{
			get { return GetIsTermWitMonths(Term); }
		}

		public bool IsEmpty
		{
			get { return Term.IsEmpty; }
		}

		public override string ToString()
		{
			return Term.IsEmpty ? Res.GetString("18192f26-db2a-4bdc-8116-4f0334f21ccc", "no value") :
				(IsTermWithoutDays ? Res.GetString("33e35c13-a88f-4609-ae5f-d81fd04e9d4a", "{0}", Term) :
				string.Format("{0}, {1} {2}", Term, Days, Days == 1 ?
					(IsTermWithMonths ? Res.GetString("CE8262A8-7701-46DC-B017-586284ABCB9D", "month") : Res.GetString("5ac353e1-0125-477c-bf9d-99db2f947af2", "day")) :
					(IsTermWithMonths ? Res.GetString("42139D8B-9B08-46DF-A29F-300379F4C42E", "months") : Res.GetString("7276ef62-8ddd-400b-910c-287b55ff6ba4", "days"))));
		}

		public static bool GetIsTermWithoutDays(ZString term)
		{
			return term == Constants.InvoiceTerms.CashOnDelivery || term == Constants.InvoiceTerms.PaymentInAdvance;
		}

		public static bool GetIsTermWitMonths(ZString term)
		{
			return term == Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
		}
	}
}
