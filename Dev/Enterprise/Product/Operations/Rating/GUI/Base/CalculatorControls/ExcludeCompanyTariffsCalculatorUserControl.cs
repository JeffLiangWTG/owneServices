using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public partial class ExcludeCompanyTariffsCalculatorUserControl : RateCalculatorUserControl
	{
		public ExcludeCompanyTariffsCalculatorUserControl()
		{
			InitializeComponent();
		}

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
		ZLabel descriptionLabel;
	}
}
