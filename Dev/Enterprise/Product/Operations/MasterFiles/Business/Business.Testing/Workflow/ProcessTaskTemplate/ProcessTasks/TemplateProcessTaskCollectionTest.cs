using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TemplateProcessTaskCollection))]
	sealed class TemplateProcessTaskCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultSort()
		{
			ProcessTask task1 = Template.WorkflowItems.AddNew();
			ProcessTask milestone1 = Template.WorkflowItems.AddNew();
			ProcessTask task2 = Template.WorkflowItems.AddNew();
			ProcessTask milestone2 = Template.WorkflowItems.AddNew();
			milestone1.IsMilestone = true;
			milestone2.IsMilestone = true;

			Template.WorkflowItems.Load();
			AssertEquals("Tasks listed first", task1.PK, Template.WorkflowItems[0].PK);
			AssertEquals("Tasks listed first", task2.PK, Template.WorkflowItems[1].PK);
			AssertEquals("Milestones listed last", milestone1.PK, Template.WorkflowItems[2].PK);
			AssertEquals("Milestones listed last", milestone2.PK, Template.WorkflowItems[3].PK);
		}

		#region Implementation

		ProcessTaskTemplate Template
		{
			get { return template ?? (template = Factory.New<ProcessTaskTemplate>()); }
		}
		ProcessTaskTemplate template;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				return new TemplateProcessTaskCollection(Template);
			}
		}

		#endregion
	}
}
