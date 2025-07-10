using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.RtfConverter;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace WinzorTestFramework;

[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
public class RichTextBoxClient
{
	readonly HtmlToPlainTextConverter htmlToPlainTextConverter = new()
	{
		PlaintextMarkup = PlaintextMarkupGeneratorConfiguration.Winforms,
	};
	public IJSHandle JSHandle { get; }

	public RichTextBoxClient(IJSHandle jsHandle)
	{
		JSHandle = jsHandle;
	}

	public async Task<T> EvaluateAsync<T>(string expression, object arg = null)
	{
		return await JSHandle.EvaluateAsync<T>(expression, arg);
	}

	public async Task<IJSHandle> EvaluateHandleAsync(string expression, object arg = null)
	{
		return await JSHandle.EvaluateHandleAsync(expression, arg);
	}

	public async Task<Dictionary<string, IJSHandle>> GetPropertiesAsync()
	{
		return await JSHandle.GetPropertiesAsync();
	}

	public async Task<IJSHandle> GetPropertyAsync(string propertyName)
	{
		return await JSHandle.GetPropertyAsync(propertyName);
	}

	public async Task<JsonElement?> EvaluateAsync(string expression, object arg = null)
	{
		return await JSHandle.EvaluateAsync(expression, arg);
	}

	public async Task<string> GetEditorHtmlAsync() => await JSHandle.EvaluateAsync<string>($"client => client.getEditorContent()");

	public async Task<string> GetEditorTextAsync() => GetTextFromHtml(await GetEditorHtmlAsync());

	public async Task<string> GetActiveHtmlAsync() => await JSHandle.EvaluateAsync<string>($"client => client.isReadOnly ? client.root.querySelector('.richtextbox__data').outerHTML : client.getEditorContent() ?? ''");

	public async Task<string> GetActiveTextAsync() => GetTextFromHtml(await GetActiveHtmlAsync());

	public async Task<int> GetCleanMarkupCount() => await JSHandle.EvaluateAsync<int>("e => e.cleanCount");

	public async Task<(int start, int end)> GetActiveSelectionAsync()
	{
		var result = await JSHandle.EvaluateAsync<int[]>(@"client => {
					let selection = client.getSelection();
					return [selection?.start, selection?.end];
				}");
		return (result[0], result[1]);
	}

	public async Task<IElementHandle> GetEditorBodyAsync()
	{
		var result = (await EvaluateHandleAsync("client => client.getEditorBody()")).AsElement();
		return result ?? throw new InvalidOperationException("Could not find editor body");
	}

	public async Task FocusEditorAsync() => await (await GetEditorBodyAsync()).FocusAsync();

	public async Task FocusOutEditorAsync() => await (await GetEditorBodyAsync()).EvaluateAsync("e => e.blur()");

	public async Task FillEditorAsync(string text, bool fireTextChangedEvent = false)
	{
		var editorBody = await GetEditorBodyAsync();
		await editorBody.FillAsync(text);
		if (fireTextChangedEvent)
		{
			await editorBody.PressAsync("Enter"); // This causes an additional empty paragraph being inserted
		}
	}

	string GetTextFromHtml(string html)
	{
		var text = htmlToPlainTextConverter.Convert(html);
		return text.TrimEnd('\n').TrimEnd('\r');
	}

	public static async Task<RichTextBoxClient> GetClientAsync(RichTextBox control = null)
	{
		await Page.WaitForFunctionAsync(
			"() => typeof(WTG) !== 'undefined' && typeof(WTG.RichTextBoxClient) !== 'undefined'",
			null,
			new() { PollingInterval = 100, Timeout = 3000, }
		);

		IJSHandle handle;
		if (control is null)
		{
			handle = await Page.WaitForFunctionAsync("() => WTG?.RichTextBoxClient?.instances?.values().next().value",
				null,
				new() { PollingInterval = 100, Timeout = 3000, }
			);
		}
		else
		{
			var task = Page.WaitForFunctionAsync(
				"controlId => WTG.RichTextBoxClient?.tryGetInstance(controlId)",
				control.WinzorControlId,
				new() { PollingInterval = 100, Timeout = 3000, }
			);
			Assert.That(await task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True,
				"Client did not become ready within 3 seconds");
			handle = await task;
			Assert.That(handle, Is.Not.Null);
		}

		// override original cleanMarkup to count how many times it's called
		await handle.EvaluateAsync(@"e => {
			var _cleanup = e.cleanMarkup;
			_cleanup = _cleanup.bind(e);
			e.cleanMarkup = (nodes, doc) => {
				e.cleanCount = ++e.cleanCount || 1;
				_cleanup(nodes, doc);
			};
		}");

		var client = new RichTextBoxClient(handle);
		return client;
	}
}

