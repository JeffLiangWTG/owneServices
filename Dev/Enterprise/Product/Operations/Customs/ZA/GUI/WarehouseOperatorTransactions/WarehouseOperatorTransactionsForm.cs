using System.Drawing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class WarehouseOperatorTransactionsForm : ZTemplateForm, IPreviousNextControlProvider
	{
		public WarehouseOperatorTransactionsForm(CusWHSOperatorTransaction businessEntity) : base(businessEntity)
		{
			ControllerID = ZAControllerIDs.WarehouseOperatorTransactions;
			NotesTabPage.TabVisible = false;
			LogsTabPage.TabVisible = false;

			receiptsGrid.Visible = CusWHSOperatorTransaction.IsReceipt;
			ordersGrid.Visible = CusWHSOperatorTransaction.IsOrder;
			CancelledByTextBox.Visible = CusWHSOperatorTransaction.IsACancelledOrder;
			CancelledDateEdit.Visible = CusWHSOperatorTransaction.IsACancelledOrder;
			
			SaveButtonUserControl.AllowOutsideOfParent();
		}

		protected CusWHSOperatorTransaction CusWHSOperatorTransaction => (CusWHSOperatorTransaction)BusinessEntity;

		protected override bool SupportsEDocs => false;

		public override Size MinimumSize
		{
			get { return CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 342); }
			set { base.MinimumSize = value; }
		}
	}
}
