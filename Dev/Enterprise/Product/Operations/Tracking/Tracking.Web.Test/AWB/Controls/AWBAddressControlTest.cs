using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBAddressControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBAddressControl();
		}
	}
}
