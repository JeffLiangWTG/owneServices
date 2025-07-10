using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public class ZAEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			short defaultDur = 10;
			var defaultUnit = TimeUnit.Year;

			yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.ZATransactionOrders, [NewInterchangeConfigObj(defaultDur, defaultUnit)], defaultDur, defaultUnit);
			yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.ZACustoms, [NewInterchangeConfigObj(defaultDur, defaultUnit)], defaultDur, defaultUnit);
		}
	}
}
