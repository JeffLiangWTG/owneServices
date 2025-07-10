using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.Extensions;

namespace WinzorFramework;

public partial class Popup : ComponentBase, IAsyncDisposable
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Analyzer runner incorrectly detecting.")]
	internal IJSObjectReference? jsObjectReference;

	bool IsRendered { get; set; }
	static Popup? OpenedBalloonPopup { get; set; }

	public async Task HideAsync()
	{
		if (jsObjectReference != null)
		{
			if (IsRendered)
			{
				await jsObjectReference.InvokeVoidAsync("unregister");
				IsRendered = false;
				if (IsBalloonPopup)
				{
					OpenedBalloonPopup = null;
				}
			}
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (jsObjectReference != null)
		{
			await ExceptionHandlerExtension.HandleJSExceptionAsync(async () =>
			{
				await HideAsync();
				await jsObjectReference.DisposeAsync();
			});
		}
	}
}
