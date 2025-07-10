using System.Windows.Forms;

namespace WinzorFramework.Extensions;

internal static class KeysExtension
{
	public static bool ProducesVisibleChar(this Keys key, bool comboKey) =>
		!comboKey &&
		((key >= Keys.D0 && key <= Keys.Z) ||
		(key >= Keys.NumPad0 && key <= Keys.Divide) ||
		(key >= Keys.OemSemicolon && key <= Keys.OemBackslash));
}
