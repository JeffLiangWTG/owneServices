using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.eTail.Business.Testing
{
	public class CheckUserDefinedMacrosActionTest : PreScreeningActionTest
	{
		public void TestPreScreeningHVLVDetails_UserDefinedMacros()
		{
			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "User Defined";
			field1.ValidationRule = "ERR";
			field1.MessageText = "Test Macros Script - Contains";
			field1.MacrosScript = "\"<HVC_ConsigneeCity>\".Contains(\"Macros\")";

			var field2 = rule.Fields.AddNew();
			field2.FieldDescription = "User Defined";
			field2.ValidationRule = "ERR";
			field2.MessageText = "Test Macros Script - Greater than";
			field2.MacrosScript = "\"<HVC_ActualWeight>\" > 800";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("No pre-screening error message", ZString.Empty, TestConsignment.PreScreeningErrorDetails);

			TestConsignment.HVC_ConsigneeName = "Script";
			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("No pre-screening error message", ZString.Empty, TestConsignment.PreScreeningErrorDetails);

			TestConsignment.HVC_ActualWeight = 700.00m;
			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("No pre-screening error message", ZString.Empty, TestConsignment.PreScreeningErrorDetails);

			TestConsignment.HVC_ConsigneeCity = "Macros Script";
			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("Test Macros Script - Contains\r\n", TestConsignment.PreScreeningErrorDetails);

			TestConsignment.HVC_ActualWeight = 1000.00m;
			PerformPreScreeningForRule(TestConsignment);
			var expectedErrorDetails =
@"Test Macros Script - Contains
Test Macros Script - Greater than
";
			AssertContainsExactLinesInAnyOrder(expectedErrorDetails, TestConsignment.PreScreeningErrorDetails);
		}

		public void TestPreScreeningHVLVDetails_UserDefinedMacrosException()
		{
			var fieldException = rule.Fields.AddNew();
			fieldException.FieldDescription = "User Defined";
			fieldException.ValidationRule = "ERR";
			fieldException.MessageText = "Test Mascro Script - Greater than";
			fieldException.MacrosScript = "'(\"<HVC_GoodsDescription>\")>' == '1'";
			const string expectedExceptionMessage = "Error evaluating '(\"Women's trousers of cotton\")>' == '1'\r\n";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			TestConsignment.HVC_GoodsDescription = "Women's trousers of cotton";
			PerformPreScreeningForRule(TestConsignment);

			AssertNullOrEmpty("No PreScreeningErrorDetails", TestConsignment.PreScreeningErrorDetails);
			AssertNullOrEmpty("No PreScreeningWarningDetails", TestConsignment.PreScreeningWarningDetails);
			AssertEquals("PreScreeningNotifyOnlyWarningDetails", expectedExceptionMessage, TestConsignment.PreScreeningNotifyOnlyWarningDetails);
			TestConsignment.Validation.ValidateHVC_PreScreeningStatus();
			AssertHasWarning(TestConsignment.HVC_PreScreeningStatusInfo, expectedExceptionMessage);
		}

		public void TestPreScreeningHVLVDetails_UserDefinedMacros_NoExceptionThrow_IfMacroEvaluatingFailed()
		{
			var field = rule.Fields.AddNew();
			field.FieldDescription = "User Defined";
			field.ValidationRule = "ERR";
			field.MessageText = "Test Mascro Script";
			field.MacrosScript = "'(\"<HVC_GoodsDescription>\")>' == '1'";

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			TestConsignment.HVC_GoodsDescription = "Women's trousers of cotton";

			AssertNoExceptionThrown(() => { PerformPreScreeningForRule(TestConsignment); });
		}

		protected override PreScreeningAction GetPreScreeningAction(HVLVPreScreeningField field, HVLVConsignment consginment)
		{
			Assert("Precondition: field is Macros field", field.IsMacrosField);
			return new CheckUserDefinedMacrosAction(field, consginment);
		}
	}
}
