using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public partial class JobComInvoiceGroupHeader : AutoJobComInvoiceGroupHeader
		, Integration.Customs.US.IJobComInvoiceGroupHeader
		, ICurrencyConverterDataProvider
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ICurrencyConverterDataProvider

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return 0; }
		}

		#endregion
	}
}
