using System;
using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class InvoiceGroupingUserControl : BaseInvoiceGroupingUserControl
	{
		public InvoiceGroupingUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			RemoveColumn();
		}

		protected void RemoveColumn()
		{
			var theColumn = GroupChargeGrid.Columns[InvoiceCharge.Schema.J7_IsGSTApplicable];
			if (theColumn != null)
			{
				GroupChargeGrid.Columns.Remove(InvoiceCharge.Schema.J7_IsGSTApplicable);
				GroupChargeGrid.RefreshTableStyles();
			}
		}

		#region Exposing for unit test

		internal ZGrid GroupChargeGridForUnitTest
		{
			get { return GroupChargeGrid; }
		}

		#endregion
	}
}

