using WinzorFramework;
using WinzorFramework.Telemetry;

namespace System.Windows.Forms;

public partial class Control
{
	class ControlUnitOfWorkContext : IWinzorDispatcherContext
	{
		public Form? Form => control.FindForm();

		public ControlUnitOfWorkContext(Control control)
		{
			this.control = control;
			formOpener = control.WinzorDispatcher.FormOpener;
		}

		public OpenFormAction OpenForm(Form form)
		{
			var contextForm = GetContextForm(form);

			if (contextForm?.CargoWiseClientServices is null || !Application.OpenForms.Contains(contextForm))
			{
				// The context form is either not provided or is no longer open. Attempt to find another form to use instead.
				// We need to filter out the form being opened as that has already been added to the OpenForms collection.
				contextForm = Application.GetSuitableContextForm(f => f != form);
			}

			if (contextForm?.CargoWiseClientServices is null)
			{
				throw new InvalidOperationException($"The context form does not have a CargoWiseClientServices instance");
			}

			var uri = contextForm.WinzorDispatcher.FormInstanceRegister.Add(contextForm.CargoWiseClientServices.ServerBaseUri, form, out var isNewForm, out _);
			formOpener.OpenForm(this, uri, contextForm, form);

			return OpenFormAction.BlockUntilShown;
		}

		Form? GetContextForm(Form form)
		{
			if (form.Owner?.CargoWiseClientServices is not null)
			{
				return form.Owner;
			}

			var controlForm = control.FindForm();
			if (controlForm?.CargoWiseClientServices is not null)
			{
				return controlForm;
			}

			return null;
		}

		public void RegisterRenderTask(Task task)
		{
			renderTasks ??= new RenderTasks();
			renderTasks.Add(task);
		}

		public async Task WaitForAllRenderTasksAsync()
		{
			if (renderTasks is not null && renderTasks.Any())
			{
				using var activity = TelemetryService.ActivitySource?.StartActivity($"{control.GetType().Name}UnitOfWork.{nameof(WaitForAllRenderTasksAsync)}");
				await renderTasks.WaitAllAsync();
			}
		}

		public void NotifyRenderRequired(Control control)
		{
			renderRequired ??= new HashSet<Control>();
			renderRequired.Add(control);
		}

		public async Task CallStateHasChangedOnRequiredControlsAsync()
		{
			var controls = GetTopLevelControlsRequiringRerender();
			if (controls.Any())
			{
				using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(CallStateHasChangedOnRequiredControlsAsync)}");
				foreach (var control in controls)
				{
					await control.InvokeStateHasChangedAsync();
				}
			}
		}

		public void CallOnBeforeRenderOnRequiredControls()
		{
			var controls = GetTopLevelControlsRequiringRerender();
			if (controls.Any())
			{
				using var activity = TelemetryService.ActivitySource.StartActivity($"{nameof(ControlUnitOfWorkContext)}.{nameof(CallOnBeforeRenderOnRequiredControls)}");
				foreach (var control in controls)
				{
					if (!control.IsDisposed)
					{
						control.OnBeforeRender();
					}
				}
			}
		}

		Control[] GetTopLevelControlsRequiringRerender()
		{
			return renderRequired != null ? renderRequired.Where(c => c.RenderParentNode == null || !renderRequired.Contains(c.RenderParentNode)).ToArray() : Array.Empty<Control>();
		}

		public void OnEnterMessageLoop()
		{
			if (renderRequired != null)
			{
				CallOnBeforeRenderOnRequiredControls();
				RegisterRenderTask(CallStateHasChangedOnRequiredControlsAsync());
			}
		}

		public void InvokeRenderDispatcher(Func<Task> workItem)
		{
			control.InvokeRenderDispatcher(workItem);
		}

		readonly Control control;
		RenderTasks? renderTasks;
		HashSet<Control>? renderRequired;
		readonly IFormOpener formOpener;
	}
}
