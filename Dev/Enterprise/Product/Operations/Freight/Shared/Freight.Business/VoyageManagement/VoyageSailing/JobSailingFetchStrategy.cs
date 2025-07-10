using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	internal class JobSailingFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobSailingFetchStrategy(BaseJobSailing jobSailing)
			: base(jobSailing) { }

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			BaseJobSailing sailing = BusinessObject as BaseJobSailing;
			if (sailing != null)
			{
				Factory.AddFetchHint(typeof(VoyageOrigin), sailing.JX_JA);
				Factory.AddFetchHint(typeof(VoyageDestination), sailing.JX_JB);
			}
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			if (columns.Any(c => c.ColumnName == nameof(JobSailing.ExchangeRate)))
			{
				var sailing = BusinessObject as BaseJobSailing;
				if (sailing?.Origin != null && !sailing.Origin.JA_JV.IsEmpty)
				{
					Factory.AddFetchHint(JobVoyageExRateSchema.E8_JV, sailing.Origin.JA_JV);
				}
			}

			base.FetchForViewCore(columns);
		}
	}
}
