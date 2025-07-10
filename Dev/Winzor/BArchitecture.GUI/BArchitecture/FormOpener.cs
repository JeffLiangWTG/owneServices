using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;

namespace WinzorFramework;

/// <inheritdoc />
public class FormOpener : IFormOpener
{
	/// <inheritdoc />
	public void OpenForm(IWinzorDispatcherContext context, Uri uri, Form opener, Form formToOpen)
	{
		if (opener.CargoWiseClientServices is null)
		{
			throw new ArgumentException($"{nameof(Form.CargoWiseClientServices)} must be initialised and non-null on the opener to open a form", nameof(opener));
		}

		formToOpen.ClientWindowOpenSent = true;

		var createWindowOptions = new CreateWindowOptions { Uri = uri };
		var showWindowOptions = formToOpen.GenerateShowWindowOptions();
		var windowStyleOptions = formToOpen.GenerateWindowStyleOptions();

		if (opener.ProxyInitialized)
		{
			context.RegisterRenderTask(opener.InvokeRenderDispatcherAsync(async () => await opener.CargoWiseClientServices.WindowService.RequestCreateAndShowWindowAsync(createWindowOptions, showWindowOptions, windowStyleOptions)));
		}
		else
		{
			opener.RegisterAfterRenderAction(async () => await opener.CargoWiseClientServices.WindowService.RequestCreateAndShowWindowAsync(createWindowOptions, showWindowOptions, windowStyleOptions));
		}
	}

	/// <inheritdoc />
	public async Task OpenFormAsync(Uri uri, IWindowService windowService, Form formToOpen)
	{
		if (formToOpen.IsClosing || formToOpen.IsDisposed)
		{
			return;
		}

		formToOpen.ClientWindowOpenSent = true;

		var createWindowOptions = new CreateWindowOptions { Uri = uri };
		var showWindowOptions = formToOpen.GenerateShowWindowOptions();
		var windowStyleOptions = formToOpen.GenerateWindowStyleOptions();

		await windowService.RequestCreateAndShowWindowAsync(createWindowOptions, showWindowOptions, windowStyleOptions);
	}
}
