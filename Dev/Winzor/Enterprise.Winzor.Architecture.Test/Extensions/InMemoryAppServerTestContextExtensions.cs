#nullable enable
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;
public static class InMemoryAppServerTestContextExtensions
{
	public static async Task<Uri> InitializeFormAsync(this InMemoryAppServerTestContext ctx, Func<Form> constructor)
	{
		return await ctx.InitializeFormAsync(constructor, ctx.ServerBaseUrl);
	}

	public static async Task<Uri> InitializeFormAsync(this InMemoryAppServerTestContext ctx, Func<Form> constructor, string serverBaseUrl)
	{
		var url = default(Uri);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = constructor();
			url = ctx.WinzorDispatcher.FormInstanceRegister.Add(new Uri(serverBaseUrl), form);
		});
		Assert.That(url, Is.Not.Null);
		return url!;
	}

	public static async Task<Uri> InitializeControlOnFormAsync<T>(this InMemoryAppServerTestContext ctx, Func<T> constructor) where T : Control
	{
		return await ctx.InitializeFormAsync(() => FormWithControl(constructor));
	}

	public static async Task<(Uri url, IInAppComponent<T> component)> InitialiseComponentOnFormAsync<T>(
		this InMemoryAppServerTestContext ctx, Action<ComponentParameterCollectionBuilder<T>>? setup = null
	)
		where T : IComponent
	{
		ComponentProxyControl<T>? component = null;
		var url = await ctx.InitializeControlOnFormAsync(() => component = new ComponentProxyControl<T>(setup));
		return (url, component ?? throw new InvalidOperationException());
	}

	public static async Task<IPage> CaptureFormShowAndLoadAsync(this InMemoryAppServerTestContext ctx, Action showFormAction, int width = 1280, int height = 720)
	{
		var dispatcherContext = new InMemoryAppServerDispatcherContext();
		var page = await PageHelper.LoadPageAsync(await ctx.InitializeFormAsync(() =>
		{
			using (WinzorDispatcher.Current.WithContext(dispatcherContext))
			{
				showFormAction();
			}
			if (dispatcherContext.Form is null)
			{
				throw new InvalidOperationException("Form was not shown");
			}
			return dispatcherContext.Form;
		}), width, height);
		await dispatcherContext.WaitForAllRenderTasksAsync();
		ctx.RegisterDisposeAction(async () => await page.CloseAsync(new PageCloseOptions { RunBeforeUnload = true }));
		await page.Locator(".form").WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 0 });
		return page;
	}

	public static async Task<IPage> LoadFormAsync(this InMemoryAppServerTestContext ctx, Func<Form> constructor, int width = 1280, int height = 720)
	{
		var page = await PageHelper.LoadPageAsync(await ctx.InitializeFormAsync(constructor), width, height);
		ctx.RegisterDisposeAction(async () => await page.CloseAsync(new PageCloseOptions { RunBeforeUnload = true }));
		await page.Locator(".form").WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 0 });
		return page;
	}

	public static async Task<IPage> LoadFormWithScriptAsync(this InMemoryAppServerTestContext ctx, Func<Form> constructor, string script, string serverBaseUrl)
	{
		var page = await PageHelper.LoadPageWithScriptAsync(await ctx.InitializeFormAsync(constructor, serverBaseUrl), script);
		ctx.RegisterDisposeAction(async () => await page.CloseAsync(new PageCloseOptions { RunBeforeUnload = true }));
		await page.Locator(".form").WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 0 });
		return page;
	}

	public static async Task<IPage> LoadControlOnFormAsync<T>(this InMemoryAppServerTestContext ctx, Func<T> constructor)
		where T : Control
		=> await ctx.LoadFormAsync(() => FormWithControl(constructor));

	public static async Task<(IPage page, IInAppComponent<T> component)> LoadComponentOnFormAsync<T>(
		this InMemoryAppServerTestContext ctx, Action<ComponentParameterCollectionBuilder<T>>? setup = null
	)
		where T : IComponent
	{
		ComponentProxyControl<T>? component = null;
		var page = await ctx.LoadControlOnFormAsync(() => component = new ComponentProxyControl<T>(setup));
		return (page, component ?? throw new InvalidOperationException());
	}

	static Form FormWithControl<T>(Func<T> constructor) where T : Control
	{
		var form = new Form();
		var control = constructor();
		form.Controls.Add(control);
		return form;
	}

	class InMemoryAppServerDispatcherContext : IWinzorDispatcherContext
	{
		readonly RenderTasks renderTasks = new();

		public Form? Form { get; private set; }

		public void InvokeRenderDispatcher(Func<Task> workItem)
		{
			throw new NotImplementedException(nameof(InvokeRenderDispatcher));
		}

		public void NotifyRenderRequired(Control control)
		{
			if (Form is not null)
			{
				control.ReadyToRender();
				RegisterRenderTask(control.InvokeStateHasChangedAsync());
			}
		}

		public void OnEnterMessageLoop()
		{
		}

		public OpenFormAction OpenForm(Form form)
		{
			Form = form;
			return OpenFormAction.BlockUntilShown;
		}

		public void RegisterRenderTask(Task task)
		{
			renderTasks.Add(task);
		}

		public async Task WaitForAllRenderTasksAsync()
		{
			await renderTasks.WaitAllAsync();
		}
	}
}
