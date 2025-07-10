namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class WhsReceiveDataFormatterTest : WhsDocketDataFormatterTest
	{
		#region Implementation

		protected override WhsDocketDataFormatter GetNewDocketDataFormatter()
		{
			return new WhsReceiveDataFormatter();
		}

		#endregion
	}
}