using System.Collections.Generic;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ZA.ServiceTasks.Testing
{
	[TestedType(typeof(ExternalWarehouseMessageProcessorService))]
	sealed class ExternalWarehouseMessageProcessorServiceTest : ServiceTaskTestCase<ExternalWarehouseMessageProcessorService>
	{
		public void TestProcessMessages()
		{
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_EI = ediInterchange.PK;
			ediMessage.EM_MessageNum = "1";
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageMissingOwnerReference();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var ediMessage2 = Factory.New<EWHMessage>();
			ediMessage2.EM_EI = ediInterchange.PK;
			ediMessage2.EM_MessageNum = "2";
			ediMessage2.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageValid();
			ediMessage2.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(new ExternalWarehouseMessageProcessorService());
			ediMessage.Reload();
			ediMessage2.Reload();

			AssertEquals("ewhMessage.EM_Status", EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertNotNull("Message contains log", ediMessage.Logs.Find(x => x.SL_Reference == "Owner Reference for Line Number 1 is missing."));
			AssertEquals("ewhMessage.EM_Status", EDIMessage.Status.ProcessedOK, ediMessage2.EM_Status);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			ExternalWarehouseInterchangeTestHelper.SetupWarehouseAndPart(Factory);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"ZA External Warehouse Message Processor",
						EDIMessageSchema.Constants.EM_Status + "=" + Messaging.Integration.EDIInterchangeStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_MessageType + "=" + ZAEDIMessageTypeList.Codes.ExternalWarehouse,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.SouthAfricanTransactionOrders),
				};
			}
		}
	}
}
