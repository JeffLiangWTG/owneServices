using System.Collections.Generic;
using System.Linq;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	[TestedType(typeof(CMMMessageRetrieverServiceTask))]
	internal class CMMMessageRetrieverServiceTaskTest : ServiceTaskTestCase<CMMMessageRetrieverServiceTask>
	{
		[ExpectNoExceptions]
		public void TestOptions()
		{
			AssertEquals("DefaultScheduleRunEvery", "30minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestIsRegistered()
		{
			HostedServiceAttribute att = TestHelper.GetAttributeRegistring<CMMMessageRetrieverServiceTask>();
			AssertNotNull("Should have found an attribute", att);
			CombineAssertions(delegate
			{
				AssertEquals("Category", ServiceTaskConstants.Category, att.Category);
				AssertEquals("Code", CMMMessageRetrieverServiceTask.Code, att.Code);
				AssertEquals("Description", CMMMessageRetrieverServiceTask.Description, att.Description);
				AssertEquals("MinimumPeriod", "30minutes", att.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, att.CanRunInAnyBranch);
			});
		}

		public void TestProcessorType()
		{
			CMMMessageRetrieverServiceTask task = new CMMMessageRetrieverServiceTask();
			AssertType(typeof(CMMMessageRetriever), task.Processor);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[] { new TaskNudgeInformationForTest(MailDBItemsSchema.Constants.TableName, "CMM Message retrieval", MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued, MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Receive, MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.CMMMessage), };
			}
		}
	}
}
