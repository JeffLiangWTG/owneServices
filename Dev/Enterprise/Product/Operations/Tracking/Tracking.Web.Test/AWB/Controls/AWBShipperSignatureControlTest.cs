using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBShipperSignatureControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBShipperSignatureControl();
		}
	}
}
