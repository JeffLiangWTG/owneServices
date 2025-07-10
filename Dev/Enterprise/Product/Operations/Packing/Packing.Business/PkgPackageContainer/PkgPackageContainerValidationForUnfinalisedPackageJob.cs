using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class PkgPackageContainerValidationForUnfinalisedPackageJob : PkgPackageContainerValidation
	{
		public PkgPackageContainerValidationForUnfinalisedPackageJob(PkgPackageContainer parent)
			: base(parent)
		{
		}

		// persistent

		#region ValidateK0_AirVentFlowRateUnit

		protected override void CheckK0_AirVentFlowRateUnit()
		{
			base.CheckK0_AirVentFlowRateUnit();
			ListValidation.ErrorIfInvalidCode(Parent.K0_AirVentFlowRateUnitInfo);
		}

		#endregion

		#region ValidateK0_ContainerMode

		protected override void CheckK0_ContainerMode()
		{
			base.CheckK0_ContainerMode();
			ListValidation.ErrorIfInvalidCode(Parent.K0_ContainerModeInfo);
		}

		#endregion

		#region ValidateK0_Quality

		protected override void CheckK0_Quality()
		{
			base.CheckK0_Quality();
			ListValidation.ErrorIfInvalidCode(Parent.K0_QualityInfo);
		}

		#endregion

		#region ValidateK0_SetPointTempUnit

		protected override void CheckK0_SetPointTempUnit()
		{
			base.CheckK0_SetPointTempUnit();

			if (Parent.K0_IsControlledAtmosphere)
			{
				MandatoryValidation.CheckEntered(Parent.K0_SetPointTempUnitInfo);
			}

			if (!Parent.K0_SetPointTempUnitInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.K0_SetPointTempUnitInfo);
			}
		}

		#endregion

		#region ValidateK0_Status

		protected override void CheckK0_Status()
		{
			base.CheckK0_Status();
			ListValidation.ErrorIfInvalidCode(Parent.K0_StatusInfo);
		}

		#endregion
	}
}
