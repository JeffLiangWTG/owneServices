using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

[DefaultBindingProperty("Text")]
public partial class TextBox : TextBoxBase
{
	public TextBox()
	{
	}

	protected internal virtual bool AutoComplete => false;
	protected internal List<string>? Suggestions { get; set; }

	string internalPlaceholderText = string.Empty;

	protected string InternalPlaceholderText
	{
		get => internalPlaceholderText;
		set => UpdateProperty(ref internalPlaceholderText, value);
	}
	string PlaceholderText => IsPassword ? new string(passwordChar, Text.Length) : internalPlaceholderText;

	public override bool UseParentDivForLayout => false;
	public override bool CaptureElementReference => true;

	public virtual bool IsDynamicMultilineTextBoxFormVisible => false;

	protected virtual bool HasDynamicExpandArrow => false;

	protected virtual void ShowDynamicMultilineTextBoxForm() { }

	protected virtual void DisposeDynamicMultilineItems() { }

	public Dictionary<char, char>? ReplacementCharacters;

	public ScrollBars ScrollBars { get; set; }

	public CharacterCasing CharacterCasing
	{
		get
		{
			return casing;
		}
		set
		{
			if (UpdateProperty(ref casing, value) && !string.IsNullOrEmpty(Text))
			{
				UpdateTextDirectly(Text);
				ResetSelection();
			}
		}
	}
	CharacterCasing casing;

	//  Keep it consistent with WinForm
	void ResetSelection()
	{
		SelectionStart = 0;
		SelectionLength = Text.Length;
	}

	public HorizontalAlignment TextAlign
	{
		get
		{
			return textAlign;
		}
		set
		{
			UpdateProperty(ref textAlign, value);
		}
	}
	HorizontalAlignment textAlign = HorizontalAlignment.Left;

	public AutoCompleteMode AutoCompleteMode { get; set; }

	public AutoCompleteSource AutoCompleteSource { get; set; }

