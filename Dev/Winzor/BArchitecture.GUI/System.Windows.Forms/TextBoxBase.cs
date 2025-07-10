using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms.Layout;
using Microsoft.AspNetCore.Components;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using static Interop;

namespace System.Windows.Forms;

public abstract class TextBoxBase : Control
{
	public TextBoxBase()
	{
		SetExtendedState(ExtendedStates.UserPreferredSizeCache, true);
		this.requestedHeight = Height;
		AutoSize = true;
	}

	protected virtual void OnReadOnlyChanged(EventArgs e)
	{
		ReadOnlyChanged?.Invoke(this, e);
	}

	public event EventHandler? ReadOnlyChanged;

	protected override void OnTextChanged(EventArgs e)
	{
		CommonProperties.xClearPreferredSizeCache(this);
		base.OnTextChanged(e);
	}
	protected virtual void OnSelectionChanged(EventArgs e)
	{
	}

	protected override Size DefaultSize => new Size(100, PreferredHeight);

	public int PreferredHeight
	{
		get
		{
			var height = FontHeight;
			if (borderStyle != BorderStyle.None)
			{
				height += SystemInformation.BorderSize.Height * 4 + 3;
			}

			return height;
		}
	}

	internal override void AdjustWindowRectEx(ref RECT rect)
	{
		base.AdjustWindowRectEx(ref rect);

		if (BorderStyle == BorderStyle.FixedSingle)
		{
			// div has 1px border
			rect = new RECT(rect.left - 1, rect.top - 1, rect.right + 1, rect.bottom + 1);
		}
		else if (BorderStyle == BorderStyle.Fixed3D)
		{
			// div has 1px border, 1px shadow
			rect = new RECT(rect.left - 2, rect.top - 2, rect.right + 2, rect.bottom + 2);
		}
	}

	internal override Size GetPreferredSizeCore(Size proposedConstraints)
	{
		var bordersAndPadding = SizeFromClientSize(Size.Empty) + Padding.Size;

		if (BorderStyle != BorderStyle.None)
		{
			bordersAndPadding += new Size(0, 3);
		}

		if (BorderStyle == BorderStyle.FixedSingle)
		{
			bordersAndPadding.Width += 2;
			bordersAndPadding.Height += 2;
		}

		proposedConstraints -= bordersAndPadding;

		var format = TextFormatFlags.NoPrefix;
		if (!Multiline)
		{
			format |= TextFormatFlags.SingleLine;
		}
		else if (WordWrap)
		{
			format |= TextFormatFlags.WordBreak;
		}

		var textSize = TextRenderer.MeasureText(Text, Font, proposedConstraints, format);

		textSize.Height = Math.Max(textSize.Height, FontHeight);
		var preferredSize = textSize + bordersAndPadding;
		return preferredSize;
	}

	public BorderStyle BorderStyle
	{
		get => borderStyle;
		set
		{
			if (UpdateProperty(ref borderStyle, value))
			{
				UpdateStyles();
				// Border style might have changed ClientSize
				this.UpdateBounds(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
				using (LayoutTransaction.CreateTransactionIf(AutoSize, Parent, this, PropertyNames.BorderStyle))
				{
					OnBorderStyleChanged(EventArgs.Empty);
				}
			}
		}
	}
	BorderStyle borderStyle = BorderStyle.Fixed3D;

	protected virtual void OnBorderStyleChanged(EventArgs e)
	{
		BorderStyleChanged?.Invoke(this, e);
	}

	public event EventHandler? BorderStyleChanged;

	int requestedHeight;

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		if ((specified & BoundsSpecified.Height) != 0)
		{
			this.requestedHeight = height;
		}
		if (AutoSize && !Multiline)
		{
			height = PreferredHeight;
		}

		base.SetBoundsCore(x, y, width, height, specified);
	}

	public virtual bool Multiline
	{
		get => multiLine;
		set
		{
			if (UpdateProperty(ref multiLine, value))
			{
				using (LayoutTransaction.CreateTransactionIf(AutoSize, Parent, this, PropertyNames.Multiline))
				{
					AdjustHeight(false);
					OnMultilineChanged(EventArgs.Empty);
				}
			}
		}
	}
	bool multiLine;

	protected virtual void OnMultilineChanged(EventArgs e)
	{
	}

	public virtual int MaxLength
	{
		get
		{
			return maxLength;
		}
		set
		{
			UpdateProperty(ref maxLength, value);
		}
	}
	int maxLength = 32767;

