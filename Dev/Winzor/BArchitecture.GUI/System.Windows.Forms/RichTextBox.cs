using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WTG.RtfConverter;
using WTG.RtfConverter.Dom;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace System.Windows.Forms;

public partial class RichTextBox : TextBoxBase
{
	PlainTextToHtmlConverter plainTextToHtmlConverter = new()
	{
		DetectUrls = false,
		SanitizeLinks = true,
	};

	public RichTextBox()
	{
		Multiline = true;
		dotNetObjectReference = dotNetObjectReference = DotNetObjectReference.Create(this);
	}

	public override bool UseParentDivForLayout => false;

	public bool IsToolBarVisible { get; set; }

	public bool EnableStyleShortcuts { get; set; }

	static readonly string HtmlTagPattern = "<[a-zA-Z]+.*?>([\\s\\S]*?)</[a-zA-Z]*?>";

	//After winzor transformation, RTF is stored in HTML
	[AllowNull]
	public string Html
	{
		get => html;
		set
		{
			value ??= string.Empty;
			if (!Regex.IsMatch(value, HtmlTagPattern))
			{
				Text = value;
			}
			else if (UpdateContent(newHtml: value) || ContentIsOutOfSyncWithClient)
			{
				UpdateClient();
			}
		}
	}

	string html = string.Empty;

	[AllowNull]
	public override string Text
	{
		get => text;
		set
		{
			value ??= string.Empty;
			if (UpdateContent(newText: value) || ContentIsOutOfSyncWithClient)
			{
				UpdateClient();
			}
		}
	}
	string text = string.Empty;

	IEnumerableTree<IWtgNode> SanitizeHtml(string html)
	{
		var result = HtmlParser.Parse(html)
			.Decode();

		result = SetDefaultFont(result);

		result = result.DetectLineEndings(PlaintextParserConfiguration.Default);
		if (DetectUrls)
		{
			result = result.DetectLinks();
		}

		result = result.PrefixWwwLinkDestinationIfProtocolMissing("./");
		result = result.SanitizeLinks();
		return result;
	}

	IEnumerableTree<IWtgNode> SetDefaultFont(IEnumerableTree<IWtgNode> tree)
	{
		return tree.Reduce(b => b.AsSafeVisitor() with
		{
			OnEnter = parent =>
			{
				if (parent is Phrase phrase)
				{
					if (phrase is Phrase { Font: not null, Size: null })
					{
						b.Open(phrase with { Size = new Unit(12, UnitType.Point) });
					}
					else
					{
						var defaults = new Phrase()
						{
							Font = Font.Name,
							Size = ConvertFontSize(Font.Size, Font.Unit)
						};
						b.Open(defaults.LayerWith(phrase));
					}
				}
				else
				{
					b.Open(parent);
				}
			}
		});
	}

	Unit ConvertFontSize(float size, GraphicsUnit unit) => unit switch
	{
		GraphicsUnit.Point => new Unit(size, UnitType.Point),
		GraphicsUnit.Inch => new Unit(size, UnitType.Inch),
		GraphicsUnit.Document => new Unit(size / 300, UnitType.Inch),
		GraphicsUnit.Millimeter => new Unit(size, UnitType.Millimeter),
		_ => new Unit(size, UnitType.Pixel),
	};

	IEnumerableTree<IWtgNode> SanitizeText(string text)
	{
		var result = PlaintextParser.Parse(text)
			.Decode();
		if (DetectUrls)
		{
			result = result.DetectLinks();
		}
		return result.SanitizeLinks();
	}

	bool UpdateContent(string? newHtml = null, string? newText = null)
	{
		IEnumerableTree<IWtgNode> newContent;
		if (newHtml is not null)
		{
			newContent = SanitizeHtml(newHtml);
		}
		else if (newText is not null)
		{
			newContent = SanitizeText(newText);
		}
		else
		{
			return false;
		}

		newContent = newContent.Collect();

		var newResultHtml = newContent.Reduce(HtmlEncoder.CreateFactory())
			.Markup();

		if (newResultHtml != html)
		{
			html = newResultHtml;
			text = newContent.Reduce(PlaintextEncoder.Factory)
				.Markup(PlaintextMarkupGeneratorConfiguration.Web);
			if (Created)
			{
				OnTextChanged(EventArgs.Empty);
			}
			return true;
		}

		return false;
	}

