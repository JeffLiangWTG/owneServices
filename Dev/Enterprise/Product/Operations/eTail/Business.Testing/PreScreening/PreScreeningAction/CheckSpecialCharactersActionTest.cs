using Enterprise.Registry.Business;

namespace Enterprise.eTail.Business.Testing
{
	class CheckSpecialCharactersActionTest : PreScreeningActionTest
	{
		public void TestPreScreeningHVLVDetails_SpecialCharacters()
		{
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Special Characters";
			field.ValidationRule = "ERR";

			var specialCharacter = field.SpecialCharacters.AddNew();
			specialCharacter.CharacterValue = "Carriage Return";

			TestConsignment.HVC_GoodsDescription = "AAABB";
			TestConsignment.HVC_ConsigneeInstructions = "BBBAA";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("", TestConsignment.PreScreeningErrorDetails.ToString());

			TestConsignment.HVC_GoodsDescription = @"AAA
BB";
			TestConsignment.HVC_ConsigneeInstructions = @"BBB
AA";

			PerformPreScreeningForRule(TestConsignment);

			var expectedErrorDetails =
$@"Goods Description Screening Error - Carriage Return has been listed for screening.
Consignee Instructions Screening Error - Carriage Return has been listed for screening.
";

			AssertContainsExactLinesInAnyOrder(expectedErrorDetails, TestConsignment.PreScreeningErrorDetails);
		}

		protected override PreScreeningAction GetPreScreeningAction(HVLVPreScreeningField field, HVLVConsignment consginment)
		{
			Assert("Precondition: field is Special Characters field", field.IsSpecialCharactersField);
			return new CheckSpecialCharactersAction(field, consginment);
		}
	}
}
