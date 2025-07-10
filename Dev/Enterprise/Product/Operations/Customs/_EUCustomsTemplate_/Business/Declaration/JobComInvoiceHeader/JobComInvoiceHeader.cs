using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration
{
	public class JobComInvoiceHeader : EU.Business.Declaration.JobComInvoiceHeader, Integration.Customs._EUCustomsTemplate_.IJobComInvoiceHeader
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

		public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

		public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

		public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			JobComInvoiceHeaderValidation result;
			if (IsImport)
			{
				result = new ImportJobComInvoiceHeaderValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceHeaderValidation(this);
			}
			else
			{
				result = new JobComInvoiceHeaderValidation(this);
			}
			return result;
		}

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			JobComInvoiceHeaderLookups result;
			if (IsImport)
			{
				result = new ImportJobComInvoiceHeaderLookups(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceHeaderLookups(this);
			}
			else
			{
				result = new JobComInvoiceHeaderLookups(this);
			}
			return result;
		}

		protected override CargoWise.Types.ZString LocalCurrencyCodeCore
		{
			get
			{
				if (!GetType().FullName.Contains("_EUCustomsTemplate_"))
				{
					throw new NotImplementedException();
				}
				return base.LocalCurrencyCodeCore;
			}
		}

		protected override Customs.Business.BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			var dec = JobDeclaration;
			return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
		}

		protected override Customs.Business.BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			var collection = new Customs.Business.InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}
	}
}
