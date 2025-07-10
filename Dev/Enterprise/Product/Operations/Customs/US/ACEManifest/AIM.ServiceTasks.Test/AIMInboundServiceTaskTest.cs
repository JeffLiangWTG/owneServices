using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using Constants = Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.AIM.ServiceTasks.Testing
{
	[TestedType(typeof(AIMInboundServiceTask))]
	class AIMInboundServiceTaskTest : ServiceTaskTestCase<AIMInboundServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("AMI", "US Air Manifest Inbound", "USC");
		}

		public void TestUnpackingAIMInterchange()
		{
			var msgText = @"FER
QF101/12DEC
081-11223344-HAWB123/123456
ERR/001ERROR DESCRIPTION
ERR/002ANOTHER ERROR
";

			var interchange = Factory.New<AIMEDIInterchange>();
			interchange.EI_InterchangeType = EDIMessageTypeList.Codes.FHL;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = "USC";
			interchange.EI_To = "SV9";
			interchange.EI_InterchangeNum = "00002345";
			interchange.EI_HeaderText = "WASUCCR\x0D\x0A.BCBTSV9";
			interchange.EI_BodyText = msgText;
			Factory.Save();

			var serviceTask = new AIMInboundServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);

			interchange.Reload();
			AssertEquals("EI_Status", EDIMessage.Status.Received, interchange.EI_Status);

			var message = (AIMEDIMessage)interchange.ContainedMessages[0];
			AssertEquals("EM_ReceiveTransmit", message.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			AssertEquals("EM_Status", message.EM_Status, EDIMessage.Status.Queued);
			AssertEquals("EM_MessageText", msgText, message.EM_MessageText);
			AssertEquals("EM_MessageType", message.EM_MessageType, EDIMessageTypeList.Codes.FHL);
			AssertEquals("EM_MessageSubType", message.EM_MessageSubType, Constants.AIMMessageSubTypes.FER);

			AssertContains("Service task log", "Interchange '00002345' has been processed successfully.", serviceTask.ServiceLogger.ToString());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"US Air Manifest Inbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.USAMA),
				};
			}
		}
	}
}
