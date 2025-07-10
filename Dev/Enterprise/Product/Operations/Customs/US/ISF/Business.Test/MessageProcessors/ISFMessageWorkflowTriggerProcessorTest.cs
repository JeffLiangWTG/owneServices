using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFMessageWorkflowTriggerProcessorTest : TestCaseWithFactory
	{
		public void TestWorkflowTriggerProcessor()
		{
			var notifications = new NotificationBuffer();
			var header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "ISF0000001";
			header.MainShipToParty.OrganisationPK = ZGuid.NewZGuid();
			IProcessor processor = new ISFMessageWorkflowTriggerProcessor(header);
			processor.Process(notifications);
			AssertContains("Please enter a Ship To Party", notifications.AsString);
			notifications.Clear();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTHIP";
			header.MainShipToParty.OrganisationPK = orgHeader.PK;
			processor.Process(notifications);
			var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
			AssertEquals(1, informationNotifications.Length);
			AssertEquals(@"ISF message has been sent to customs for Job:ISF0000001", informationNotifications[0].Message);
			AssertEquals(1, header.Messages.Count);
			AssertEquals(MessageStatusList.Codes.AwaitingISFAdd, header.BF_CustomsStatus);
			notifications.Clear();
			processor.Process(notifications);
			AssertContains("please check whether it's waiting for response from customs", notifications.AsString);
			AssertEquals(1, header.Messages.Count);
			AssertEquals(MessageStatusList.Codes.AwaitingISFAdd, header.BF_CustomsStatus);
			notifications.Clear();
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			header.BF_CustomsReference = "ABC-12345678901";
			processor.Process(notifications);
			AssertContains("ISF message has been sent to customs for Job:ISF0000001", notifications.AsString);
			AssertEquals(2, header.Messages.Count);
			AssertEquals(MessageStatusList.Codes.AwaitingISFReplace, header.BF_CustomsStatus);
			notifications.Clear();
			processor.Process(notifications);
			AssertContains("please check whether it's waiting for response from customs", notifications.AsString);
			AssertEquals(2, header.Messages.Count);
			AssertEquals(MessageStatusList.Codes.AwaitingISFReplace, header.BF_CustomsStatus);
			notifications.Clear();
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFDelete;
			processor.Process(notifications);
			AssertContains("this Customs Reference 'ABC-12345678901' has been deleted from Customs system.", notifications.AsString);
			AssertEquals(2, header.Messages.Count);
			AssertEquals(MessageStatusList.Codes.ClearISFDelete, header.BF_CustomsStatus);
		}
	}
}
