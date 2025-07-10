using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.eTail.Business.Testing
{
	public abstract class PreScreeningActionTest : TestCaseWithFactory
	{
		protected HVLVPreScreeningRule rule;

		protected HVLVConsignment TestConsignment;

		protected override void SetUp()
		{
			base.SetUp();

			rule = new HVLVPreScreeningRule();
			rule.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader;

			TestConsignment = Factory.New<HVLVConsignment>();
		}

		protected HVLVConsignmentPreScreeningResult PerformPreScreeningForRule(HVLVConsignment consignment)
		{
			consignment.ClearPreScreeningDetails();
			var result = new HVLVConsignmentPreScreeningResult(consignment);

			foreach (HVLVPreScreeningField field in rule.Fields)
			{
				var action = GetPreScreeningAction(field, consignment);
				action.PerformPreScreening(result);
			}

			return result;
		}

		protected abstract PreScreeningAction GetPreScreeningAction(HVLVPreScreeningField field, HVLVConsignment consginment);
	}

	class PreScreeningActionBaseOnlyTest : PreScreeningActionTest
	{
		public void TestAddPreScreeningDetailsCore_Error()
		{
			var field = rule.Fields.AddNew();
			field.ValidationRule = HVLVPreScreeningField.ValidationRuleCodes.Error;

			var action = GetPreScreeningAction(field, TestConsignment);

			var result = new HVLVConsignmentPreScreeningResult(TestConsignment);
			action.PerformPreScreening(result);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("TestConsignment.PreScreeningNotifyOnlyWarningDetails", TestConsignment.PreScreeningNotifyOnlyWarningDetails);
				AssertNullOrEmpty("TestConsignment.PreScreeningWarningDetails", TestConsignment.PreScreeningWarningDetails);
				AssertEquals("TestConsignment.PreScreeningErrorDetails", "This is the result message", TestConsignment.PreScreeningErrorDetails.Trim());

				AssertNullOrEmpty("result.FormattedNotifyOnlyWarningMessage", result.FormattedNotifyOnlyWarningMessage);
				AssertNullOrEmpty("result.FormattedWarningMessage", result.FormattedWarningMessage);
				AssertEquals("result.FormattedErrorMessage", "This is the result message", result.FormattedErrorMessage);
			});
		}

		public void TestAddPreScreeningDetailsCore_Warning()
		{
			var field = rule.Fields.AddNew();
			field.ValidationRule = HVLVPreScreeningField.ValidationRuleCodes.Warning;

			var action = GetPreScreeningAction(field, TestConsignment);

			var result = new HVLVConsignmentPreScreeningResult(TestConsignment);
			action.PerformPreScreening(result);
			CombineAssertions(() =>
			{
				AssertNullOrEmpty("TestConsignment.PreScreeningNotifyOnlyWarningDetails", TestConsignment.PreScreeningNotifyOnlyWarningDetails);
				AssertEquals("TestConsignment.PreScreeningWarningDetails", "This is the result message", TestConsignment.PreScreeningWarningDetails.Trim());
				AssertNullOrEmpty("TestConsignment.PreScreeningErrorDetails", TestConsignment.PreScreeningErrorDetails);

				AssertNullOrEmpty("result.FormattedNotifyOnlyWarningMessage", result.FormattedNotifyOnlyWarningMessage);
				AssertEquals("result.FormattedWarningMessage", "This is the result message", result.FormattedWarningMessage);
				AssertNullOrEmpty("result.FormattedErrorMessage", result.FormattedErrorMessage);
			});
		}

		public void TestAddPreScreeningDetailsCore_None()
		{
			var field = rule.Fields.AddNew();
			field.ValidationRule = HVLVPreScreeningField.ValidationRuleCodes.Non;

			var action = GetPreScreeningAction(field, TestConsignment);

			var result = new HVLVConsignmentPreScreeningResult(TestConsignment);
			action.PerformPreScreening(result);

			CombineAssertions(() => {
				AssertNullOrEmpty("TestConsignment.PreScreeningNotifyOnlyWarningDetails", TestConsignment.PreScreeningNotifyOnlyWarningDetails.Trim());
				AssertNullOrEmpty("TestConsignment.PreScreeningWarningDetails", TestConsignment.PreScreeningWarningDetails);
				AssertNullOrEmpty("TestConsignment.PreScreeningErrorDetails", TestConsignment.PreScreeningErrorDetails);

				AssertNullOrEmpty("result.FormattedNotifyOnlyWarningMessage", result.FormattedNotifyOnlyWarningMessage);
				AssertNullOrEmpty("result.FormattedWarningMessage", result.FormattedWarningMessage);
				AssertNullOrEmpty("result.FormattedErrorMessage", result.FormattedErrorMessage);
			});
		}

		public void TestAddPreScreeningDetailsCore_UsesOverrideMessageLevel()
		{
			var field = rule.Fields.AddNew();
			field.ValidationRule = HVLVPreScreeningField.ValidationRuleCodes.Error;

			var action = (PreScreeningActionForTest)GetPreScreeningAction(field, TestConsignment);
			action.OverrideMessageLevel = PreScreeningAction.MessageLevel.NotifyOnlyWarning;

			var result = new HVLVConsignmentPreScreeningResult(TestConsignment);
			action.PerformPreScreening(result);

			CombineAssertions(() =>
			{
				AssertEquals("TestConsignment.PreScreeningNotifyOnlyWarningDetails", "This is the result message", TestConsignment.PreScreeningNotifyOnlyWarningDetails.Trim());
				AssertNullOrEmpty("TestConsignment.PreScreeningWarningDetails", TestConsignment.PreScreeningWarningDetails);
				AssertNullOrEmpty("TestConsignment.PreScreeningErrorDetails", TestConsignment.PreScreeningErrorDetails);

				AssertEquals("result.FormattedNotifyOnlyWarningMessage", "This is the result message", result.FormattedNotifyOnlyWarningMessage);
				AssertNullOrEmpty("result.FormattedWarningMessage", result.FormattedWarningMessage);
				AssertNullOrEmpty("result.FormattedErrorMessage", result.FormattedErrorMessage);
			});
		}

		protected override PreScreeningAction GetPreScreeningAction(HVLVPreScreeningField field, HVLVConsignment consginment)
		{
			return new PreScreeningActionForTest(field, consginment);
		}
	}

	class PreScreeningActionForTest : PreScreeningAction
	{
		public PreScreeningActionForTest(HVLVPreScreeningField field, HVLVConsignment consignment) : base(field, consignment)
		{
		}

		public string ResultMessage = "This is the result message";

		public MessageLevel? OverrideMessageLevel
		{
			get => overrideMessageLevel;
			set => overrideMessageLevel = value;
		}
		MessageLevel? overrideMessageLevel;

		protected override void PerformPreScreeningAndPopulateResult(HVLVConsignmentPreScreeningResult result)
		{
			AddPreScreeningDetailsCore(ResultMessage, result, overrideMessageLevel);
		}
	}
}
