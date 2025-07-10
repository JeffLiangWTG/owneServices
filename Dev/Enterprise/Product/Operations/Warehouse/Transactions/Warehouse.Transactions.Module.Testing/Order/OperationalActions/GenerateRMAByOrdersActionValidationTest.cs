using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	sealed class GenerateRMAByOrdersActionValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateWhsOverride

		public void TestValidateWhsOverride()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "A", 3, 1);

			var applicator = GetNewApplicator();
			applicator.WhsOverride = ZGuid.NewZGuid();
			AssertHasError(applicator.WhsOverrideInfo, "Enter a valid Warehouse Override.");

			applicator.WhsOverride = warehouse.PK;
			AssertNoError(applicator.WhsOverrideInfo, "Enter a valid Warehouse Override.");
		}

		#endregion

		#region TestAutoValidationType

		public void TestAutoValidationType()
		{
			AssertEquals(typeof(GenerateRMAByOrdersActionValidation), GetNewApplicator().Validation.AutoValidationType);
		}

		#endregion

		#region Implementation

		GenerateRMAByOrdersActionMethodApplicator GetNewApplicator()
		{
			return new GenerateRMAByOrdersActionMethodApplicator(Factory);
		}

		#endregion
	}
}
