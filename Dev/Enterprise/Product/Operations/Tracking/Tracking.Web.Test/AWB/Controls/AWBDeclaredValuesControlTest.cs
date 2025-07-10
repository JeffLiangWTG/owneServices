using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBDeclaredValuesControlTest : AWBBaseControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBDeclaredValuesControl();
		}
	}
}
