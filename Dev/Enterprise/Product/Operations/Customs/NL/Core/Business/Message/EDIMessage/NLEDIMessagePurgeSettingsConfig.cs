using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.NL.Business;

public class NLEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
{
	public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
	{
		yield return AddApplicationCodeMessageTypePurgeType(
			ApplicationCodeList.Codes.NLCustoms,
			new InterchangeObjCollection() { NewInterchangeConfigObj(6, TimeUnit.Month) },
			new MessageTypePurgeTypeObjCollection()
				.Add(NLEDIMessageTypes.Codes.DMS, NLEDIMessageTypes.Descriptions.DMS, 8, TimeUnit.Year)
				.Add(NLEDIMessageTypes.Codes.EXT, NLEDIMessageTypes.Descriptions.EXT, 8, TimeUnit.Year)
				.Add(NLEDIMessageTypes.Codes.NCT, NLEDIMessageTypes.Descriptions.NCT, 8, TimeUnit.Year));
	}
}
