using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowEventContextValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateMasterClassifier()
		{
			var eventContext = new WorkflowEventContext();
			eventContext.AvailableContextStepPairs = new[]
			{
				new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null)),
				new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T2", null)),
			};

			eventContext.Validation.ValidateMasterClassifier();
			AssertNoNotifications(eventContext.MasterClassifierInfo);

			eventContext.MasterClassifier = "C1";
			eventContext.Validation.ValidateMasterClassifier();
			AssertNoNotifications(eventContext.MasterClassifierInfo);

			eventContext.MasterClassifier = "XX";
			eventContext.Validation.ValidateMasterClassifier();
			AssertHasErrors(eventContext.MasterClassifierInfo);

			eventContext.MasterClassifier = "C2";
			eventContext.Validation.ValidateMasterClassifier();
			AssertNoNotifications(eventContext.MasterClassifierInfo);
		}

		public void TestValidateMasterType()
		{
			var eventContext = new WorkflowEventContext();
			eventContext.AvailableContextStepPairs = new[]
			{
				new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null)),
				new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T2", null)),
			};

			eventContext.Validation.ValidateMasterType();
			AssertHasErrors("Validate that MasterType has some value", eventContext.MasterTypeInfo);

			eventContext.MasterType = "T1";
			eventContext.Validation.ValidateMasterType();
			AssertNoNotifications(eventContext.MasterTypeInfo);

			eventContext.MasterType = "YY";
			eventContext.Validation.ValidateMasterType();
			AssertHasErrors(eventContext.MasterTypeInfo);

			eventContext.MasterType = "T2";
			eventContext.Validation.ValidateMasterType();
			AssertNoNotifications(eventContext.MasterTypeInfo);
		}
	}
}
