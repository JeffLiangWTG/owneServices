using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	class LVSAutoSendCargoReleaseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestAutoSendCargoReleaseMessageWithError()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_JobNumber = "SEC000001";
			clearance.Validation.ValidateAll();
			IProcessor processor = new LVSAutoSendCargoReleaseMessageProcessor(clearance, string.Empty);
			var notification = new NotificationBuffer();
			processor.Process(notification);
			AssertContains(@"System cannot send release message because of following errors on Job:SEC000001, please fix all of them and try again.
Error - ULH_EntryFilerCode: An entry filer code has not been set up for this branch or company. Please set up one in Admin->System->Registry Customs -> Country or Region Specific -> United States of America -> Import -> ABI -> Entry Filer
There is no release message sent to customs for Job:SEC00000001", notification.AsString);

			clearance.ULH_EntryFilerCode = "SV9";
			clearance.Validation.ValidateAll();
			processor = new LVSAutoSendCargoReleaseMessageProcessor(clearance, string.Empty);
			notification.Clear();
			processor.Process(notification);
			AssertContains(@"There is no release message sent to customs for Job:SEC00000001", notification.AsString);
		}

		public void TestMutexLock_Message()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_JobNumber = "SEC000001";
			clearance.ULH_EntryFilerCode = "XJ5";
			var consignmentOne = clearance.CusUSLVConsignments.AddNew();
			consignmentOne.ULB_HouseBill = "HB1";
			consignmentOne.FirstCusUSLVItem.ULI_Tariff = "1234567890";

			Factory.Save();
			var clearanceInNewFactory = new BusinessObjectFactory().Load<CusUSLVClearance>(clearance.PK);
			clearanceInNewFactory.LockSendCustomsMessageMutex();

			IProcessor processor = new LVSAutoSendCargoReleaseMessageProcessor(clearance, string.Empty);
			var notification = new NotificationBuffer();
			processor.Process(notification);

			AssertContains("is sending messages for this Low Value Entries job, please try again later.", notification.AsString);
			clearanceInNewFactory.UnlockSendCustomsMessageMutex();
		}

		public void TestAutoSendCargoReleaseMessageForConsignments()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_JobNumber = "SEC000001";
			clearance.ULH_EntryFilerCode = "XJ5";
			clearance.ULH_TransportMode = Core.Constants.TransportModes.Sea;
			clearance.ULH_MasterBill = "MB1234567";

			var consignmentOne = clearance.CusUSLVConsignments.AddNew();
			consignmentOne.ULB_HouseBill = "HB1";
			consignmentOne.FirstCusUSLVItem.ULI_Tariff = "1234567890";
			var consignmentTwo = clearance.CusUSLVConsignments.AddNew();
			consignmentTwo.ULB_HouseBill = "HB2";
			consignmentTwo.FirstCusUSLVItem.ULI_Tariff = "2345678901";
			consignmentTwo.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
			var consignmentThree = clearance.CusUSLVConsignments.AddNew();
			consignmentThree.ULB_HouseBill = "HB3";
			consignmentThree.FirstCusUSLVItem.ULI_Tariff = "3456789012";
			consignmentThree.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;
			var consignmentFour = clearance.CusUSLVConsignments.AddNew();
			consignmentFour.ULB_HouseBill = "HB4";
			consignmentFour.FirstCusUSLVItem.ULI_Tariff = "4567890123";
			consignmentFour.CE_EntryNum = "71002057";
			Factory.Save();

			IProcessor processor = new LVSAutoSendCargoReleaseMessageProcessor(clearance, string.Empty);
			var notification = new NotificationBuffer();
			processor.Process(notification);
			AssertContains(@"System cannot allocate entry number for house bill:HB1, becase The Entry Number Range of Branch", notification.AsString);
			AssertContains(@"System cannot send the release message for house bill:HB3, please check whether it's waiting for response from customs.", notification.AsString);

			var informationNotifications = notification.GetEventsByType(NotificationType.Information);
			AssertEquals(1, informationNotifications.Length);
			AssertEquals(@"Release message has been sent to customs for Job:SEC00000001", informationNotifications[0].Message);

			var newFactory = new BusinessObjectFactory();
			var consignmentReloaded = newFactory.Load<CusUSLVConsignment>(consignmentOne.PK);
			AssertEquals("Message status is empty for HB1", "", consignmentReloaded.ULB_MessageStatus);
			AssertEquals("No message generated for HB1", 0, consignmentReloaded.Messages.Count);
			consignmentReloaded = newFactory.Load<CusUSLVConsignment>(consignmentTwo.PK);
			AssertEquals("Message status is empty for HB2", "", consignmentReloaded.ULB_MessageStatus);
			AssertEquals("No message generated for HB2", 0, consignmentReloaded.Messages.Count);
			consignmentReloaded = newFactory.Load<CusUSLVConsignment>(consignmentThree.PK);
			AssertEquals("Message status is ASA for HB3", "ASA", consignmentReloaded.ULB_MessageStatus);
			AssertEquals("No message generated for HB3", 0, consignmentReloaded.Messages.Count);
			consignmentReloaded = newFactory.Load<CusUSLVConsignment>(consignmentFour.PK);
			AssertEquals("Message status is ASA for HB4", "ASA", consignmentReloaded.ULB_MessageStatus);
			AssertEquals("No message generated for HB4", 1, consignmentReloaded.Messages.Count);
		}

		public void TestAutoSendCargoReleaseMessageForConsignments_WhenEventCodeIsEmpty()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_JobNumber = "SEC000001";
			clearance.ULH_EntryFilerCode = "XJ5";
			clearance.ULH_TransportMode = Core.Constants.TransportModes.Sea;
			clearance.ULH_MasterBill = "MB1234567";

			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 9999, 10000);

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;

			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			IProcessor processor = new LVSAutoSendCargoReleaseMessageProcessor(clearance, string.Empty);
			var notification = new NotificationBuffer();
			processor.Process(notification);

			CombineAssertions("process all consignments", () =>
			{
				AssertEquals(ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd, consignment1.ULB_MessageStatus);
				AssertEquals(ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd, consignment2.ULB_MessageStatus);
			});
		}

		public void TestAutoSendCargoReleaseMessageForConsignments_WhenEventCodeIsMessagePendingProcessingCode()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_JobNumber = "SEC000001";
			clearance.ULH_EntryFilerCode = "XJ5";
			clearance.ULH_TransportMode = Core.Constants.TransportModes.Sea;
			clearance.ULH_MasterBill = "MB1234567";

			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10000);

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;

			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			IProcessor processor = new LVSAutoSendCargoReleaseMessageProcessor(clearance, AutoEvents.MessagePendingProcessingCode);
			var notification = new NotificationBuffer();
			processor.Process(notification);

			CombineAssertions("only process consignments which ULB_MessageStatus is ORP", () =>
			{
				AssertEquals(ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd, consignment1.ULB_MessageStatus);
				AssertEquals(string.Empty, consignment2.ULB_MessageStatus);
			});
		}
	}
}
