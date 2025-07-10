using System.Collections.Generic;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ForwarderManifest.ServiceTasks.Test
{
	[TestedType(typeof(UEMMessageServiceTask))]
	public class UEMMessageServiceTaskTest : ServiceTaskTestCase<UEMMessageServiceTask>
	{
		public void TestRunMainTask()
		{
			var message1 = Factory.New<UEMEDIMessage>();
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var message2 = Factory.New<EDIMessage>();
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			Factory.Save();

			var serviceTask = new UEMMessageServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);

			message1.Reload();
			message2.Reload();
			AssertEquals(EDIMessage.Status.Failed, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Export Manifest Message",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USExportManifest,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}
