using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccExchangeRateConfigurationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public AccExchangeRateConfigurationFetchStrategy(AccExchangeRateConfiguration configuration) : base(configuration)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(AccJobConfigPivotSchema.JCT_JCF_JobConfig, BusinessObject.PK);
		}
	}
}
