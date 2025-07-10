using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Dom;
using Microsoft.Playwright;

namespace WinzorTestFramework;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
public static class IPageExtensions
{
	public static T GetControl<T>(this IPage page) where T : Control => page.GetForm().Controls.OfType<T>().Single();

	public static Form GetForm(this IPage page) => Application.OpenForms.FirstOrDefault(f => f.Uri == new Uri(page.Url)) ?? throw new InvalidOperationException($"No form exists at {page.Url}");

	public static async Task MockClipboardRead(this IPage page, JSClipboardData[] clipboardData)
	{
		await page.EvaluateAsync($@"
navigator.clipboard.read = () => Promise.resolve([
	new ClipboardItem({{
		{string.Join(',', clipboardData.Select(c => $"'{c.MimeType}': new Blob([`{c.Content}`], {{ type: '{c.MimeType}' }})"))}
	}})
]);");
	}

	public static async Task AttachMockClipboardWrite(this IPage page)
	{
		await page.EvaluateAsync(@"
			navigator.clipboard.write = (data) => {{ navigator.clipboard.writtenData = data; }}
			navigator.clipboard.writeText = (text) => {{ navigator.clipboard.writtenText = text; }}
			navigator.clipboard.read = async () => {{
				if (typeof navigator.clipboard.writtenData === 'undefined') return [];
				return navigator.clipboard.writtenData;
			}}
		");
	}

	public static async Task AttachClipboardPaste(this ILocator element)
	{
		await element.EvaluateAsync(@"rtb => {
			rtb.addEventListener('paste', e  => {
				e.preventDefault(); e.stopPropagation();
				if (typeof navigator.clipboard.writtenData === 'undefined' || !navigator.clipboard.writtenData) return;
				if (navigator.clipboard.writtenData.length === 0) return;
				for (const item of navigator.clipboard.writtenData) {
					if (item.types.includes('text/html')) {
						item.getType('text/html').then((clipboardMarkup) => {
							clipboardMarkup.text().then((clipText) => rtb.insertContent(clipText));
						});
					} else if (item.types.includes('text/plain')) {
						item.getType('text/plain').then((clipboardMarkup) => {
							clipboardMarkup.text().then((clipText) => rtb.insertText(clipText));
						});
					}
					break;
				}
				return;
			});
		}");
	}

	public static async Task DumpClipboard(this IPage page)
	{
		await page.EvaluateAsync<object>(@"async () => {
			let clipboardData = {'Html': [], 'Plain': []};
			window.clipboardData = clipboardData;

			if (typeof navigator.clipboard.writtenData === 'undefined' || !navigator.clipboard.writtenData) return;
			if (navigator.clipboard.writtenData.length === 0) return;

			let promises = [];

			for (const item of navigator.clipboard.writtenData) {
				if (item.types.includes('text/html')) {
					promises.push(item.getType('text/html').then((clipboardMarkup) => {
						promises.push(clipboardMarkup.text().then((clipText) => clipboardData['Html'].push(clipText)));
					}));
				}
				if (item.types.includes('text/plain')) {
					promises.push(item.getType('text/plain').then((clipboardMarkup) => {
						promises.push(clipboardMarkup.text().then((clipText) => clipboardData['Plain'].push(clipText)));
					}));
				}
				break;
			}

			Promise.allSettled(promises).finally(() => {{
				window.clipboardData = clipboardData;
			}});
		}");
	}

	public static async Task<string> GetClipboardPlain(this IPage page)
	{
		await page.DumpClipboard();
		return await page.EvaluateAsync<string>("window.clipboardData.Plain[0] ?? ''");
	}

	public static async Task<string> GetClipboardHtml(this IPage page)
	{
		await page.DumpClipboard();
		return await page.EvaluateAsync<string>("window.clipboardData.Html[0] ?? ''");
	}

	public static async Task FakeClipboardCopy(this ILocator element)
	{
		await element.EvaluateAsync(@"element => {{
			const copyEvent = new Event('copy', { bubbles: true });
			element.dispatchEvent(copyEvent);
		}}");
	}

	public static async Task FakeClipboardCut(this ILocator element)
	{
		await element.EvaluateAsync(@"element => {{
			const cutEvent = new Event('cut', { bubbles: true });
			element.dispatchEvent(cutEvent);
		}}");
	}

