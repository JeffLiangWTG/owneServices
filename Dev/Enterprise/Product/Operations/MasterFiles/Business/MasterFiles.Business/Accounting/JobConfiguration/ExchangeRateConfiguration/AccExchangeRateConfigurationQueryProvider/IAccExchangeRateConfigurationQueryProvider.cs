using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IAccExchangeRateConfigurationsQueryProvider
	{
		public ZQuery GetQuery();
	}
}