	public bool ReadOnly
	{
		get
		{
			return readOnly;
		}
		set
		{
			if (UpdateProperty(ref readOnly, value))
			{
				OnReadOnlyChanged(EventArgs.Empty);
			}
		}
	}
	bool readOnly;

	public bool WordWrap
	{
		get => wordWrap;
		set => UpdateProperty(ref wordWrap, value);
	}

	bool wordWrap = true;

	public bool AcceptsTab
	{
		get => acceptsTab;
		set => UpdateProperty(ref acceptsTab, value);
	}
	bool acceptsTab;

	public virtual int TextLength => Text.Length;

	public string[] Lines { get; set; } = Array.Empty<string>();

	public event EventHandler? HideSelectionChanged;

	internal bool hideSelection;

	/// <summary>
	///  Gets or sets a value indicating whether the selected
	///  text in the text box control remains highlighted when the control loses focus.
	/// </summary>
	public bool HideSelection
	{
		get => hideSelection;
		set
		{
			if (value != hideSelection)
			{
				hideSelection = value;
				OnHideSelectionChanged(EventArgs.Empty);
			}
		}
	}

	protected virtual void OnHideSelectionChanged(EventArgs e)
	{
		HideSelectionChanged?.Invoke(this, e);
	}

	public abstract ITextBoxBaseJSInterop? Interop { get; }

	// C# and Web have different new line symbols "\r\n" and '\n'
	// needs to compensate the different length of new line symbols when interact with the frontend
	int AdjustNewLineOffset(int position)
	{
		if (Multiline && position < Text.Length)
		{
			return Regex.Matches(Text.Substring(0, position), "\r\n").Count;
		}
		return 0;
	}

	internal int selectionStart;
	public int SelectionStart
	{
		get
		{
			AdjustSelectionStartAndEnd(selectionStart, selectionLength, out var start, out var end, TextLength);
			return start;
		}
		set
		{
			if (UpdateProperty(ref selectionStart, value))
			{
				WinzorOnSelectionChanged();

				var length = selectionLength;
				Select(value, length);
			}
		}
	}

	internal int selectionLength;
	public int SelectionLength
	{
		get
		{
			AdjustSelectionStartAndEnd(selectionStart, selectionLength, out var start, out var end, TextLength);
			return end - start;
		}
		set
		{
			if (UpdateProperty(ref selectionLength, value))
			{
				WinzorOnSelectionChanged();

				var start = selectionStart;
				Select(start, value);
			}
		}
	}

	internal virtual void WinzorOnSelectionChanged()
	{
		OnSelectionChanged(EventArgs.Empty);
	}

	protected async Task OnTextBoxSelectionChangedAsync(TextboxSelectionChangeEventArgs args)
	{
		if (!Enabled)
		{
			return;
		}

		var adjustedSelectionStart = args.SelectionStart + AdjustNewLineOffset(args.SelectionStart);
		var adjustedSelectionEnd = args.SelectionEnd + AdjustNewLineOffset(args.SelectionEnd);
		await InvokeWinzorDispatcherAsync(() =>
		{
			OnTextBoxSelectionChanged(adjustedSelectionStart, adjustedSelectionEnd - adjustedSelectionStart);
		});
	}

	// this needs to be accessed from the winzor thread so rich text box text changes can compete with button presses
	protected internal virtual void OnTextBoxSelectionChanged(int newStart, int newLength)
	{
		var start = SelectionStart;
		var length = SelectionLength;
		selectionStart = newStart;
		selectionLength = newLength;
		if (keyDownHandled || textHandled || supressKeyPress)
		{
			SelectionStart = start;
			SelectionLength = length;
			keyDownHandled = false;
		}
	}

	public virtual string SelectedText
	{
		get
		{
			AdjustSelectionStartAndEnd(selectionStart, selectionLength, out int start, out int end, TextLength);
			return Text.Substring(start, end - start);
		}
		set
		{
			if (SelectedText != value)
			{
				var start = SelectionStart;
				var end = SelectionStart + SelectionLength;
				SetSelectedTextInternal(value, start, end);
				ScrollToCaret();
			}
		}
	}

