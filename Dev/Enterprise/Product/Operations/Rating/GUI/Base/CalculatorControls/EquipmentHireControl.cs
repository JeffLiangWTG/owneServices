namespace Enterprise.Rating.GUI
{
	public partial class EquipmentHireControl : RateCalculatorUserControl
	{
		public EquipmentHireControl()
		{
			InitializeComponent();
		}

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
