using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business.Testing
{
	class CheckItemLineTariffActionTest : PreScreeningActionTest
	{
		public void TestPreScreeningHVLVDetails_ItemLineTariff()
		{
			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "Origin HS Code";
			field1.ValidationRule = "ERR";

			var screeningValue1 = field1.ScreeningValues.AddNew();
			screeningValue1.FromHSCode = "1234.56.78";

			var field2 = rule.Fields.AddNew();
			field2.FieldDescription = "Destination HS Code";
			field2.ValidationRule = "ERR";

			var screeningValue2 = field2.ScreeningValues.AddNew();
			screeningValue2.FromHSCode = "1234.56.78";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var item = TestConsignment.Items.AddNew();
			var line = item.Lines.AddNew();
			line.HVS_FormattedOriginTariff = "1234.56.78";
			line.HVS_FormattedDestinationTariff = "1234.56.78";

			PerformPreScreeningForRule(TestConsignment);

			var expectedErrorDetails =
$@"{DataBoundResourceStrings.GetDataForProperty(line.HVS_FormattedDestinationTariffInfo).Caption} Screening Error -  has been listed for screening.
{DataBoundResourceStrings.GetDataForProperty(line.HVS_FormattedOriginTariffInfo).Caption} Screening Error -  has been listed for screening.
";
			AssertContainsExactLinesInAnyOrder(expectedErrorDetails, TestConsignment.PreScreeningErrorDetails.ToString());
		}

		protected override PreScreeningAction GetPreScreeningAction(HVLVPreScreeningField field, HVLVConsignment consginment)
		{
			Assert("Precondition: field is item line tariff field",
				(field.FieldName == HVLVItemLineSchema.Constants.HVS_OriginTariff || field.FieldName == HVLVItemLineSchema.Constants.HVS_DestinationTariff)
				&& !field.IsSpecialCharactersField
				&& !field.IsDeminimusField
				&& !field.IsMacrosField);
			return new CheckItemLineTariffAction(field, consginment);
		}
	}
}
