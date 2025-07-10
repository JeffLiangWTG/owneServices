namespace Enterprise.TransportConsignment.GUI.Testing
{
	public class CarrierFilterControlTest : DtbChildFilterControlTest
	{
		#region Implementation 

		protected override DtbChildFilterControl GetNewFilterControl()
		{
			return new DtbCarrierFilterControl();
		}

		#endregion
	}
}
