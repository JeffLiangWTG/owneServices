using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardAirEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			yield return AddApplicationCodeMessageSubTypePurgeType(
				ApplicationCodeList.Codes.ForwardAir,
				new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) },
				new MessageSubTypePurgeTypeObjCollection()
					.Add("XXX", string.Empty, 12, TimeUnit.Month)
			);
		}
	}
}
