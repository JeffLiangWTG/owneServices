using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Forms.Integration;

public partial class ElementHost : Control
{
	[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in ElementHost.razor")]
	const string UnsupportedText = "Please Tick the Disable WPF Forms Checkbox in Registry Module to View Content";

	[SuppressMessage("CodeQuality", "IDE0052:Remove unread private member", Justification = "Referenced in ElementHost.razor")]
	bool isChildWinzorSupported;

	public UserControl? Child
	{
		get { return WinzorSpecificControls.SingleOrDefault() as UserControl; }
		set
		{
			WinzorSpecificControls.Clear();
			if (value != null)
			{
				value.Size = Size;
				WinzorSpecificControls.Add(value);
			}

			isChildWinzorSupported = value is IWinzorSupportedWPFContent;
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnResize(e);
		if (Child != null)
		{
			Child.Size = Size;
		}
	}
}

public interface IWinzorSupportedWPFContent
{
}
