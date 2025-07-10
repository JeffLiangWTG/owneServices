namespace System.Windows.Forms;

public static class MessageBox
{
	public static DialogResult Show(Control? control, string? text) => Show(text);

	public static DialogResult Show(Control? control, string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon) => Show(text, caption, buttons, icon);

	public static DialogResult Show(Control? control, string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton) => Show(text, caption, buttons, icon, defaultButton);

	public static DialogResult Show(Control? control, string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options) => Show(text, caption, buttons, icon, defaultButton, options);

	public static DialogResult Show(string? text) => DialogResult.Cancel;

	public static DialogResult Show(string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon) => DialogResult.Cancel;

	public static DialogResult Show(string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton) => DialogResult.Cancel;

	public static DialogResult Show(string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options) => DialogResult.Cancel;
}
