using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class EIDOEDIMessagePurgeSettingsCongfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			yield return AddApplicationCodeMessageSubTypePurgeType(
				ApplicationCodeList.Codes.EIDO,
				new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) },
				new MessageSubTypePurgeTypeObjCollection()
					.Add(EInvoiceAPICommandList.Codes.GenerateCancellationRequest, EInvoiceAPICommandList.Descriptions.GenerateCancellationRequest, 12, TimeUnit.Month)
					.Add(ApplicationCodeList.Codes.EIDO, ApplicationCodeList.Descriptions.EIDO, 12, TimeUnit.Month)
					.Add(EDIMessageSubTypeList.Codes.Organizations, EDIMessageSubTypeList.Descriptions.Organizations, 12, TimeUnit.Month)
			);
		}
	}
}
