using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class UpdateExpiryNotificationPeriodValidationTest : BusinessObjectValidationTestCase
	{
		#region ValidateClientPK

		public void TestValidateClientPK()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var applicator = GetNewApplicator();
			applicator.ClientPK = ZGuid.NewZGuid();
			AssertHasError(applicator.ClientPKInfo, "Enter a valid Organization.");

			applicator.ClientPK = client.PK;
			AssertNoError(applicator.ClientPKInfo, "Enter a valid Organization.");
		}

		#endregion

		#region ValidateWarehousePK

		public void TestValidateWarehousePK()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "A", 3, 1);

			var applicator = GetNewApplicator();
			applicator.WarehousePK = ZGuid.NewZGuid();
			AssertHasError(applicator.WarehousePKInfo, "Enter a valid Warehouse.");

			applicator.WarehousePK = warehouse.PK;
			AssertNoError(applicator.WarehousePKInfo, "Enter a valid Warehouse.");
		}

		#endregion

		#region ValidateExpiryNotificationPeriod

		public void TestValidateExpiryNotificationPeriod()
		{
			var applicator = GetNewApplicator();
			applicator.ExpiryNotificationPeriod = -5;
			AssertHasError(applicator.ExpiryNotificationPeriodInfo, "Expiry Notification Period (Days) cannot be negative.");

			applicator.ExpiryNotificationPeriod = 0;
			AssertNoError(applicator.ExpiryNotificationPeriodInfo, "Expiry Notification Period (Days) cannot be negative.");

			applicator.ExpiryNotificationPeriod = 5;
			AssertNoError(applicator.ExpiryNotificationPeriodInfo, "Expiry Notification Period (Days) cannot be negative.");
		}

		#endregion

		#region ValidateExpiryNotificationPeriod

		public void TestValidateOverrideExistingNonZeroValues()
		{
			var applicator = GetNewApplicator();

			applicator.ExpiryNotificationPeriod = 5;

			applicator.OverrideNonZeroExpiryNotificationPeriod = true;
			AssertNoErrors("No Errors when true if Expiry Notification Period is set to a non-zero value.", applicator.OverrideNonZeroExpiryNotificationPeriodInfo);

			applicator.OverrideNonZeroExpiryNotificationPeriod = false;
			AssertNoErrors("No Errors when false if Expiry Notification Period is set to a non-zero value.", applicator.OverrideNonZeroExpiryNotificationPeriodInfo);

			applicator.ExpiryNotificationPeriod = 0;

			applicator.OverrideNonZeroExpiryNotificationPeriod = true;
			AssertNoErrors("No Errors when true if Expiry Notification Period is set to zero.", applicator.OverrideNonZeroExpiryNotificationPeriodInfo);

			applicator.OverrideNonZeroExpiryNotificationPeriod = false;
			AssertHasError("Has Errors when false if Expiry Notification Period is set to zero.", applicator.OverrideNonZeroExpiryNotificationPeriodInfo, "Override Non-Zero Expiry Notification Period must be true when clearing Expiry Notification Periods.");
		}

		#endregion

		#region TestAutoValidationType

		public void TestAutoValidationType()
		{
			AssertEquals(typeof(UpdateExpiryNotificationPeriodValidation), GetNewApplicator().Validation.AutoValidationType);
		}

		#endregion

		#region Implementation

		UpdateExpiryNotificationPeriodMethodApplicator GetNewApplicator()
		{
			return new UpdateExpiryNotificationPeriodMethodApplicator("test", Factory);
		}

		#endregion
	}
}
