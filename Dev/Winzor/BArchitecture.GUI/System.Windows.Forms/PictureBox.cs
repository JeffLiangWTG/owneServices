using System.ComponentModel;
using System.Drawing;
using WinzorFramework.Extensions;

namespace System.Windows.Forms;

[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private member", Justification = "Referenced in PictureBox.razor")]
public partial class PictureBox : Control, ISupportInitialize
{
	BorderStyle _borderStyle = BorderStyle.None;

	public PictureBox()
	{
		SetStyle(ControlStyles.Selectable, false);
		TabStop = false;
	}

	public Image? Image
	{
		get => image;
		set
		{
			imageWidth = value?.Width ?? 0;
			imageHeight = value?.Height ?? 0;
			imageSrc = value?.ToBase64DataUrl() ?? string.Empty;
			UpdateProperty(ref image, value);
		}
	}
	Image? image;
	int imageWidth;
	int imageHeight;
	string imageSrc = string.Empty;

	public Image? ErrorImage { get; set; }

	public PictureBoxSizeMode SizeMode
	{
		get => sizeMode;
		set => UpdateProperty(ref sizeMode, value);
	}
	PictureBoxSizeMode sizeMode = PictureBoxSizeMode.Normal;

	public BorderStyle BorderStyle
	{
		get => _borderStyle;
		set
		{
			if (UpdateProperty(ref _borderStyle, value))
			{
				// Border style might have changed ClientSize
				UpdateBounds(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
			}
		}
	}

	protected override Size DefaultSize => new Size(100, 50);

	public Image? InitialImage { get; set; }

	public void BeginInit()
	{
	}

	public void EndInit()
	{
	}

	internal override void AdjustWindowRectEx(ref Interop.RECT rect)
	{
		// In regular WinForms, the pictureBox maintains its width but contracts its client area by 1px / 2px, depending on the border style

		// The current CSS for this control is implemented slightly different, so the below adjustments reflect the CSS

		base.AdjustWindowRectEx(ref rect);

		switch (BorderStyle)
		{
			case BorderStyle.FixedSingle:
				// img tag has 1px border
				rect = new Interop.RECT(rect.left - 1, rect.top - 1, rect.right + 1, rect.bottom + 1);
				break;
			case BorderStyle.Fixed3D:
				// img tag has 1px border
				// Outer div has 1px border
				rect = new Interop.RECT(rect.left - 2, rect.top - 2, rect.right + 2, rect.bottom + 2);

				// Outer div border moves the box by 1px in x and y
				rect = new Interop.RECT(rect.left - 1, rect.top - 1, rect.right - 1, rect.bottom - 1);
				break;
		}
	}
}
