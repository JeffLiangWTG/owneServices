using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public static class ClickableUserControlHelper
	{
		public static void SetUserControlClickable(this ZUserControl userControl, ZPanel contentPanel, Action<object, EventArgs> screenedPartyListItemUserControl_Click, Func<ZUserControl, (Color RawBackColor, Color RawBorderColor)> getRawBackAndBorderColor)
		{
			var controls = new List<Control>()
			{
				userControl
			};

			GetAllChildControls(userControl, controls);
			foreach (var control in controls)
			{
				control.MouseHover += (s, e) =>
				{
					contentPanel.BackColor = WinformConstants.SelectedColor;
					userControl.BackColor = WinformConstants.BorderColor;
				};

				control.MouseLeave += (s, e) =>
				{
					if (!IsMouseHover(userControl))
					{
						var backAndBorderColor = getRawBackAndBorderColor(userControl);
						contentPanel.BackColor = backAndBorderColor.RawBackColor;
						userControl.BackColor = backAndBorderColor.RawBorderColor;
					}
				};

				control.Click += (s, e) =>
				{
					screenedPartyListItemUserControl_Click(userControl, e);
				};
			}
		}

		static bool IsMouseHover(ZUserControl container)
		{
			return container.DisplayRectangle.Contains(container.PointToClient(Control.MousePosition));
		}

		public static void SetSelected(this ZUserControl userControl, ZPanel contentPanel)
		{
			contentPanel.BackColor = WinformConstants.SelectedColor;
			userControl.BackColor = WinformConstants.BorderColor;
		}

		public static void SetUnselected(this ZUserControl userControl, ZPanel contentPanel)
		{
			contentPanel.BackColor = WinformConstants.UnselectedColor;
			userControl.BackColor = WinformConstants.UnselectedColor;
		}

		public static void GetAllChildControls(Control control, List<Control> controls)
		{
			foreach (Control childControl in control.Controls)
			{
				controls.Add(childControl);

				if (childControl.Controls.Count > 0)
				{
					GetAllChildControls(childControl, controls);
				}
			}
		}
	}
}
