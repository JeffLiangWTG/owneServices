using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface ICrossTradeDebtorDefaultingConfigurationProvider
	{
		List<ICrossTradeDebtorDefaultingConfigurationItem> GetConfiguration();
	}
}
