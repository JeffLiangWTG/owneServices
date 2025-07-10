using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBCarrierSignatureControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBCarrierSignatureControl();
		}
	}
}
