using Blazor.Diagrams.Core.Events;

namespace CargoWise.NetworkVisualisation.GUI.Extensions;

public static class PointerEventArgsExtensions
{
	public static bool IsMiddleButton(this PointerEventArgs pointerEventArgs)
	{
		return pointerEventArgs.Button == 1;
	}
}
