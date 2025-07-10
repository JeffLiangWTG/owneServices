namespace Enterprise.Rating.GUI
{
	public partial class TimeControl : BaseCombinedCalculatorControl
	{
		public TimeControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			base.SetBindings();

			SetBinding(ExcludeDropEdit, "String1");
			SetListBinding(ExcludeDropEdit, "List1");
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
