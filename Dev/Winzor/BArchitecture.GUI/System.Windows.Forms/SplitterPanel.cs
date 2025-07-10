using System.Drawing;

namespace System.Windows.Forms;

public partial class SplitterPanel : Panel
{
	public SplitterPanel(SplitContainer owner) : base()
	{
		Owner = owner;
	}

	internal SplitContainer Owner { get; }

	protected override string ClassName => "splitterpanel";

	internal bool Collapsed { get; set; }

	protected override Padding DefaultMargin => new Padding(0, 0, 0, 0);

	public override AutoSizeMode AutoSizeMode
	{
		get => AutoSizeMode.GrowOnly;
		set { }
	}

	public new int Height
	{
		get => Collapsed ? 0 : base.Height;
		set => throw new NotSupportedException(SR.SplitContainerPanelHeight);
	}

	internal int HeightInternal
	{
		get => ((Panel)this).Height;
		set => ((Panel)this).Height = value;
	}

	public new int Width
	{
		get => Collapsed ? 0 : base.Width;
		set => throw new NotSupportedException(SR.SplitContainerPanelWidth);
	}

	internal int WidthInternal
	{
		get => base.Width;
		set => base.Width = value;
	}

	public new Size Size
	{
		get => Collapsed ? Size.Empty : base.Size;
		set => base.Size = value;
	}
}
