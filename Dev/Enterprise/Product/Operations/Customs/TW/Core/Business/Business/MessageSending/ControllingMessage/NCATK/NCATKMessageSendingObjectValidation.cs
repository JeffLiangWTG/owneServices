namespace Enterprise.Customs.TW.Business
{
	public class NCATKMessageSendingObjectValidation : ControllingMessageSendingObjectValidation
	{
		public NCATKMessageSendingObjectValidation(AutoControllingMessageSendingObject parent) : base(parent)
		{
		}

		protected override void CheckShouldSendWhenTrue()
		{
			base.CheckShouldSendWhenTrue();
			var clientSetting = RegistryHelper.TWNCATKClientSetting;
			var isValidTWNCATKClientSetting = clientSetting != null && !clientSetting.EHubClientID.IsEmpty && clientSetting.EHubClientStatus == Constants.TWNCATKClient.EHubClientStatusOK;
			if (!isValidTWNCATKClientSetting)
			{
				Parent.ShouldSendInfo.AddError(ValidationConstants.NCATKMessageSendingObject.MissingNCATKRegistryConfiguration);
			}
		}
	}
}
