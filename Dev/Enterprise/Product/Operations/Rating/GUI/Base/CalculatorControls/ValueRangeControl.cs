namespace Enterprise.Rating.GUI
{
	public partial class ValueRangeControl : RateCalculatorUserControl
	{
		public ValueRangeControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			base.SetBindings();

			SetBinding(ApplyToDropEdit, "String1");
			SetListBinding(ApplyToDropEdit, "List1");
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
