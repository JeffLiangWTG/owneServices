using System.Collections.Generic;
using System.Linq;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	[TestedType(typeof(EIDOMessageRetrieverServiceTask))]
	internal class EIDOMessageRetrieverServiceTaskTest : ServiceTaskTestCase<EIDOMessageRetrieverServiceTask>
	{
		[ExpectNoExceptions]
		public void TestOptions()
		{
			AssertEquals("DefaultScheduleRunEvery", "30minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestIsRegistered()
		{
			HostedServiceAttribute att = TestHelper.GetAttributeRegistring<EIDOMessageRetrieverServiceTask>();
			AssertNotNull("Should have found an attribute", att);
			CombineAssertions(delegate
			{
				AssertEquals("Category", ServiceTaskConstants.Category, att.Category);
				AssertEquals("Code", EIDOMessageRetrieverServiceTask.Code, att.Code);
				AssertEquals("Description", EIDOMessageRetrieverServiceTask.Description, att.Description);
				AssertEquals("CanRunInAnyBranch", true, att.CanRunInAnyBranch);
			});
		}

		public void TestProcessorType()
		{
			EIDOMessageRetrieverServiceTask task = new EIDOMessageRetrieverServiceTask();
			AssertType(typeof(EIDOMessageRetriever), task.Processor);
		}

		public void TestHostedServiceBusinessObjectBindingAttribute()
		{
			var matchedAttributes = AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>().Where(x => x.ServiceTaskCode == EIDOMessageRetrieverServiceTask.Code && x.Table == MailDBItemsSchema.Constants.TableName);
			AssertEquals(1, matchedAttributes.Count());
			var matchedAttribute = matchedAttributes.FirstOrDefault();
			AssertEquals("MI_Status=QUE", matchedAttribute.Predicates[0]);
			AssertEquals("MI_Direction=RCV", matchedAttribute.Predicates[1]);
			AssertEquals("MI_Application=EID", matchedAttribute.Predicates[2]);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("30minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[] { new TaskNudgeInformationForTest(MailDBItemsSchema.Constants.TableName, "EIDO Message retrieval", MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued, MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Receive, MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.EIDOMessageRetriever), };
			}
		}
	}
}
