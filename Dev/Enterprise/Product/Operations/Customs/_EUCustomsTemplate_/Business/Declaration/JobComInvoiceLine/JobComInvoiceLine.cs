using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration
{
	public class JobComInvoiceLine : EU.Business.Declaration.JobComInvoiceLine
		, Integration.Customs._EUCustomsTemplate_.IJobComInvoiceLine
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

		public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

		protected override CargoWise.Types.ZString CustomsCountryCodeCore => Core.Constants.CountryCodes._EUTemplateCountryName_;
		protected override System.Type TypeOfPartUsedCore => typeof(MasterFiles.OrgSupplierPart);

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			JobComInvoiceLineLookups result;
			if (IsImport)
			{
				result = new ImportJobComInvoiceLineLookups(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceLineLookups(this);
			}
			else
			{
				result = new JobComInvoiceLineLookups(this);
			}
			return result;
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			JobComInvoiceLineValidation result;
			if (IsImport)
			{
				result = new ImportJobComInvoiceLineValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceLineValidation(this);
			}
			else
			{
				result = new JobComInvoiceLineValidation(this);
			}
			return result;
		}
	}
}
