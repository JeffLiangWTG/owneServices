using System.Drawing;
using BArchitecture;
using CargoWise.Blazor.Client.Integration.Messaging;
using Microsoft.Extensions.Logging;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace WinzorFramework;

/// <summary>
/// Winzor specific decorator of <see cref="IWindowService"/> to add required logging.
/// </summary>
public class WinzorWindowService : IWindowService
{
	readonly IWindowService clientAppWindowService;
	readonly ILogger<WinzorWindowService>? logger;
	readonly ICircuitIdProvider? circuitIdProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="WinzorWindowService"/> class.
	/// Constructors a new window manager
	/// </summary>
	public WinzorWindowService(IWindowService clientAppWindowService, ILogger<WinzorWindowService>? logger = null, ICircuitIdProvider? circuitIdProvider = null)
	{
		this.clientAppWindowService = clientAppWindowService;
		this.clientAppWindowService.SetCircuitId(circuitIdProvider?.CircuitId ?? string.Empty);

		this.logger = logger;
		this.circuitIdProvider = circuitIdProvider;
	}

	[Obsolete($"Replaced by {nameof(RequestCreateHiddenWindowAsync)} or {nameof(RequestCreateAndShowWindowAsync)}")]
	public Task<RequestSentResult> RequestWindowOpenAsync(Uri uri, ClientWindowOpenOptions options)
		=> clientAppWindowService.RequestWindowOpenAsync(uri, options);

	public async Task<RequestSentResult> RequestRestrictedWindowOpenAsync(Uri uri, Func<Task> closeTask)
		=> await clientAppWindowService.RequestRestrictedWindowOpenAsync(uri, closeTask);

	[Obsolete($"Replaced by {nameof(RequestShowWindowAsync)}")]
	public Task<RequestSentResult> InitializeFormOnClientAsync(Guid? parentWindowId, bool modal, bool keepParentActive, Size preferSize, Size minimumSize, Size maximumSize, bool maximizeBox, bool minimizeBox, bool controlBox, FormBorderStyle formBorderStyle, FormStartPosition formStartPosition, bool topMost)
		=> clientAppWindowService.InitializeFormOnClientAsync(parentWindowId, modal, keepParentActive, preferSize, minimumSize, maximumSize, maximizeBox, minimizeBox, controlBox, formBorderStyle, formStartPosition, topMost);

	[Obsolete($"Replaced by {nameof(RequestUpdateWindowStyleAsync)}")]
	public Task<RequestSentResult> SetFormSizeOnClientAsync(Size preferSize, Size minimumSize, Size maximumSize)
		=> clientAppWindowService.SetFormSizeOnClientAsync(preferSize, minimumSize, maximumSize);

	public Task<RequestSentResult> RequestCloseAsync()
	{
		logger?.LogInformation($"RequestCloseAsync on circuit {circuitIdProvider?.CircuitId}");
		return clientAppWindowService.RequestCloseAsync();
	}

	public Task<RequestSentResult> RequestRestrictedFormCloseAsync()
		=> clientAppWindowService.RequestRestrictedFormCloseAsync();

	public Task<RequestSentResult> ConfigureClientSettingsAsync()
		=> clientAppWindowService.ConfigureClientSettingsAsync();

	[Obsolete($"Replaced by {nameof(RequestShowWindowAsync)}")]
	public Task<RequestSentResult> RequestShowAsync()
		=> clientAppWindowService.RequestShowAsync();

	public Task<RequestSentResult> RequestHideWindowAsync()
		=> clientAppWindowService.RequestHideWindowAsync();

	public Task<RequestSentResult> RegisterCloseListenerAsync(Func<Task> closeTask)
		=> clientAppWindowService.RegisterCloseListenerAsync(closeTask);

	[Obsolete($"Replaced by {nameof(RequestUpdateWindowStyleAsync)}")]
	public Task<RequestSentResult> UpdateFormStyleAsync(bool maximizeBox, bool minimizeBox, bool controlBox, bool topMost, FormBorderStyle formBorderStyle)
		=> clientAppWindowService.UpdateFormStyleAsync(maximizeBox, minimizeBox, controlBox, topMost, formBorderStyle);

	public Task<RequestSentResult> UpdateWindowStateAsync(FormWindowState windowState)
		=> clientAppWindowService.UpdateWindowStateAsync(windowState);

	public Task<RequestSentResult> FormActivateAsync()
		=> clientAppWindowService.FormActivateAsync();

	public Task<RequestSentResult> RegisterUrlHandlerAsync(UrlHandlerInstanceInfo instanceInfo, Func<string, Task<bool>> executeUrlAsyncCallback)
		=> clientAppWindowService.RegisterUrlHandlerAsync(instanceInfo, executeUrlAsyncCallback);

	public Task<RequestSentResult> OpenUrlAsync(SystemToSystemTrustMessage trustMessage)
		=> clientAppWindowService.OpenUrlAsync(trustMessage);

	public Task<RequestSentResult> ScreenShotAsync(ScreenShotSetting setting)
		=> clientAppWindowService.ScreenShotAsync(setting);

	public Task<RequestSentResult> CloseRequestReceivedAsync()
		// A keep alive message for Client App to Prevent the Dialog to show up on Closing the form
		=> clientAppWindowService.CloseRequestReceivedAsync();

	public Task<RequestSentResult> RequestCreateHiddenWindowAsync(CreateWindowOptions createWindowOptions)
		=> clientAppWindowService.RequestCreateHiddenWindowAsync(createWindowOptions);

	public Task<RequestSentResult> RequestUpdateWindowStyleAsync(WindowStyleOptions windowStyleOptions)
		=> clientAppWindowService.RequestUpdateWindowStyleAsync(windowStyleOptions);

	public Task<RequestSentResult> RequestShowWindowAsync(ShowWindowOptions showWindowOptions, WindowStyleOptions windowStyleOptions)
		=> clientAppWindowService.RequestShowWindowAsync(showWindowOptions, windowStyleOptions);

	public Task<RequestSentResult> RequestCreateAndShowWindowAsync(CreateWindowOptions createWindowOptions, ShowWindowOptions showWindowOptions, WindowStyleOptions windowStyleOptions)
		=> clientAppWindowService.RequestCreateAndShowWindowAsync(createWindowOptions, showWindowOptions, windowStyleOptions);

	public void SetCircuitId(string id)
	{
		// do nothing
	}
}