	void SetSelectedTextInternal(string? text, int start, int end)
	{
		if (start > end)
		{
			return;
		}

		text ??= string.Empty;

		var textLength = Text?.Length ?? 0;
		start = Math.Min(start, textLength);
		end = Math.Min(end, textLength);

		if (start < end)
		{
			Text = Text?.Remove(start, end - start).Insert(start, text) ?? text;
		}
		else
		{
			Text = Text?.Insert(start, text) ?? text;
		}
		selectionStart = start + text.Length;
		selectionLength = 0;
		RegisterAfterRenderAction(async () => await (Interop?.SetSelectionAsync(textboxReference, SelectionStart - AdjustNewLineOffset(SelectionStart), SelectionStart - AdjustNewLineOffset(SelectionStart)) ?? Task.CompletedTask));
	}

	public virtual void Select(int start, int length)
	{
		if (IsOnInput)
		{
			return;
		}
		var textLength = Text?.Length ?? 0;
		start = Math.Min(start, textLength);
		length = Math.Min(textLength - start, length);
		selectionStart = start;
		selectionLength = length;
		if (textLength > 0)
		{
			var end = start + length;
			RegisterAfterRenderAction(async () => await (Interop?.SetSelectionAsync(textboxReference, start - AdjustNewLineOffset(start), end - AdjustNewLineOffset(end)) ?? Task.CompletedTask));
		}

		NotifyRenderRequired();
	}

	public void DeselectAll()
	{
		SelectionLength = 0;
	}

	// Send in -1 if you don't have the text length cached
	// when calling this method. It will be computed. If not,
	// please pass in the text length as the last parameter.
	// This will avoid the expensive call to the TextLength
	// property.
	internal void AdjustSelectionStartAndEnd(int selStart, int selLength, out int start, out int end, int textLen)
	{
		start = selStart;
		end = 0;

		if (start <= -1)
		{
			start = -1;
		}
		else
		{
			int textLength;

			if (textLen >= 0)
			{
				textLength = textLen;
			}
			else
			{
				textLength = TextLength;
			}

			if (start > textLength)
			{
				start = textLength;
			}

			checked
			{
				try
				{
					end = start + selLength;
				}
				catch (OverflowException)
				{
					//Since we overflowed, cap at the max/min value: we'll correct the value below
					end = start > 0 ? int.MaxValue : int.MinValue;
				}
			}

			// Make sure end is in range
			if (end < 0)
			{
				end = 0;
			}
			else if (end > textLength)
			{
				end = textLength;
			}
		}
	}

	public virtual int GetLineFromCharIndex(int index) => -1;

	public int GetFirstCharIndexFromLine(int lineNumber) => -1;

	public virtual int GetCharIndexFromPosition(Point pt)
	{
		if (Text.Length == SelectionStart)
		{
			return SelectionStart - 1;
		}

		return SelectionStart;
	}

