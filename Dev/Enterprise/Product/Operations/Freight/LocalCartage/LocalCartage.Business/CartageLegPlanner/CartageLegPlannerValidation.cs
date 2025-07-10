namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageLegPlannerValidation : AutoCartageLegPlannerValidation
	{
		public CartageLegPlannerValidation(AutoCartageLegPlanner parent)
			: base(parent) { }

		public new CartageLegPlanner Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (CartageLegPlanner)base.Parent; }
		}
	}
}
