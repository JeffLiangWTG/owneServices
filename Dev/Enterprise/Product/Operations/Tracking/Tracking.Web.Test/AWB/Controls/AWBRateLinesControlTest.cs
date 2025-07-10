using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBRateLinesControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBRateLinesControl();
		}
	}
}
