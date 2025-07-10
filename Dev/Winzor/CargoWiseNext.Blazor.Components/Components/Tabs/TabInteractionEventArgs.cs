namespace CargoWiseNext.Blazor.Components;

public class TabInteractionEventArgs
{
	public int PanelIndex { get; set; }
	public TabInteractionType InteractionType { get; set; }
	public bool Cancel { get; set; }
}
