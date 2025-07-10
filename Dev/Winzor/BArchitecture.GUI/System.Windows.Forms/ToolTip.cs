using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms;

public class ToolTip : Component
{
	public ToolTip()
	{
	}

	public ToolTip(IContainer cont)
	{
	}

	public bool Active { get; set; }

	public bool ShowAlways { get; set; }

	public bool IsBalloon { get; set; }

	public object? Tag { get; set; }

	public ToolTipIcon ToolTipIcon { get; set; }

	public string ToolTipTitle { get; set; } = string.Empty;

	public void SetToolTip(Control control, string caption)
	{
		var info = new TipInfo(caption, TipInfo.Type.Auto);
		SetToolTipInternal(control, info);
	}

	void SetToolTipInternal(Control control, TipInfo info)
	{
		if (control == null)
		{
			throw new ArgumentNullException(nameof(control));
		}

		if (info != null && !string.IsNullOrEmpty(info.Caption))
		{
			control.ToolTipText = info.Caption;
		}
	}

	public string? GetToolTip(Control? control)
	{
		if (control is null)
		{
			return string.Empty;
		}

		return control.ToolTipText;
	}

	public int InitialDelay { get; set; }

	public void Show(string text, Control window, Point point)
	{
	}

	public void Show(string text, Control window, Point point, int duration)
	{
	}

	public void Show(string text, IWin32Window window, int x, int y)
	{
	}

	public void Show(string text, IWin32Window window, int x, int y, int duration)
	{
	}

	public void Hide(Control window)
	{
	}

	class TipInfo
	{
		[Flags]
		public enum Type
		{
			None = 0x0000,
			Auto = 0x0001,
			Absolute = 0x0002,
			SemiAbsolute = 0x0004
		}

		public Type TipType { get; set; } = Type.Auto;
		string? _caption;
		readonly string? _designerText;
		public Point Position { get; set; }

		public TipInfo(string caption, Type type)
		{
			_caption = caption;
			TipType = type;
			if (type == Type.Auto)
			{
				_designerText = caption;
			}
		}

		public string? Caption
		{
			get => ((TipType & (Type.Absolute | Type.SemiAbsolute)) != 0) ? _caption : _designerText;
			set => _caption = value;
		}
	}
}
