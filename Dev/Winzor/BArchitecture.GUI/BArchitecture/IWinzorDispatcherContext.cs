using System.Windows.Forms;

namespace WinzorFramework;

public interface IWinzorDispatcherContext
{
	OpenFormAction OpenForm(Form form);

	Form? Form { get; }

	void RegisterRenderTask(Task task);

	void NotifyRenderRequired(Control control);

	void InvokeRenderDispatcher(Func<Task> workItem);

	void OnEnterMessageLoop();
}
