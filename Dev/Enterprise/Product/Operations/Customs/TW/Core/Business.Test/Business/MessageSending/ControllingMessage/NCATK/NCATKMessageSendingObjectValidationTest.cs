using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NCATKMessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckTWNCATKClientSetting()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			header.TW1_ControllingMessageType = "A";
			header.TW1_RequestDescription = "B";
			header.TW1_FunctionalReferenceId = "C";
			var action = new NCATKMessageSendingObject(header);
			action.ShouldSend = true;
			var targetInfo = action.ShouldSendInfo;
			AssertHasErrorContaining(targetInfo, ValidationConstants.NCATKMessageSendingObject.MissingNCATKRegistryConfiguration);
			var clientSetting = new TWNCATKClientSetting(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			clientSetting.MachineName = "Machine Name";
			clientSetting.SendToFolder = @"D:\Folders\SendFolder";
			clientSetting.RunningIntervalInSeconds = 15;
			using (TWCustomsDataRegistry.Instance.TWNCATKClientSetting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, clientSetting))
			{
				action = new NCATKMessageSendingObject(header);
				action.ShouldSend = true;
				targetInfo = action.ShouldSendInfo;
				AssertNoErrorContaining(targetInfo, ValidationConstants.NCATKMessageSendingObject.MissingNCATKRegistryConfiguration);
			}
		}
	}
}
