using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.AMS.ServiceTasks.Testing
{
	[TestedType(typeof(AMSInterchangeServiceTask))]
	sealed class AMSInterchangeServiceTaskTest : ServiceTaskTestCase<AMSInterchangeServiceTask>
	{
		public void TestCanRunInAnyBranch()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			Assert("CanRunInAnyBranch", hostedServiceAttribute.CanRunInAnyBranch);
		}

		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("UAI", "United States AMS Customs Interchange Messaging", "USC");
		}

		public void TestSettingsValidation_SendCBPViaEHub_CompanyWithCarrierCode()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_Status = CBPEDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Receive;
			interchange.EI_InterchangeNum = "1";
			interchange.EI_HeaderText =
				"ACR8CAR      MR11032301200500101                                                ";
			interchange.EI_BodyText =
				"M01OTT111USTOWER BRIDGE           451  00001000007 8505989                      " +
				"M02123456789012_79                                                              " +
				"P01270403261100002    0001                                                      " +
				"W02OTT11103230120050100100001000000000000000000000000100017                     ";
			interchange.EI_FooterText =
				"ZCR8CAR      MR                   00004";

			Factory.Save();
			var serviceTask = new AMSInterchangeServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertContains($"Interchange '{interchange.EI_InterchangeNum}' has been processed successfully.", log.ToString());
			interchange.Reload();
			AssertEquals("Interchange is received.", CBPEDIInterchange.Status.Received, interchange.EI_Status);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"US Customs AMS interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.AMS),
				};
			}
		}
	}
}
