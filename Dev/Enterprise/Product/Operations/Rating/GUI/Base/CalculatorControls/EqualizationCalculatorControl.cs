using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public partial class EqualizationCalculatorControl : RateCalculatorUserControl
	{
		public EqualizationCalculatorControl()
		{
			InitializeComponent();
		}

		#region Binding

		protected override void SetBindings()
		{
			base.SetBindings();
			SetBinding(UseInclusiveBreaksCheckBox, "Bool1");
			SetBinding(contractTypeHint, "String1");
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

		internal class EqualizationGrid : ZGrid
		{
			protected override void EnableNewRows()
			{
				//Do nothing
			}

			protected override void DisableNewRows()
			{
				//Do nothing
			}
		}
	}
}

