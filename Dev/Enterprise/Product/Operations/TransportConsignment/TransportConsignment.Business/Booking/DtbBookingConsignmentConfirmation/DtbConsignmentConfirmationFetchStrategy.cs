using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentConfirmationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbConsignmentConfirmationFetchStrategy(DtbConsignmentConfirmation confirmation)
			: base(confirmation)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			// Route Planner : Drivers > Run Sheets > Instructions > Confirmations > Instructions > Address
			Factory.AddFetchHint(typeof(DtbConsignmentInstruction), Confirmation.KK_KN_BookingInstruction);
		}

		DtbConsignmentConfirmation Confirmation
		{
			get { return (DtbConsignmentConfirmation)BusinessObject; }
		}
	}
}
