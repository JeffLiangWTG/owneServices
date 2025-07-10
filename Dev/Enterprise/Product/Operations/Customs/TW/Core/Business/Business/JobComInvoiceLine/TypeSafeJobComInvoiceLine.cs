using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public partial class JobComInvoiceLine : AutoTWJobComInvoiceLine
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceLine Clone()
		{
			return (JobComInvoiceLine)base.Clone();
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

		public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		#endregion

		#region protected override

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			return new JobComInvoiceLineLookups(this);
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			if (IsImport)
			{
				return new ImportJobComInvoiceLineValidation(this);
			}
			else if (IsExport)
			{
				return new ExportJobComInvoiceLineValidation(this);
			}
			else
			{
				return new JobComInvoiceLineValidation(this);
			}
		}

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public new CusEntryLine CusEntryLine => (CusEntryLine)base.CusEntryLine;

		protected override Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage) => new InvoiceLinePackageValidation(linkPackage, this);

		protected override Customs.Business.InvoiceLinePackagePivotCollection GetNewPackagesPivotCore() => new InvoiceLinePackagePivotCollection(this);

		protected override BaseCusLinkPackageCollection PackagesForInvoiceLinesCore() => new InvoiceLineCusLinkPackageCollection(this);
		#endregion
	}
}
