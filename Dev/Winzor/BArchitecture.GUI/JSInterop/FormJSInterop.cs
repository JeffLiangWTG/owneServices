using System.Windows.Forms;
using Microsoft.JSInterop;

namespace WinzorFramework.JSInterop;

public interface IFormJSInterop : IJSInterop
{
	Task ChangeMouseCursorStyleAsync(int clientX, int clientY);
	Task InitializeAsync(Form form);
	Task SaveShortcutFileAsync(string fileName, string content);
	Task ResizeWindowWhenNoMainMenuStripAsync(Form form, int height, int width);
	Task CreateNotificationAsync(string title, string body);
}

public class FormJSInterop : JSInteropBase, IFormJSInterop
{
	public FormJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/form.js", fileVersionHash)
	{
	}

	public Task InitializeAsync(Form form)
	{
		return InvokeJsAsync("form.initialize", DotNetObjectReference.Create(form));
	}

	public Task ChangeMouseCursorStyleAsync(int clientX, int clientY)
	{
		return InvokeJsAsync("form.changeMouseCursorStyle", clientX, clientY);
	}

	public async Task SaveShortcutFileAsync(string fileName, string content)
	{
		await InvokeJsAsync("form.saveShortcut", fileName, content);
	}

	public async Task ResizeWindowWhenNoMainMenuStripAsync(Form form, int height, int width)
	{
		await InvokeJsAsync("form.resizeWindowWhenNoMainMenuStrip", DotNetObjectReference.Create(form), height, width);
	}

	public async Task CreateNotificationAsync(string title, string body)
	{
		await InvokeJsAsync("form.createNotification", title, body);
	}
}
