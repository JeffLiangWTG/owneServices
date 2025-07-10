using System.Threading.Tasks;
using WinzorFramework.JSInterop;

namespace CargoWise.GUI.TileBar.JSInterop;

public interface ITileBarJSInterop : IJSInterop
{
	Task<float[]> GetBoundingBoxForFavoriteItemAsync(int index);
	Task ResetDividerVisibilityAsync();
	Task SetDividerVisibilityAsync(int index);
}

public sealed class TileBarJSInterop : JSInteropBase, ITileBarJSInterop
{
	public TileBarJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/CargoWise.GUI.TileBar/js/tileBar.js", fileVersionHash)
	{
	}

	public async Task<float[]> GetBoundingBoxForFavoriteItemAsync(int index)
	{
		return await InvokeJsAsync<float[]>("getBoundingBoxForFavoriteItem", index);
	}

	public async Task SetDividerVisibilityAsync(int index)
	{
		await InvokeJsAsync("setDividerVisibility", index);
	}

	public async Task ResetDividerVisibilityAsync()
	{
		await InvokeJsAsync("resetDividerVisibility");
	}
}
