using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBTextBoxTest : AWBFieldControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBTextBox();
		}
	}
}
