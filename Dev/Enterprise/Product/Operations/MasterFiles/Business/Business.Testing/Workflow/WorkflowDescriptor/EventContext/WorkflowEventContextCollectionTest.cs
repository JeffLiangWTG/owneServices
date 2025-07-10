using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(WorkflowEventContextCollection))]
	sealed class WorkflowEventContextCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WorkflowEventContextCollection>
	{
		public void TestLoad()
		{
			var contextPair1 = new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null));
			var contextPair2 = new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T2", null));
			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest = new[] { contextPair1, contextPair2 };

			var collection = GetNewCollection();
			collection.Load(new[] { contextPair1, contextPair2, contextPair1 });

			AssertEquals(3, collection.Count);

			AssertEquals("C1", collection[0].MasterClassifier);
			AssertEquals("T1", collection[0].MasterType);
			AssertEquals(contextPair1, collection[0].ToWorkflowEventContextPair());

			AssertEquals("C2", collection[1].MasterClassifier);
			AssertEquals("T2", collection[1].MasterType);
			AssertEquals(contextPair2, collection[1].ToWorkflowEventContextPair());

			AssertEquals("C1", collection[2].MasterClassifier);
			AssertEquals("T1", collection[2].MasterType);
			AssertEquals(contextPair1, collection[2].ToWorkflowEventContextPair());
		}

		public void TestRefreshAll()
		{
			var contextPair1 = new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null));
			var contextPair2 = new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T2", null));
			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest = new[] { contextPair1, contextPair2 };

			var collection = GetNewCollection();
			collection.Load(new[] { contextPair1, contextPair2, contextPair1, contextPair2 });
			ValidateContextPropertiesInCollection(collection);

			AssertNoErrors(collection[0]);
			AssertNoErrors(collection[1]);
			AssertNoErrors(collection[2]);
			AssertNoErrors(collection[3]);

			collection.RefreshAll();
			ValidateContextPropertiesInCollection(collection);

			AssertNoErrors(collection[0]);
			AssertNoErrors(collection[1]);
			AssertNoErrors(collection[2]);
			AssertNoErrors(collection[3]);

			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest = new[] { contextPair1 };
			collection.RefreshAll();
			ValidateContextPropertiesInCollection(collection);

			AssertNoErrors(collection[0]);
			AssertHasErrors(collection[1].MasterClassifierInfo);
			AssertHasErrors(collection[1].MasterTypeInfo);
			AssertNoErrors(collection[2]);
			AssertHasErrors(collection[3].MasterClassifierInfo);
			AssertHasErrors(collection[3].MasterTypeInfo);
		}

		void ValidateContextPropertiesInCollection(WorkflowEventContextCollection collection)
		{
			foreach (WorkflowEventContext eventContext in collection)
			{
				eventContext.Validation.ValidateMasterClassifier();
				eventContext.Validation.ValidateMasterType();
			}
		}

		public void TestElementsHooking()
		{
			var collection = GetNewCollection();
			int refreshCount = 0;
			var refreshHandler = new EventHandler((sender, e) => refreshCount++);
			collection.Refreshed += refreshHandler;

			try
			{
				AssertEquals(0, refreshCount);

				collection.AddNew();
				AssertEquals(1, refreshCount);

				collection.Add(new WorkflowEventContext());
				AssertEquals(2, refreshCount);

				collection[0].MasterClassifier = "XX";
				AssertEquals(3, refreshCount);

				collection[0].MasterType = "YY";
				AssertEquals(4, refreshCount);

				collection.Remove(collection[0]);
				AssertEquals(5, refreshCount);

				collection.RemoveAll();
				AssertEquals(6, refreshCount);
			}
			finally
			{
				collection.Refreshed += refreshHandler;
			}
		}

		public void TestGetPathUpToElement()
		{
			var contextPair1 = new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null));
			var contextPair2 = new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T2", null));
			var contextPair3 = new WorkflowEventContextPair(new CodeDescriptionPair("C3", null), new CodeDescriptionPair("T3", null));
			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest = new[] { contextPair1, contextPair2, contextPair3 };

			var collection = GetNewCollection();
			collection.Load(new[] { contextPair1, contextPair2, contextPair3 });

			var contextPath = collection.GetPathUpToElement(collection[0]);
			AssertEquals(0, contextPath.Count());

			contextPath = collection.GetPathUpToElement(collection[1]);
			AssertEquals(1, contextPath.Count());
			AssertEquals(contextPair1, contextPath.ElementAt(0));

			contextPath = collection.GetPathUpToElement(collection[2]);
			AssertEquals(2, contextPath.Count());
			AssertEquals(contextPair1, contextPath.ElementAt(0));
			AssertEquals(contextPair2, contextPath.ElementAt(1));

			contextPath = collection.GetPathUpToElement(null);
			AssertEquals(3, contextPath.Count());
			AssertEquals(contextPair1, contextPath.ElementAt(0));
			AssertEquals(contextPair2, contextPath.ElementAt(1));
			AssertEquals(contextPair3, contextPath.ElementAt(2));
		}

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WorkflowEventContextCollection);
		}

		WorkflowEventContextCollection GetNewCollection()
		{
			return new WorkflowEventContextCollection(DummyWorkflowDescriptor.Instance);
		}

		protected override WorkflowEventContextCollection GetCollectionToTest()
		{
			return GetNewCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WorkflowEventContext(new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("C2", null)));
		}

		#endregion
	}
}
