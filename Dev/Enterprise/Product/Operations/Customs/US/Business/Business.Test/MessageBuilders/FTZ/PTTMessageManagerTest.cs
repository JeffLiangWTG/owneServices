using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PTTMessageManagerTest : TestCaseWithFactory
	{
		[TestDate(2011, 12, 30)]
		public void TestPopulate()
		{
			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "123456789";
			bill.CU_NoOfPacks = 12m;
			var container = bill.Containers.AddNew();
			container.CO_ContainerNumber = "ABC";
			var container1 = bill.Containers.AddNew();
			container1.CO_ContainerNumber = "CBA";

			var manager = new PTTMessageManager(declaration, PTTSendingOption.SendPTTMessage);
			manager.PopulateMessage();
			AssertEquals("Message should be generated", 1, declaration.Messages.Count);
			AssertEquals("PTT Message", EM_MessageSubTypeList.Codes.FTZPermitToTransfer, declaration.Messages[0].EM_MessageSubType);
			AssertEquals(FTZMessageStatusList.Codes.AwaitingPermitToTransfer, declaration.FTZPTTStatus);

			declaration.Messages.RemoveAndDeleteAll();
			manager = new PTTMessageManager(declaration, PTTSendingOption.CancellPTTMessage);
			manager.PopulateMessage();
			AssertEquals("New message should be generated", 1, declaration.Messages.Count);
			AssertEquals("PTT Message", EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer, declaration.Messages[0].EM_MessageSubType);
			AssertEquals(FTZMessageStatusList.Codes.AwaitingCancelPermitToTransfer, declaration.FTZPTTStatus);

			declaration.Messages.RemoveAndDeleteAll();
			manager = new PTTMessageManager(declaration, PTTSendingOption.SendPTTArrival);
			manager.PopulateMessage();
			AssertEquals("New message should be generated", 1, declaration.Messages.Count);
			AssertEquals("PTT Message", EM_MessageSubTypeList.Codes.FTZPermitToTransferArrival, declaration.Messages[0].EM_MessageSubType);
			AssertEquals(FTZMessageStatusList.Codes.AwaitingPermitToTransferArrival, declaration.FTZPTTStatus);

			declaration.Messages.RemoveAndDeleteAll();
			manager = new PTTMessageManager(declaration, PTTSendingOption.SendPTTUnArrival);
			manager.PopulateMessage();
			AssertEquals("New message should be generated", 1, declaration.Messages.Count);
			AssertEquals("PTT Message", EM_MessageSubTypeList.Codes.FTZSendPermitToTransferUnArrival, declaration.Messages[0].EM_MessageSubType);
			AssertEquals(FTZMessageStatusList.Codes.AwaitingPermitToTransferUnArrival, declaration.FTZPTTStatus);
		}

		public void TestCanSendThisMessage()
		{
			declaration.JE_MasterBill = "GTFR9008007";
			var manager = new PTTMessageManager(declaration, PTTSendingOption.SendPTTMessage);
			manager.CanSendThisMessage();
			AssertEquals("Cannot send message because FT Original is not accepted", false, manager.CanSendThisMessage());

			declaration.US_F_DirectDelivery = true;
			AssertEquals("Can send PTT for direct delivery zone, before FT Admission", true, manager.CanSendThisMessage());

			declaration.US_F_DirectDelivery = false;
			manager = new PTTMessageManager(declaration, PTTSendingOption.SendPTTArrival);
			manager.CanSendThisMessage();
			AssertEquals("Cannot send message because FT Original is not accepted", false, manager.CanSendThisMessage());

			declaration.US_F_DirectDelivery = true;
			AssertEquals("Can send PTT for direct delivery zone, before FT Admission", true, manager.CanSendThisMessage());
		}

		protected override void SetUp()
		{
			base.SetUp();

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_US_NKLocationOfGoods = "W004";
			declaration.FTZAdmissionNumber = "1530001|11|00000001";
			declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 11);

			header = declaration.ActiveEntryHeaders.AddNew();
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone;
		}
		JobDeclaration declaration;
		CusEntryHeader header;
	}
}
