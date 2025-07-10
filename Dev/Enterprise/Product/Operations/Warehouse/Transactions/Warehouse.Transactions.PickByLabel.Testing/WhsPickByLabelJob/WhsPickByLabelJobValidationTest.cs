namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Warehouse.Transactions.Business.Testing;
	using Enterprise.ZArchitecture.Schema;

	public class WhsPickByLabelJobValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var pickByLabelJob = Factory.New<WhsPickByLabelJob>();
			var validation = new TestWhsPickByLabelJobValidation(pickByLabelJob);

			var list = new string[]
			{
				WhsPickByLabelJobSchema.Constants.WTK_P9_Task,
			};

			foreach (var propertyInfo in pickByLabelJob.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		class TestWhsPickByLabelJobValidation : WhsPickByLabelJobValidation
		{
			public TestWhsPickByLabelJobValidation(WhsPickByLabelJob parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region TestValidateLabelsDockDoorLocations

		public void TestValidateLabelsDockDoorLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, "Bob");
			var pickByLabelLabel = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			pickByLabelJob.Validation.ValidateAll();
			AssertNoRowError(pickByLabelJob, "Label or labels [ 'PACKAGE-1' ] are not assigned to dock door location 'DOCKDOOR', please contact Support.");

			pick.WP_WL_DockDoor = ZGuid.NewZGuid();
			pickByLabelJob.Validation.ValidateAll();
			AssertHasRowError("Add package for different DDL should have error.", pickByLabelJob, "Label or labels [ 'PACKAGE-1' ] are not assigned to dock door location 'DOCKDOOR', please contact Support.");
		}

		#endregion
	}
}
