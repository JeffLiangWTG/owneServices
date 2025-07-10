using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	public abstract class WorkTaskRelatedItemCollectionTestCase<T> : BusinessObjectCollectionTestCase where T : BusinessObject, IWorkTaskRelatedItem
	{
		public abstract void TestShouldAddToCollection();
	}

	[TestedType(typeof(WorkTaskRelatedItemGenPivotCollection<Project>))]
	public class WorkTaskRelatedItemCollectionGenericTest : WorkTaskRelatedItemCollectionTestCase<WorkItem>
	{
		public override void TestShouldAddToCollection()
		{
			Project relatedItem = Factory.NewWithValidTestData<Project>();
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();

			AssertEquals(0, Collection.Count);

			Collection.Add(relatedItem);
			Collection.Add(workItem);

			AssertEquals("Should contain only", 1, Collection.Count);
			Assert("Should have", Collection.Contains(relatedItem));
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WorkTaskRelatedItemGenPivotCollection<Project>);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WorkTaskRelatedItemGenPivotCollection<Project>(Factory.NewWithValidTestData<Project>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<Project>();
		}
	}

	[TestedType(typeof(WorkTaskRelatedItemGenPivotCollection))]
	public class WorkTaskRelatedItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetTypeOfElementsFromPK()
		{
			WorkItem workItem = Factory.New<WorkItem>();
			Project incident = Factory.New<Project>();
			workItem.RelatedItems.Add(incident);

			AssertEquals(typeof(IWorkTaskRelatedItem), workItem.RelatedItems.GetTypeOfElementsFromPK(incident.PK));
		}

		public void TestGetElements()
		{
			WorkItem workItem = Factory.New<WorkItem>();
			Project incident = Factory.New<Project>();
			workItem.RelatedItems.Add(incident);

			AssertEquals(1, workItem.RelatedItems.GetElements<Project>().Count());
			Assert(workItem.RelatedItems.GetElements<Project>().Contains(incident));
		}

		public void TestLoadCollection()
		{
			var workItem = Factory.New<WorkItem>();
			var project1 = Factory.New<Project>();
			var project2 = Factory.New<Project>();
			var project3 = Factory.New<Project>();

			workItem.RelatedItems.Add(project1);
			AssertEquals(1, project1.RelatedItems.Count);
			AssertCollectionContains(workItem, project1.RelatedItems);

			workItem.RelatedItems.Add(project2);
			AssertEquals(1, project2.RelatedItems.Count);
			AssertCollectionContains(workItem, project2.RelatedItems);

			workItem.RelatedItems.Remove(project1);
			AssertEquals(0, project1.RelatedItems.Count);
			AssertCollectionNotContains(workItem, project1.RelatedItems);

			workItem.RelatedItems.Remove(project2);
			AssertEquals(0, project2.RelatedItems.Count);
			AssertCollectionNotContains(workItem, project2.RelatedItems);

			project1.RelatedItems.Add(workItem);
			project2.RelatedItems.Add(workItem);
			AssertEquals(2, workItem.RelatedItems.Count);
			AssertCollectionContains(project1, workItem.RelatedItems);
			AssertCollectionContains(project2, workItem.RelatedItems);
			AssertCollectionNotContains(project3, workItem.RelatedItems);

			project1.RelatedItems.Remove(workItem);
			project3.RelatedItems.Add(workItem);
			AssertEquals(2, workItem.RelatedItems.Count);
			AssertCollectionNotContains(project1, workItem.RelatedItems);
			AssertCollectionContains(project2, workItem.RelatedItems);
			AssertCollectionContains(project3, workItem.RelatedItems);
		}

		public void TestAttachDetachWorkItemToProjectEvent()
		{
			var workitem = Factory.NewWithValidTestData<WorkItem>();
			workitem.WKI_WorkItemNumber = "WI00000001";
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_ProjectNumber = "PJ00000001";

			project.RelatedItems.Add(workitem);
			Factory.Save();

			var attachedEvent = workitem.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.Event.SE_Code == AutoEvents.Attached.Code);
			AssertEquals("Should have an attached event", true, attachedEvent != null);
			AssertEquals("WI00000001 attached to PJ00000001", attachedEvent.SL_Reference);

			project.RelatedItems.Remove(workitem);
			Factory.Save();

			var detachedEvent = workitem.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.Event.SE_Code == AutoEvents.Detached.Code);
			AssertEquals("Should have a detached event", true, detachedEvent != null);
			AssertEquals("WI00000001 detached from PJ00000001", detachedEvent.SL_Reference);
		}

		public void TestAttachDetachProjectToWorkItemEvent()
		{
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_ProjectNumber = "PJ00000001";
			var workitem = Factory.NewWithValidTestData<WorkItem>();
			workitem.WKI_WorkItemNumber = "WI00000001";

			workitem.RelatedItems.Add(project);
			Factory.Save();

			var attachedEvent = workitem.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.Event.SE_Code == AutoEvents.Attached.Code);
			AssertEquals("Should have an attached event", true, attachedEvent != null);
			AssertEquals("PJ00000001 attached to WI00000001", attachedEvent.SL_Reference);

			workitem.RelatedItems.Remove(project);
			Factory.Save();

			var detachedEvent = workitem.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.Event.SE_Code == AutoEvents.Detached.Code);
			AssertEquals("Should have a detached event", true, detachedEvent != null);
			AssertEquals("PJ00000001 detached from WI00000001", detachedEvent.SL_Reference);
		}

		public void TestAttachDetachRaisesRelatedItemEvent()
		{
			var workitem1 = Factory.NewWithValidTestData<WorkItem>();
			workitem1.WKI_WorkItemNumber = "WI00000001";
			var workitem2 = Factory.NewWithValidTestData<WorkItem>();
			workitem2.WKI_WorkItemNumber = "WI00000002";
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_ProjectNumber = "PJ00000001";

			var projectAddedEvents = new List<RelatedItemEventArgs>();
			var projectRemovedEvents = new List<RelatedItemEventArgs>();
			project.RelatedItems.RelatedItemAdded += (o, e) => { projectAddedEvents.Add(e); };
			project.RelatedItems.RelatedItemRemoved += (o, e) => { projectRemovedEvents.Add(e); };

			workitem1.RelatedItems.Add(project);
			AssertEquals("Removed event should not be raised for project yet", projectRemovedEvents.Count, 0);
			AssertEquals("Added event should not be raised for project yet", projectAddedEvents.Count, 0);

			project.RelatedItems.Add(workitem2);

			AssertEquals("Removed event should not be raised for project yet", projectRemovedEvents.Count, 0);
			AssertEquals("Added event should be raised for project", projectAddedEvents.Count, 1);
			AssertRelatedItemEvent("Project Added Event", projectAddedEvents[0], expectedBusinessObject: workitem2, expectedIsParent: false);

			project.RelatedItems.Remove(workitem1);
			project.RelatedItems.Remove(workitem2);

			AssertEquals("Removed events should be raised for project", projectRemovedEvents.Count, 2);
			AssertEquals("Added events should not have changed for project", projectAddedEvents.Count, 1);
			AssertRelatedItemEvent("Project Removed Event 1", projectRemovedEvents[0], expectedBusinessObject: workitem1, expectedIsParent: true);
			AssertRelatedItemEvent("Project Removed Event 2", projectRemovedEvents[1], expectedBusinessObject: workitem2, expectedIsParent: false);
		}

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WorkTaskRelatedItemGenPivotCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WorkTaskRelatedItemGenPivotCollection(Factory.NewWithValidTestData<Project>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<WorkItem>();
		}

		void AssertRelatedItemEvent(string message, RelatedItemEventArgs relatedItemEvent, BusinessObject expectedBusinessObject, bool expectedIsParent)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("BusinessObject", expectedBusinessObject, relatedItemEvent.BusinessObject);
				AssertEquals("IsParent", expectedIsParent, relatedItemEvent.IsParent);
			});
		}

		#endregion
	}
}
