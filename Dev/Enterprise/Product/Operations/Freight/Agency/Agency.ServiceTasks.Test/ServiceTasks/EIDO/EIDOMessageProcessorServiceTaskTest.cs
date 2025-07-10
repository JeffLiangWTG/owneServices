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
	[TestedType(typeof(EIDOMessageProcessorServiceTask))]
	internal class EIDOMessageProcessorServiceTaskTest : ServiceTaskTestCase<EIDOMessageProcessorServiceTask>
	{
		[ExpectNoExceptions]
		public void TestOptions()
		{
			AssertEquals("DefaultScheduleRunEvery", "30minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestIsRegistered()
		{
			HostedServiceAttribute att = TestHelper.GetAttributeRegistring<EIDOMessageProcessorServiceTask>();
			AssertNotNull("Should have found an attribute", att);
			CombineAssertions(delegate
			{
				AssertEquals("Category", ServiceTaskConstants.Category, att.Category);
				AssertEquals("Code", EIDOMessageProcessorServiceTask.Code, att.Code);
				AssertEquals("Description", EIDOMessageProcessorServiceTask.Description, att.Description);
				AssertEquals("RequiresCompanyInCountry", "AU", att.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, att.CanRunInAnyBranch);
			});
		}

		public void TestProcessorType()
		{
			EIDOMessageProcessorServiceTask task = new EIDOMessageProcessorServiceTask();
			AssertType(typeof(EIDOBaseMessageProcessor), task.Processor);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("30minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[] { new TaskNudgeInformationForTest(EDIMessageSchema.Constants.TableName, "EIDO Messages", EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive, EDIMessageSchema.Constants.EM_IsActive + "=" + "Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.EIDO), };
			}
		}
	}
}
