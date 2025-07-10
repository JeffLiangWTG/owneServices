using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture;

public static partial class Initialization
{
	class InitializationWinzorDispatcherContext : IWinzorDispatcherContext, IDisposable
	{
		bool applicationExiting;
		IWinzorDispatcherContext workingDispatcherContext;
		readonly List<Form> pendingOpenFormActions = new List<Form>();

		public InitializationWinzorDispatcherContext()
		{
			Application.ApplicationExit += OnApplicationExit;
		}

		void OnApplicationExit(object sender, EventArgs e)
		{
			applicationExiting = true;
		}

		public OpenFormAction OpenForm(Form form)
		{
			// if the application is exiting, stop context from opening forms
			if (applicationExiting)
			{
				return OpenFormAction.None;
			}

			if (workingDispatcherContext != null)
			{
				return workingDispatcherContext.OpenForm(form);
			}
			else
			{
				pendingOpenFormActions.Add(form);
				return OpenFormAction.None;
			}
		}

		public Form Form => workingDispatcherContext?.Form;

		public void InvokeRenderDispatcher(Func<Task> workItem)
		{
			if (workingDispatcherContext != null)
			{
				workingDispatcherContext.InvokeRenderDispatcher(workItem);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		public void NotifyRenderRequired(Control control)
		{
			if (workingDispatcherContext != null)
			{
				workingDispatcherContext.NotifyRenderRequired(control);
			}
		}

		public void OnEnterMessageLoop()
		{
			if (workingDispatcherContext != null)
			{
				workingDispatcherContext.OnEnterMessageLoop();
			}
		}

		public void RegisterRenderTask(Task task)
		{
			if (workingDispatcherContext != null)
			{
				workingDispatcherContext.RegisterRenderTask(task);
			}
		}

		public async Task SetWorkingDispatcherContextAsync(IWinzorDispatcherContext workingDispatcherContext)
		{
			if (this.workingDispatcherContext == null)
			{
				this.workingDispatcherContext = workingDispatcherContext;
				if (pendingOpenFormActions.Count > 0)
				{
					await workingDispatcherContext.Form.InvokeWinzorDispatcherAsync(() =>
					{
						foreach (var form in pendingOpenFormActions)
						{
							workingDispatcherContext.OpenForm(form);
						}
					});
				}
			}
		}

		public void Dispose()
		{
			Application.ApplicationExit -= OnApplicationExit;
		}
	}
}
