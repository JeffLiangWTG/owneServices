using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Dom;
using Bunit;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace WinzorTestFramework;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1109:Do Not Use System.Windows.Forms. Form Or KForm Class", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
public static class ControlAssert
{
	public static async Task ImplementsEventAsync<TControl, TDelegate>(string eventName, Func<Action, TDelegate> delegateConstructor, string cssSelector, Action<IElement> triggerEvent) where TControl : Control, new() where TDelegate : Delegate
	{
		await ImplementsEventAsync<TControl, TDelegate>(eventName, c => { }, delegateConstructor, cssSelector, triggerEvent);
	}

	public static async Task ImplementsEventAsync<TControl, TDelegate>(string eventName, Action<TControl> controlInitializer, Func<Action, TDelegate> delegateConstructor, string cssSelector, Action<IElement> triggerEvent) where TControl : Control, new() where TDelegate : Delegate
	{
		using var ctx = new WinzorTestContext();
		ctx.JSInterop.Mode = JSRuntimeMode.Loose;
		var onMethodThreadId = 0;
		var cargoWiseTcs = new TaskCompletionSource();
		var eventFiredTcs = new TaskCompletionSource();
		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var control = new TControl();
			controlInitializer(control);
			var eventInfo = control.GetType().GetEvent(eventName);
			eventInfo.AddEventHandler(control, delegateConstructor(() =>
			{
				eventFiredTcs.SetResult();
				cargoWiseTcs.Task.GetAwaiter().GetResult();
				onMethodThreadId = Environment.CurrentManagedThreadId;
			}));

			if (control is ToolStripItem)
			{
				var toolStrip = new ToolStrip();
				toolStrip.Items.Add(control as ToolStripItem);
				form.Controls.Add(toolStrip);
			}
			else
			{
				form.Controls.Add(control);
			}
		});

		try
		{
			triggerEvent(rendered.Find(cssSelector));
			Assert.That(await eventFiredTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True, "Event was not fired within 1 second timeout.");
			Assert.That(onMethodThreadId, Is.EqualTo(0), "Render thread should not block on dispatcher invoke");
			cargoWiseTcs.SetResult();
			Assert.That(() => onMethodThreadId, Is.EqualTo(ctx.WinzorDispatcher.ManagedThreadId).After(100, 10));
		} finally
		{
			cargoWiseTcs.TrySetResult();
		}
	}

	public static async Task ImplementsProtectedOnMethodAsync<TControl, TEventArgs>(string methodName, string cssSelector, Action<IElement> triggerEvent) where TControl : Control, new()
	{
		await ImplementsProtectedOnMethodAsync<TControl, TEventArgs>(methodName, c => { }, cssSelector, triggerEvent);
	}

	public static async Task ImplementsProtectedOnMethodAsync<TControl, TEventArgs>(string methodName, Action<TControl> controlInitializer, string cssSelector, Action<IElement> triggerEvent) where TControl : Control
	{
		using var ctx = new WinzorTestContext();
		var onMethodThreadId = 0;
		var cargoWiseTcs = new TaskCompletionSource();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var control = new Mock<TControl>() { CallBase = true };
			controlInitializer(control.Object);
			control.Protected().Setup(methodName, ItExpr.IsAny<TEventArgs>()).Callback(() =>
			{
				cargoWiseTcs.Task.GetAwaiter().GetResult();
				onMethodThreadId = Environment.CurrentManagedThreadId;
			});
			return control.Object;
		});
		triggerEvent(rendered.Find(cssSelector));
		Assert.That(onMethodThreadId, Is.EqualTo(0), "Render thread should not block on dispatcher invoke");
		cargoWiseTcs.SetResult();
		Assert.That(() => onMethodThreadId, Is.EqualTo(ctx.WinzorDispatcher.ManagedThreadId).After(100, 10));
	}

	public static async Task EventHandlerOnlyInvokedWhenEnabled<TControl>(string eventHandler, Func<TControl, Task> invokeEvent) where TControl : Control
	{
		await EventHandlerOnlyInvokedWhenEnabled(eventHandler, invokeEvent, Array.Empty<object>());
	}

	public static async Task EventHandlerOnlyInvokedWhenEnabled<TControl, TEventArgs>(string eventHandler, Func<TControl, Task> invokeEvent) where TControl : Control
	{
		await EventHandlerOnlyInvokedWhenEnabled(eventHandler, invokeEvent, ItExpr.IsAny<TEventArgs>());
	}

	public static async Task EventHandlerOnlyInvokedWhenEnabled<TControl>(string eventHandler, Func<TControl, Task> invokeEvent, params object[] eventHandlerArgs) where TControl : Control
	{
		using var ctx = new WinzorTestContext();
		TControl control = null;
		var eventHandlerFired = false;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var mock = new Mock<TControl>() { CallBase = true };
			mock.Protected().Setup(eventHandler, eventHandlerArgs).Callback(() =>
			{
				eventHandlerFired = true;
			});
			control = mock.Object;
			control.Enabled = false;

			var form = new Form();
			form.Controls.Add(control);
		});
		await invokeEvent(control);
		Assert.That(eventHandlerFired, Is.False, "Event hander was fired while control was disabled");

		await ctx.WinzorDispatcher.InvokeAsync(() => control.Enabled = true);
		await invokeEvent(control);
		Assert.That(eventHandlerFired, Is.True, "Event handler was not fired while control was enabled");
	}

	public static async Task ImplementsChangeFromServerAsync<TControl>(Action<TControl> change) where TControl : Control, new()
	{
		using var ctx = new WinzorTestContext();
		var cargoWiseTcs = new TaskCompletionSource();
		TControl control = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			control = new TControl();
			if (control is ToolStripItem)
			{
				var toolStrip = new ToolStrip();
				toolStrip.Items.Add(control as ToolStripItem);
				form.Controls.Add(toolStrip);
			}
			else
			{
				form.Controls.Add(control);
			}
			return form;
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(1));

		await control.InvokeWinzorDispatcherAsync(() => change(control));

		rendered.WaitForState(() => rendered.RenderCount >= 2);
		Assert.That(() => rendered.RenderCount, Is.Not.GreaterThan(2).After(5000, 100), "Too many renders.");
	}

	public static async Task NoOpFromServerAsync<TControl>(Action<TControl> change) where TControl : Control, new()
	{
		using var ctx = new WinzorTestContext();
		var (rendered, control) = await ctx.RenderControlOnFormAsync<TControl>();
		Assert.That(rendered.RenderCount, Is.EqualTo(1));
		var cargoWiseTcs = new TaskCompletionSource();
		var cargoWiseTask = control.InvokeWinzorDispatcherAsync(() =>
		{
			change(control);
			cargoWiseTcs.Task.GetAwaiter().GetResult();
		});
		rendered.WaitForState(() => rendered.RenderCount == 1);
		Assert.That(await cargoWiseTask.WithTimeout(TimeSpan.FromMilliseconds(100)), Is.False);
		cargoWiseTcs.SetResult();
		Assert.That(await cargoWiseTask.WithTimeout(TimeSpan.FromMilliseconds(100)), Is.True);
	}

	public static async Task FiresControlEvent<TControl, TDelegate>(string eventName, Func<Action, TDelegate> delegateConstructor, Action<TControl> triggerEvent) where TControl : Control, new() where TDelegate : Delegate
	{
		await FiresControlEvent(eventName, c => { }, delegateConstructor, triggerEvent);
	}

	public static async Task FiresControlEvent<TControl, TDelegate>(string eventName, Action<TControl> controlInitializer, Func<Action, TDelegate> delegateConstructor, Action<TControl> triggerEvent) where TControl : Control, new() where TDelegate : Delegate
	{
		var eventFired = false;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var control = new TControl();
			controlInitializer(control);
			var eventInfo = control.GetType().GetEvent(eventName);
			eventInfo.AddEventHandler(control, delegateConstructor(() =>
			{
				eventFired = true;
			}));
			triggerEvent(control);
			return control;
		});
		Assert.That(eventFired, Is.True);
	}
}
