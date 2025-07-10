using System.Web.UI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class AWBDropEditListBoxTest : AWBFieldControlTest
	{
		protected override Control GetNewControl()
		{
			return new AWBDropEditListBox();
		}
	}
}
