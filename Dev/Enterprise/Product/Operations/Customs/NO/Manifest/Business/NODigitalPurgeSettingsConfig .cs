using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.NO.Manifest.Business;

sealed class NODigitalPurgeSettingsConfig : PurgeSettingsConfig
{
	public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
	{
		yield return AddApplicationCodeMessageSubTypePurgeType(ApplicationCodeList.Codes.NOCustomsDMO, new InterchangeObjCollection() { NewInterchangeConfigObj(5, TimeUnit.Year) }, new MessageSubTypePurgeTypeObjCollection()
			.Add(NODMOMessageFunctionList.Codes.NEW, NODMOMessageFunctionList.Descriptions.NEW, 5, TimeUnit.Year)
			.Add(NODMOMessageFunctionList.Codes.UPD, NODMOMessageFunctionList.Descriptions.UPD, 5, TimeUnit.Year)
			.Add(NODMOMessageFunctionList.Codes.DEL, NODMOMessageFunctionList.Descriptions.DEL, 5, TimeUnit.Year)
			.Add(NODMOMessageFunctionList.Codes.VAL, NODMOMessageFunctionList.Descriptions.VAL, 5, TimeUnit.Year)
		);
	}
}