	/// <summary>
	///  Overridden to handle RETURN key.
	/// </summary>
	protected override bool IsInputKey(Keys keyData)
	{
		if (Multiline && (keyData & Keys.Alt) == 0)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Return:
					return AcceptsReturn;
			}
		}

		return base.IsInputKey(keyData);
	}

	public bool AcceptsReturn { get; set; }

	public bool ExpandOnEdit { get; set; }

	public char PasswordChar
	{
		get
		{
			return passwordChar;
		}
		set
		{
			if (UpdateProperty(ref passwordChar, value) && IsPassword)
			{
				textForBinding = string.Empty;
			}
		}
	}
	char passwordChar;

	public bool UseSystemPasswordChar
	{
		get => useSystemPasswordChar;
		set
		{
			if (UpdateProperty(ref useSystemPasswordChar, value))
			{
				PasswordChar = value ? '●' : '\0';
			}
		}
	}
	bool useSystemPasswordChar;

	bool IsPassword => PasswordChar != '\0';

	public string MatchExpression { get; set; } = string.Empty;

	string InputType
	{
		get
		{
			if (IsPassword)
			{
				return "password";
			}
			else
			{
				return "text";
			}
		}
	}

	string CharacterCasingCssClass
	{
		get
		{
			var css = string.Empty;

			if (CharacterCasing == CharacterCasing.Lower)
			{
				css = "textbox--lower";
			}
			else if (CharacterCasing == CharacterCasing.Upper)
			{
				css = "textbox--upper";
			}

			return css;
		}
	}

	string Expand => ExpandOnEdit ? "textbox--expand" : string.Empty;

	[AllowNull]
	public override string Text
	{
		get => FormatTextWithCasing(base.Text);
		set
		{
			if (IsOnInput && !textHandled)
			{
				return;
			}
			// There is a bug (or maybe a feature?) in Blazor where setting the value of an input during an @oninput event handler
			// to be the same as the value prior to the event does not update correctly on the client. This is caused by the client
			// state and the server state falling out of sync. The client has a modified value while the server does not, so it does
			// not think the state of the component has been modified and will not rerender.
			// See https://github.com/dotnet/aspnetcore/issues/38656 for a similar issue.
			// If the text being set is the same as the previous value bound to the input, we will force update the value using JS
			var unchangedTextRequireUpdate = base.Text == value && IsElementReferenceCaptured;
			if (unchangedTextRequireUpdate && !IsPassword)
			{
				InvokeRenderDispatcher(async () =>
				{
					await (Interop?.SetTextContentAsync(textboxReference, Text) ?? Task.CompletedTask);
				}, needElementRendered: true);
			}
			value = FormatTextWithCasing(value ?? string.Empty);
			if (value != base.Text && IsHandleCreated)
			{
				selectionStart = 0;
				selectionLength = 0;
			}
			base.Text = value;
			selectionSet = false;
			// If this is a password field, then we should not be binding the new value to the input as it will be sent in plain text
			// Instead clear the bound value to show the placeholder, which will contain the masked value of the password
			textForBinding = FromTextboxToInputConverter(IsPassword ? string.Empty : value);
		}
	}

	protected internal async Task OnInputAsync(string? value)
	{
		var text = value ?? string.Empty;
		if (AllowTextChangeFromClient && (MaxLength == 0 || text.Length <= MaxLength))
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				try
				{
					IsOnInput = true;
					OnInput(value);
				}
				finally
				{
					IsOnInput = false;
				}
			});
		}
		else
		{
			textHandled = false;
		}
	}

	protected internal virtual void OnInput(string? value)
	{
		CanUndo = true;
		if (selectionStartBeforeHandle + 1 != selectionStart)
		{
			PreText = Text;
		}

		selectionStartBeforeHandle = selectionStart;
		selectionLengthBeforeHandle = selectionLength;

		if (value is not null && !textHandled && !supressKeyPress)
		{
			textBeforeHandle = Text;
			UpdateTextDirectly(FromInputToTextboxConverter(FormatTextWithCasing(value)));

			if (IsTextBoxContentChanged)
			{
				NotifyRenderRequired();
			}
		}
		else
		{
			InvokeRenderDispatcher(async () =>
			await (Interop?.SetTextAndSelectionAsync(
				textboxReference,
				Text,
				selectionStart,
				selectionStart + selectionLength) ?? Task.CompletedTask));
		}

		textBeforeHandle = null;
		textHandled = false;
	}

	string FormatTextWithCasing(string text)
	{
		var res = text;
		if (IsHandleCreated && !string.IsNullOrWhiteSpace(res))
		{
			if (CharacterCasing == CharacterCasing.Lower)
			{
				res = res.ToLowerInvariant();
			}
			else if (CharacterCasing == CharacterCasing.Upper)
			{
				res = res.ToUpperInvariant();
			}
		}

		return res;
	}

	internal string FromInputToTextboxConverter(string value)
	{
		if (this.Multiline && !value.Contains("\r\n"))
		{
			value = value.Replace("\n", "\r\n");
		}
		return value;
	}

	internal string FromTextboxToInputConverter(string value)
	{
		if (this.Multiline && !value.Contains("\r\n"))
		{
			value = value.Replace("\r\n", "\n");
		}
		return value;
	}

	protected internal override string ControlStyleString => base.ControlStyleString + this.TextAlign();

	public override ITextBoxJSInterop? Interop => GetJSInterop<ITextBoxJSInterop>();

	protected override void CreateHandle()
	{
		base.CreateHandle();
		if (CharacterCasing == CharacterCasing.Upper || CharacterCasing == CharacterCasing.Lower)
		{
			// In WinForms, CreateHandle will apply the CharacterCasing, but will not raise the TextChanged event.
			UpdateTextSilently(Text);
			textForBinding = FromTextboxToInputConverter(Text);
		}
	}

	internal override void AdjustWindowRectEx(ref Interop.RECT rect)
	{
		base.AdjustWindowRectEx(ref rect);

		if (Multiline)
		{
			if (ScrollBars == ScrollBars.Both)
			{
				rect = new Interop.RECT(rect.left, rect.top, rect.right + SystemInformation.VerticalScrollBarWidth, rect.bottom + SystemInformation.HorizontalScrollBarHeight);
			}
			else if (ScrollBars == ScrollBars.Vertical)
			{
				rect = new Interop.RECT(rect.left, rect.top, rect.right + SystemInformation.VerticalScrollBarWidth, rect.bottom);
			}
			else if (ScrollBars == ScrollBars.Horizontal)
			{
				rect = new Interop.RECT(rect.left, rect.top, rect.right, rect.bottom + SystemInformation.HorizontalScrollBarHeight);
			}
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (IsPassword && Keyboard.IsCapsLockOn && e.KeyData == Keys.Capital)
		{
			InvokeRenderDispatcher(async () => await (Interop?.RaiseWarningAsync(textboxReference, Keyboard.IsCapsLockOn) ?? Task.CompletedTask));
		}

		base.OnKeyDown(e);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		const char CtrlCChar = (char)3;
		if (IsPassword && e.KeyChar == CtrlCChar)
		{
			InvokeRenderDispatcher(async () => await (Interop?.RaiseWarningAsync(textboxReference, false) ?? Task.CompletedTask));
		}

		base.OnKeyPress(e);
	}

	protected internal override void OnBeforeRender()
	{
		base.OnBeforeRender();

		if (AutoComplete)
		{
			Suggestions ??= LoadSuggestions();
		}
	}

	protected virtual List<string> LoadSuggestions()
	{
		return new List<string>();
	}

	protected override void OnGotFocus(EventArgs e)
	{
		if (IsPassword && Keyboard.IsCapsLockOn)
		{
			InvokeRenderDispatcher(async () => await (Interop?.RaiseWarningAsync(textboxReference, Keyboard.IsCapsLockOn) ?? Task.CompletedTask));
		}

		base.OnGotFocus(e);

		if (!selectionSet)
		{
			// We get one shot at selecting when we first get focus.  If we don't
			// do it, we still want to act like the selection was set.
			selectionSet = true;

			// If the user didn't provide a selection, force one in.
			if (SelectionLength == 0 && Control.MouseButtons == MouseButtons.None)
			{
				SelectAll();
			}
		}
	}

	public override void Select(int start, int length)
	{
		// If user set selection into text box, mark it so we don't
		// clobber it when we get focus.
		selectionSet = true;
		base.Select(start, length);
	}

	/// <summary>
	///  True if the selection has been set by the user.  If the selection has
	///  never been set and we get focus, we focus all the text in the control
	///  so we mimic the Windows dialog manager.
	/// </summary>
	bool selectionSet;
}
