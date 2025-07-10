namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageContainerValidationForUnfinalisedPackageJobTest : PackingBusinessObjectValidationTestCase
	{
		// persistent

		#region TestValidateK0_AirVentFlowRateUnit

		public void TestValidateK0_AirVentFlowRateUnit()
		{
			var container = Factory.New<PkgPackageContainer>();
			AssertListValidation(container.K0_AirVentFlowRateUnitInfo, "2L", "xXx");
		}

		#endregion

		#region TestValidateK0_ContainerMode

		public void TestValidateK0_ContainerMode()
		{
			var container = Factory.New<PkgPackageContainer>();
			AssertListValidation(container.K0_ContainerModeInfo, "FCL", "xXx");
		}

		#endregion

		#region TestValidateK0_Quality

		public void TestValidateK0_Quality()
		{
			var container = Factory.New<PkgPackageContainer>();
			AssertListValidation(container.K0_QualityInfo, "GEN", "xXx");
		}

		#endregion

		#region TestValidateK0_SetPointTempUnit

		public void TestValidateK0_SetPointTempUnit()
		{
			var container = Factory.New<PkgPackageContainer>();
			AssertListValidation(container.K0_SetPointTempUnitInfo, Core.Constants.Temperature.Centigrade, "X");

			container.K0_SetPointTempUnit = "";
			container.Validation.ValidateK0_SetPointTempUnit();
			AssertNoErrors(container.K0_SetPointTempUnitInfo);

			container.K0_IsControlledAtmosphere = true;
			container.Validation.ValidateK0_SetPointTempUnit();
			AssertHasErrors("If the Container is temperature controlled, the temperature unit should be required.", container.K0_SetPointTempUnitInfo);
		}

		#endregion

		#region TestValidateK0_Status

		public void TestValidateK0_Status()
		{
			var container = Factory.New<PkgPackageContainer>();
			AssertListValidation(container.K0_StatusInfo, "AVL", "xXx");
		}

		#endregion
	}
}
