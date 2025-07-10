using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(WorkflowEventContext))]
	sealed class WorkflowEventContextTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWorkflowEventContext()
		{
			var eventContext = new WorkflowEventContext();

			AssertEquals(ZString.Empty, eventContext.MasterClassifier);
			AssertEquals(ZString.Empty, eventContext.MasterType);
			AssertEquals(0, eventContext.AvailableContextStepPairs.Length);

			var contextPair = new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null));
			eventContext = new WorkflowEventContext(contextPair);

			AssertEquals("C1", eventContext.MasterClassifier);
			AssertEquals("T1", eventContext.MasterType);
			AssertEquals(1, eventContext.AvailableContextStepPairs.Length);
			AssertEquals(contextPair, eventContext.AvailableContextStepPairs[0]);
		}

		public void TestMasterClassifiers()
		{
			var eventContext = new WorkflowEventContext();
			eventContext.AvailableContextStepPairs = new[]
			{
				new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null)),
				new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T2", null)),
				new WorkflowEventContextPair(new CodeDescriptionPair("C3", null), new CodeDescriptionPair("T1", null)),
			};

			AssertEquals(3, eventContext.MasterClassifiers.Count);
			AssertEquals("C1", eventContext.MasterClassifiers[0].Code);
			AssertEquals("C2", eventContext.MasterClassifiers[1].Code);
			AssertEquals("C3", eventContext.MasterClassifiers[2].Code);

			eventContext.MasterType = "T1";
			AssertEquals(2, eventContext.MasterClassifiers.Count);
			AssertEquals("C1", eventContext.MasterClassifiers[0].Code);
			AssertEquals("C3", eventContext.MasterClassifiers[1].Code);

			eventContext.MasterType = "T2";
			AssertEquals(1, eventContext.MasterClassifiers.Count);
			AssertEquals("C2", eventContext.MasterClassifiers[0].Code);
		}

		public void TestMasterClassifierDescription()
		{
			var eventContext = new WorkflowEventContext();
			eventContext.AvailableContextStepPairs = new[]
			{
				new WorkflowEventContextPair(new CodeDescriptionPair("C1", "CCC 111"), new CodeDescriptionPair("T1", "TTT 111")),
				new WorkflowEventContextPair(new CodeDescriptionPair("C2", "CCC 222"), new CodeDescriptionPair("T2", "TTT 222")),
			};

			eventContext.MasterClassifier = "C1";
			AssertEquals("CCC 111", eventContext.MasterClassifierDescription);

			eventContext.MasterClassifier = "C2";
			AssertEquals("CCC 222", eventContext.MasterClassifierDescription);

			eventContext.MasterClassifier = "XX";
			AssertEquals(ZString.Empty, eventContext.MasterClassifierDescription);
		}

		public void TestMasterTypes()
		{
			var eventContext = new WorkflowEventContext();
			eventContext.AvailableContextStepPairs = new[]
			{
				new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null)),
				new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T2", null)),
				new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T3", null)),
			};

			AssertEquals(3, eventContext.MasterTypes.Count);
			AssertEquals("T1", eventContext.MasterTypes[0].Code);
			AssertEquals("T2", eventContext.MasterTypes[1].Code);
			AssertEquals("T3", eventContext.MasterTypes[2].Code);

			eventContext.MasterClassifier = "C1";
			AssertEquals(2, eventContext.MasterTypes.Count);
			AssertEquals("T1", eventContext.MasterTypes[0].Code);
			AssertEquals("T3", eventContext.MasterTypes[1].Code);

			eventContext.MasterClassifier = "C2";
			AssertEquals(1, eventContext.MasterTypes.Count);
			AssertEquals("T2", eventContext.MasterTypes[0].Code);
		}

		public void TestMasterTypeDescription()
		{
			var eventContext = new WorkflowEventContext();
			eventContext.AvailableContextStepPairs = new[]
			{
				new WorkflowEventContextPair(new CodeDescriptionPair("C1", "CCC 111"), new CodeDescriptionPair("T1", "TTT 111")),
				new WorkflowEventContextPair(new CodeDescriptionPair("C2", "CCC 222"), new CodeDescriptionPair("T2", "TTT 222")),
			};

			eventContext.MasterType = "T1";
			AssertEquals("TTT 111", eventContext.MasterTypeDescription);

			eventContext.MasterType = "T2";
			AssertEquals("TTT 222", eventContext.MasterTypeDescription);

			eventContext.MasterType = "YY";
			AssertEquals(ZString.Empty, eventContext.MasterTypeDescription);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(WorkflowEventContextValidation), new WorkflowEventContext().Validation.GetType());
		}

		public void TestToWorkflowEventContextPair()
		{
			var pair1 = new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null));
			var pair2 = new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T2", null));
			var eventContext = new WorkflowEventContext { AvailableContextStepPairs = new[] { pair1, pair2 } };

			eventContext.MasterClassifier = "C1";
			eventContext.MasterType = "T1";
			AssertEquals(pair1, eventContext.ToWorkflowEventContextPair());

			eventContext.MasterClassifier = "C2";
			eventContext.MasterType = "T2";
			AssertEquals(pair2, eventContext.ToWorkflowEventContextPair());

			eventContext.MasterClassifier = "XX";
			eventContext.MasterType = "YY";
			var contextPair = eventContext.ToWorkflowEventContextPair();
			AssertNotNull(contextPair);
			AssertEquals("XX", contextPair.MasterClassifier.Code);
			AssertEquals("XX", contextPair.MasterClassifier.Description);
			AssertEquals("YY", contextPair.MasterType.Code);
			AssertEquals("YY", contextPair.MasterType.Description);
		}
	}
}
