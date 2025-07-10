using Microsoft.AspNetCore.Components;

namespace WinzorFramework;

[EventHandler("onlinkclicked", typeof(WinzorLinkClickedEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onsplittermoved", typeof(SplitterMovedEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onrichtextboxcontextmenu", typeof(RichTextBoxContextMenuEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("ontextboxselectionchange", typeof(TextboxSelectionChangeEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onwinzorfocusin", typeof(WinzorFocusInEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onwinzorfocusout", typeof(WinzorFocusOutEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onmodified", typeof(EventArgs), enableStopPropagation: false, enablePreventDefault: false)]
[EventHandler("oncontentchanged", typeof(ContentChangedEventArgs), enableStopPropagation: false, enablePreventDefault: false)]
[EventHandler("onwinzordrop", typeof(WinzorDragEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onwinzorpaste", typeof(WinzorPasteEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onthrottledwheel", typeof(ThrottledWheelEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onwinzordragend", typeof(WinzorDragEndEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers
{
	// This static class doesn't need to contain any members. It's just a place where we can put
	// [EventHandler] attributes to configure event types on the Razor compiler. This affects the
	// compiler output as well as code completions in the editor.
}
