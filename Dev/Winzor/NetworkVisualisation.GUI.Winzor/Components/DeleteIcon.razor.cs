using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace CargoWise.NetworkVisualisation.GUI.Components;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in DeleteIcon.razor")]
/// <summary>
/// Represents the partial class for the Delete Icon for all type of Nodes - JobNode, BufferNode, Annotations
/// </summary>
public partial class DeleteIcon
{
	/// <summary>
	/// Parameter to check if the Delete Icon is visible
	/// </summary>
	[Parameter]
	public bool Visible { get; set; }

	/// <summary>
	/// Parameter to Hide the Delete Icon to set the hidden css property in DeleteIcon.razor
	/// </summary>
	[Parameter]
	public bool QuickHide { get; set; }

	/// <summary>
	/// Parameter to get and set ToolTip for the Delete icon for all Nodes
	/// </summary>
	[Parameter]
	public string Tooltip { get; set; }

	/// <summary>
	/// Parameter to set and get the details of the path of the Delete icon image
	/// </summary>
	[Parameter]
	public string ImagePath { get; set; }

	/// <summary>
	/// Parameter to implement the Call back function for Delete to be used by DeleteIcon, NetworkLink.razor and InDiagramNetworkNode.razor 
	/// </summary>
	[Parameter]
	public Func<Task> DeleteCallback { get; set; }

	async void OnClick()
	{
		await DeleteCallback();
	}

	void OnMouseOver()
	{
		Visible = true;
	}

	void OnMouseLeave()
	{
		Visible = false;
	}

	/// <summary>
	/// This is to set the class for the Delete icon based on the Visible parameter
	/// </summary>
	public string VisibleClass => Visible ? string.Empty : "deleteicon--hide";
}
