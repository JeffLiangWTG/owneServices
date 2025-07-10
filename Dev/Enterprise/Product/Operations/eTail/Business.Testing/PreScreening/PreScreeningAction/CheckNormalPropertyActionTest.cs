using System.Text;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.eTail.Business.Testing
{
	class CheckNormalPropertyActionTest : PreScreeningActionTest
	{
		public void TestPreScreeningHVLVDetails_NormalProperty()
		{
			var fileDescriptionList = rule.Fields.AddNew().FieldDescriptionList;
			rule.Fields.RemoveAll();

			foreach (var fieldDescription in fileDescriptionList.GetAllCodes())
			{
				if (fieldDescription != "Goods Value"
					&& fieldDescription != "Goods Description"
					&& fieldDescription != "Origin HS Code"
					&& fieldDescription != "Destination HS Code"
					&& fieldDescription != "User Defined"
					&& fieldDescription != "Special Characters"
					&& fieldDescription != "Total Lines Customs Value"
					&& fieldDescription != "Total Lines Intrinsic Value")
				{
					var field = rule.Fields.AddNew();
					field.FieldDescription = fieldDescription;
					field.ValidationRule = "ERR";
					var screeningValue = field.ScreeningValues.AddNew();
					screeningValue.ScreeningValue = "AA";
				}
			}

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			foreach (HVLVPreScreeningField field in rule.Fields)
			{
				if (TestConsignment.GetPropertyType(field.FieldName) != null)
				{
					TestConsignment.GetType().GetProperty(field.FieldName)?.SetValue(TestConsignment, new ZString("AA"));
				}
			}

			PerformPreScreeningForRule(TestConsignment);

			var expectedErrorDetails = new StringBuilder();
			foreach (HVLVPreScreeningField field in rule.Fields)
			{
				expectedErrorDetails.AppendLine($"{field.FieldDescription} Screening Error - AA has been listed for screening.");
			}

			AssertContainsExactLinesInAnyOrder(expectedErrorDetails.ToString(), TestConsignment.PreScreeningErrorDetails.ToString());
		}

		public void TestHVLVPreScreeningNormalPropertyIsCaseInsensitive()
		{
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Consignee";
			field.ValidationRule = "ERR";
			var screeningValue = field.ScreeningValues.AddNew();

			var normalPropertyCases = new[] { "aa", "AA", "aA", "Aa" };
			var screeningComparisonOperators = screeningValue.ScreeningComparisonOperatorsList;

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			CombineAssertions("NormalPropertyCaseInsensitive", () =>
			{
				foreach (var screeningComparisonCode in screeningComparisonOperators.GetAllCodes())
				{
					screeningValue.ScreeningComparisonOperatorCode = screeningComparisonCode;
					foreach (var screeningValueCase in normalPropertyCases)
					{
						screeningValue.ScreeningValue = screeningValueCase;

						foreach (var fieldNameCase in normalPropertyCases)
						{
							TestConsignment.GetType().GetProperty(field.FieldName)?.SetValue(TestConsignment, new ZString(fieldNameCase));

							PerformPreScreeningForRule(TestConsignment);
							AssertEquals($"{screeningValueCase}: Field name {fieldNameCase} and Screening Value {screeningValueCase}", $"{field.FieldDescription} Screening Error - {screeningValueCase} has been listed for screening.\r\n", TestConsignment.PreScreeningErrorDetails);
						}
					}
				}
			});
		}

		public void TestPreScreeningHVLVDetails_Mandatory()
		{
			var item = TestConsignment.Items.AddNew();
			var itemLine = item.Lines.AddNew();

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			SetPreScreeningRule(false);
			PerformPreScreeningForRule(TestConsignment);
			Assert("no error after pre-screening when no mandatory field", TestConsignment.PreScreeningErrorDetails.IsEmpty);

			SetPreScreeningRule(true);
			PerformPreScreeningForRule(TestConsignment);

			var expectedErrorDetails = new StringBuilder();
			foreach (HVLVPreScreeningField field in rule.Fields)
			{
				expectedErrorDetails.AppendLine($"{field.FieldDescription} Screening Error - Please enter a value");
			}

			AssertContainsExactLinesInAnyOrder(expectedErrorDetails.ToString(), TestConsignment.PreScreeningErrorDetails.ToString());

			void SetPreScreeningRule(bool isMandatory)
			{
				var fileDescriptionList = rule.Fields.AddNew().FieldDescriptionList;
				rule.Fields.RemoveAll();

				foreach (var fieldDescription in fileDescriptionList.GetAllCodes())
				{
					if (fieldDescription != "Goods Value"
						&& fieldDescription != "User Defined"
						&& fieldDescription != "Special Characters"
						&& fieldDescription != "Total Lines Customs Value"
						&& fieldDescription != "Total Lines Intrinsic Value")
					{
						var field = rule.Fields.AddNew();
						field.FieldDescription = fieldDescription;
						field.ValidationRule = "ERR";
						field.IsMandatory = isMandatory;
					}
				}
			}
		}

		public void TestHVLVPreScreeningNotificationMessageTextFallBack()
		{
			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "Goods Description";
			field1.ValidationRule = "ERR";
			field1.MessageText = "Error Message On Field Level";

			var field2 = rule.Fields.AddNew();
			field2.FieldDescription = "Consignee";
			field2.ValidationRule = "ERR";

			var screeningValue1 = field1.ScreeningValues.AddNew();
			screeningValue1.ScreeningValue = "XBOX";
			screeningValue1.ScreeningComparisonOperatorCode = "Contains";
			screeningValue1.MessageTextPerValue = "Error Message On Value Level - XBOX";

			var screeningValue2 = field1.ScreeningValues.AddNew();
			screeningValue2.ScreeningValue = "PlayStation";
			screeningValue2.ScreeningComparisonOperatorCode = "Contains";
			screeningValue2.MessageTextPerValue = "Error Message On Value Level - PlayStation";

			var screeningValue3 = field1.ScreeningValues.AddNew();
			screeningValue3.ScreeningComparisonOperatorCode = "Contains";
			screeningValue3.ScreeningValue = "SWITCH";

			var screeningValue4 = field2.ScreeningValues.AddNew();
			screeningValue4.ScreeningComparisonOperatorCode = "Contains";
			screeningValue4.ScreeningValue = "SHAWN";

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_GoodsDescription = "I HAVE ONE XBOX";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_GoodsDescription = "I HAVE ONE PlayStation";

			var consignment3 = bookingHeader.Consignments.AddNew();
			consignment3.HVC_GoodsDescription = "I HAVE ONE SWITCH";

			var consignment4 = bookingHeader.Consignments.AddNew();
			consignment4.HVC_GoodsDescription = "I HAVE ONE SWITCH, I HAVE ONE PlayStation, I HAVE ONE XBOX";

			var consignment5 = bookingHeader.Consignments.AddNew();
			consignment5.HVC_ConsigneeName = "SHAWN";

			CombineAssertions("Message content", () =>
			{
				var result1 = PerformPreScreeningForRule(consignment1);
				AssertEquals("Error Message On Value Level - XBOX", "Goods Description Screening Error - Error Message On Value Level - XBOX.", result1.PreScreeningErrorDetails[0].Message);

				var result2 = PerformPreScreeningForRule(consignment2);
				AssertEquals("Error Message On Value Level - PlayStation", "Goods Description Screening Error - Error Message On Value Level - PlayStation.", result2.PreScreeningErrorDetails[0].Message);

				var result3 = PerformPreScreeningForRule(consignment3);
				AssertEquals("Error Message On Field Level", "Goods Description Screening Error - Error Message On Field Level.", result3.PreScreeningErrorDetails[0].Message);

				var result4 = PerformPreScreeningForRule(consignment4);
				AssertEquals("Both Field Level And Value Level Matched", "Goods Description Screening Error - Error Message On Field Level\r\nError Message On Value Level - XBOX\r\nError Message On Value Level - PlayStation.", result4.PreScreeningErrorDetails[0].Message);

				var result5 = PerformPreScreeningForRule(consignment5);
				AssertEquals("Default Error Message", "Consignee Screening Error - SHAWN has been listed for screening.", result5.PreScreeningErrorDetails[0].Message);
			});
		}

		protected override PreScreeningAction GetPreScreeningAction(HVLVPreScreeningField field, HVLVConsignment consginment)
		{
			Assert("Precondition: field is normal property field",
				!field.IsSpecialCharactersField
				&& !field.IsDeminimusField
				&& !field.IsMacrosField);
			return new CheckNormalPropertyAction(field, consginment);
		}
	}
}
