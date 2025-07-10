using System.Collections.Generic;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ForwarderManifest.ServiceTasks.Test
{
	[TestedType(typeof(UEMInboundServiceTask))]
	public class UEMInboundServiceTaskTest : ServiceTaskTestCase<UEMInboundServiceTask>
	{
		public void TestRunMainTask()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "BR1";
			branch1.GB_GC = Env.CurrentCompanyPK;

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "BR2";
			branch2.GB_GC = Env.CurrentCompanyPK;
			Factory.Save();

			var interchange1 = Factory.New<UEMEDIInterchange>();
			interchange1.EI_Status = EDIInterchange.Status.Queued;
			interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange1.EI_GB = branch1.EntityPK;
			interchange1.EI_BodyText = "TEST BODY 1";
			interchange1.EI_From = "USC";
			interchange1.EI_To = "CW1";

			var interchange2 = Factory.New<UEMEDIInterchange>();
			interchange2.EI_Status = EDIInterchange.Status.Queued;
			interchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange2.EI_GB = branch2.EntityPK;
			interchange2.EI_BodyText = "TEST BODY 2";
			interchange2.EI_From = "USC";
			interchange2.EI_To = "CW1";
			Factory.Save();

			AssertEquals(0, interchange1.ContainedMessages.Count);
			AssertEquals(0, interchange2.ContainedMessages.Count);

			var serviceTask = new UEMInboundServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);

			interchange1.ContainedMessages.Reload(true);
			interchange2.ContainedMessages.Reload(true);
			AssertEquals(1, interchange1.ContainedMessages.Count);
			AssertEquals(1, interchange2.ContainedMessages.Count);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"US Export Manifest Inbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.USExportManifest),
				};
			}
		}
	}
}
