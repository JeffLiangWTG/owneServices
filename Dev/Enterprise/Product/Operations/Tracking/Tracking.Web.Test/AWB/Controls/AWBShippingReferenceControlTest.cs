using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBShippingReferenceControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBShippingReferenceControl();
		}
	}
}
