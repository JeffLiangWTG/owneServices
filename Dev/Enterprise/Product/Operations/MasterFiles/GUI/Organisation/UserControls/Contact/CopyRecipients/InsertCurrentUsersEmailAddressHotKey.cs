using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Grid.Internal;

namespace Enterprise.MasterFiles.GUI
{
	static class InsertCurrentUsersEmailAddressHotKey
	{
		public static void Register(ZCustomControlColumnStyle columnStyle, IGridControl gridControl)
		{
			columnStyle.Hotkeys.RegisterHotKey(Keys.Control | Keys.E, GetProcessor(gridControl), Res.GetString("8d6c4ed6-4117-4db2-9dcb-58276400e078", "Add your email address"));
		}

		public static HotKeyPressedProcessor GetProcessor(IGridControl gridControl)
		{
			return (sender, key) =>
			{
				var columnStyle = (ZCustomControlColumnStyle)sender;
				if (columnStyle.EditControl.GetReadOnly() || !columnStyle.EditControl.Enabled)
				{
					return false;
				}

				var emailToAdd = Env.CurrentUser.EmailAddress;
				if (string.IsNullOrEmpty(emailToAdd))
				{
					Globals.Message.ShowInformation(Res.GetString("d776f79e-cea4-4d16-9cbd-8673f0a585f1", "Your email address has not been set"));
					return true;
				}

				var controlToBeEdited = gridControl;
				if (gridControl is ZMultiCombinationControl multiCombinationControl)
				{
					controlToBeEdited = multiCombinationControl.CurrentEditor;
				}

				if (controlToBeEdited.SelectionLength > 0)
				{
					var highlightedText = controlToBeEdited.Text.Substring(controlToBeEdited.SelectionStart, controlToBeEdited.SelectionLength);
					var notHighlightedText = controlToBeEdited.Text.Replace(highlightedText, string.Empty);

					if (!notHighlightedText.Contains(emailToAdd))
					{
						controlToBeEdited.Text = controlToBeEdited.Text.Replace(highlightedText, emailToAdd);
						controlToBeEdited.SelectionStart = controlToBeEdited.Text.Length;

						return true;
					}
				}

				var emails = controlToBeEdited.Text
					.Split(',')
					.Select(s => s.Trim())
					.Where(s => !string.IsNullOrEmpty(s))
					.ToList();

				if (!emails.Contains(emailToAdd))
				{
					emails.Add(emailToAdd);

					controlToBeEdited.Text = string.Join(", ", emails);
					controlToBeEdited.SelectionStart = controlToBeEdited.Text.Length;

					return true;
				}

				return false;
			};
		}
	}
}
