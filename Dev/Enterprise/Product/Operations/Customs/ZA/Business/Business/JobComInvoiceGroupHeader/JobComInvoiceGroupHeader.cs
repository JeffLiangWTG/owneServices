using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class JobComInvoiceGroupHeader : BaseJobComInvoiceGroupHeader, Integration.Customs.ZA.IJobComInvoiceGroupHeader, ICurrencyConverterDataProvider
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		public new InvoiceHeaderActiveCollection JobComInvoiceHeaders
		{
			get { return (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders; }
		}

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection()
		{
			return new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);
		}

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(this, true);
		}

		public new JobComInvoiceGroupHeaderLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new JobComInvoiceGroupHeaderLookups(this);
				}
				return fLookups;
			}
		}

		JobComInvoiceGroupHeaderLookups fLookups;

		public new JobComInvoiceGroupHeaderValidation Validation
		{
			get { return new JobComInvoiceGroupHeaderValidation(this); }
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			return base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetCustomsChargeTypeListCacheKey();
		}

		#region ICurrencyConverterDataProvider

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return 0; }
		}

		#endregion
	}
}
