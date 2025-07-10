using Bunit.Rendering;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using Microsoft.JSInterop;
using WinzorFramework;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

public class NetworkUserControlForTest : INetworkUserControl
{
	readonly NetworkViewModel? networkViewModel;
	readonly ITestRenderer? renderer;

	/// <summary>
	/// DO NOT USE Constructor <c>NetworkUserControlForTest(ITestRenderer, NetworkViewModel?)</c>.
	/// </summary>
	public NetworkUserControlForTest(ITestRenderer renderer, NetworkViewModel networkViewModel)
	{
		this.renderer = renderer;
		viewModel = new NetworkUserControlViewModel();
		this.networkViewModel = networkViewModel;
	}

	public NetworkUserControlForTest(ITestRenderer? renderer = null, INetwork? network = null, NodeViewModelProvider? nodeViewModelProvider = null)
	{
		this.renderer = renderer;
		viewModel = new NetworkUserControlViewModel(network ?? new NetworkForTest(), nodeViewModelProvider);
		networkViewModel = viewModel.NetworkViewModel;
	}

	public virtual NetworkViewModel NetworkViewModel => networkViewModel!;

	NetworkUserControlViewModel INetworkUserControl.ViewModel => viewModel;

	readonly NetworkUserControlViewModel viewModel;

	public event Action<ThrottledWheelEventArgs>? Wheel;

	public virtual void TriggerWheel(ThrottledWheelEventArgs args)
	{
		Wheel?.Invoke(args);
	}

	public virtual void ExecuteRibbonAction(INetworkAction action) => throw new NotImplementedException();

	public virtual void InvokeRenderDispatcher(Func<Task> workItem, bool needElementRendered = false)
	{
		Task task;
		if (renderer is not null)
		{
			task = renderer.Dispatcher.InvokeAsync(workItem);
		}
		else
		{
			task = workItem();
		}
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
		task.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
	}

	public virtual Task InvokeWinzorDispatcherAsync(Action action)
	{
		action();
		return Task.CompletedTask;
	}

	public virtual void ZoomIn(double scaleFactor = 0.1)
	{
		throw new NotImplementedException();
	}

	public virtual void ZoomOut(double scaleFactor = 0.1)
	{
		throw new NotImplementedException();
	}

	public void SetZoom(double zoom)
	{
		throw new NotImplementedException();
	}

	public void SavePrevZoom()
	{
		throw new NotImplementedException();
	}
	public virtual void FitNodes()
	{
	}

	public virtual string StatusMessage => string.Empty;

	public virtual string StatusTooltip => string.Empty;
	public IJSRuntime? JSRuntime { get; set; }

	bool IWinzorThreadInfo.IsWinzorThread => true;

	bool IWinzorThreadInfo.IsRenderThread => true;
}
