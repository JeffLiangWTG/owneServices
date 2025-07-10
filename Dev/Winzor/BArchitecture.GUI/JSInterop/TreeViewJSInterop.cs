using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WinzorFramework.JSInterop;

public interface ITreeViewJSInterop : IJSInterop
{
	Task InitializeAsync(ElementReference treeview, DotNetObjectReference<TreeView> dotNet);
	Task CopyTreeNodeContentAsync(ElementReference reference);
	Task ScrollIntoViewAndFocusAsync(TreeNode node);
	Task ScrollIntoViewAsync(ElementReference reference);
}

public sealed class TreeViewJSInterop : JSInteropBase, ITreeViewJSInterop
{
	public TreeViewJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/treeview.js", fileVersionHash)
	{
	}

	public async Task CopyTreeNodeContentAsync(ElementReference reference)
	{
		await InvokeJsAsync("copyTreeNodeContent", reference);
	}

	public async Task ScrollIntoViewAsync(ElementReference reference)
	{
		await InvokeJsAsync("scrollIntoView", reference);
	}

	public async Task ScrollIntoViewAndFocusAsync(TreeNode node)
	{
		if ((node?.TreeView?.Focused ?? false) && !node.ElementReference.Equals(default(ElementReference)))
		{
			await InvokeJsAsync("scrollIntoViewAndFocus", node.ElementReference);
		}
	}

	public async Task InitializeAsync(ElementReference treeview, DotNetObjectReference<TreeView> dotNet)
	{
		await InvokeJsAsync("initialize", treeview, dotNet);
	}
}
