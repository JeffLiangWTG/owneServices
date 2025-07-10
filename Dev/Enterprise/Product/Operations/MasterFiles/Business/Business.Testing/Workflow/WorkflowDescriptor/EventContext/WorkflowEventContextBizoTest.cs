using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(WorkflowEventContextBizo))]
	sealed class WorkflowEventContextBizoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContextCollection()
		{
			var bizo = GetNewWorkflowEventContextBizo();

			var collection = bizo.ContextCollection;
			AssertNotNull(collection);
			Assert(bizo.IsRegisteredEditableChildObject(collection));
			AssertSame(collection, bizo.ContextCollection);
		}

		public void TestContextPathCode()
		{
			var contextPair1 = new WorkflowEventContextPair(new CodeDescriptionPair("C1", "CCC 111"), new CodeDescriptionPair("T1", "TTT 111"));
			var contextPair2 = new WorkflowEventContextPair(new CodeDescriptionPair("C2", "CCC 222"), new CodeDescriptionPair("T2", "TTT 222"));
			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest = new[] { contextPair1, contextPair2 };

			var bizo = GetNewWorkflowEventContextBizo();
			bizo.ContextCollection.Load(new[] { contextPair1, contextPair2, contextPair1 });

			AssertEquals("C1 T1,C2 T2,C1 T1", bizo.ContextPathCode);
		}

		public void TestContextPathDescriptive()
		{
			var contextPair1 = new WorkflowEventContextPair(new CodeDescriptionPair("C1", "CCC 111"), new CodeDescriptionPair("T1", "TTT 111"));
			var contextPair2 = new WorkflowEventContextPair(new CodeDescriptionPair("C2", "CCC 222"), new CodeDescriptionPair("T2", "TTT 222"));
			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest = new[] { contextPair1, contextPair2 };

			var bizo = GetNewWorkflowEventContextBizo();
			bizo.ContextCollection.Load(new[] { contextPair1, contextPair2, contextPair1 });

			AssertEquals("CCC 111 TTT 111,CCC 222 TTT 222,CCC 111 TTT 111", bizo.ContextPathDescriptive);
		}

		#region Implementation

		WorkflowEventContextBizo GetNewWorkflowEventContextBizo()
		{
			return new WorkflowEventContextBizo(DummyWorkflowDescriptor.Instance);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewWorkflowEventContextBizo();
		}

		#endregion
	}
}
