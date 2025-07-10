namespace Enterprise.Customs.ZA.Module
{
	public partial class ExportOperationalActionUserControl : LockingOperationalActionUserControl
	{
		public ExportOperationalActionUserControl()
		{
			InitializeComponent();
		}

		protected override void UpdateGuiForLockStatus(bool isLockedToCurrentOp)
		{
			LockInfoLabel.Visible = !isLockedToCurrentOp;
			TopPanel.Visible = isLockedToCurrentOp;
			TransactionsDisplayGrid.Visible = isLockedToCurrentOp;
		}
	}
}
