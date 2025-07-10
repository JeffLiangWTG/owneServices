using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms.Adaptive;

public class PreviewBox : Control
{
	public PreviewBoxSizeMode SizeMode { get; set; } = PreviewBoxSizeMode.Normal;

	public Control? PreviewControl
	{
		get => previewControl;
		set
		{
			if (previewControl != null)
			{
				WinzorSpecificControls.Remove(previewControl);
			}
			previewWidth = value?.Width ?? 0;
			previewHeight = value?.Height ?? 0;
			UpdateProperty(ref previewControl, value);
			if (previewControl is not null)
			{
				WinzorSpecificControls.Add(previewControl);
			}
		}
	}
	Control? previewControl;
	int previewWidth;
	int previewHeight;

	Rectangle PreviewRectangleFromSizeMode(PreviewBoxSizeMode mode)
	{
		var result = LayoutUtils.DeflateRect(ClientRectangle, Padding);
		if (previewControl != null)
		{
			switch (mode)
			{
				case PreviewBoxSizeMode.Normal:
				case PreviewBoxSizeMode.AutoSize:
					result.Size = previewControl.Size;
					break;
				case PreviewBoxSizeMode.Center:
					result.X += (result.Width - previewControl.Width) / 2;
					result.Y += (result.Height - previewControl.Height) / 2;
					result.Size = previewControl.Size;
					break;
				case PreviewBoxSizeMode.Zoom:
					var size = previewControl.Size;
					var num = Math.Min(ClientRectangle.Width / (float)previewWidth, ClientRectangle.Height / (float)previewHeight);
					result.Width = (int)(previewWidth * num);
					result.Height = (int)(previewHeight * num);
					result.X = (ClientRectangle.Width - result.Width) / 2;
					result.Y = (ClientRectangle.Height - result.Height) / 2;
					break;
			}
		}
		return result;
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		if (previewControl != null)
		{
			var rect = PreviewRectangleFromSizeMode(SizeMode);
			previewControl.Size = new Size(rect.Width, rect.Height);
			previewControl.Top = rect.Y;
			previewControl.Left = rect.X;
			previewControl.NotifyRenderRequired();
		}
		base.OnPaint(pe);
	}
}
