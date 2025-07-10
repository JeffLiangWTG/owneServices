using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	internal class ProcessFieldChangeRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPFR_ProcessType()
		{
			var row = Factory.New<ProcessFieldChangeRule>();
			row.Validation.ValidatePFR_ProcessType();
			AssertHasError(row.PFR_ProcessTypeInfo, "Please enter a Workflow Type.");
			row.PFR_ProcessType = "XXY";
			AssertHasError(row.PFR_ProcessTypeInfo, "Enter a valid Workflow Type.");
		}

		public void TestPFR_GroupName()
		{
			var row = Factory.New<ProcessFieldChangeRule>();
			row.Validation.ValidatePFR_GroupName();
			AssertHasError(row.PFR_GroupNameInfo, "Please enter a Name.");
			row.PFR_GroupName = "TEST";

			var row2 = Factory.New<ProcessFieldChangeRule>();
			row2.PFR_GroupName = "test";
			AssertHasError(row2.PFR_GroupNameInfo, "test is already used for a field change event name.");
		}

		public void TestPFR_SE_NKEvent()
		{
			var row = Factory.New<ProcessFieldChangeRule>();
			row.Validation.ValidatePFR_SE_NKEvent();
			AssertHasError(row.PFR_SE_NKEventInfo, "Please enter an Event Code.");
			row.PFR_SE_NKEvent = "ZZZ";
			AssertHasError(row.PFR_SE_NKEventInfo, "Enter a valid Event Code.");
			row.PFR_SE_NKEvent = "Z00";
		}

		public void TestTypeEventReferenceIsUnique()
		{
			var row = Factory.New<ProcessFieldChangeRule>();
			row.PFR_ProcessType = "SHP";
			row.PFR_SE_NKEvent = "Z00";
			row.PFR_Reference = "BLAH";

			var row2 = Factory.New<ProcessFieldChangeRule>();
			row2.PFR_ProcessType = "SHP";
			row2.PFR_SE_NKEvent = "Z00";
			row2.PFR_Reference = "BLAH";

			row2.Validation.ValidatePFR_SE_NKEvent();
			AssertHasError(row2.PFR_SE_NKEventInfo, "Workflow Type, Event Code and Reference must be unique on Field Change Rule. The duplicate values are (SHP,Z00,BLAH)");

			row2.PFR_Reference = "BLAH                    ";
			AssertHasError(row2.PFR_SE_NKEventInfo, "Workflow Type, Event Code and Reference must be unique on Field Change Rule. The duplicate values are (SHP,Z00,BLAH)");

			row2.PFR_Reference = "BLAH BLAH";
			AssertNoErrors(row2.PFR_SE_NKEventInfo);

			row.PFR_IsActive = false;
			row2.PFR_Reference = "BLAH";
			AssertNoErrors(row2.PFR_SE_NKEventInfo);
		}

		public void TestPFR_Description()
		{
			var row = Factory.New<ProcessFieldChangeRule>();
			row.Validation.ValidatePFR_Description();
			AssertNoNotifications(row.PFR_DescriptionInfo);
		}

		public void TestFields()
		{
			var row = Factory.New<ProcessFieldChangeRule>();
			row.PFR_GroupName = "TEST";
			row.Validation.ValidateAll();
			AssertHasError(row.PFR_GroupNameInfo, "This field change event must contain at least 1 field.");
		}
	}
}
