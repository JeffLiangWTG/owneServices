using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBDestinationControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBDestinationControl();
		}
	}
}
