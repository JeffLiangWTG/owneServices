using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class ContainerManagementEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			yield return AddApplicationCodeMessageSubTypePurgeType(
				ApplicationCodeList.Codes.ContainerManagement,
				new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) },
				new MessageSubTypePurgeTypeObjCollection()
					.Add(string.Empty, string.Empty, 12, TimeUnit.Month)
					.Add(ApplicationCodeList.Codes.ContainerManagement, ApplicationCodeList.Descriptions.ContainerManagement, 12, TimeUnit.Month)
					.Add(EDIMessageSubTypeList.Codes.ContainerMovements, EDIMessageSubTypeList.Descriptions.ContainerMovements, 12, TimeUnit.Month)
			);
		}
	}
}
