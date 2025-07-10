using System.Drawing;

namespace System.Windows.Forms;

[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private member", Justification = "Referenced in ToolStripButton.razor")]
public partial class ToolStripButton : ToolStripItem, IButtonControl
{
	public ToolStripButton()
	{
	}

	public ToolStripButton(string? text) : base(text, null, null)
	{
	}

	public ToolStripButton(string? text, Image? image, EventHandler? onClick) : base(text, image, onClick)
	{
	}

	protected virtual bool IsLeftButtonDown => MouseButtons == MouseButtons.Left;

	bool isPerformClicking;

	protected override void OnClick(EventArgs e)
	{
		if (Enabled && IsLeftButtonDown || isPerformClicking)
		{
			if (CheckOnClick)
			{
				Checked = !Checked;
			}

			base.OnClick(e);
		}
	}

	public override void PerformClick()
	{
		if (Enabled)
		{
			isPerformClicking = true;
			OnClick(EventArgs.Empty);
			isPerformClicking = false;
		}
	}

	protected override Task OnClickCoreAsync(WebMouseEventArgs e)
	{
		if (isHandlingClick)
		{
			return Task.CompletedTask;
		}

		isHandlingClick = true;
		return base.OnClickCoreAsync(e).ContinueWith(t => isHandlingClick = false, TaskScheduler.Default);
	}

	bool isHandlingClick;

	protected virtual void OnCheckedChanged(EventArgs e)
	{
		CheckedChanged?.Invoke(this, e);
	}

	public override bool Checked
	{
		get => base.Checked;
		set
		{
			if (base.Checked != value)
			{
				base.Checked = value;
				OnCheckedChanged(EventArgs.Empty);
			}
		}
	}

	protected override bool DefaultAutoToolTip => true;

	public override bool UseParentDivForLayout => false;

	public bool CheckOnClick { get; set; }

	public CheckState CheckState { get; set; }

	public DialogResult DialogResult { get; set; }

	public void NotifyDefault(bool value)
	{
	}

	public event EventHandler? CheckedChanged;

	public event EventHandler? CheckStateChanged;

	const int StandardButtonWidth = 23;
	public override Size GetPreferredSize(Size constrainingSize)
	{
		var prefSize = base.GetPreferredSize(constrainingSize);
		prefSize.Width = Math.Max(prefSize.Width, StandardButtonWidth);
		return prefSize;
	}

	int buttonImageWidth;
	int buttonImageHeight;

	/// <summary>
	///  Inheriting classes should override this method to handle this event.
	/// </summary>
	protected override void OnPaint(PaintEventArgs e)
	{
		if (Owner is null)
		{
			return;
		}

		if (ShowImage)
		{
			var imageSize = InternalLayout.ImageRectangle;
			buttonImageWidth = imageSize.Width;
			buttonImageHeight = imageSize.Height;
		}

		ToolStripRenderer renderer = Renderer!;
		renderer.DrawButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		if (CanProcessMnemonic() && IsMnemonic(charCode, Text))
		{
			PerformClick();
			return true;
		}

		return base.ProcessMnemonic(charCode);
	}

	bool ShowImage => Image != null && ((DisplayStyle & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image);
}
