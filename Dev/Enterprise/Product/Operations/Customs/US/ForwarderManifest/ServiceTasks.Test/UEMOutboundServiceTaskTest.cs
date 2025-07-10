using System.Collections.Generic;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ForwarderManifest.ServiceTasks.Test
{
	[TestedType(typeof(UEMOutboundServiceTask))]
	public class UEMOutboundServiceTaskTest : ServiceTaskTestCase<UEMOutboundServiceTask>
	{
		public void TestRunMainTask()
		{
			var message1 = Factory.New<UEMEDIMessage>();
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_LinkedObject = Factory.New<USExportAsycudaManifestHeader>().Bills.AddNew();

			var orgCusCode = message1.Branch.Company.OrgProxy.CustomsCodes.AddNew();
			orgCusCode.OK_CustomsRegNo = "CW1";
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_RN_NKCodeCountry = "US";

			var message2 = Factory.New<US.Business.MQEDIMessage>();
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			Factory.Save();

			var serviceTask = new UEMOutboundServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);

			message1.Reload();
			message2.Reload();
			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);

			AssertNotNull(message1.Interchange);
			AssertNull(message2.Interchange);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Export Manifest Outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USExportManifest,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}
