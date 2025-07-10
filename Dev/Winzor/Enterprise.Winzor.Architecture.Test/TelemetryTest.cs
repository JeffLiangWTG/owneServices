using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Winzor.Telemetry;
using Enterprise.BufferManagement.NetworkVisualisation.GUI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Trace;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

public class TelemetryTest
{
	[Test]
	public async Task TestEntryPointComponentTraces()
	{
		using var ctx = new EnterpriseTestContext();
		ctx.Services.AddSingleton<IWinzorTelemetry, WinzorTelemetry>();

		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		Form form = null;
		var rendered = await ctx.RenderEntryPointComponent(() => form = new Form(), Mock.Of<IWindowService>());
		rendered.WaitForState(() => rendered.Instance.ComponentResolved);
		var component = rendered.Instance;

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using var context = ctx.WinzorDispatcher.WithContext(Mock.Of<IWinzorDispatcherContext>());
			((IWinzorDispatcherContext)component).OpenForm(form);
			component.RegisterRenderTask(Task.CompletedTask);
		});

		await component.WaitForAllRenderTasksAsync();
		var traceNames = traces.Select(t => t.OperationName).Distinct().ToList();

		Assert.Multiple(() =>
		{
			Assert.That(traceNames, Does.Contain("EntryPointComponent.OnInitializedAsync"));
			Assert.That(traceNames, Does.Contain("EntryPointComponent.BuildRenderTree"));
			Assert.That(traceNames, Does.Contain("EntryPointComponent.OpenForm"));
			Assert.That(traceNames, Does.Contain("EntryPointComponent.LoadFormTask"));
			Assert.That(traceNames, Does.Contain("EntryPointComponent.WaitForAllRenderTasksAsync"));
		});

		var initTrace = traces.First(t => t.OperationName == "EntryPointComponent.OnInitializedAsync");
		Assert.Multiple(() =>
		{
			Assert.That(initTrace.TagObjects.Any(t => t.Key == "pooled"));
			Assert.That(initTrace.TagObjects.Any(t => t.Key == "form"));
		});
	}

	[Test]
	public async Task LoadFormFromQueueTrace()
	{
		using var ctx = new EnterpriseTestContext();

		var queue = new OpeningFormQueue();
		ctx.Services.AddSingleton<IOpeningFormQueue>(queue);
		ctx.Services.AddSingleton<EntryPointPool>();
		const string url = "https://test.wisecloud.com/pool/123456";
		ctx.Services.AddSingleton<NavigationManager>(new TestNavigationManager(url));
		ctx.Services.AddSingleton(ctx.WinzorDispatcher);
		ctx.Services.AddTransient<IWindowService, WinzorWindowService>();
		new MockCargoWiseClientSeviceProvider().AddCargoWiseClient(ctx.Services);
		ctx.Services.AddSingleton(Mock.Of<IFileService>());
		ctx.Services.AddSingleton(Mock.Of<IClientEventService>());
		ctx.Services.AddSingleton(Mock.Of<IPingService>());
		ctx.Services.AddSingleton<IWinzorTelemetry, WinzorTelemetry>();

		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		var entryPointComponent = ctx.RenderComponent<EntryPointComponent>();

		long queueFormTimestamp = 0;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };
			queueFormTimestamp = DateTimeOffset.UtcNow.Ticks;
			queue.Write(form);
		});

		entryPointComponent.WaitForState(() => entryPointComponent.Instance.ComponentResolved);

		Assert.That(() => traces.SingleOrDefault(t => t.OperationName == "EntryPointComponent.LoadFormFromQueueAsync"), Is.Not.Null.After(500, 100));
		// Assert that the trace was started after the form was enqueued
		Assert.That(((DateTimeOffset)traces.Single(t => t.OperationName == "EntryPointComponent.LoadFormFromQueueAsync").StartTimeUtc).Ticks, Is.GreaterThan(queueFormTimestamp));
	}

	[Test]
	public async Task AssemblyResolveTrace()
	{
		using var ctx = new EnterpriseTestContext();

		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			// It's used for firing the AssemblyResolve event.
			new NetworkDiagramForm();
		});

		var assemblyResolveTraces = traces.Where(t => t.OperationName == "LoadAssembly").ToList();
		Assert.That(assemblyResolveTraces, Has.Count.GreaterThan(0));

		foreach (var trace in assemblyResolveTraces)
		{
			Assert.That(trace.GetTagItem("name").ToString(), Is.Not.Empty);
			Assert.That(trace.GetTagItem("path").ToString(), Is.Not.Empty);
		}
	}
}
