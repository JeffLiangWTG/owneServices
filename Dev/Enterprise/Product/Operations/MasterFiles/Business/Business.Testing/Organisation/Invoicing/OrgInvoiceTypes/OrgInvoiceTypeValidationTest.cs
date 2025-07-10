using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgInvoiceTypeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPI_Calc_IsInclude()
		{
			AssertEquals(0, TestInvoiceType.DeferredCharges.Count);

			TestInvoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;
			AssertNoError(TestInvoiceType.PI_Calc_IsIncludeInfo, "Deferred Charges should be entered");

			TestInvoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.EXC;
			AssertHasError(TestInvoiceType.PI_Calc_IsIncludeInfo, "Deferred Charges should be entered");

			TestInvoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			AssertHasError(TestInvoiceType.PI_Calc_IsIncludeInfo, "Deferred Charges should be entered");
		}

		public void TestCheckModule()
		{
			TestInvoiceType.PI_Module = ZString.Empty;
			AssertHasErrors("Mandatory Validation with error", TestInvoiceType.PI_ModuleInfo);

			TestInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			AssertNoErrors("List Validation no error", TestInvoiceType.PI_ModuleInfo);

			TestInvoiceType.PI_Module = "XXX";
			AssertHasErrors("List Validation with error", TestInvoiceType.PI_ModuleInfo);

			TestInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			AssertNoErrors("No error - SHP is unique", TestInvoiceType.PI_ModuleInfo);

			OrgInvoiceType newInvoiceType = TestHeader.CompanyData.InvoiceTypes.AddNew();
			newInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			AssertHasError("FWD already exists", newInvoiceType.PI_ModuleInfo, "Can NOT have more than one Line with the same Job Type, Direction, Mode and Service Level");

			newInvoiceType.PI_Module = JobInvoicingConsumerTypes.CFSLoadList.Code;
			AssertNoErrors("CUS does not exist, so no duplicate error", TestInvoiceType.PI_ModuleInfo);

			TestInvoiceType.PI_Module = JobInvoicingConsumerTypes.CFSLoadList.Code;
			AssertHasError("CUS already exists", TestInvoiceType.PI_ModuleInfo, "Can NOT have more than one Line with the same Job Type, Direction, Mode and Service Level");
		}

		public void TestCheckInterval()
		{
			TestInvoiceType.PI_Interval = ZString.Empty;
			AssertHasErrors("Mandatory Validation with error", TestInvoiceType.PI_IntervalInfo);

			TestInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.WKY;
			AssertNoErrors("List Validation no error", TestInvoiceType.PI_IntervalInfo);

			TestInvoiceType.PI_Interval = "XXX";
			AssertHasErrors("List Validation with error", TestInvoiceType.PI_IntervalInfo);
		}

		public void TestCheckInvoiceLayoutType()
		{
			TestInvoiceType.PI_Type = ZString.Empty;
			AssertHasErrors("Mandatory Validation with error", TestInvoiceType.PI_TypeInfo);

			TestInvoiceType.PI_Type = "XXX";
			AssertHasErrors("List Validation with error", TestInvoiceType.PI_TypeInfo);

			TestInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			AssertNoErrors("List Validation no error", TestInvoiceType.PI_TypeInfo);

			TestInvoiceType.PI_Module = InvoiceTypeModuleList.Codes.FWD;
			AssertNoErrors("The Layout should be revalidated and without errors.", TestInvoiceType.PI_TypeInfo);

			TestInvoiceType.PI_Module = InvoiceTypeModuleList.Codes.MSC;
			AssertHasErrors("The Layout should be revalidated and must be 'CHG'", TestInvoiceType.PI_TypeInfo);

			TestInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			AssertNoErrors("The Layout is correct.", TestInvoiceType.PI_TypeInfo);
		}

		public void TestCheckStartDay()
		{
			TestInvoiceType.PI_Interval = ZString.Empty;
			TestInvoiceType.PI_StartDay = ZString.Empty;
			AssertHasErrors("Mandatory Validation with error - No Interval, Empty Start", TestInvoiceType.PI_StartDayInfo);
			TestInvoiceType.PI_StartDay = "XXX";
			AssertHasErrors("List Validation with error - No Interval, Invalid Start", TestInvoiceType.PI_StartDayInfo);

			TestInvoiceType.PI_Interval = "XXX";
			TestInvoiceType.PI_StartDay = ZString.Empty;
			AssertHasErrors("Mandatory Validation with error - Invalid Interval, Empty Start", TestInvoiceType.PI_StartDayInfo);
			TestInvoiceType.PI_StartDay = "XXX";
			AssertHasErrors("List Validation with error - Invalid Interval, Invalid Start", TestInvoiceType.PI_StartDayInfo);

			TestInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.WKY;
			TestInvoiceType.PI_StartDay = ZString.Empty;
			AssertHasErrors("Mandatory Validation with error - Weekly Interval, Empty Start", TestInvoiceType.PI_StartDayInfo);
			TestInvoiceType.PI_StartDay = "XXX";
			AssertHasErrors("List Validation with error - Weekly Interval, Invalid Start", TestInvoiceType.PI_StartDayInfo);
			TestInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.M01;
			AssertHasErrors("List Validation with error - Weekly Interval, Monthly Start", TestInvoiceType.PI_StartDayInfo);
			TestInvoiceType.PI_StartDay = "WED";
			AssertNoErrors("List Validation no error - Weekly Interval, Weekly Start", TestInvoiceType.PI_StartDayInfo);

			TestInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			TestInvoiceType.PI_StartDay = ZString.Empty;
			AssertHasErrors("Mandatory Validation with error - Monthly Interval, Empty Start", TestInvoiceType.PI_StartDayInfo);
			TestInvoiceType.PI_StartDay = "XXX";
			AssertHasErrors("List Validation with error - Monthly Interval, Invalid Start", TestInvoiceType.PI_StartDayInfo);
			TestInvoiceType.PI_StartDay = "WED";
			AssertHasErrors("List Validation with error - Monthly Interval, Weekly Start", TestInvoiceType.PI_StartDayInfo);
			TestInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.M01;
			AssertNoErrors("List Validation no error - Monthly Interval, Monthly Start", TestInvoiceType.PI_StartDayInfo);
		}

		#region Set Up

		OrgInvoiceType TestInvoiceType;
		OrgHeader TestHeader;

		protected override void SetUp()
		{
			TestHeader = OrgHeader.New(Factory);
			TestHeader.FillWithValidTestData();
			TestInvoiceType = TestHeader.CompanyData.InvoiceTypes.AddNew();

			base.SetUp();
		}

		#endregion
	}
}
