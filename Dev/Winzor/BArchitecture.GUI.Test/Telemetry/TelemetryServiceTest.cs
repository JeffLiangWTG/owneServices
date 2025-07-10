using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Trace;
using WinzorTestFramework;
using static WinzorTestFramework.WinzorTestContext;

namespace WinzorFramework.Test;

class TelemetryServiceTest
{
	[Test]
	public async Task TestControlProxyTraces()
	{
		using var ctx = new WinzorTestContext();

		TextBox textBox = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => textBox = new TextBox());

		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		var component = ctx.RenderComponent<ControlProxyComponent>(parameters => parameters.Add(p => p.Control, textBox));
		var proxy = component.Instance;
		await proxy.InvokeAsync(() =>
		{
			proxy.StateHasChanged();
			return Task.CompletedTask;
		});

		Assert.Multiple(() =>
		{
			Assert.That(traces.Any(t => t.OperationName == "TextBoxProxy.InvokeAsync"));
		});
	}

	[Test]
	public async Task TestFormTraces()
	{
		using var ctx = new WinzorTestContext();

		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.None);
		var showDialogTask = ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new Form();
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				form.ShowDialog();
			}
		});
		Assert.That(() => dispatcherContext.Rendered, Is.Not.Null.After(1000));
		var form = dispatcherContext.Rendered.GetForm();

		await form.CloseHandlerAsync();
		await form.InvokeWinzorDispatcherAsync(() => form.Dispose());
		await showDialogTask;

		Assert.Multiple(() =>
		{
			Assert.That(traces.Any(t => t.OperationName == "Form.OnInitializedAsync"));
			Assert.That(traces.Any(t => t.OperationName == "Form.RenderMainMenuAsync"));
			Assert.That(traces.Any(t => t.OperationName == "Form.ShowDialog"));
			Assert.That(traces.Any(t => t.OperationName == "Form.ShowCore"));
			Assert.That(traces.Any(t => t.OperationName == "Form.CloseHandlerAsync"));
			Assert.That(traces.Any(t => t.OperationName == "Form.OnFormClosing"));
		});
	}

	[Test]
	public async Task TestControlTraces()
	{
		using var ctx = new WinzorTestContext();

		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => control = new Control());
		control.CreateControl();
		control.ReadyToRender();
		await control.InvokeWinzorDispatcherAsync(() => { });
		control.RegisterAfterRenderAction(() => Task.CompletedTask);
		await ((IHandleAfterRender)control).OnAfterRenderAsync();

		Assert.Multiple(() =>
		{
			Assert.That(traces.Any(t => t.OperationName == "Control.CreateControl"));
			Assert.That(traces.Any(t => t.OperationName == "Control.ReadyToRender"));
			Assert.That(traces.Any(t => t.OperationName == "Control.InvokeWinzorDispatcherAsync"));
			Assert.That(traces.Any(t => t.OperationName == "Control.TaskWithOnBeforeRender"));
			Assert.That(traces.Any(t => t.OperationName == "Control.WinzorDispatcherTask"));
			Assert.That(traces.Any(t => t.OperationName == "Control.AfterRenderAction"));
			Assert.That(traces.Any(t => t.OperationName == "Control.OnAfterRenderAsync"));
		});
	}

	[Test]
	public async Task TestDataGridTraces()
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		DataGrid dataGrid = null;
		using var ctx = new WinzorTestContext();
		await ctx.RenderControlOnFormAsync(() =>
		{
			dataGrid = new DataGrid() { DataSource = new DataTable() };
			return dataGrid;
		});

		dataGrid.CurrentCell = new DataGridCell(0, 0);
		await dataGrid.OnCellFocusInAsync(new WinzorFocusInEventArgs() { InitiatedFromServer = false }, 0, 0);

		Assert.Multiple(() =>
		{
			Assert.That(traces.Any(t => t.OperationName == "DataGrid.SetDataGridRows"));
			Assert.That(traces.Any(t => t.OperationName == "DataGrid.OnBeforeRender"));
			Assert.That(traces.Any(t => t.OperationName == "DataGrid.SetCurrentCell"));
			Assert.That(traces.Any(t => t.OperationName == "DataGrid.OnCellFocusInAsync"));
		});
	}

	[Test]
	public async Task TestMenuInteropTraces()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => control = new Control());

		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		var menuInterop = new MenuInterop(control, MenuType.MainMenu, Guid.NewGuid());
		await menuInterop.OnLoadSubMenuAsync(new SubMenuLoadRequest(Guid.NewGuid(), Guid.NewGuid()));

		Assert.That(traces.Any(t => t.OperationName == "MenuInterop.OnLoadSubMenuAsync"));
	}

	[Test]
	[SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	public async Task TabPageChangedTrace()
	{
		using var ctx = new WinzorTestContext();

		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tabControl = new TabControl();
			var fooTabPage = new TabPage { Text = "Foo" };
			var barTabPage = new TabPage { Text = "Bar" };
			tabControl.TabPages.Add(fooTabPage);
			tabControl.TabPages.Add(barTabPage);
			return tabControl;
		});

		var operationNameList = traces.Select(t => t.OperationName).ToList();
		Assert.That(operationNameList, Does.Not.Contain("TabControl.TabChanged"));

		await rendered.Find("button:contains('Bar')").ClickAsync(new WebMouseEventArgs());

		operationNameList = traces.Select(t => t.OperationName).ToList();
		Assert.That(operationNameList, Does.Contain("TabControl.TabChanged"));
	}
}
