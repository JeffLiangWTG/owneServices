namespace Enterprise.TransportConsignment.GUI.Testing
{
	public class DriverFilterControlTest : DtbChildFilterControlTest
	{
		#region Implementation

		protected override DtbChildFilterControl GetNewFilterControl()
		{
			return new DtbDriverFilterControl();
		}

		#endregion
	}
}
