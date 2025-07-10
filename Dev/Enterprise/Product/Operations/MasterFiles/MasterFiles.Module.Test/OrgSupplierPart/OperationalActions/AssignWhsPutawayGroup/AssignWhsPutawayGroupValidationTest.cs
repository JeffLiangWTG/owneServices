using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AssignWhsPutawayGroupValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateWhsPutawayGroupPK

		public void TestValidateWhsPutawayGroupPK()
		{
			var group = Factory.New<IWhsPutawayGroup>();
			group.WPG_Code = "G1";
			group.WPG_Description = "Group";

			var applicator = GetNewApplicator();
			applicator.WhsPutawayGroupPK = ZGuid.NewZGuid();
			AssertHasError(applicator.WhsPutawayGroupPKInfo, "Enter a valid Warehouse Putaway Group.");

			applicator.WhsPutawayGroupPK = group.PK;
			AssertNoError(applicator.WhsPutawayGroupPKInfo, "Enter a valid Warehouse Putaway Group.");
		}

		#endregion

		#region TestValidateWarehousePK

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

		#region TestAutoValidationType

		public void TestAutoValidationType()
		{
			AssertEquals(typeof(AssignWhsPutawayGroupValidation), GetNewApplicator().Validation.AutoValidationType);
		}

		#endregion

		#region Implementation

		AssignWhsPutawayGroupMethodApplicator GetNewApplicator()
		{
			return new AssignWhsPutawayGroupMethodApplicator("test", Factory);
		}

		#endregion
	}
}
