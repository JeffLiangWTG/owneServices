using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ZA.Business
{
	public partial class JobComInvoiceLine
	{
		protected override Type InvoiceHeaderType
		{
			get { return typeof(JobComInvoiceHeader); }
		}

		public new CusEntryLine CusEntryLine
		{
			get { return base.CusEntryLine as CusEntryLine; }
		}

		public new JobDeclaration Declaration
		{
			get { return base.Declaration as JobDeclaration; }
		}

		public new CusEntryInstruction EntryInstruction
		{
			get { return base.EntryInstruction as CusEntryInstruction; }
		}

		public new JobComInvoiceHeader InvoiceHeader
		{
			get { return base.InvoiceHeader as JobComInvoiceHeader; }
		}

		public new JobComInvoiceLineLookups Lookups
		{
			get { return base.Lookups as JobComInvoiceLineLookups; }
		}

		public new JobComInvoiceLineValidation Validation
		{
			get { return (JobComInvoiceLineValidation)base.Validation; }
		}

		public new JobComInvChargeCollection<InvoiceLineCharge> Charges
		{
			get { return (JobComInvChargeCollection<InvoiceLineCharge>)base.Charges; }
		}

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection()
		{
			return new JobComInvChargeCollection<InvoiceLineCharge>(this);
		}
	}
}
