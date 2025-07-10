using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business.Testing
{
	class CheckGoodsDescriptionActionTest : PreScreeningActionTest
	{
		public void TestPreScreeningHVLVDetails_GoodsDescription()
		{
			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "Goods Description";
			field1.ValidationRule = "ERR";

			var screeningValue1 = field1.ScreeningValues.AddNew();
			screeningValue1.ScreeningValue = "Shawn";
			var screeningValue2 = field1.ScreeningValues.AddNew();
			screeningValue2.ScreeningValue = "Bone";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			TestConsignment.HVC_GoodsDescription = "Shawn,Bone,Stephen";

			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("Consignment Goods Description", $"{DataBoundResourceStrings.GetDataForProperty(TestConsignment.HVC_GoodsDescriptionInfo).Caption} Screening Error - Shawn,Bone has been listed for screening.\r\n", TestConsignment.PreScreeningErrorDetails);

			TestConsignment.HVC_GoodsDescription = "";
			var item = TestConsignment.Items.AddNew();
			item.HVI_GoodsDescription = "Shawn,Bone,Stephen";

			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("Item Goods Description", $"{DataBoundResourceStrings.GetDataForProperty(item.HVI_GoodsDescriptionInfo).Caption} Screening Error - Shawn,Bone has been listed for screening.\r\n", TestConsignment.PreScreeningErrorDetails);

			item.HVI_GoodsDescription = "";
			var line = item.Lines.AddNew();
			line.HVS_GoodsDescription = "Shawn,Bone,Stephen";

			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("Line Goods Description", $"{DataBoundResourceStrings.GetDataForProperty(line.HVS_GoodsDescriptionInfo).Caption} Screening Error - Shawn,Bone has been listed for screening.\r\n", TestConsignment.PreScreeningErrorDetails);
		}

		public void TestHVLVPreScreeningGoodsDescriptionIsCaseInsensitive()
		{
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Description";
			field.ValidationRule = "ERR";
			var screeningValue = field.ScreeningValues.AddNew();

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			var item = TestConsignment.Items.AddNew();
			var line = item.Lines.AddNew();
			line.HVS_Quantity = 1;

			var goodsDescriptionCases = new[] { "aa", "AA", "aA", "Aa" };

			CombineAssertions("GoodsDescriptionCaseInsensitive", () =>
			{
				foreach (var screeningValueCase in goodsDescriptionCases)
				{
					screeningValue.ScreeningValue = screeningValueCase;

					foreach (var goodsDescriptionCase in goodsDescriptionCases)
					{
						TestConsignment.HVC_GoodsDescription = goodsDescriptionCase;
						item.HVI_GoodsDescription = ZString.Empty;
						line.HVS_GoodsDescription = ZString.Empty;

						PerformPreScreeningForRule(TestConsignment);
						AssertEquals($"Consignment with Goods Description {goodsDescriptionCase} and Screening Value {screeningValueCase}", $"{DataBoundResourceStrings.GetDataForProperty(TestConsignment.HVC_GoodsDescriptionInfo).Caption} Screening Error - {screeningValueCase} has been listed for screening.\r\n", TestConsignment.PreScreeningErrorDetails);

						TestConsignment.HVC_GoodsDescription = ZString.Empty;
						item.HVI_GoodsDescription = goodsDescriptionCase;

						PerformPreScreeningForRule(TestConsignment);
						AssertEquals($"Item with Goods Description {goodsDescriptionCase} and Screening Value {screeningValueCase}", $"{DataBoundResourceStrings.GetDataForProperty(item.HVI_GoodsDescriptionInfo).Caption} Screening Error - {screeningValueCase} has been listed for screening.\r\n", TestConsignment.PreScreeningErrorDetails);

						item.HVI_GoodsDescription = ZString.Empty;
						line.HVS_GoodsDescription = goodsDescriptionCase;

						PerformPreScreeningForRule(TestConsignment);
						AssertEquals($"Consignment with Goods Description {goodsDescriptionCase} and Screening Value {screeningValueCase}", $"{DataBoundResourceStrings.GetDataForProperty(line.HVS_GoodsDescriptionInfo).Caption} Screening Error - {screeningValueCase} has been listed for screening.\r\n", TestConsignment.PreScreeningErrorDetails);
					}
				}
			});
		}

		protected override PreScreeningAction GetPreScreeningAction(HVLVPreScreeningField field, HVLVConsignment consginment)
		{
			Assert("Precondition: field is Goods Description field",
				field.FieldName == HVLVConsignmentSchema.Constants.HVC_GoodsDescription
				&& !field.IsSpecialCharactersField
				&& !field.IsDeminimusField
				&& !field.IsMacrosField);
			return new CheckGoodsDescriptionAction(field, consginment);
		}
	}
}
