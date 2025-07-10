using System.Drawing;
using WinzorFramework;

namespace System.Windows.Forms;

public partial class ToolStripMenuItem : ToolStripDropDownItem, IWinzorMenuItem
{
	public override bool UseParentDivForLayout => false;

	protected override Padding DefaultMargin => Padding.Empty;

	public ToolStripMenuItem()
	{
	}

	public ToolStripMenuItem(string text)
	{
		Text = text;
	}

	public ToolStripMenuItem(string? text, Image? image, EventHandler? onClick)
	{
		Text = text;
		Image = image;
		Click += onClick;
	}

	public ToolStripMenuItem(string text, Image? image, EventHandler? onClick, string? name)
	{
		Text = text;
		Image = image;
		Click += onClick;
		Name = name;
	}

	bool showShortcutKeys = true;

	public bool ShowShortcutKeys
	{
		get => showShortcutKeys;
		set => showShortcutKeys = value;
	}

	public Keys ShortcutKeys { get; set; }

	public override void ShowShortcutString()
	{
		if (ShowShortcutKeys && ShortcutKeys != Keys.None)
		{
			ShortcutString = new KeysConverter().ConvertToString(ShortcutKeys) ?? string.Empty;
		}
	}

	public string? ShortcutString { get; private set; }

	public bool CheckOnClick { get; set; }

	public CheckState CheckState { get; set; }
	
	protected virtual void OnCheckedChanged(EventArgs e)
	{
		CheckedChanged?.Invoke(this, e);
	}

	public event EventHandler? CheckedChanged;

	internal void HandleAutoExpansion()
	{
		if (!Enabled || ParentInternal is null || !HasDropDownItems)
		{
			return;
		}

		ShowDropDown();

		DropDown.SelectNextToolStripItem(start: null, forward: true);
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

	/// <summary>
	///  An item is toplevel if it is parented to anything other than a ToolStripDropDownMenu
	///  This implies that a ToolStripMenuItem in an overflow IS a toplevel item
	/// </summary>
	internal bool IsTopLevel => ParentInternal as ToolStripDropDown is null;

	protected override bool ProcessCmdKey(ref Message m, Keys keyData)
	{
		if (Enabled && ShortcutKeys == keyData && !HasDropDownItems)
		{
			OnClick(EventArgs.Empty);
			return true;
		}

		return base.ProcessCmdKey(ref m, keyData);
	}

	internal bool ProcessCmdKeyInternal(ref Message m, Keys keyData)
	{
		return ProcessCmdKey(ref m, keyData);
	}

	protected override void OnClick(EventArgs e)
	{
		if (CheckOnClick)
		{
			Checked = !Checked;
		}
		if (IsTopLevel)
		{
			ShowDropDown();
		}
		base.OnClick(e);
	}

	public override void OnSelect()
	{
		DropDown.OnOpening();
		OnDropDownShow(EventArgs.Empty);
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		if (IsMnemonic(charCode, Text) && HasDropDownItems && CanProcessMnemonic())
		{
			Select();
			ShowDropDown();

			DropDown.SelectNextToolStripItem(null, /*forward=*/true);
			return true;
		}

		return base.ProcessMnemonic(charCode);
	}
}
