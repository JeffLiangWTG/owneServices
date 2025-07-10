namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsPickICartageParentTest : WhsOrderICartageTest
	{
		#region Implementation

		protected override ITransportJobLinkProvider GetTransportLinkProvider(WhsOrder order) => order.Pick;

		#endregion
	}
}
