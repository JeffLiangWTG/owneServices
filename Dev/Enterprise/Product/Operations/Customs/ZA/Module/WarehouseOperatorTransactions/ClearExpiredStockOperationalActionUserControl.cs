namespace Enterprise.Customs.ZA.Module
{
	public partial class ClearExpiredStockOperationalActionUserControl : LockingOperationalActionUserControl
	{
		public ClearExpiredStockOperationalActionUserControl()
		{
			InitializeComponent();
		}

		protected override void UpdateGuiForLockStatus(bool isLockedToCurrentOp)
		{
			LockInfoLabel.Visible = !isLockedToCurrentOp;
			TransactionsDisplayGrid.Visible = isLockedToCurrentOp;
		}
	}
}
