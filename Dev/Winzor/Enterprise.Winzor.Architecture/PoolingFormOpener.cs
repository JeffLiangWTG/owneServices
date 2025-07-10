using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture;

/// <summary>
/// Adds new forms to a queue
/// Entry point components then read forms off the queue
/// </summary>
public class PoolingFormOpener : IFormOpener
{
	readonly IOpeningFormQueue queue;

	/// <summary>
	/// ctor
	/// </summary>
	/// <param name="register"></param>
	/// <param name="queue"></param>
	/// <exception cref="NotImplementedException"></exception>
	public PoolingFormOpener(IOpeningFormQueue queue)
	{
		this.queue = queue;
	}

	/// <inheritdoc />
	public void OpenForm(IWinzorDispatcherContext context, Uri uri, Form opener, Form formToOpen)
	{
		queue.Write(formToOpen);
	}

	/// <inheritdoc />
	public Task OpenFormAsync(Uri uri, IWindowService windowService, Form formToOpen)
	{
		queue.Write(formToOpen);
		return Task.CompletedTask;
	}
}
