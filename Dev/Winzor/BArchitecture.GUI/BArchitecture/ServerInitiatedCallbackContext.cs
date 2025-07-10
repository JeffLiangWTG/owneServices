using System.Windows.Forms;

namespace WinzorFramework;

public class ServerInitiatedCallbackContext : IWinzorDispatcherContext
{
	public ServerInitiatedCallbackContext()
	{
	}

	public ServerInitiatedCallbackContext(IWinzorDispatcherContext parent)
	{
		this.parent = parent;
	}

	public OpenFormAction OpenForm(Form form)
	{
		if (parent is not null)
		{
			return parent.OpenForm(form);
		}

		var contextForm = Application.GetSuitableContextForm(f => f != form);
		if (contextForm?.CargoWiseClientServices is null)
		{
			throw new InvalidOperationException();
		}

		var uri = contextForm.WinzorDispatcher.FormInstanceRegister.Add(contextForm.CargoWiseClientServices.ServerBaseUri, form, out var isNewForm, out _);
		contextForm.WinzorDispatcher.FormOpener.OpenForm(this, uri, contextForm, form);

		return OpenFormAction.BlockUntilShown;
	}

	public void RegisterRenderTask(Task task)
	{
		var dispatcher = WinzorDispatcher.Current;
		_ = task.ContinueWith(async t =>
		{
			await dispatcher.InvokeAsync(() => Application.OnThreadException(t.Exception!.InnerException ?? t.Exception));
		}, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
	}

	public void NotifyRenderRequired(Control control)
	{
		if (control.HasRendered)
		{
			control.OnBeforeRender();
			RegisterRenderTask(control.InvokeStateHasChangedAsync());
		}
	}

	public void InvokeRenderDispatcher(Func<Task> workItem)
	{
		Form?.InvokeRenderDispatcher(workItem);
	}

	public virtual void OnEnterMessageLoop()
	{
	}

	public CargoWiseClientServices CargoWiseClientServices => throw new NotSupportedException();

	public Form? Form => parent?.Form ?? Application.OpenForms.LastOrDefault(f => f.WinzorDispatcher == WinzorDispatcher.Current && f.ShouldRender);

	readonly IWinzorDispatcherContext? parent;
}
