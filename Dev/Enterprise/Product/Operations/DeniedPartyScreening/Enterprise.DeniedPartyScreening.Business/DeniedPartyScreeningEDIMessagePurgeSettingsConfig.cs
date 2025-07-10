using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DeniedPartyScreening.Business
{
	internal class DeniedPartyScreeningEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			ZShort defaultDur = 6;
			var defaultUnit = TimeUnit.Month;
			yield return AddApplicationCodeMessageSubTypePurgeType(ApplicationCodeList.Codes.DPSRequestMessage, new InterchangeObjCollection() { NewInterchangeConfigObj(defaultDur, defaultUnit) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(EDIMessageSubTypeList.Codes.ScreeningRequest, EDIMessageSubTypeList.Descriptions.ScreeningRequest, defaultDur, defaultUnit)
			);
		}
	}
}
