using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using NUnit.Framework;

namespace WinzorFramework.Extensions;
public class ExceptionHandlerExtensionTest
{
	[Test]
	public void HandleJSExceptionAsyncSwallowJSDisconnectedException()
	{
		Func<Task> failingAction = () =>
		{
			throw new JSDisconnectedException("JSDisconnected");
		};
		Assert.DoesNotThrowAsync(async () => await ExceptionHandlerExtension.HandleJSExceptionAsync(failingAction));
	}

	[Test]
	public void HandleJSExceptionAsyncThrowsOtherExceptions()
	{
		Func<Task> failingAction = () =>
		{
			throw new Exception("OtherException");
		};
		Assert.ThrowsAsync<Exception>(async () => await ExceptionHandlerExtension.HandleJSExceptionAsync(failingAction));
	}
}
