using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms;

public partial class ImageList : Component
{
	public ImageList()
	{
	}

	public ImageList(IContainer container)
	{
	}

	public ImageCollection Images => imageCollection ??= new ImageCollection();
	ImageCollection? imageCollection;

	public ImageListStreamer? ImageStream {
		get => Images.Empty ? null : new ImageListStreamer(this);
		set
		{
			Images.Clear();

			if (value is not null)
			{
				Images.AddRange(value.Images);
				ImageSize = value.ImageSize;
				ColorDepth = value.ColorDepth;
				Images.ResetKeys();
			}
		}
	}

	public Color TransparentColor { get; set; } = Color.Transparent;

	public ColorDepth ColorDepth { get; set; } = ColorDepth.Depth8Bit;

	public Size ImageSize { get; set; } = new Size(16, 16);
}
