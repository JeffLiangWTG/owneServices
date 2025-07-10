using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Environment.Testing
{
	class WhsCartonGroupValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckWCG_Code

		public void TestCheckWCG_Code()
		{
			var cartonGroup1 = Factory.New<WhsCartonGroup>();
			cartonGroup1.Validation.ValidateAll();
			AssertHasError(cartonGroup1.WCG_CodeInfo, "Please enter a Code.");

			cartonGroup1.WCG_Code = "TEST";
			var cartonGroup2 = Helper.CreateWhsCartonGroup("TEST", "Group");
			AssertHasError(cartonGroup2.WCG_CodeInfo, "Code must be unique.");

			cartonGroup2.WCG_Code = "T3ST";
			AssertNoError(cartonGroup2.WCG_CodeInfo, "Code must be unique.");
		}

		#endregion

		#region TestCheckWCG_Description

		public void TestCheckWCG_Description()
		{
			var cartonGroup = Factory.New<WhsCartonGroup>();
			cartonGroup.Validation.ValidateAll();
			AssertHasError(cartonGroup.WCG_DescriptionInfo, "Please enter a Description.");
		}

		#endregion

		#region TestValidateOptimizationMode

		public void TestValidateOptimizationMode()
		{
			var cartonGroup1 = Helper.CreateWhsCartonGroup("G", "G");
			cartonGroup1.Validation.ValidateAll();
			AssertNoErrors(cartonGroup1.OptimizationModeInfo);

			cartonGroup1.OptimizationMode = "";
			AssertHasError(cartonGroup1.OptimizationModeInfo, "Please enter an Optimization Mode.");

			cartonGroup1.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeCartons;
			AssertNoErrors(cartonGroup1.OptimizationModeInfo);

			cartonGroup1.OptimizationMode = "TST";
			AssertHasError(cartonGroup1.OptimizationModeInfo, "Enter a valid Optimization Mode.");

			cartonGroup1.OptimizationMode = CartonizationOptimizationModes.Codes.MinimizeVolume;
			AssertNoErrors(cartonGroup1.OptimizationModeInfo);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
