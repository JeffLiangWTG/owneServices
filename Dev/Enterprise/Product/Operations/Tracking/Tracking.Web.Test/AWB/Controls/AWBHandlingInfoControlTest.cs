using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBHandlingInfoControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBHandlingInfoControl();
		}
	}
}
