#nullable enable
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using NUnit.Framework;

namespace WinzorTestFramework;

internal static class InMemoryTestServerContextExtensions
{
	public static async Task<Uri> InitializeFormAsync(this InMemoryTestServerContext ctx, Func<Form> constructor, string serverBaseUrl)
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

	public static async Task<Uri> InitializeFormAsync(this InMemoryTestServerContext ctx, Func<Form> constructor)
	{
		return await ctx.InitializeFormAsync(constructor, ctx.ServerBaseUrl);
	}

	public static async Task<Uri> InitializeControlOnFormAsync<T>(this InMemoryTestServerContext ctx, Func<T> constructor) where T : Control
	{
		return await ctx.InitializeFormAsync(() => FormWithControl(constructor));
	}

	public static async Task<(Uri url, IInAppComponent<T> component)> InitialiseComponentOnFormAsync<T>(
		this InMemoryTestServerContext ctx, Action<ComponentParameterCollectionBuilder<T>>? setup = null
	)
		where T : IComponent
	{
		ComponentProxyControl<T>? component = null;
		var url = await ctx.InitializeControlOnFormAsync(() => component = new ComponentProxyControl<T>(setup));
		return (url, component ?? throw new InvalidOperationException());
	}

	public static async Task<IPage> LoadFormAsync(this InMemoryTestServerContext ctx, Func<Form> constructor, string serverBaseUrl, string formClassName = ".form", int width = 1280, int height = 720, bool pageCloseOnDispose = true)
	{
		var page = await PageHelper.LoadPageAsync(await ctx.InitializeFormAsync(constructor, serverBaseUrl), width, height);
		if (pageCloseOnDispose)
		{
			ctx.RegisterDisposeAction(async () => await page.CloseAsync(new PageCloseOptions { RunBeforeUnload = true }));
		}
		await page.Locator(formClassName).WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 0 });
		return page;
	}

	public static Task<IPage> LoadFormAsync(this InMemoryTestServerContext ctx, Func<Form> constructor, string formClassName = ".form", int width = 1280, int height = 720, bool pageCloseOnDispose = true)
	{
		return ctx.LoadFormAsync(constructor, ctx.ServerBaseUrl, formClassName, width, height, pageCloseOnDispose);
	}

	public static async Task<IPage> LoadFormWithScriptAsync(this InMemoryTestServerContext ctx, Func<Form> constructor, string script, string serverBaseUrl)
	{
		var page = await PageHelper.LoadPageWithScriptAsync(await ctx.InitializeFormAsync(constructor, serverBaseUrl), script);
		ctx.RegisterDisposeAction(async () => await page.CloseAsync());
		await page.Locator(".form").WaitForAsync(new () { State = WaitForSelectorState.Attached, Timeout = 0 });
		return page;
	}

	public static async Task<IPage> LoadControlOnFormAsync<T>(this InMemoryTestServerContext ctx, Func<T> constructor)
		where T : Control
		=> await ctx.LoadFormAsync(() => FormWithControl(constructor));

	public static async Task<(IPage page, IInAppComponent<T> component)> LoadComponentOnFormAsync<T>(
		this InMemoryTestServerContext ctx, Action<ComponentParameterCollectionBuilder<T>>? setup = null
	)
		where T : IComponent
	{
		ComponentProxyControl<T>? component = null;
		var page = await ctx.LoadControlOnFormAsync(() => component = new ComponentProxyControl<T>(setup));
		return (page, component ?? throw new InvalidOperationException());
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1109:Do Not Use System.Windows.Forms. Form Or KForm Class", Justification = "<Pending>")]
	static Form FormWithControl<T>(Func<T> constructor) where T : Control
	{
		var form = new Form();
		var control = constructor();
		form.Controls.Add(control);
		return form;
	}
}
