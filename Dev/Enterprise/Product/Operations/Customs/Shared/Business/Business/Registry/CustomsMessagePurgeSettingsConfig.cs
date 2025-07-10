using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.Business;

sealed class CustomsMessagePurgeSettingsConfig : PurgeSettingsConfig
{
	public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
	{
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.JPCustoms, new InterchangeObjCollection() { NewInterchangeConfigObj(10, TimeUnit.Year) }, 10, TimeUnit.Year);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.TWCustoms, new InterchangeObjCollection() { NewInterchangeConfigObj(10, TimeUnit.Year) }, 10, TimeUnit.Year);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.TRCustoms, new InterchangeObjCollection() { NewInterchangeConfigObj(5, TimeUnit.Year) }, 5, TimeUnit.Year);
		yield return AddApplicationCodePurgeType(ApplicationCodeList.Codes.UAECustoms, new InterchangeObjCollection() { NewInterchangeConfigObj(6, TimeUnit.Month) }, 6, TimeUnit.Month);
	}
}
