using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;

#nullable enable
namespace JSInterop;
public class JsInteropBaseWithSpecificExceptionIgnoreTest
{
	readonly static TestCaseData[] ExceptionsShouldBeIgnored =
	{
					new TestCaseData(new JSDisconnectedException("")) { TestName = "JSDisconnectedException" },
					new TestCaseData(new ObjectDisposedException("")) { TestName = "ObjectDisposedException" },
					new TestCaseData(new TaskCanceledException("")) { TestName = "TaskCanceledException" }
	};

	readonly static TestCaseData[] ExceptionsShouldNotBeIgnored =
	{
						new TestCaseData(new Exception("")) { TestName = "General Exception" ,TypeArgs = [typeof(Exception)] } ,
						new TestCaseData(new InvalidOperationException("")) { TestName = "InvalidOperationException" , TypeArgs = [typeof(InvalidOperationException)] },
						new TestCaseData(new NullReferenceException("")) { TestName = "NullReferenceException" , TypeArgs = [typeof(NullReferenceException)] }
	};

	[TestCaseSource(nameof(ExceptionsShouldBeIgnored))]
	public async Task InvokeJsAsync_HandleSpecificException_IgnoreException(Exception exception)
	{
		// Arrange
		var jsRuntimeMock = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();
		mockModule.Setup(js => js.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>())).Throws(exception);
		jsRuntimeMock.Setup(i => i.InvokeAsync<IJSObjectReference>(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(mockModule.Object);
		var fileVersionHashMock = new Mock<IFileVersionHash>();
		fileVersionHashMock.Setup(i => i.Get(It.IsAny<string>())).Returns("");
		await using var jsInteropBase = new TestJsInterop(jsRuntimeMock.Object, "moduleSrc", fileVersionHashMock.Object);

		// Act
		await jsInteropBase.InvokeJsAsync(identifier: "identifier", args: "mockargs");
	}

	[TestCaseSource(nameof(ExceptionsShouldNotBeIgnored))]
	public async Task InvokeJsAsync_HandleSpecificException_ThrowException<T>(Exception exception) where T : Exception
	{
		// Arrange
		var jsRuntimeMock = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();
		mockModule.Setup(js => js.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>())).Throws(exception);
		jsRuntimeMock.Setup(i => i.InvokeAsync<IJSObjectReference>(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(mockModule.Object);
		var fileVersionHashMock = new Mock<IFileVersionHash>();
		fileVersionHashMock.Setup(i => i.Get(It.IsAny<string>())).Returns("");
		await using var jsInteropBase = new TestJsInterop(jsRuntimeMock.Object, "moduleSrc", fileVersionHashMock.Object);

		Assert.ThrowsAsync<T>(async () => await jsInteropBase.InvokeJsAsync(identifier: "identifier", args: "mockargs"));
	}

	class TestJsInterop : JsInteropBaseWithSpecificExceptionIgnore
	{
		public TestJsInterop(IJSRuntime jsRuntime, string moduleSrc, IFileVersionHash fileVersionHash) : base(jsRuntime, moduleSrc, fileVersionHash)
		{
		}
		public new async Task InvokeJsAsync(string identifier, params object[] args)
		{
			await base.InvokeJsAsync(identifier, args);
		}
	}
}

