namespace Enterprise.Customs.TW.Business
{
	public class AssignedCusClassPartPivotRefValidation : Customs.Business.CusClassPartPivotRefValidation
	{
		public AssignedCusClassPartPivotRefValidation(AssignedCusClassPartPivotRef parent)
			: base(parent)
		{
		}

		protected new AssignedCusClassPartPivotRef Parent => (AssignedCusClassPartPivotRef)base.Parent;
	}
}
