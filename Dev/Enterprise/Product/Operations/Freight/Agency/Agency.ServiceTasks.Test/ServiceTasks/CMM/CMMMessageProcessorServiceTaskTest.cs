using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	[TestedType(typeof(CMMMessageProcessorServiceTask))]
	internal class CMMMessageProcessorServiceTaskTest : ServiceTaskTestCase<CMMMessageProcessorServiceTask>
	{
		[ExpectNoExceptions]
		public void TestOptions()
		{
			AssertEquals("DefaultScheduleRunEvery", "30minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestIsRegistered()
		{
			HostedServiceAttribute att = TestHelper.GetAttributeRegistring<CMMMessageProcessorServiceTask>();
			AssertNotNull("Should have found an attribute", att);
			CombineAssertions(delegate
			{
				AssertEquals("Category", ServiceTaskConstants.Category, att.Category);
				AssertEquals("Code", CMMMessageProcessorServiceTask.Code, att.Code);
				AssertEquals("Description", CMMMessageProcessorServiceTask.Description, att.Description);
				AssertEquals("CanRunInAnyBranch", true, att.CanRunInAnyBranch);
			});
		}

		public void TestProcessorType()
		{
			CMMMessageProcessorServiceTask task = new CMMMessageProcessorServiceTask();
			AssertType(typeof(CMMBaseMessageProcessor), task.Processor);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("30minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[] { new TaskNudgeInformationForTest(EDIMessageSchema.Constants.TableName, "Container Management Messages", EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive, EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.ContainerManagement), };
			}
		}
	}
}
