using System.Drawing;

namespace System.Windows.Forms;

public partial class ErrorProvider : IDisposable
{
	string error = string.Empty;

	public bool AnyError()
	{
		return !string.IsNullOrEmpty(this.error);
	}

	public string? NotificationType { get; set; }

	public Icon? Icon { get; set; }

	public ErrorBlinkStyle BlinkStyle { get; set; }

	public string GetError(Control control) => Error;

	public void SetError(Control control, string value)
	{
		Error = value;
	}

	public void SetIconAlignment(Control control, ErrorIconAlignment value)
	{
	}

	public void SetIconPadding(Control control, int padding)
	{
	}

	public new void Dispose()
	{
		Error = string.Empty;
		Icon?.Dispose();
	}

	string Error
	{
		get => error;
		set
		{
			if (value is null)
			{
				value = string.Empty;
			}

			if (error.Equals(value) && BlinkStyle != ErrorBlinkStyle.AlwaysBlink)
			{
				return;
			}

			error = value;
		}
	}
}
