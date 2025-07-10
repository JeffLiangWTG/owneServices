using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace WinzorFramework.JSInterop;

class JSInteropBaseTest
{
	Mock<IJSRuntime> mockJsRuntime;
	Mock<IJSObjectReference> mockModule;
	Mock<IFileVersionHash> mockFileVersionHash;
	IJSRuntimeWithMonitor jSRuntimeWithMonitor;

	class TestInterop : JSInteropBase
	{
		internal static string ModuleSrc = "/_content/WinzorFramework/js/module/mock.js";

		public TestInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
			: base(jsRuntime, ModuleSrc, fileVersionHash)
		{
		}

		public new async Task InvokeJsAsync(string identifier, params object[] args)
			=> await base.InvokeJsAsync(identifier, args: args);

		public new async Task<T> InvokeJsAsync<T>(string identifier, CancellationToken cancellationToken, params object[] args)
			=> await base.InvokeJsAsync<T>(identifier, cancellationToken: cancellationToken, args: args);

		public new async Task<T> InvokeJsAsync<T>(string identifier, params object[] args)
			=> await base.InvokeJsAsync<T>(identifier, args: args);
	}

	[SetUp]
	public void Setup()
	{
		mockJsRuntime = new Mock<IJSRuntime>();
		mockModule = new Mock<IJSObjectReference>();
		mockFileVersionHash = new Mock<IFileVersionHash>();
		mockJsRuntime.Setup(i => i.InvokeAsync<IJSObjectReference>(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(mockModule.Object);
		mockFileVersionHash.Setup(m => m.Get(It.IsAny<string>())).Returns("?v=version");
		jSRuntimeWithMonitor = new JSRuntimeWithMonitor(mockJsRuntime.Object);
	}

	[Test]
	public async Task ModuleIsLazyLoaded()
	{
		mockModule.Setup(js => js.InvokeAsync<int>(It.IsAny<string>(), It.IsAny<object[]>()));

		await using var interop = new TestInterop(jSRuntimeWithMonitor, mockFileVersionHash.Object);
		mockJsRuntime.Verify(m => m.InvokeAsync<IJSObjectReference>("import", It.IsAny<object[]>()), Times.Never);

		await interop.InvokeJsAsync<object>("mockAsync", "mockArgs");
		mockJsRuntime.Verify(m => m.InvokeAsync<IJSObjectReference>("import", new object[] { TestInterop.ModuleSrc + "?v=version" }), Times.Once);
	}

	[Test]
	public async Task ModuleIsOnlyLoadedOnce()
	{
		mockModule.Setup(js => js.InvokeAsync<int>(It.IsAny<string>(), It.IsAny<object[]>()));

		await using var interop = new TestInterop(jSRuntimeWithMonitor, mockFileVersionHash.Object);
		mockJsRuntime.Verify(m => m.InvokeAsync<IJSObjectReference>("import", It.IsAny<object[]>()), Times.Never);

		await interop.InvokeJsAsync<object>("mockAsync", "mockArgs");
		await interop.InvokeJsAsync<object>("mockAsync", "mockArgs");
		await interop.InvokeJsAsync<object>("mockAsync", "mockArgs");
		mockJsRuntime.Verify(m => m.InvokeAsync<IJSObjectReference>("import", new object[] { TestInterop.ModuleSrc + "?v=version" }), Times.Once);
	}

	public static IEnumerable<TestCaseData> ExceptionTestCases
	{
		get
		{
			yield return new TestCaseData(new JSException("JS Exception")) { TestName = "{m}_JSException" };
			yield return new TestCaseData(new JSDisconnectedException("JS Disconnected Exception")) { TestName = "{m}_JSDisconnectedException" };
		}
	}

	[TestCaseSource(nameof(ExceptionTestCases))]

	public async Task JSExceptionsAreNotHandled(Exception ex)
	{
		mockModule.Setup(js => js.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>())).ThrowsAsync(ex);

		await using var interop = new TestInterop(jSRuntimeWithMonitor, mockFileVersionHash.Object);
		Assert.ThrowsAsync(ex.GetType(), async () => await interop.InvokeJsAsync<object>("mockAsync", "mockArgs"));
	}

	[Test]
	public async Task InvokeVoidAsyncCalledOnModule()
	{
		mockModule.Setup(js => js.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>()));

		await using var interop = new TestInterop(jSRuntimeWithMonitor, mockFileVersionHash.Object);
		await interop.InvokeJsAsync("mockAsync", "mockArgs");

		mockModule.Verify(m => m.InvokeAsync<IJSVoidResult>("mockAsync", new object[] { "mockArgs" }), Times.Once);
	}

	[Test]
	public async Task InvokeAsyncCalledOnModule()
	{
		mockModule.Setup(js => js.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()));

		await using var interop = new TestInterop(jSRuntimeWithMonitor, mockFileVersionHash.Object);
		await interop.InvokeJsAsync<object>("mockAsync", "mockArgs");

		mockModule.Verify(m => m.InvokeAsync<object>("mockAsync", new object[] { "mockArgs" }), Times.Once);
	}

	[Test]
	public async Task InvokeAsyncWithCancellationTokenCalledOnModule()
	{
		mockModule.Setup(js => js.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()));

		await using var interop = new TestInterop(jSRuntimeWithMonitor, mockFileVersionHash.Object);
		await interop.InvokeJsAsync<object>("mockAsync", CancellationToken.None, "mockArgs");

		mockModule.Verify(m => m.InvokeAsync<object>("mockAsync", CancellationToken.None, new object[] { "mockArgs" }), Times.Once);
	}

	[Test]
	public async Task TestJSInteropTraces()
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("WinzorFramework")
			.AddInMemoryExporter(traces)
			.Build();

		mockModule.Setup(js => js.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()));

		await using var interop = new TestInterop(jSRuntimeWithMonitor, mockFileVersionHash.Object);
		await interop.InvokeJsAsync<object>("mockAsync", "mockArgs");

		Assert.That(traces, Has.Count.EqualTo(2));
		Assert.That(traces[0].OperationName, Is.EqualTo("JsInvoke_import_mock.js"));
		Assert.That(traces[1].OperationName, Is.EqualTo("JsInvoke_mock.js_mockAsync"));
	}
}
