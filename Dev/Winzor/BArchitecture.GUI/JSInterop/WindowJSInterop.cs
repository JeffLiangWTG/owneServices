using System.Drawing;

namespace WinzorFramework.JSInterop;

public interface IWindowJSInterop : IJSInterop
{
	public Task OpenUrlAsync(string url);
}

public sealed class WindowJSInterop : JSInteropBase, IWindowJSInterop
{
	public WindowJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/window.js", fileVersionHash)
	{
	}

	readonly string[] NewWindowProtocols = { "https", "http" };

	//These protocols are considered dangerous by chromium to open, see https://chromium.googlesource.com/chromium/src/+/89eb57915ce2ae2c2f06e5474bd3c12b23397540/chrome/browser/external_protocol/external_protocol_handler.cc#52.
	readonly string[] blockedProtocols = { "afp", "data", "disk", "disks", "file", "hcp", "ie.http", "javascript", "ms-help", "nntp", "res", "shell", "vbscript", "view-source", "vnd.ms.radio" };

	public async Task OpenUrlAsync(string url)
	{
		if (blockedProtocols.Any(protocol => protocol == url.Split(':')[0]))
		{
			//Please see https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/8262/URL-has-a-protocol-that's-dangerous-to-open for trouble shooting.
			throw new InvalidOperationException(url + " has a protocol that's dangerous to open");
		}

		var newWindow = url.Split(':').Length <= 1 || NewWindowProtocols.Any(protocol => protocol == url.Split(':')[0]);
		if (url.ToLower().StartsWith("www.") && url[url.Length - 1] != '.' && !url.ToLower().StartsWith("www.."))
		{
			url = "http://" + url;
		}
		await InvokeJsAsync("openUrl", url, newWindow);
	}
}
