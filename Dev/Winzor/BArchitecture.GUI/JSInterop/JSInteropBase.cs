using Microsoft.JSInterop;
using WinzorFramework.Extensions;

namespace WinzorFramework.JSInterop;

public interface IJSInterop : IAsyncDisposable
{
	/// <summary>
	/// fire and forget pattern, no need to wait for import js process to complete,
	/// preload the JSObjectReference to speed up the latter invoke process.
	/// </summary>
	public void PreloadInterop();
}

public abstract class JSInteropBase : IJSInterop
{
	readonly Lazy<Task<IJSObjectReference>> module;
	readonly IFileVersionHash fileVersionHash;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD011:Use AsyncLazy<T>", Justification = "AsyncLazy<T> is not available in our codebase as we don't have the VS async types")]
	protected JSInteropBase(IJSRuntime jsRuntime, string moduleSrc, IFileVersionHash fileVersionHash)
	{
		this.fileVersionHash = fileVersionHash;
		module = new Lazy<Task<IJSObjectReference>>(async () => await LoadModuleAsync(jsRuntime, moduleSrc));
	}

	/// <inheritdoc />
	public void PreloadInterop()
	{
		_ = module.Value;
	}

	async Task<IJSObjectReference> LoadModuleAsync(IJSRuntime jsRuntime, string moduleSrc)
	{
		var path = moduleSrc + fileVersionHash.Get(moduleSrc);
		var module = await jsRuntime.InvokeAsync<IJSObjectReference?>("import", path);
		return module ?? throw new InvalidOperationException($"Could not load module {moduleSrc}");
	}

	protected virtual async Task InvokeJsAsync(string identifier, params object?[] args)
		=> await (await module.Value).InvokeVoidAsync(identifier, args: args);

	protected async Task<T> InvokeJsAsync<T>(string identifier, CancellationToken cancellationToken, params object?[] args)
		=> await (await module.Value).InvokeAsync<T>(identifier, cancellationToken: cancellationToken, args: args);

	protected async Task<T> InvokeJsAsync<T>(string identifier, params object?[] args)
		=> await (await module.Value).InvokeAsync<T>(identifier, args: args);

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Suggested name from IAsyncDisposable documentation")]
	protected virtual async ValueTask DisposeAsyncCore()
	{
		await ExceptionHandlerExtension.HandleJSExceptionAsync(async () =>
		{
			if (module.IsValueCreated)
			{
				await (await module.Value).DisposeAsync();
			}
		});
	}
}