	public static async Task FakeClipboardPaste(this ILocator element, string pasteFormat = "text/html")
	{
		await element.EvaluateAsync($@"rtb => {{
			if (typeof navigator.clipboard.writtenData === 'undefined') return;
			for (const item of navigator.clipboard.writtenData) {{
				if (item.types.includes('text/html') && '{pasteFormat}' === 'text/html') {{
					item.getType('{pasteFormat}').then((clipboardMarkup) => {{
						clipboardMarkup.text().then((clipText) => rtb.insertContent(clipText));
					}});
				}} else if (item.types.includes('text/plain')) {{
					item.getType('text/plain').then((clipboardMarkup) => {{
						clipboardMarkup.text().then((clipText) => rtb.insertText(clipText));
					}});
				}}
				break;
			}}
		}}");
	}

	public static async Task FakeClipboardPasteText(this ILocator element)
	{
		await element.EvaluateAsync(@"rtb => {{
			if (typeof navigator.clipboard.writtenText === 'undefined') return;
			rtb.insertText(navigator.clipboard.writtenText);
		}}");
	}

	const string execCommandInvocations = "execCommandInvocations";

	public static async Task AttachExecCommandListener(this IPage page)
		=> await page.EvaluateAsync(
@$"document.execCommandCore = document.execCommand;
execCommandInvocations = [];
document.execCommand = (...args) => {{
	if (args.length === 1 && args[0] === null) return;
	{execCommandInvocations}.push(args[0]);
	document.execCommandCore(...args);
}};");

	public static async Task<string[]> ExecCommandInvocations(this IPage page) => await page.EvaluateAsync<string[]>(execCommandInvocations);

	public static Task<JsonElement?> WaitForTheScrollToFinishAsync(this IPage page, string elementSelector, int timeInMs = 100)
	{
		return page.EvaluateAsync(@"([elementSelector, timeInMs]) => {
        return new Promise(resolve => {
            const scrollElement = document.querySelector(elementSelector);
            if (!scrollElement) {
                resolve(); // Element not found, resolve immediately
                return;
            }
            let timeout;
            function onScroll() {
                clearTimeout(timeout);
                timeout = setTimeout(() => {
                    scrollElement.removeEventListener('scroll', onScroll);
                    resolve();
                }, timeInMs);
            }
            scrollElement.addEventListener('scroll', onScroll);
        });
    }", new object[] { elementSelector, timeInMs });
	}

	public static async Task PasteFiles(this IElementHandle element, List<JSClipboardData> files)
	{
		var jsCode = BuildJsClipboard(null, null, files);
		await element.EvaluateAsync(jsCode);
	}

	public static async Task PasteText(this IElementHandle element, string text)
	{
		var jsCode = BuildJsClipboard(text, null, null);
		await element.EvaluateAsync(jsCode);
	}
	public static async Task PasteHtml(this IElementHandle element, string html)
	{
		var jsCode = BuildJsClipboard(null, html, null);
		await element.EvaluateAsync(jsCode);
	}

	public static string BuildJsClipboard(string text, string html, List<JSClipboardData> files)
	{
		var setTextToClipboard = string.IsNullOrEmpty(text)
			? ""
			: $"dataTransfer.setData('text/plain', '{text}');";

		var setHtmlToClipboard = string.IsNullOrEmpty(html)
			? ""
			: $"dataTransfer.setData('text/html', '{html}');";

		var setFilesToClipboard = files == null
			? ""
			: string.Join("\n", files.Select((file, index) => $@"
				const fileData{index} = 'data:{file.MimeType};base64,{file.Content}';
				const response{index} = await fetch(fileData{index});
				const blob{index} = await response{index}.blob();
				const file{index} = new File([blob{index}], '{file.FileName}', {{type: '{file.MimeType}'}});
				dataTransfer.items.add(file{index});
			"));

		return $@"
			async element => {{
				const dataTransfer = new DataTransfer();
            
				{setFilesToClipboard}

				{setTextToClipboard}

				{setHtmlToClipboard}

				const pasteEvent = new ClipboardEvent('paste', {{
					bubbles: true,
					cancelable: true,
					clipboardData: dataTransfer
				}});
            
				element.dispatchEvent(pasteEvent);
			}}
		";
	}
}

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
public record JSClipboardData(string MimeType, string Content, string FileName = null);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
