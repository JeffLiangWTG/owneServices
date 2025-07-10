using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public partial class InvoiceLineChargesUserControl : ZUserControl
	{
		public InvoiceLineChargesUserControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				ChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGSTApplies);
				ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGSTApplies);
			}
		}

		protected virtual string ColumnTitleForGSTApplies
		{
			get { return BaseCustomsSupplierHeaderUserControl.IsGSTApplicableCaption; }
		}

		public void ControlVisibilityOfChargesGrid(bool isVisible)
		{
			if (isVisible)
			{
				SplitContainer.Panel1Collapsed = false;
				SplitContainer.Panel1.Show();
			}
			else
			{
				SplitContainer.Panel1Collapsed = true;
				SplitContainer.Panel1.Hide();
			}
		}

		public void RemoveColumn(string columnName)
		{
			ChargesGrid.RemoveFromAvailableColumns(columnName);
			ApportionedChargesGrid.RemoveFromAvailableColumns(columnName);
		}
	}
}
