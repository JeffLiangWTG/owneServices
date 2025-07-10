using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBCarrierControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBCarrierControl();
		}
	}
}
