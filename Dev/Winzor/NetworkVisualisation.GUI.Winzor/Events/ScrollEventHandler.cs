using System;
using Microsoft.AspNetCore.Components;

namespace CargoWise.NetworkVisualisation.GUI.Events.Scroll;

[EventHandler("onthrottledscroll", typeof(ThrottledScrollEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers
{
}

public class ThrottledScrollEventArgs : EventArgs
{
	public double ScrollLeft { get; set; }

	public double ScrollTop { get; set; }
}
