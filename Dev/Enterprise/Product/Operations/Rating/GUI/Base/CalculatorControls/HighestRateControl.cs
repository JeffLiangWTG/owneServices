using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public partial class HighestRateControl : RateCalculatorUserControl
	{
		public HighestRateControl()
		{
			InitializeComponent();
		}

		protected override void SetBindings()
		{
			base.SetBindings();

			SetBinding(UnitAsFreightedDropEdit, "String1");
			SetListBinding(UnitAsFreightedDropEdit, "List1");
		}

		internal override void OnSwitched(Calculator calculator)
		{
			UnitAsFreightedDropEdit.Visible = ((HighestRateCalculator)calculator).EnableAsFreightedMode;
		}
	}
}
