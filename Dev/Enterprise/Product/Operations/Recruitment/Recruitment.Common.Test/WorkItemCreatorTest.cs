using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Registry;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Registry
{
	sealed class WorkItemCreatorTest : TransactionedTestCase
	{
		public void TestCreateWorkItem()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var template = new WorkItemTemplateProperties();
			template.FriendlyName = "hello world";

			var wiToUseAsTemplate = factory.NewWithValidTestData<WorkItem>();
			wiToUseAsTemplate.WKI_WorkItemType = "ABC";
			wiToUseAsTemplate.WKI_WorkItemArea = "DEF";
			wiToUseAsTemplate.WKI_ActivityType = "GHI";
			wiToUseAsTemplate.WKI_ActivitySubtype = "JKL";
			wiToUseAsTemplate.WKI_Priority = "MNO";
			factory.Save();

			template.WKI_PK = wiToUseAsTemplate.PK;

			// act
			var wi = WorkItemCreator.CreateWorkItem(factory, template, "foo bar");

			// assert
			AssertNotNull(wi);

			AssertEquals(wiToUseAsTemplate.WKI_WorkItemType, wi.WKI_WorkItemType);
			AssertEquals(wiToUseAsTemplate.WKI_WorkItemArea, wi.WKI_WorkItemArea);
			AssertEquals(wiToUseAsTemplate.WKI_ActivityType, wi.WKI_ActivityType);
			AssertEquals(wiToUseAsTemplate.WKI_ActivitySubtype, wi.WKI_ActivitySubtype);
			AssertEquals(wiToUseAsTemplate.WKI_Priority, wi.WKI_Priority);
			AssertEquals("hello world - foo bar", wi.WKI_Summary);
		}

		public void TestCreateWorkItemNullTemplate()
		{
			// arrange
			var factory = new BusinessObjectFactory();

			// act
			var wi = WorkItemCreator.CreateWorkItem(factory, null);

			// assert
			AssertNotNull(wi);
		}
	}
}
