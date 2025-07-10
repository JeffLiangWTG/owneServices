namespace Enterprise.Rating.GUI
{
	public partial class NoteCalculatorUserControl : RateCalculatorUserControl
	{
		public NoteCalculatorUserControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			base.SetBindings();

			SetBinding(ShowWithoutPrefixCheckBox, "Bool1");
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
