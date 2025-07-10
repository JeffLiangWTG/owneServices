using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBNumericTextBoxTest : AWBFieldControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBNumericTextBox();
		}
	}
}