	protected bool keyDownHandled;

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		var producesVisibleChar = e.KeyCode.ProducesVisibleChar(e.Control || e.Alt) || e.KeyCode == Keys.Back || e.KeyCode == Keys.Space;
		keyDownHandled = e.Handled && !producesVisibleChar;
	}

	protected bool textHandled;

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		base.OnKeyPress(e);
		textHandled = e.Handled;
	}

	private protected override bool AllowNonCharKeyPress(Keys key) => base.AllowNonCharKeyPress(key) || key == Keys.Enter || key == Keys.Escape;

	protected override bool ProcessKeyEventArgs(ref Message m)
	{
		var isHandled = base.ProcessKeyEventArgs(ref m);
		if (isHandled)
		{
			textHandled = true;
		}

		return isHandled;
	}

	protected string? textBeforeHandle;
	protected int selectionStartBeforeHandle;
	protected int selectionLengthBeforeHandle;
	protected bool IsOnInput { get; set; }

	internal bool IsTextBoxContentChanged => textBeforeHandle != Text || selectionStartBeforeHandle != selectionStart || selectionLengthBeforeHandle != selectionLength;

	public void SelectAll()
	{
		Select(0, Text.Length);
	}

	public bool Modified
	{
		get => modified;
		set
		{
			if (UpdateProperty(ref modified, value))
			{
				OnModifiedChanged(EventArgs.Empty);
			}
		}
	}
	internal bool modified;

	protected virtual void OnModifiedChanged(EventArgs e)
	{
		ModifiedChanged?.Invoke(this, e);
	}

	public event EventHandler? ModifiedChanged;

	/// <summary>
	///  Ensures that the caret is visible in the TextBox window, by scrolling the
	///  TextBox control surface if necessary.
	///  Scrolling to any position in Multiline TextBox is not yet supported.
	/// </summary>
	public void ScrollToCaret()
	{
		RegisterAfterRenderAction(async () => await (Interop?.ScrollToCaretAsync(textboxReference, SelectionStart, SelectionStart + SelectionLength, Text.Length) ?? Task.CompletedTask));
		NotifyRenderRequired();
	}

	public void AppendText(string text)
	{
		AppendTextCore(text);
	}

	protected virtual void AppendTextCore(string text)
	{
		selectionStart = Text.Length;
		selectionLength = 0;
		SelectedText = text;
	}

	string CurrentText = string.Empty;
	internal string PreText = string.Empty;
	public bool CanUndo;

	public void Undo()
	{
		if (CanUndo)
		{
			CurrentText = Text;
			Text = PreText;
			PreText = CurrentText;
		}
	}

	/// <summary>
	///  Clears information about the most recent operation
	///  from the undo buffer of the text box.
	/// </summary>
	public void ClearUndo()
	{
		CanUndo = false;
		PreText = string.Empty;
	}

	public void Copy()
	{
		InvokeRenderDispatcher(async () => await (GetJSInterop<IClipboardJSInterop>()?.CopyAsync(ElementReference) ?? Task.CompletedTask), true);
	}

	public void Cut()
	{
		InvokeRenderDispatcher(async () => await (GetJSInterop<IClipboardJSInterop>()?.CutAsync(ElementReference) ?? Task.CompletedTask), true);
	}

	public void Paste()
	{
		InvokeRenderDispatcher(async () => await (GetJSInterop<IClipboardJSInterop>()?.PasteAsync(ElementReference) ?? Task.CompletedTask), true);
	}

	public void Clear()
	{
		Text = String.Empty;
	}

	public virtual Point GetPositionFromCharIndex(int index) => Point.Empty;

	protected virtual ElementReference textboxReference => ElementReference;

	protected override bool AllowTextChangeFromClient => base.AllowTextChangeFromClient && !ReadOnly;

	public override Color BackColor
	{
		get
		{
			if (ShouldSerializeBackColor())
			{
				return base.BackColor;
			}
			else if (ReadOnly)
			{
				return SystemColors.Control;
			}

			return SystemColors.Window;
		}
		set => base.BackColor = value;
	}

	public override bool AutoSize
	{
		get => autoSize;
		set
		{
			// Note that we intentionally do not call base.
			if (UpdateProperty(ref autoSize, value))
			{
				// AutoSize's effects are ignored for a multi-line textbox
				if (!Multiline)
				{
					AdjustHeight(false);
				}

				OnAutoSizeChanged(EventArgs.Empty);
			}
		}
	}
	bool autoSize = true;

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		AdjustHeight(false);
	}

	protected override void OnPaddingChanged(EventArgs e)
	{
		base.OnPaddingChanged(e);
		AdjustHeight(false);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		if (!IsHandleCreated)
		{
			return;
		}

		CommonProperties.xClearPreferredSizeCache(this);
		AdjustHeight(true);
	}

	void AdjustHeight(bool returnIfAnchored)
	{
		if (returnIfAnchored && (Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom))
		{
			return;
		}
		int requestedHeight = this.requestedHeight;
		try
		{
			if (AutoSize && !Multiline)
			{
				Height = PreferredHeight;
			}
			else
			{
				Height = requestedHeight;
			}
		}
		finally
		{
			this.requestedHeight = requestedHeight;
		}
	}

	protected override bool WantArrowKeys => true;

	protected override bool WantChars => true;

	/// <summary>
	///  Overridden to handle TAB key.
	/// </summary>
	protected override bool IsInputKey(Keys keyData)
	{
		if ((keyData & Keys.Alt) != Keys.Alt)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Tab:
					// Single-line RichEd's want tab characters (see WM_GETDLGCODE),
					// so we don't ask it
					return Multiline && AcceptsTab && ((keyData & Keys.Control) == 0);
				case Keys.Escape:
					if (Multiline)
					{
						return false;
					}
					break;
				case Keys.Back:
					if (!ReadOnly)
					{
						return true;
					}
					break;
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.Home:
				case Keys.End:
					return true;
					// else fall through to base
			}
		}

		return base.IsInputKey(keyData);
	}

	/// <summary>
	///  Provides some interesting information for the TextBox control in
	///  String form.
	/// </summary>
	public override string ToString()
	{
		var txt = Text;
		if (txt.Length > 40)
		{
			txt = txt.Substring(0, 40) + "...";
		}

		return "Text: " + txt;
	}
}
