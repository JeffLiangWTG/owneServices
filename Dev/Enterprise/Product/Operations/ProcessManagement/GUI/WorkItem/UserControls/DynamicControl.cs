using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	/// <summary>
	/// Dynamic control creator that expands any collapsed parent splitters when it is shown.
	/// </summary>
	public class DynamicControl : ZDynamicControlCreationUserControl
	{
		protected override void SetVisibleCore(bool value)
		{
			if (value && !Visible)
			{
				var parent = Parent;
				while (parent != null)
				{
					var splitterPanel = parent as SplitterPanel;
					if (splitterPanel != null)
					{
						var splitter = splitterPanel.Parent as SplitContainer;
						if (splitter != null)
						{
							if (splitter.Panel2Collapsed && splitterPanel == splitter.Panel2)
							{
								splitter.Panel2Collapsed = false;
							}
							else if (splitter.Panel1Collapsed && splitterPanel == splitter.Panel1)
							{
								splitter.Panel1Collapsed = false;
							}
						}
					}
					parent = parent.Parent;
				}
			}

			base.SetVisibleCore(value);
		}
	}
}
