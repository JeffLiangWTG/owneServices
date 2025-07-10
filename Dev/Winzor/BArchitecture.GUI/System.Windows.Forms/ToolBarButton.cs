using System.Drawing;

namespace System.Windows.Forms;

public class ToolBarButton : IDisposable
{
	public ToolBarButton()
	{
	}

	public ToolBarButton(string text)
	{
		Text = text;
	}

	public string Text
	{
		get => text;
		set
		{
			UpdateProperty(ref text, value);
		}
	}
	string text = string.Empty;

	public ToolBarButtonStyle Style { get; set; } = ToolBarButtonStyle.PushButton;

	public string ToolTipText { get; set; } = string.Empty;

	public bool Visible { get; set; } = true;

	public bool Enabled { get; set; } = true;

	public int ImageIndex {
		get => imageIndex;
		set => UpdateProperty(ref imageIndex, value);
	}
	int imageIndex = -1;

	public Menu? DropDownMenu { get; set; }

	public string Name { get; set; } = string.Empty;

	public object? Tag { get; set; }

	public bool Pushed {
		get => pushed;
		set
		{
			UpdateProperty(ref pushed, value);
		}
	}
	bool pushed;

	public Rectangle Rectangle { get; }

	public void Dispose()
	{
	}

	internal Action Refresh { get; set; } = () => { };

	protected void UpdateProperty<T>(ref T field, T value)
	{
		if (!field?.Equals(value) ?? value != null)
		{
			field = value;
			Refresh();
		}
	}
}
