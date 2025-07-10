using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class QuotationDateAndNumberControl : ZUserControl
	{
		public QuotationDateAndNumberControl()
		{
			InitializeComponent();
			CancellationReasonDropEdit.Visible = false;
		}

		public void ShowQuoteCancellationReasonDropEdit()
		{
			CancellationReasonDropEdit.Visible = true;
			QuotationStartDateEdit.Visible = false;
			QuotationEndDateEdit.Visible = false;
		}

		#region Dispose

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
