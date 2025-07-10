using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public class ComTracEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			yield return AddApplicationCodeMessageSubTypePurgeType(
				ApplicationCodeList.Codes.ComTrac,
				new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) },
				new MessageSubTypePurgeTypeObjCollection()
					.Add(string.Empty, string.Empty, 12, TimeUnit.Month)
					.Add("XXX", string.Empty, 12, TimeUnit.Month)
			);
		}
	}
}
