using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBRequestedFlightControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBRequestedFlightControl();
		}
	}
}
