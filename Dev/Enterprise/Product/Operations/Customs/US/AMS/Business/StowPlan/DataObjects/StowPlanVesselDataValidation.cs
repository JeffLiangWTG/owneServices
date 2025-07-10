using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanVesselDataValidation : AutoStowPlanVesselDataValidation
	{
		public StowPlanVesselDataValidation(AutoStowPlanVesselData parent)
			: base(parent)
		{ }

		protected override void CheckIMONumber()
		{
			base.CheckIMONumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.IMONumberInfo);
		}

		protected override void CheckVesselName()
		{
			base.CheckVesselName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.VesselNameInfo);
		}

		protected override void CheckVesselOperator()
		{
			base.CheckVesselOperator();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.VesselOperatorInfo);
		}
	}
}