	void UpdateClient()
	{
		ContentIsOutOfSyncWithClient = false;
		modified = false;
		Interlocked.Exchange(ref clientUpdateRequired, 1);
		NotifyRenderRequired();
	}

	public RichTextBoxScrollBars ScrollBars
	{
		get => scrollBars;
		set => UpdateProperty(ref scrollBars, value);
	}

	RichTextBoxScrollBars scrollBars = RichTextBoxScrollBars.Both;

	public event LinkClickedEventHandler? LinkClicked;

	public event ContentsResizedEventHandler? ContentsResized;

	public bool DetectUrls
	{
		get => plainTextToHtmlConverter.DetectUrls;
		set
		{
			plainTextToHtmlConverter = plainTextToHtmlConverter with { DetectUrls = value };
		}
	}

	public bool CanPaste(DataFormats.Format clipFormat) => false;

	protected virtual void OnContentsResized(ContentsResizedEventArgs value)
	{
		ContentsResized?.Invoke(this, value);
	}

	protected virtual void OnHScroll(EventArgs e)
	{
	}

	protected virtual void OnVScroll(EventArgs e)
	{
	}

	protected virtual void OnLinkClicked(LinkClickedEventArgs e)
	{
		LinkClicked?.Invoke(this, e);
	}

	async Task OnLinkClickedAsync(WinzorLinkClickedEventArgs args)
	{
		if (Enabled)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				OnLinkClicked(new LinkClickedEventArgs(args.LinkText));
			});
		}
	}

	public bool SelectionProtected { get; set; }

	public event EventHandler? SelectionChanged;

	async Task OnRichTextBoxContextMenuAsync(RichTextBoxContextMenuEventArgs e)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			SelectionStart = e.SelectionStart;
			SelectionLength = e.SelectionLength;
			OnContextMenuCore(e);
		});
	}

	public void LoadFile(Stream data, RichTextBoxStreamType fileType)
	{
	}

	public override async Task DetachFromRendererAsync()
	{
		await WithExceptionHandlingAsync(async () =>
		{
			await (Interop?.DeleteRichTextBoxClientAsync(WinzorControlId) ?? Task.CompletedTask);
		});
		await base.DetachFromRendererAsync();
	}

	MarkupString? placeholderContent;

	protected internal override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		placeholderContent = new MarkupString(Html);
		Interop?.PreloadInterop();
	}

	protected internal override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			// We will set the initial content as part of LoadEditorAsync, so clear any pending updates
			Interlocked.Exchange(ref clientUpdateRequired, 0);
			var initializeParameters = new RichTextBoxJSInterop.InitializeParameters
			{
				WinzorControlId = WinzorControlId,
				Font = Font,
				EnableToolBar = IsToolBarVisible,
				EnableStyleShortcuts = EnableStyleShortcuts
			};
			await (Interop?.LoadEditorAsync(ElementReference.ElementReferenceOrNull(), dotNetObjectReference, Html, initializeParameters) ?? Task.CompletedTask);
			if (f5InserterEnabled)
			{
				await (Interop?.RegisterF5InserterAsync(WinzorControlId) ?? Task.CompletedTask);
			}
			// Reset the placeholder content so it will be removed from the markup.
			placeholderContent = null;
			await InvokeStateHasChangedAsync();
		}
		if (Interlocked.Exchange(ref clientUpdateRequired, 0) == 1)
		{
			await (Interop?.SetEditorContentAsync(WinzorControlId, Html) ?? Task.CompletedTask);
		}

		await Task.WhenAll(ExtendedAfterRenderActions.Select(action => action(firstRender)));

		await base.OnAfterRenderAsync(firstRender);
	}

	public WrappedList<Func<bool, Task>> ExtendedAfterRenderActions { get; } = [];

	/*
	 * Using int instead of a bool to use InterLocked.Exchange
	 * 0 = false
	 * 1 = true
	 */
	int clientUpdateRequired;

	public void ClearUndoManager()
	{
		InvokeRenderDispatcher(async () => await (Interop?.ClearUndoManagerAsync(WinzorControlId) ?? Task.CompletedTask));
	}

	bool f5InserterEnabled;
	Func<string>? userTimestampFunctor;
	public void RegisterF5Inserter(Func<string> userTimestampFunctor)
	{
		this.userTimestampFunctor = userTimestampFunctor;
		f5InserterEnabled = true;
	}

	internal DotNetObjectReference<RichTextBox> dotNetObjectReference { get; private set; }

	public override IRichTextBoxJSInterop? Interop => GetJSInterop<IRichTextBoxJSInterop>();

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			dotNetObjectReference?.Dispose();
		}

		base.Dispose(disposing);
	}

	protected override Size DefaultSize => new Size(100, 96);

	internal override void AdjustWindowRectEx(ref Interop.RECT rect)
	{
		base.AdjustWindowRectEx(ref rect);

		if (BorderStyle == BorderStyle.Fixed3D)
		{
			// RichTextBox has different style to textBox: cancel out 1px of border
			rect = new Interop.RECT(rect.left + 1, rect.top + 1, rect.right - 1, rect.bottom - 1);
		}
	}

	public override string SelectedText
	{
		get => base.SelectedText;

		set
		{
			var newContent = SanitizeText(value);
			selectedHtml = newContent.Reduce(HtmlEncoder.CreateFactory())
				.Markup();
			InvokeSetSelectionContentInternal(selectedHtml);
		}
	}

	public override void Select(int start, int length)
	{
		selectionStart = start;
		selectionLength = length;
		var end = start + length;

		RegisterAfterRenderAction(async () => await (Interop?.SetSelectionAsync(textboxReference, start, end) ?? Task.CompletedTask));
		NotifyRenderRequired();
	}

	protected override void AppendTextCore(string text)
	{
		Text += text;
		Select(Text.Length, 0);
	}

	internal virtual async Task OnModifiedAsync()
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			ContentIsOutOfSyncWithClient = true;
			Modified = true;
		});
	}

	internal virtual async Task OnContentChangedAsync(ContentChangedEventArgs args)
	{
		var content = default(string);
		try
		{
			// ideally, we would be calling InvokeWinzorDispatcherAsync and NOT await any other tasks to ensure events are handled in the order they were sent
			// ... but we need to get the content stream from JS which is an asynchronous method
			content = args.Content is not null ? await args.Content.GetContentAsync() : null;
		}
		catch (Exception ex)
		{
			Application.ReportDeveloperException("Error reading content from JS stream.", ex);
		}

		if (content is null)
		{
			return;
		}

		//invoke the dispatcher urgently incase something might destroy the editor or save before this arrives etc
		await InvokeWinzorDispatcherAsync(() =>
		{
			if (AllowTextChangeFromClient && ContentIsOutOfSyncWithClient)
			{
				UpdateContent(content);
				ContentIsOutOfSyncWithClient = false;
			}
		});
	}

	// This variable is NOT thread safe and should only be used from the Winzor Dispatcher thread
	protected internal bool ContentIsOutOfSyncWithClient;

	public void TryUpdateValueFromClient()
	{
		if (!ContentIsOutOfSyncWithClient)
		{
			return;
		}

		string? contentValue = null;
		Exception? exception = null;
		var cts = new CancellationTokenSource();
		WinzorDispatcher.Current.CurrentContext.InvokeRenderDispatcher(async () =>
		{
			try
			{
				if (Interop is not null)
				{
					var editorContent = await Interop.GetEditorContentAsync(WinzorControlId);
					contentValue = await editorContent.GetContentAsync();
				}
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			finally
			{
				await cts.CancelAsync();
			}
		});

		WinzorDispatcher.Current.RunMessageLoop(cts);
		cts = null;

		if (contentValue is not null)
		{
			UpdateContent(contentValue, null);
			ContentIsOutOfSyncWithClient = false;
		}
		else
		{
			Application.ReportDeveloperException("Could not fetch the editor value from the client", exception);
		}
	}

	string colorPicked = "#000000";
	internal string ColorPicked
	{
		get
		{
			return colorPicked;
		}
		set
		{
			colorPicked = value;
			InvokeRenderDispatcher(async () => await (Interop?.SetSelectionForeColorAsync(WinzorControlId, value) ?? Task.CompletedTask));
		}
	}

	async Task PickColorAsync()
	{
		if (Interop is null)
		{
			return;
		}

		var currentForeColor = await Interop.GetSelectionForeColorAsync(WinzorControlId);

		await InvokeWinzorDispatcherAsync(() =>
		{
			using var colorDialog = new ColorDialog();
			colorDialog.Color = ColorTranslator.FromHtml(currentForeColor);

			if (colorDialog.ShowDialog(this) == DialogResult.OK)
			{
				var color = colorDialog.Color;
				ColorPicked = ColorTranslator.ToHtml(color);
			}
		});
	}

	#region Original RTF obsolete attribute
	[Obsolete("This legacy attribute is no longer supported in Winzor")]
	public bool SelectionBullet
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	[Obsolete("This legacy attribute is no longer supported in Winzor")]
	public int SelectionIndent
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	[Obsolete("This legacy attribute is no longer supported in Winzor")]
	public bool SelectionNumberedList
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	[Obsolete("This legacy attribute is no longer supported in Winzor")]
	public bool SelectionNumbered
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}

	[Obsolete("This legacy attribute is no longer supported in Winzor")]
	public string SelectedRtf
	{
		get { throw new NotImplementedException(); }
		set { throw new NotImplementedException(); }
	}
	#endregion

	#region html version attribute
	Color selectionColor;
	public Color SelectionColor
	{
		get => selectionColor;
		set
		{
			selectionColor = value;
			if (SelectionLength != 0)
			{
				InvokeRenderDispatcher(async () => await (Interop?.SetSelectionColorAsync(WinzorControlId, SelectionStart, SelectionStart + SelectionLength, selectionColor) ?? Task.CompletedTask));
			}
		}
	}

	Font? selectionFont;

	public Font? SelectionFont
	{
		get => selectionFont;
		set
		{
			if (value is not null)
			{
				selectionFont = value;
				if (SelectionLength != 0)
				{
					InvokeRenderDispatcher(async () => await (Interop?.SetSelectionFontAsync(WinzorControlId, SelectionStart, SelectionStart + SelectionLength, selectionFont) ?? Task.CompletedTask));
				}
			}
		}
	}

	string? selectedHtml;

	public string? SelectedHtml
	{
		get => selectedHtml;
		set
		{
			if (value is not null)
			{
				var newContent = SanitizeHtml(value);
				selectedHtml = newContent.Reduce(HtmlEncoder.CreateFactory())
					.Markup();
				InvokeSetSelectionContentInternal(selectedHtml);
			}
		}
	}

	void InvokeSetSelectionContentInternal(string? selectedHtml)
	{
		if (!(SelectionLength == 0 && string.IsNullOrEmpty(selectedHtml)))
		{
			InvokeRenderDispatcher(async () => await (Interop?.SetSelectionContentAsync(WinzorControlId, SelectionStart, SelectionStart + SelectionLength, selectedHtml!) ?? Task.CompletedTask));
		}
	}

	#endregion

	protected override void OnReadOnlyChanged(EventArgs e)
	{
		base.OnReadOnlyChanged(e);
		Interlocked.Exchange(ref clientUpdateRequired, 1);
	}

	internal override async Task FocusOnClientAsync(IJSRuntime jsRuntime) => await (Interop?.FocusEditorAsync(WinzorControlId) ?? Task.CompletedTask);
}
