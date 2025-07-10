namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageBehaviorStrategyProvider : CommonCartageBehaviorStrategyProvider, Integration.ICartageBehaviorStrategyProvider
	{
		public CartageBehaviorStrategyProvider()
			: base()
		{
		}

		public override CommonBookedCtgMoveBehaviorStrategy BookedCtgMoveBehaviorStrategy
		{
			get { return new CartageBookedCtgMoveBehaviorStrategy(); }
		}

		public override CommonCartageBehaviorStrategy CartageBehaviorStrategy
		{
			get { return new CartageCartageBehaviorStrategy(); }
		}

		public override CommonCartageBehaviorStrategy InternalCartageBehaviorStrategy
		{
			get { return new CartageInternalCartageBehaviorStrategy(); }
		}

		public override CommonCartageLegBehaviorStrategy CartageLegBehaviorStrategy
		{
			get { return new CartageCartageLegBehaviorStrategy(); }
		}

		public override CommonWorkSheetBehaviorStrategy WorkSheetBehaviorStrategy
		{
			get { return new CartageWorkSheetBehaviorStrategy(); }
		}
	}
}
