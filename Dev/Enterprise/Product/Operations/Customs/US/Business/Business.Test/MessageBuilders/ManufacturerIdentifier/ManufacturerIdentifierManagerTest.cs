using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ManufacturerIdentifierManagerTest : TestCaseWithFactory
	{
		public void TestGenerateAddMessage()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "ABC Import Pty Ltd";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "1234 Main Road";
			org.MainAddress.OA_Address2 = "Suite 4563";
			org.MainAddress.OA_City = "Sydney";
			org.MainAddress.OA_PostCode = "2000";

			var wrapper = OrgHeaderWrapper.New(org);
			var messageAddData = new ManufacturerAddMessageData(wrapper);
			ManufacturerIdentifierManager.GenerateAddMessage(messageAddData);
			AssertEquals("message generated", 1, messageAddData.wrapper.Messages.Count);
		}

		public void TestGenerateUpdateMessage()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "ABC Import Pty Ltd";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "1234 Main Road";
			org.MainAddress.OA_Address2 = "Suite 4563";
			org.MainAddress.OA_City = "Sydney";
			org.MainAddress.OA_PostCode = "2000";

			var wrapper = OrgHeaderWrapper.New(org);
			var messageAddData = new ManufacturerAddMessageData(wrapper);
			ManufacturerIdentifierManager.GenerateUpdateMessage(messageAddData);
			AssertEquals("message generated", 1, messageAddData.wrapper.Messages.Count);
		}
	}
}
