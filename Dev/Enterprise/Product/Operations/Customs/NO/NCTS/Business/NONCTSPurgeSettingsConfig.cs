using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.NO.NCTS.Business;

sealed class NONCTSPurgeSettingsConfig : PurgeSettingsConfig
{
	public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
	{
		yield return AddApplicationCodeMessageSubTypePurgeType(ApplicationCodeList.Codes.NOCustomsNcts, new InterchangeObjCollection() { NewInterchangeConfigObj(5, TimeUnit.Year) }, new MessageSubTypePurgeTypeObjCollection()
			.Add(NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification, NctsArrivalMessageTypeCodeList.Descriptions.ArrivalNotification, 5, TimeUnit.Year)
			.Add(NctsArrivalMessageTypeCodeList.Codes.UnloadingRemarks, NctsArrivalMessageTypeCodeList.Descriptions.UnloadingRemarks, 5, TimeUnit.Year)
		);
	}
}
