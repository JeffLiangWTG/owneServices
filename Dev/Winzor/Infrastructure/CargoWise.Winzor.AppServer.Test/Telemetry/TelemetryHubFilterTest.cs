using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.Winzor.AppServer.Telemetry;
using CargoWise.Winzor.Telemetry;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace CargoWise.Winzor.AppServer.Test.Telemetry;

public class TelemetryHubFilterTest
{
	[Test]
	public async Task TestInvokeMethodAsync()
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		var signalRTelemetry = new Mock<ISignalRTelemetry>();
		using var activitySource = new ActivitySource("CargoWise.Winzor.SignalR");
		signalRTelemetry.Setup(s => s.ActivitySource).Returns(activitySource);
		var hubCallerContext = new Mock<HubCallerContext>();
		var serviceProvider = new Mock<IServiceProvider>();
		var hub = new Mock<Hub>();
		var hubMethod = new Mock<MethodInfo>();
		hubMethod.Setup(m => m.Name).Returns("BeginInvokeDotNetFromJS");
		var dotNetMethodName = "Test";
		var hubMethodArguments = new List<object> { null, null, dotNetMethodName };
		var hubInvocationContext = new HubInvocationContext(hubCallerContext.Object, serviceProvider.Object, hub.Object, hubMethod.Object, hubMethodArguments);
		var next = new Func<HubInvocationContext, ValueTask<object>>(_ => ValueTask.FromResult<object>(null));

		var filter = new TelemetryHubFilter(signalRTelemetry.Object);
		await filter.InvokeMethodAsync(hubInvocationContext, next);

		Assert.That(traces.Any(t => t.OperationName == $"BeginInvokeDotNetFromJS.{dotNetMethodName}"));
	}
}
