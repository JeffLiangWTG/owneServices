using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace CargoWise.NetworkVisualisation.GUI.Components;

/// <summary>
/// Represents a component for rendering a job node in the network visualization.
/// </summary>
public partial class JobNode : NetworkNode<JobNodeModel>
{
	/// <summary>
	/// Gets or sets the interface for displaying menus.
	/// </summary>
	[Inject]
	public IMenuDisplayer MenuDisplayer { get; set; }

	/// <summary>
	/// Gets or sets the interface for interacting with clipboard functionality through JavaScript interop.
	/// </summary>
	[Inject]
	public IClipboardJSInterop ClipboardInterop { get; set; }

	/// <summary>
	/// Gets the text color used for displaying completion criteria associated with the job node.
	/// </summary>
	public string CompletionCriteriaTextColor => Node.CompletionCriteriaTextColor.GetColorStyleValue();

	/// <summary>
	/// Gets the CSS border style used for displaying the border of the job node.
	/// </summary>
	public string BorderStyle => Node.IsOnCriticalPath ? (NoResString)"outline: 2px solid red;" : (NoResString)"outline: 1.3px solid black;";

	/// <summary>
	/// Gets the CSS style used for displaying notifications and warnings associated with the job node.
	/// </summary>
	public string NotificationWarningStyle => (NoResString)"border-color:" + NotificationsBrushColor + (NoResString)";";

	/// <summary>
	/// Gets the path to the image representing the status of the job node.
	/// </summary>
	public string StatusImage => Node.Status switch
	{
		WorkStatus.Startable => Resources.StatusStartable,
		WorkStatus.Suspended => Resources.StatusSuspended,
		WorkStatus.Working => Resources.StatusWorking,
		WorkStatus.Complete => Resources.StatusComplete,
		WorkStatus.Cancelled => Resources.StatusCancelled,
		_ => null
	};

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in JobNode.razor")]
	async Task OpenTextBoxContextMenuAsync(WebMouseEventArgs args)
	{
		await MenuDisplayer.ShowClipboardContextMenuAsync(args, ClipboardInterop, jobNameElementReference);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Analyzer runner incorrectly detecting.")]
	ElementReference jobNameElementReference;
}
