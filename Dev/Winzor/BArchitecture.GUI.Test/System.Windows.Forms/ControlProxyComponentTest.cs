using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;
class ControlProxyComponentTest
{
	[Test]
	public async Task ControlProxyComponentResetsControlWhenRemovedFromRenderTree()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlForTest());
		var control = rendered.GetControl<ControlForTest>();

		Assert.That(control.Proxy, Is.Not.Null);
		Assert.That(control.ElementReference, Is.Not.EqualTo(default(ElementReference)));
		await control.Proxy.DisposeAsync();
		Assert.That(control.Proxy, Is.Null);
		Assert.That(control.ElementReference, Is.EqualTo(default(ElementReference)));
	}

	[Test]
	public async Task ControlProxyComponentDoesNotResetControlWhenMovedToDifferentProxyComponent()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ControlForTest());
		var control = rendered.GetControl<ControlForTest>();

		Assert.That(control.Proxy, Is.Not.Null);
		Assert.That(control.ElementReference, Is.Not.EqualTo(default(ElementReference)));

		var rendered2 = await ctx.RenderControlOnFormAsync(() => control);

		Assert.That(control.Proxy, Is.Not.Null);
		Assert.That(control.ElementReference, Is.Not.EqualTo(default(ElementReference)));
	}

	[Test]
	public async Task ShouldThrowWhenHavingExtraParametersAsync()
	{
		var param = ParameterView.FromDictionary(new Dictionary<string, object>()
		{
			{ "Extra", null }
		});
		await using var component = new ControlProxyComponent();
		_ = Assert.ThrowsAsync<InvalidOperationException>(async () => await component.SetParametersAsync(param));
	}

	[Test]
	public void ShouldNotThrowWhenControlParameterIsNull()
	{
		using var ctx = new WinzorTestContext();
		_ = ctx.RenderComponent<ComponentForTest>(Array.Empty<Bunit.ComponentParameter>());
		Assert.Pass("No exception thrown.");
	}

	[Test]
	public async Task EnsureNullControlPartwayThroughExecutionIsHandledGracefully()
	{
		var exceptionThrown = false;
		using var ctx = new WinzorTestContext();
		ctx.ThreadExceptionExceptionRaised += e => exceptionThrown = true;
		SelfUnregisteringControlForTest control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new Form();
			control = new SelfUnregisteringControlForTest();
			form.Controls.Add(control);
		});
		_ = ctx.RenderComponent<ControlProxyComponent>(param => param.Add(c => c.Control, control));
		Assert.That(exceptionThrown, Is.False);
		Assert.That(control.AfterRenderCalled, Is.True);
	}

	class ComponentForTest : ComponentBase
	{
		public Form Form { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.OpenComponent<ControlProxyComponent>(1);
			builder.AddAttribute(2, "Control", Form);
			builder.CloseComponent();
		}
	}

	class SelfUnregisteringControlForTest : ControlForTest
	{
		public bool AfterRenderCalled { get; private set; }

		protected internal async override Task OnAfterRenderAsync(bool firstRender)
		{
#pragma warning disable BL0005 // Component parameter should not be set outside of its component. Justification: This is a test component to test an edge case where this is set to null in a race condition.
			Proxy.Control = null;
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
			await base.OnAfterRenderAsync(firstRender);
			AfterRenderCalled = true;
		}
	}
}
