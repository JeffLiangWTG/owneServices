using System.Windows.Forms;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	public static class GuiTestHelper
	{
		public static T FindControl<T>(Control.ControlCollection controls, string name)
			where T : Control
		{
			foreach (Control control in controls)
			{
				var controlAsT = control as T;
				if (controlAsT != null && controlAsT.Name == name)
				{
					return controlAsT;
				}

				var childControl = FindControl<T>(control.Controls, name);
				if (childControl != null)
				{
					return childControl;
				}
			}

			return null;
		}
	}
}
