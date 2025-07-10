using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;

namespace WinzorFramework.JSInterop.ClientEventService;

public class RegisteredClientEventTest
{
	Mock<IJSObjectReference> mockJSObjectReference;

	[SetUp]
	public void Setup()
	{
		mockJSObjectReference = new Mock<IJSObjectReference>();
	}

	[Test]
	public async Task DisposeAsyncShouldNotThrowWhenJSDisconnectedExceptionHappened()
	{
		mockJSObjectReference.Setup(m => m.DisposeAsync())
			.Throws(new JSDisconnectedException("Circuit is disconnected"));

		await using var registeredClientEvent = new RegisteredClientEvent(mockJSObjectReference.Object);

		Assert.DoesNotThrowAsync(async () => await registeredClientEvent.DisposeAsync());
	}

	[Test]
	public async Task DisposeAsyncShouldThrowWhenExceptionHappened()
	{
		mockJSObjectReference.Setup(m => m.DisposeAsync())
			.Throws(new Exception("test exception"));

		await using var registeredClientEvent = new RegisteredClientEvent(mockJSObjectReference.Object);

		Assert.ThrowsAsync<Exception>(async () => await registeredClientEvent.DisposeAsync(), "test exception");
	}

	[Test]
	public void DisposeAsyncInvokedMultipleTimesAsync()
	{
		mockJSObjectReference.Setup(m => m.DisposeAsync()).Returns(async () =>
		{
			await Task.Delay(500);
		});
		mockJSObjectReference.Setup(m => m.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()));

		var registeredClientEvent = new RegisteredClientEvent(mockJSObjectReference.Object);
		_ = registeredClientEvent.DisposeAsync();
		_ = registeredClientEvent.DisposeAsync();
		_ = registeredClientEvent.DisposeAsync();
		mockJSObjectReference.Verify(m => m.DisposeAsync(), Times.Once);
	}
}
