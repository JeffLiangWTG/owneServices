namespace Enterprise.Warehouse.Transactions.Business
{
	public class DPSSecurityNonInteractiveProvider : IDPSSecurityProvider
	{
		public bool ValidateDPS(WhsDocket docket)
		{
			return !docket.IsDPSMovementRestricted();
		}
	}
}
