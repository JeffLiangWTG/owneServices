using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class BulkCommunicationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTypeOfCall()
		{
			var bulk = new BulkCommunication(Factory, Campaign);
			bulk.TypeOfCall = "XXX";
			bulk.Validation.ValidateTypeOfCall();
			AssertListValidationInvalidCodeError(bulk.TypeOfCallInfo, true);

			bulk.TypeOfCall = "PHN";
			bulk.Validation.ValidateTypeOfCall();
			AssertNoErrors(bulk.TypeOfCallInfo);
		}

		public void TestValidateCategory()
		{
			var bulk = new BulkCommunication(Factory, Campaign);
			bulk.Category = "XXX";
			bulk.Validation.ValidateCategory();
			AssertListValidationInvalidCodeError(bulk.CategoryInfo, true);

			bulk.Category = "UDF";
			bulk.Validation.ValidateCategory();
			AssertNoErrors(bulk.CategoryInfo);
		}

		public void TestValidateStatus()
		{
			var bulk = new BulkCommunication(Factory, Campaign);
			bulk.Status = "XXX";
			bulk.Validation.ValidateStatus();
			AssertListValidationInvalidCodeError(bulk.StatusInfo, true);

			bulk.Status = "SCH";
			bulk.Validation.ValidateStatus();
			AssertNoErrors(bulk.StatusInfo);
		}

		public void TestValidateStaffCoordinator()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "EO";

			var bulk = new BulkCommunication(Factory, Campaign);
			bulk.StaffCoordinator = "XXX";
			bulk.Validation.ValidateStaffCoordinator();
			AssertListValidationInvalidCodeError(bulk.StaffCoordinatorInfo, true);

			bulk.StaffCoordinator = "EO";
			bulk.Validation.ValidateStaffCoordinator();
			AssertNoErrors(bulk.StaffCoordinatorInfo);
		}

		[TestDate(2014, 5, 7, 1, 1, 2)]
		public void TestValidateNextCallLocal()
		{
			var bulk = new BulkCommunication(Factory, Campaign);
			bulk.NextCallLocal = ZDateTime.Empty;
			bulk.Validation.ValidateNextCallLocal();
			AssertHasErrors(bulk.NextCallLocalInfo);

			bulk.NextCallLocal = new ZDateTime(2014, 4, 4, 4, 4, 4);
			bulk.Validation.ValidateNextCallLocal();
			AssertNoErrors(bulk.NextCallLocalInfo);

			bulk.NextCallLocal = ZDateTime.Empty;
			bulk.CallDate = new ZDateTime(2014, 5, 5, 5, 5, 4);
			bulk.Validation.ValidateNextCallLocal();
			AssertNoErrors(bulk.NextCallLocalInfo);
		}

		[TestDate(2014, 5, 7, 1, 1, 2)]
		public void TestValidateCallDate()
		{
			var bulk = new BulkCommunication(Factory, Campaign);
			bulk.CallDate = ZDateTime.Empty;
			bulk.NextCallLocal = ZDate.Empty;
			bulk.Validation.ValidateCallDate();
			AssertHasErrors(bulk.NextCallLocalInfo);

			bulk.NextCallLocal = new ZDateTime(2014, 4, 4, 4, 4, 4);
			bulk.Validation.ValidateCallDate();
			AssertNoErrors(bulk.CallDateInfo);

			bulk.CallDate = new ZDateTime(2014, 5, 5, 5, 4, 4);
			bulk.Validation.ValidateCallDate();
			AssertNoErrors(bulk.CallDateInfo);
		}

		#region Implementation

		GlbCompanyCampaign Campaign;

		protected override void SetUp()
		{
			base.SetUp();

			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
		}

		#endregion
	}
}
