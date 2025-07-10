using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;

namespace WinzorFramework;

/// <summary>
/// Methods for opening forms
/// </summary>
public interface IFormOpener
{
	/// <summary>
	/// Opens a form in a new window from a WinzorDispatcher Thread
	/// </summary>
	/// <param name="context"></param>
	/// <param name="uri">Uri of form</param>
	/// <param name="opener">A form that is already rendered and has an active client connection</param>
	/// <param name="formToOpen">The form to open</param>
	/// <returns></returns>
	void OpenForm(IWinzorDispatcherContext context, Uri uri, Form opener, Form formToOpen);

	/// <summary>
	/// Open a form from a Blazor render context
	/// Request is only sent if formToOpen is not disposed or closing
	/// </summary>
	/// <param name="openingFormId"></param>
	/// <param name="uri">Uri of form</param>
	/// <param name="windowService"></param>
	/// <param name="formToOpen"></param>
	/// <returns></returns>
	Task OpenFormAsync(Uri uri, IWindowService windowService, Form formToOpen);
}
