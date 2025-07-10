using WinzorFramework;

namespace System.Windows.Forms;

public partial class ToolBar
{
	public class ToolBarButtonCollection : WrappedList<ToolBarButton>
	{
		public ToolBarButtonCollection()
		{
		}

		public Control? this[string key] => null;
	}
}
