using System;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using WinzorFramework;

namespace WinzorTestFramework;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
public static class ElementExtensionMethods
{
	public static string GetElementReferenceId(this IElement element)
	{
		return element.GetAttribute("blazor:elementReference");
	}

	public static void MouseEnter(this IElement element) => element.TriggerEvent("onmouseenter", EventArgs.Empty);

	public static void MouseLeave(this IElement element) => element.TriggerEvent("onmouseleave", EventArgs.Empty);

	public static Task FocusOutAsync(this IElement element, IElement relatedTarget = null, bool initiatedFromServer = false)
	{
		return element.TriggerEventAsync("onwinzorfocusout", new WinzorFocusOutEventArgs() 
		{
			TargetWinzorControlId = element.GetAttribute("data-winzor-control-id"),
			RelatedTargetWinzorControlId = relatedTarget?.GetAttribute("data-winzor-control-id"),
			InitiatedFromServer = initiatedFromServer
		});
	}
}
