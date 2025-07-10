using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ServiceManager.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	public abstract class SendTSWBaseTest : TestCaseWithFactory
	{
		public void TestWarningWhenServiceTaskIsNotRunning()
		{
			using (NZCustomsDataRegistry.Instance.EnableNZServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var tswSender = GetNewSender(TSWTransactionTypes.Original);
				var flag = tswSender.CheckWarningsBeforeGeneratingMessage();
				var warning = string.Join(",", tswSender.MessageWarnings.ToList<string>());
				AssertEquals(false, flag);
				AssertContains(string.Empty, warning);
			}

			using (NZCustomsDataRegistry.Instance.EnableNZServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				serviceTask.S5_IsActive = false;
				serviceTask.S5_ScheduleType = "NCS";
				serviceTask.S5_TypeOfDocument = "NZC";
				var serviceTask1 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				serviceTask1.S5_IsActive = false;
				serviceTask1.S5_ScheduleType = "NCP";
				serviceTask1.S5_TypeOfDocument = "NZC";
				Factory.Save();

				var querierMock = new Mock<IServiceManagerQuerier>();
				querierMock.Setup(q => q.CheckStateOfNamedServiceTask("NCS")).Returns(ServiceTaskStatus.ServiceTaskIsInactive);

				using (ObjectFactory.Substitute(querierMock.Object))
				{
					var tswSender = GetNewSender(TSWTransactionTypes.Original);
					var flag = tswSender.CheckWarningsBeforeGeneratingMessage();
					var warning = string.Join(",", tswSender.MessageWarnings.ToList<string>());
					AssertEquals(true, flag);
					AssertContains(@"Please have your Administrator check the tasks with these codes. NCS: ServiceTaskIsInactive", warning);

					serviceTask.S5_IsActive = true;
					Factory.Save();

					querierMock.Setup(q => q.CheckStateOfNamedServiceTask("NCS")).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

					flag = tswSender.CheckWarningsBeforeGeneratingMessage();
					warning = string.Join(",", tswSender.MessageWarnings.ToList<string>());
					AssertEquals(false, flag);
					AssertNotContains("Please have your Administrator check the tasks with these codes. NCS: ServiceTaskIsInactive", warning);
				}
			}
		}

		public void TestErrorWhenBrokerageIsNotSet()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ""))
			{
				var tswSender = GetNewSender(TSWTransactionTypes.Original);
				AssertContains("Error message should contain", NZCustomsDataRegistry.Instance.NZBrokerageID.GetLocationInEnglish(), tswSender.Errors);
			}
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1234"))
			{
				var tswSender = GetNewSender(TSWTransactionTypes.Original);
				AssertContains("Error message should contain", NZCustomsDataRegistry.Instance.NZBrokerageID.GetLocationInEnglish(), tswSender.Errors);
			}
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			{
				var tswSender = GetNewSender(TSWTransactionTypes.Original);
				AssertNotContains("Error message should not contain", NZCustomsDataRegistry.Instance.NZBrokerageID.GetLocationInEnglish(), tswSender.Errors);
			}
		}

		#region Implementation

		protected abstract SendTSW GetNewSender(TSWTransactionTypes transactionType);

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper.SetupMessagingEnvironment();
		}

		#endregion
	}

	public class SendTSWTest : SendTSWBaseTest
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new SendTSWForTest(null, TSWTransactionTypes.Original);
		}

		public void TestSubmitterCode()
		{
			var tswSender = GetNewSender(TSWTransactionTypes.Original);
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "9917B"))
			{
				AssertEquals("00009917B", tswSender.SubmitterCode);
			}
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "40237571L"))
			{
				AssertEquals("40237571L", tswSender.SubmitterCode);
			}
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType)
		{
			var dummyObject = Factory.New<DummyBusinessObjectWithMessages>();
			return new SendTSWForTest(dummyObject, transactionType);
		}

		public void TestErrorAndWarningMessages()
		{
			using (NZCustomsDataRegistry.Instance.EnableNZServiceTaskCheckForSendingMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				serviceTask.S5_IsActive = false;
				serviceTask.S5_ScheduleType = "NCS";
				serviceTask.S5_TypeOfDocument = "NZC";
				var serviceTask1 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				serviceTask1.S5_IsActive = false;
				serviceTask1.S5_ScheduleType = "NCP";
				serviceTask1.S5_TypeOfDocument = "NZC";
				Factory.Save();

				var tswSender = GetNewSender(TSWTransactionTypes.Original);
				AssertEquals(1, tswSender.ErrorAndWarningCount);
				var messages = string.Join(",", tswSender.ErrorAndWarningMessages.ToList<string>());
				AssertContains(@"Please have your Administrator check the tasks with these codes. NCS: ServiceTaskIsInactive", messages);
			}
		}
	}

	public class SendTSWForTest : SendTSW
	{
		public SendTSWForTest(DummyBusinessObjectWithMessages hostEntity, TSWTransactionTypes transactionType) : base(hostEntity, null, transactionType)
		{
		}

		public override ZString DeclarantPinEncrypted => ZString.Empty;

		public override ZBool DeclarantPinRequired => false;

		public override ZString ApplicationReference => ZString.Empty;

		protected override ZString MessageType => ZString.Empty;

		protected override ZString MessageSubType => ZString.Empty;

		protected override ZBool GetIsMessageInTestMode => false;

		public override ZString GetMessageText() => ZString.Empty;

		protected override void AddMessageToMessages(TSWMessage message) { }

		protected override void CheckErrorsBeforeGeneratingMessageCore() { }

		protected override void SetStatusOnSuccess(StatusTransactionScope scope) { }
	}

	public class DummyBusinessObjectWithMessages : DummyBusinessObject
	{
		public DummyBusinessObjectWithMessages(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ChildEditable(true)]
		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this, Factory);
					RegisterEditableChildObject(messages);
					messages.Load();
				}
				return messages;
			}
		}
		EDIMessageCollection messages;
	}
}
