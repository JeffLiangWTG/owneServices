using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Messaging;
using Enterprise.ZArchitecture.GUI;
using Microsoft.AspNetCore.Components;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

class ProgressFormTest
{
	[Test]
	public async Task TestRequestCloseOnDisposed()
	{
		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);

		ProgressFormForTest progressForm = null;
		await ctx.RenderFormAsync(() => progressForm = new ProgressFormForTest(), clientServices);
		progressForm.Dispose();
		progressForm.DisposedEvent.WaitOne();

		windowService.Verify(w => w.RequestCloseAsync(), Times.Once);
	}

	[Test]
	public async Task TestRequestShowWithoutRenderAndThenDisposed()
	{
		using var ctx = new EnterpriseTestContext();
		var windowService = new Mock<IWindowService>();
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);

		ProgressFormForTest progressForm = null;
		await ctx.ShowFormWithoutRenderAsync(() => progressForm = new ProgressFormForTest(), clientServices);
		Assert.That(() => progressForm.HasRendered, Is.False);

		await progressForm.InvokeWinzorDispatcherAsync(() => progressForm.Hide());

		progressForm.Dispose();
		progressForm.DisposedEvent.WaitOne();
		Assert.That(() => progressForm.IsDisposed, Is.True);
		windowService.Verify(w => w.RequestCloseAsync(), Times.Never);

		await (progressForm as IHandleAfterRender).OnAfterRenderAsync();
		windowService.Verify(w => w.RequestCloseAsync(), Times.Once);
	}

	class ProgressFormForTest : ProgressForm
	{
		internal ProgressFormForTest()
		{
			DisposedEvent = new(false);
			Disposed += delegate
			{
				DisposedEvent.Set();
			};
		}

		internal readonly AutoResetEvent DisposedEvent;
	}
}
