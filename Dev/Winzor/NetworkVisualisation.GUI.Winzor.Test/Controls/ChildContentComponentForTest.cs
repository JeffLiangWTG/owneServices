using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace NetworkVisualisation.GUI.Winzor.Test.Controls;
internal class ChildContentComponentForTest : ComponentBase
{
	[Parameter]
	public Action? OnAfterRenderAction { get; set; }

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		builder.OpenElement(0, "p");
		builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, ThrowTaskCanceledException));
		builder.AddContent(2, "This is the child content");
		builder.CloseElement();
	}

	protected override void OnAfterRender(bool firstRender)
	{
		base.OnAfterRender(firstRender);
		OnAfterRenderAction?.Invoke();
	}

	public void ThrowTaskCanceledException()
	{
		throw new TaskCanceledException();
	}
}
