using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration
{
	public class JobDeclaration : EU.Business.Declaration.JobDeclaration
		, Integration.Customs._EUCustomsTemplate_.IJobDeclaration
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

		public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

		protected override Customs.Business.JobDeclarationLookups GetNewLookups()
		{
			JobDeclarationLookups result;
			if (IsImport)
			{
				result = new ImportJobDeclarationLookups(this);
			}
			else if (IsExport)
			{
				result = new ExportJobDeclarationLookups(this);
			}
			else
			{
				result = new JobDeclarationLookups(this);
			}
			return result;
		}

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			JobDeclarationValidation result;
			if (IsImport)
			{
				result = new ImportJobDeclarationValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobDeclarationValidation(this);
			}
			else
			{
				result = new JobDeclarationValidation(this);
			}
			return result;
		}

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override CargoWise.Types.ZString LocalCurrencyCodeCore
		{
			get
			{
				var fullName = GetType().FullName;
				if (!fullName.Contains("_EUCustomsTemplate_") && !fullName.Contains("Castle.Proxies.JobDeclarationProxy"))
				{
					throw new NotImplementedException();
				}
				return base.LocalCurrencyCodeCore;
			}
		}

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("_EUCustomsTemplate_"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}
	}
}
