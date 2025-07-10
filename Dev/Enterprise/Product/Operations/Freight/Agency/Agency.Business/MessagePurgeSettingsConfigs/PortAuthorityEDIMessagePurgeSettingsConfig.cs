using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class PortAuthorityEDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			yield return AddApplicationCodeMessageSubTypePurgeType(
				ApplicationCodeList.Codes.PortAuthority,
				new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) },
				new MessageSubTypePurgeTypeObjCollection()
					.Add(EInvoiceAPICommandList.Codes.GenerateCancellationRequest, EInvoiceAPICommandList.Descriptions.GenerateCancellationRequest, 12, TimeUnit.Month)
					.Add(EDIMessageSubTypeList.Codes.Organizations, EDIMessageSubTypeList.Descriptions.Organizations, 12, TimeUnit.Month)
					.Add(EDIInterchangeTypeList.Codes.USDISReply, EDIInterchangeTypeList.Descriptions.USDISReply, 12, TimeUnit.Month)
					.Add("XXX", string.Empty, 12, TimeUnit.Month)
			);
		}
	}
}
