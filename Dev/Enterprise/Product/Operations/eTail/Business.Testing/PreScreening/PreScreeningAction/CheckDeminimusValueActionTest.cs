using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business.Testing
{
	public class CheckDeminimusValueActionTest : PreScreeningActionTest
	{
		public void TestPreScreeningHVLVDetails_GoodsValue()
		{
			var usExchangeRate = Factory.Load<RefExchangeRate>(new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, "USD")).FirstOrDefault() ?? Factory.New<RefExchangeRate>();
			usExchangeRate.RE_SellRate = 0.61;
			usExchangeRate.RE_GC = Environment.Env.CurrentCompanyPK;
			usExchangeRate.RE_RX_NKExCurrency = "USD";
			usExchangeRate.RE_ExRateType = "CUS";
			usExchangeRate.RE_StartDate = ZDateTime.Today;
			usExchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);

			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			TestConsignment.HVC_GoodsValue = 1000;
			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";

			PerformPreScreeningForRule(TestConsignment);
			AssertNullOrEmpty("Currency Empty - Not screen Goods Value and Total Line Values when Goods Value Currency is empty", TestConsignment.PreScreeningErrorDetails);

			TestConsignment.HVC_RX_NKGoodsValueCurrency = "USD";
			PerformPreScreeningForRule(TestConsignment);
			var expectedErrorDetails = "Screening Error - Goods Value has failed Pre-Screening.  \r\n";
			AssertEquals("Different Currency - Goods Value has failed Pre-Screening", expectedErrorDetails, TestConsignment.PreScreeningErrorDetails);

			TestConsignment.HVC_GoodsValue = 400;
			var item = TestConsignment.Items.AddNew();
			var line1 = item.Lines.AddNew();
			line1.HVS_CustomsValue = 500;
			var line2 = item.Lines.AddNew();
			line2.HVS_CustomsValue = 500;
			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("Should not check Total Line Customs Values", string.Empty, TestConsignment.PreScreeningErrorDetails.ToString());

			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			PerformPreScreeningForRule(TestConsignment);
			AssertNullOrEmpty(TestConsignment.PreScreeningErrorDetails);

			TestConsignment.HVC_GoodsValue = 1201;
			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("Same Currency - Goods Value has failed Pre-Screening", "Screening Error - Goods Value has failed Pre-Screening.  \r\n", TestConsignment.PreScreeningErrorDetails.ToString());

			TestConsignment.HVC_GoodsValue = 500;
			line1.HVS_CustomsValue = 600;
			line2.HVS_CustomsValue = 601;
			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("Should not check Total Line Customs Values", string.Empty, TestConsignment.PreScreeningErrorDetails.ToString());
		}

		public void TestPreScreeningHVLVDetails_GoodsValueTotalByConsignees()
		{
			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_GoodsValue = 1000;
			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var consigneeAddress = TestConsignment.Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Address1 = "Address1";
			consigneeAddress.Address2 = "Address2";
			consigneeAddress.City = "City";
			consigneeAddress.State = "State";
			consigneeAddress.Postcode = "2222";
			consigneeAddress.OA_RN_NKCountryCode = "NZ";

			var consignmentWithSameConsignee = TestConsignment.Factory.New<HVLVConsignment>();
			consignmentWithSameConsignee.HVC_GoodsValue = 500;
			consignmentWithSameConsignee.HVC_RN_NKConsigneeCountryCode = "NZ";
			consignmentWithSameConsignee.HVC_RX_NKGoodsValueCurrency = "NZD";
			consignmentWithSameConsignee.HVC_HVH_BookingHeader = bookingHeader.PK;

			TestConsignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignmentWithSameConsignee.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			Factory.Save();

			AssertArrayEqualsByElements("Precondition:", new HVLVConsignment[] { TestConsignment, consignmentWithSameConsignee }, TestConsignment.ConsignmentsBelongToSameConsignee.ToArray());

			PerformPreScreeningForRule(TestConsignment);

			var expectedErrorDetails =
$@"Screening Error - Combined Goods Value of the same consignee checked. Consignment IDs: {string.Join(", ", TestConsignment.ConsignmentsBelongToSameConsignee.Select(x => x.HVC_ConsignmentId))}. Custom Message Text
";
			AssertEquals("Screening Error, Consignment IDs and custom message text should be included", expectedErrorDetails, TestConsignment.PreScreeningErrorDetails);
		}

		public void TestPreScreeningHVLVDetails_TotalLineCustomsValuesByConsignees()
		{
			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Total Lines Customs Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_GoodsValue = 500;
			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var consigneeAddress = TestConsignment.Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Address1 = "Address1";
			consigneeAddress.Address2 = "Address2";
			consigneeAddress.City = "City";
			consigneeAddress.State = "State";
			consigneeAddress.Postcode = "2222";
			consigneeAddress.OA_RN_NKCountryCode = "NZ";

			TestConsignment.HVC_GoodsValue = 400;
			var item1 = TestConsignment.Items.AddNew();
			var line1 = item1.Lines.AddNew();
			line1.HVS_CustomsValue = 601;
			line1.HVS_Quantity = 1;

			var consignmentWithSameConsignee = bookingHeader.Consignments.AddNew();
			consignmentWithSameConsignee.HVC_GoodsValue = 500;
			consignmentWithSameConsignee.HVC_RN_NKConsigneeCountryCode = "NZ";
			consignmentWithSameConsignee.HVC_RX_NKGoodsValueCurrency = "NZD";
			var item2 = consignmentWithSameConsignee.Items.AddNew();
			var line2 = item2.Lines.AddNew();
			line2.HVS_CustomsValue = 601;
			line2.HVS_Quantity = 1;

			TestConsignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignmentWithSameConsignee.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			Factory.Save();

			AssertArrayEqualsByElements("Precondition:", new HVLVConsignment[] { TestConsignment, consignmentWithSameConsignee }, TestConsignment.ConsignmentsBelongToSameConsignee.ToArray());

			PerformPreScreeningForRule(TestConsignment);

			AssertStartsWith("TestConsignment", "Screening Error - Combined Total Line Customs Values of the same consignee checked. Consignment IDs: ", TestConsignment.PreScreeningErrorDetails);
		}

		public void TestPreScreeningHVLVDetails_TotalLineIntrinsicValuesByConsignees()
		{
			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Total Lines Intrinsic Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_GoodsValue = 500;
			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var consigneeAddress = TestConsignment.Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Address1 = "Address1";
			consigneeAddress.Address2 = "Address2";
			consigneeAddress.City = "City";
			consigneeAddress.State = "State";
			consigneeAddress.Postcode = "2222";
			consigneeAddress.OA_RN_NKCountryCode = "NZ";

			TestConsignment.HVC_GoodsValue = 400;
			var item1 = TestConsignment.Items.AddNew();
			var line1 = item1.Lines.AddNew();
			line1.HVS_IntrinsicValue = 601;
			line1.HVS_Quantity = 1;

			var consignmentWithSameConsignee = bookingHeader.Consignments.AddNew();
			consignmentWithSameConsignee.HVC_GoodsValue = 500;
			consignmentWithSameConsignee.HVC_RN_NKConsigneeCountryCode = "NZ";
			consignmentWithSameConsignee.HVC_RX_NKGoodsValueCurrency = "NZD";
			var item2 = consignmentWithSameConsignee.Items.AddNew();
			var line2 = item2.Lines.AddNew();
			line2.HVS_IntrinsicValue = 601;
			line2.HVS_Quantity = 1;

			TestConsignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignmentWithSameConsignee.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			Factory.Save();

			AssertArrayEqualsByElements("Precondition:", new HVLVConsignment[] { TestConsignment, consignmentWithSameConsignee }, TestConsignment.ConsignmentsBelongToSameConsignee.ToArray());

			PerformPreScreeningForRule(TestConsignment);

			AssertStartsWith("TestConsignment", "Screening Error - Combined Total Line Intrinsic Values of the same consignee checked. Consignment IDs: ", TestConsignment.PreScreeningErrorDetails);
		}

		public void TestGoodsValueAndTotalLineValues_WhenEqualZero_ThenDisplayTwoValueErrorMessages()
		{
			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_GoodsValue = 0;
			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var consigneeAddress = TestConsignment.Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Address1 = "Address1";
			consigneeAddress.Address2 = "Address2";
			consigneeAddress.City = "City";
			consigneeAddress.State = "State";
			consigneeAddress.Postcode = "2222";
			consigneeAddress.OA_RN_NKCountryCode = "NZ";

			Factory.Save();

			PerformPreScreeningForRule(TestConsignment);
			AssertEquals("Should display errors", "Screening Error - Goods Value checked. Please enter a value\r\n", TestConsignment.PreScreeningErrorDetails);
		}

		public void TestGetConsignmentIDsMessage()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				rule.DestinationCountryCode = "NZ";
				var field = rule.Fields.AddNew();
				field.FieldDescription = "Goods Value";
				field.DeminimusValue = 1000;
				field.ValidationRule = "ERR";
				field.MessageText = "Custom Message Text";
				field.CheckSameConsignee = true;

				var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

				TestConsignment.HVC_GoodsValue = 600;
				TestConsignment.HVC_ConsignmentId = "HVC000000000000001";
				TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
				TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
				TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;
				var item = TestConsignment.Items.AddNew();
				var itemLine = item.Lines.AddNew();
				itemLine.HVS_Quantity = 1;
				itemLine.HVS_CustomsValue = 500;

				var consigneeAddress = TestConsignment.Factory.NewWithValidTestData<OrgAddress>();
				consigneeAddress.Address1 = "Address1";
				consigneeAddress.Address2 = "Address2";
				consigneeAddress.City = "City";
				consigneeAddress.State = "State";
				consigneeAddress.Postcode = "2222";
				consigneeAddress.OA_RN_NKCountryCode = "NZ";

				TestConsignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

				var consignmentWithSameConsignee = TestConsignment.Factory.New<HVLVConsignment>();
				consignmentWithSameConsignee.HVC_GoodsValue = 600;
				consignmentWithSameConsignee.HVC_ConsignmentId = "HVC000000000000002";
				consignmentWithSameConsignee.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
				consignmentWithSameConsignee.HVC_RN_NKConsigneeCountryCode = "NZ";
				consignmentWithSameConsignee.HVC_RX_NKGoodsValueCurrency = "NZD";
				consignmentWithSameConsignee.HVC_HVH_BookingHeader = bookingHeader.PK;

				var consignmentWithSameConsigneeItem = consignmentWithSameConsignee.Items.AddNew();
				var consignmentWithSameConsigneeItemLine = consignmentWithSameConsigneeItem.Lines.AddNew();
				consignmentWithSameConsigneeItemLine.HVS_Quantity = 1;
				consignmentWithSameConsigneeItemLine.HVS_CustomsValue = 500;

				Factory.Save();

				PerformPreScreeningForRule(TestConsignment);
				AssertEquals("TestConsignment should fail prescreening as combined goods value exceeds deminimus value", "Screening Error - Combined Goods Value of the same consignee checked. Consignment IDs: HVC000000000000001, HVC000000000000002. Custom Message Text\r\n", TestConsignment.PreScreeningErrorDetails);

				PerformPreScreeningForRule(consignmentWithSameConsignee);
				AssertEquals("ConsignmentWithSameConsignee should fail prescreening as combined goods value exceeds deminimus value", "Screening Error - Combined Goods Value of the same consignee checked. Consignment IDs: HVC000000000000001, HVC000000000000002. Custom Message Text\r\n", consignmentWithSameConsignee.PreScreeningErrorDetails);
			}
		}

		public void TestGivenSingleConsignmentWithConsignee_WhenExceedsDeminimusValue_ThenDisplayGoodsValueErrorMessage()
		{
			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Hurricane Katrina?? More like Hurricane Tortilla";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			TestConsignment.HVC_ConsignmentId = "hehe";
			TestConsignment.HVC_GoodsValue = 9000;

			var item = TestConsignment.Items.AddNew();
			var line = item.Lines.AddNew();
			line.HVS_Quantity = 1;
			line.HVS_CustomsValue = 500;

			var consigneeAddress = TestConsignment.Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Address1 = "Address1";
			consigneeAddress.Address2 = "Address2";
			consigneeAddress.City = "City";
			consigneeAddress.State = "State";
			consigneeAddress.Postcode = "2222";
			consigneeAddress.OA_RN_NKCountryCode = "NZ";

			TestConsignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			Factory.Save();

			PerformPreScreeningForRule(TestConsignment);

			AssertEquals("Test Consignment Error message", "Screening Error - Goods Value checked.  Hurricane Katrina?? More like Hurricane Tortilla\r\n", TestConsignment.PreScreeningErrorDetails);
		}

		public void TestGivenAConsignmentWithNoGoodsCurrency_AndOtherConsignementsWithSameConsigneesThatHaveForeignCurrencyThatExceedDeminimusValue_WhenPreScreeningConsignmentGoodsValueTotalByConsignee_ThenConsignmentHasPreScreeningError()
		{
			var usExchangeRate = Factory.Load<RefExchangeRate>(new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, "USD")).FirstOrDefault() ?? Factory.New<RefExchangeRate>();
			usExchangeRate.RE_SellRate = 0.75;
			usExchangeRate.RE_GC = Environment.Env.CurrentCompanyPK;
			usExchangeRate.RE_RX_NKExCurrency = "USD";
			usExchangeRate.RE_ExRateType = "CUS";
			usExchangeRate.RE_StartDate = ZDateTime.Today;
			usExchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);

			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_GoodsValue = 0;
			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var consigneeAddress = TestConsignment.Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Address1 = "Address1";
			consigneeAddress.Address2 = "Address2";
			consigneeAddress.City = "City";
			consigneeAddress.State = "State";
			consigneeAddress.Postcode = "2222";
			consigneeAddress.OA_RN_NKCountryCode = "NZ";

			var consignmentInUSD1 = TestConsignment.Factory.New<HVLVConsignment>();
			consignmentInUSD1.HVC_GoodsValue = 500;
			consignmentInUSD1.HVC_RN_NKConsigneeCountryCode = "NZ";
			consignmentInUSD1.HVC_RX_NKGoodsValueCurrency = "USD";
			consignmentInUSD1.HVC_HVH_BookingHeader = bookingHeader.PK;

			var consignmentInUSD2 = TestConsignment.Factory.New<HVLVConsignment>();
			consignmentInUSD2.HVC_GoodsValue = 500;
			consignmentInUSD2.HVC_RN_NKConsigneeCountryCode = "NZ";
			consignmentInUSD2.HVC_RX_NKGoodsValueCurrency = "USD";
			consignmentInUSD2.HVC_HVH_BookingHeader = bookingHeader.PK;

			TestConsignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignmentInUSD1.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignmentInUSD2.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			Factory.Save();

			AssertArrayEqualsByElements("Precondition:", new HVLVConsignment[] { TestConsignment, consignmentInUSD1, consignmentInUSD2 }, TestConsignment.ConsignmentsBelongToSameConsignee.ToArray());

			PerformPreScreeningForRule(TestConsignment);
			PerformPreScreeningForRule(consignmentInUSD1);
			PerformPreScreeningForRule(consignmentInUSD2);

			CombineAssertions("There should be a pre-screening error message regarding the goods values for all the consignments. This includes TestConsignment which has no goods currency value", () =>
			{
				AssertPreScreeningMessageContainsExpectedString("TestConsignment", TestConsignment.PreScreeningErrorDetails, "Screening Error - Combined Goods Value of the same consignee checked.");
				AssertPreScreeningMessageContainsExpectedString("consignmentInUSD1", consignmentInUSD1.PreScreeningErrorDetails, "Screening Error - Combined Goods Value of the same consignee checked.");
				AssertPreScreeningMessageContainsExpectedString("consignmentInUSD2", consignmentInUSD2.PreScreeningErrorDetails, "Screening Error - Combined Goods Value of the same consignee checked.");
			});
		}

		public void TestGivenConsignmentsWithDifferentCurrencyCodesThatExceedDeminimusValue_WhenPreScreeningConsignmentGoodsValueTotalByConsignee_ThenConsignmentsHasPreScreeningError()
		{
			var usExchangeRate = Factory.Load<RefExchangeRate>(new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, "USD")).FirstOrDefault() ?? Factory.New<RefExchangeRate>();
			usExchangeRate.RE_SellRate = 0.75;
			usExchangeRate.RE_GC = Environment.Env.CurrentCompanyPK;
			usExchangeRate.RE_RX_NKExCurrency = "USD";
			usExchangeRate.RE_ExRateType = "CUS";
			usExchangeRate.RE_StartDate = ZDateTime.Today;
			usExchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);

			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_GoodsValue = 600;
			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var consigneeAddress = TestConsignment.Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Address1 = "Address1";
			consigneeAddress.Address2 = "Address2";
			consigneeAddress.City = "City";
			consigneeAddress.State = "State";
			consigneeAddress.Postcode = "2222";
			consigneeAddress.OA_RN_NKCountryCode = "NZ";

			var consignmentWithSameConsignee = TestConsignment.Factory.New<HVLVConsignment>();
			consignmentWithSameConsignee.HVC_GoodsValue = 500;
			consignmentWithSameConsignee.HVC_RN_NKConsigneeCountryCode = "NZ";
			consignmentWithSameConsignee.HVC_RX_NKGoodsValueCurrency = "USD";
			consignmentWithSameConsignee.HVC_HVH_BookingHeader = bookingHeader.PK;

			TestConsignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignmentWithSameConsignee.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			Factory.Save();

			AssertArrayEqualsByElements("Precondition:", new HVLVConsignment[] { TestConsignment, consignmentWithSameConsignee }, TestConsignment.ConsignmentsBelongToSameConsignee.ToArray());

			PerformPreScreeningForRule(TestConsignment);
			PerformPreScreeningForRule(consignmentWithSameConsignee);

			CombineAssertions("There should be a pre-screening error message regarding the goods values for both consignments", () =>
			{
				AssertPreScreeningMessageContainsExpectedString("TestConsignment", TestConsignment.PreScreeningErrorDetails, "Screening Error - Combined Goods Value of the same consignee checked.");
				AssertPreScreeningMessageContainsExpectedString("consignmentWithSameConsignee", consignmentWithSameConsignee.PreScreeningErrorDetails, "Screening Error - Combined Goods Value of the same consignee checked.");
			});
		}

		public void TestGivenConsignmentHasNoGoodsCurrencyThatExceedsDeminimusValue_WhenPreScreeningConsignmentGoodsValueTotalByConsignee_ThenConsignmentHasPreScreeningError()
		{
			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_GoodsValue = 1000;
			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var consigneeAddress = TestConsignment.Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Address1 = "Address1";
			consigneeAddress.Address2 = "Address2";
			consigneeAddress.City = "City";
			consigneeAddress.State = "State";
			consigneeAddress.Postcode = "2222";
			consigneeAddress.OA_RN_NKCountryCode = "NZ";

			var consignment1 = TestConsignment.Factory.New<HVLVConsignment>();
			consignment1.HVC_GoodsValue = 500;
			consignment1.HVC_RN_NKConsigneeCountryCode = "NZ";
			consignment1.HVC_RX_NKGoodsValueCurrency = "NZD";
			consignment1.HVC_HVH_BookingHeader = bookingHeader.PK;

			var consignmentWithNoCurrency = TestConsignment.Factory.New<HVLVConsignment>();
			consignmentWithNoCurrency.HVC_GoodsValue = 0;
			consignmentWithNoCurrency.HVC_HVH_BookingHeader = bookingHeader.PK;

			TestConsignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment1.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignmentWithNoCurrency.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			Factory.Save();

			AssertArrayEqualsByElements("Precondition:", new HVLVConsignment[] { TestConsignment, consignment1, consignmentWithNoCurrency }, TestConsignment.ConsignmentsBelongToSameConsignee.ToArray());

			PerformPreScreeningForRule(TestConsignment);
			PerformPreScreeningForRule(consignment1);
			PerformPreScreeningForRule(consignmentWithNoCurrency);

			CombineAssertions("There should be a pre-screening error message regarding the goods values for all the consignments. This includes consignmentWithNoCurrency which has no goods currency value", () =>
			{
				AssertPreScreeningMessageContainsExpectedString("TestConsignment", TestConsignment.PreScreeningErrorDetails, "Screening Error - Combined Goods Value of the same consignee checked.");
				AssertPreScreeningMessageContainsExpectedString("consignment1", consignment1.PreScreeningErrorDetails, "Screening Error - Combined Goods Value of the same consignee checked.");
				AssertPreScreeningMessageContainsExpectedString("consignmentWithNoCurrency", consignmentWithNoCurrency.PreScreeningErrorDetails, "Screening Error - Combined Goods Value of the same consignee checked.");
			});
		}

		public void TestPreScreeningHVLVDetails_GoodsValueTotalByConsignees_ExcludeInactiveConsignments()
		{
			rule.DestinationCountryCode = "NZ";
			var field = rule.Fields.AddNew();
			field.FieldDescription = "Goods Value";
			field.DeminimusValue = 1200;
			field.ValidationRule = "ERR";
			field.MessageText = "Custom Message Text";
			field.CheckSameConsignee = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			TestConsignment.HVC_GoodsValue = 1000;
			TestConsignment.HVC_RN_NKConsigneeCountryCode = "NZ";
			TestConsignment.HVC_RX_NKGoodsValueCurrency = "NZD";
			TestConsignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			var item = TestConsignment.Items.AddNew();
			var line = item.Lines.AddNew();
			line.HVS_Quantity = 1;
			line.HVS_CustomsValue = 500;

			var consigneeAddress = TestConsignment.Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Address1 = "Address1";
			consigneeAddress.Address2 = "Address2";
			consigneeAddress.City = "City";
			consigneeAddress.State = "State";
			consigneeAddress.Postcode = "2222";
			consigneeAddress.OA_RN_NKCountryCode = "NZ";

			var consignmentWithSameConsignee = TestConsignment.Factory.New<HVLVConsignment>();
			consignmentWithSameConsignee.HVC_GoodsValue = 500;
			consignmentWithSameConsignee.HVC_RN_NKConsigneeCountryCode = "NZ";
			consignmentWithSameConsignee.HVC_RX_NKGoodsValueCurrency = "NZD";
			consignmentWithSameConsignee.HVC_HVH_BookingHeader = bookingHeader.PK;
			var consignmentWithSameConsigneeItem = consignmentWithSameConsignee.Items.AddNew();
			var consignmentWithSameConsigneeLine = item.Lines.AddNew();
			consignmentWithSameConsigneeLine.HVS_Quantity = 1;
			consignmentWithSameConsigneeLine.HVS_CustomsValue = 500;

			TestConsignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignmentWithSameConsignee.HVC_OA_ConsigneeAddress = consigneeAddress.PK;

			Factory.Save();

			PerformPreScreeningForRule(TestConsignment);
			PerformPreScreeningForRule(consignmentWithSameConsignee);

			CombineAssertions("There should be a pre-screening error message about the goods values for each consignment", () =>
			{
				AssertPreScreeningMessageContainsExpectedString("TestConsignment", TestConsignment.PreScreeningErrorDetails, "Screening Error - Combined Goods Value of the same consignee checked.");
				AssertPreScreeningMessageContainsExpectedString("consignmentWithSameConsignee", consignmentWithSameConsignee.PreScreeningErrorDetails, "Screening Error - Combined Goods Value of the same consignee checked.");
			});

			TestConsignment.ClearPreScreeningDetails();
			consignmentWithSameConsignee.HVC_IsActive = false;
			Factory.Save();

			PerformPreScreeningForRule(TestConsignment);
			AssertNullOrEmpty("TestConsignment", TestConsignment.PreScreeningErrorDetails);
		}

		void AssertPreScreeningMessageContainsExpectedString(string consignmentVariableName, string expectedString, string substring)
		{
			Assert($"Expected pre-screening message in  {consignmentVariableName}: '{expectedString}' to contain '{substring}'", expectedString.Contains(substring));
		}

		protected override PreScreeningAction GetPreScreeningAction(HVLVPreScreeningField field, HVLVConsignment consginment)
		{
			Assert("Precondition: field is Deminimus field", field.IsDeminimusField);
			return new CheckDeminimusValueAction(field, consginment);
		}
	}
}
