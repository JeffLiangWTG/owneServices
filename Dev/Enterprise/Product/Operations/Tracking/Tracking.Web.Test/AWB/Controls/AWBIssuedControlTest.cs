using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBIssuedControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBIssuedControl();
		}
	}
}
