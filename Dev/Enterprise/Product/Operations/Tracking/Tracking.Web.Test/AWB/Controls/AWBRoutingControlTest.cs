using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBRoutingControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBRoutingControl();
		}
	}
}
