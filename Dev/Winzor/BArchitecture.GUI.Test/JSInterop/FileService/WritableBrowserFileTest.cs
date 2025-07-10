using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;

namespace WinzorFramework.JSInterop.FileService;

public class WritableBrowserFileTest
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

		await using var writableBrowserFile = new WritableBrowserFile(string.Empty, mockJSObjectReference.Object);

		Assert.DoesNotThrowAsync(async () => await writableBrowserFile.DisposeAsync());
	}

	[Test]
	public void DisposeAsyncShouldThrowWhenExceptionHappened()
	{
		mockJSObjectReference.Setup(m => m.DisposeAsync())
			.Throws(new Exception("test exception"));

#pragma warning disable CA2000 // Dispose objects before losing scope - this test is about disposing the object
		var writableBrowserFile = new WritableBrowserFile(string.Empty, mockJSObjectReference.Object);
#pragma warning restore CA2000 // Dispose objects before losing scope - this test is about disposing the object

		Assert.ThrowsAsync<Exception>(async () => await writableBrowserFile.DisposeAsync(), "test exception");
	}
}
