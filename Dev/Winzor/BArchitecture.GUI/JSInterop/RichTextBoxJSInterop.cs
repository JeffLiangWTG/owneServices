using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.Extensions;

namespace WinzorFramework.JSInterop;

public interface IRichTextBoxJSInterop : ITextBoxBaseJSInterop
{
	Task ClearUndoManagerAsync(string winzorControlId);
	Task DeleteRichTextBoxClientAsync(string winzorControlId);
	Task FocusEditorAsync(string winzorControlId);
	Task<EditorContent> GetEditorContentAsync(string winzorControlId);
	Task LoadEditorAsync(ElementReference? elementReference, DotNetObjectReference<RichTextBox> dotNetObjectReference, string content, RichTextBoxJSInterop.InitializeParameters initializeParameters);
	Task RegisterF5InserterAsync(string winzorControlId);
	Task SetEditorContentAsync(string winzorControlId, string content);
	Task SetSelectionColorAsync(string winzorControlId, int start, int end, Color color);
	Task SetSelectionContentAsync(string winzorControlId, int start, int end, string html);
	Task SetSelectionFontAsync(string winzorControlId, int start, int end, Font font);
	Task SetSelectionForeColorAsync(string winzorControlId, string color);
	Task<string> GetSelectionForeColorAsync(string winzorControlId);
}

public class RichTextBoxJSInterop : JSInteropBase, IRichTextBoxJSInterop
{
	public RichTextBoxJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/richTextBox.js", fileVersionHash)
	{
	}

	public async Task LoadEditorAsync(ElementReference? elementReference, DotNetObjectReference<RichTextBox> dotNetObjectReference, string content, InitializeParameters initializeParameters)
	{
		await InvokeJsAsync("initialize", elementReference, content, dotNetObjectReference, initializeParameters);
	}

	public async Task ClearUndoManagerAsync(string winzorControlId)
	{
		await InvokeJsAsync("clearUndoManager", winzorControlId);
	}

	public async Task RegisterF5InserterAsync(string winzorControlId)
	{
		await InvokeJsAsync("registerF5Inserter", winzorControlId);
	}

	public async Task SetSelectionColorAsync(string winzorControlId, int start, int end, Color color)
	{
		await InvokeJsAsync("setSelectionColor", winzorControlId, start, end, color.GetColorStyleValue());
	}

	public async Task SetSelectionFontAsync(string winzorControlId, int start, int end, Font font)
	{
		await InvokeJsAsync("setSelectionFont", winzorControlId, start, end, font);
	}

	public async Task SetSelectionAsync(ElementReference reference, int start, int end)
	{
		await InvokeJsAsync("setSelection", reference, start, end);
	}

	public async Task SetSelectionContentAsync(string winzorControlId, int start, int end, string html)
	{
		await InvokeJsAsync("setSelectionContent", winzorControlId, start, end, html);
	}

	public async Task<EditorContent> GetEditorContentAsync(string winzorControlId)
	{
		return await InvokeJsAsync<EditorContent>("getEditorContent", winzorControlId);
	}

	public async Task SetEditorContentAsync(string winzorControlId, string content)
	{
		await InvokeJsAsync<EditorContent>("setEditorContent", winzorControlId, content);
	}

	public record InitializeParameters
	{
		public string WinzorControlId { get; set; } = string.Empty;
		public Font Font { get; set; } = Control.DefaultFont;
		public bool EnableToolBar { get; set; }
		public bool EnableStyleShortcuts { get; set; }
	}

	public async Task DeleteRichTextBoxClientAsync(string winzorControlId)
	{
		await InvokeJsAsync("deleteRichTextBoxClient", winzorControlId);
	}

	public async Task FocusEditorAsync(string winzorControlId)
	{
		await InvokeJsAsync("focusEditor", winzorControlId);
	}

	public async Task ScrollToCaretAsync(ElementReference reference, int start, int end, int textLength)
	{
		await InvokeJsAsync("scrollToCaret", reference, start, end, textLength);
	}

	public async Task SetSelectionForeColorAsync(string winzorControlId, string color)
	{
		await InvokeJsAsync("setCurrentSelectionForeColor", winzorControlId, color);
	}

	public async Task<string> GetSelectionForeColorAsync(string winzorControlId)
	{
		return await InvokeJsAsync<string>("getCurrentSelectionForeColor", winzorControlId);
	}
}

public record EditorContent(string? ContentString, IJSStreamReference? ContentStream, int ContentLength)
{
	public async Task<string?> GetContentAsync()
	{
		if (ContentStream is not null)
		{
			await using var stream = await ContentStream.OpenReadStreamAsync(ContentLength);
			using var streamReader = new StreamReader(stream);
			return await streamReader.ReadToEndAsync();
		}
		return ContentString;
	}
}
