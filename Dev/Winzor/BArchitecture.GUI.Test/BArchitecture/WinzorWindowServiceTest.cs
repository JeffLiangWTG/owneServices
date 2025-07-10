using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BArchitecture;
using CargoWise.Blazor.Client.Integration.Messaging;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using FormBorderStyle = CargoWise.Blazor.Client.Integration.Messaging.FormBorderStyle;
using FormStartPosition = CargoWise.Blazor.Client.Integration.Messaging.FormStartPosition;

namespace WinzorFramework;

using static PlaywrightTestContext;

class WinzorWindowServiceTest
{
	[Test]
	public async Task OpenUrlAsync_ShouldCallClientAppWindowService()
	{
		var mockClientAppWindowService = new Mock<IWindowService>();
		var jsRuntime = new Mock<IJSRuntimeWithMonitor>();
		var windowManager = new WinzorWindowService(mockClientAppWindowService.Object);
		var trustMessage = new SystemToSystemTrustMessage();
		await windowManager.OpenUrlAsync(trustMessage);
		mockClientAppWindowService.Verify(x => x.OpenUrlAsync(trustMessage), Times.Once);
	}

	[Test]
	public async Task TestCloseWindowLog()
	{
		var circuitId = new Mock<ICircuitIdProvider>();
		circuitId.Setup(c => c.CircuitId).Returns("CircuitId");
		var logger = new TestLogger<WinzorWindowService>();
		var windowManager = new WinzorWindowService(new Mock<IWindowService>().Object, logger, circuitId.Object);
		await windowManager.RequestCloseAsync();
		Assert.That(logger.logs, Does.Contain("RequestCloseAsync on circuit CircuitId"));
	}
}
