using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class GlobalTariffsHeaderUserControl : ZUserControl
	{
		public GlobalTariffsHeaderUserControl()
		{
			InitializeComponent();
		}

		public bool CompanyTariffDiscountVisible
		{
			get { return DiscountCalcEdit.Visible; }
			set
			{
				DiscountCalcEdit.Visible = value;
				DiscountTypeDropEdit.Visible = value;
				DiscountPercentSignLabel.Visible = value;
				ServiceLevelCodeFindBox.Visible = value;
			}
		}

		#region IDisposable Members

		readonly System.ComponentModel.Container components;

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
