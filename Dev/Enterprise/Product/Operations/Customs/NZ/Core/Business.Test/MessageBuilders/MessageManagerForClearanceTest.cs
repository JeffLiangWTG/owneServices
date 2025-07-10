using System;
using CargoWise.Application;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.Testing
{
	public abstract class MessageManagerForClearanceTest : MessageManagerTest
	{
		//
		//                                             MessageManager
		//                                                    |
		//                                 >>>>>> MessageManagerForClearance <<<<<<
		//                               /                    |                     \
		//         MessageManagerForDeclaration    MessageManagerForCusMAWB     MessageManagerForSeaCargo
		//                   /              \
		//  ECIWriteOff.MessageManager     FormalEntry.MessageManager
		//                 |
		//  ECIWriteOff.Manifesting.MessageManager
		//
		//
		//  eBACCa/IPI/Outward Report in SendTSW.

		public void TestErrorWhenBrokerageIsNotSet()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ""))
			{
				var messageManager = GetNewManager(MessageManager.OperationType.SubmitMessage);
				AssertEquals(false, messageManager.IsOkToExecute);
				AssertStartsWith("Error message should display as", "You must setup the following registry items before sending to Customs.", messageManager.LastHumanReadableStatus);
				AssertContains("Error message should contain", NZCustomsDataRegistry.Instance.NZBrokerageID.GetLocationInEnglish(), messageManager.LastHumanReadableStatus);
			}
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1234"))
			{
				var messageManager = GetNewManager(MessageManager.OperationType.SubmitMessage);
				AssertEquals(false, messageManager.IsOkToExecute);
				AssertStartsWith("Error message should display as", "You must setup the following registry items before sending to Customs.", messageManager.LastHumanReadableStatus);
				AssertContains("Error message should contain", NZCustomsDataRegistry.Instance.NZBrokerageID.GetLocationInEnglish(), messageManager.LastHumanReadableStatus);
			}
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			{
				var messageManager = GetNewManager(MessageManager.OperationType.SubmitMessage);
				AssertNotContains("Error message should not contain", NZCustomsDataRegistry.Instance.NZBrokerageID.GetLocationInEnglish(), messageManager.LastHumanReadableStatus);
			}
		}

		public void TestWarningWhenServiceTaskIsNotActive()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var wrapper = currentUser.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "TST";
			var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			serviceTask.S5_IsActive = false;
			serviceTask.S5_ScheduleType = "NCS";
			serviceTask.S5_TypeOfDocument = "NZC";
			var serviceTask1 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			serviceTask1.S5_IsActive = false;
			serviceTask1.S5_ScheduleType = "NCP";
			serviceTask1.S5_TypeOfDocument = "NZC";
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.EnableNZServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var messageManager = GetManagerWithValidParent();
				AssertEquals(true, messageManager.IsOkToExecute);
				AssertEquals(string.Empty, messageManager.LastHumanReadableWarning);
			}

			using (NZCustomsDataRegistry.Instance.EnableNZServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var messageManager = GetManagerWithValidParent();
				AssertEquals(true, messageManager.IsOkToExecute);
				AssertEquals(@"Please have your Administrator check the tasks with these codes. NCS: ServiceTaskIsInactive", messageManager.LastHumanReadableWarning);

				serviceTask.S5_IsActive = true;
				Factory.Save();

				var mockQuerier = new Mock<IServiceManagerQuerier>();
				mockQuerier.Setup(q => q.CheckStateOfNamedServiceTask("NCS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				using (ObjectFactory.Substitute(mockQuerier.Object))
				{
					_ = messageManager.IsOkToExecute;
					AssertEquals(string.Empty, messageManager.LastHumanReadableWarning);
				}
			}
		}

		protected virtual MessageManagerForClearance GetManagerWithValidParent()
		{
			return GetNewManager(MessageManager.OperationType.SubmitMessage);
		}

		#region Implementation

		MessageManagerForClearance GetNewManager(MessageManager.OperationType operationType)
		{
			return (MessageManagerForClearance)GetNewMessageManager(operationType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper.SetupMessagingEnvironment();
		}

		#endregion
	}
}
