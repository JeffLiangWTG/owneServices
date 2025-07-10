using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Winzor.Architecture.Test;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace CargoWise.Winzor.AppServer.Test;

internal class ProgramTest
{
	[Test, WithPlaywrightPage, WithDefaultLatencyNetworkEffect]
	public async Task WinzorDispatcherDisposeThrowNoExceptionWhenShutdownCausedByCircuitClosed()
	{
		System.Environment.SetEnvironmentVariable("CargoWiseOptions__DisconnectedCircuitRetentionPeriod", "0:0:2");
		await using (var ctx = new InMemoryAppServerTestContext(keepCircuitHandler: true))
		{
			var page = await ctx.LoadFormWithScriptAsync(() => new Form(), ToxiProxyHelper.CustomReconnectionOptions(0, TimeSpan.FromSeconds(1)), WithDefaultLatencyNetworkEffect.ServerBaseUrl);
			_ = ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				using (ctx.WinzorDispatcher.WithContext(Mock.Of<IWinzorDispatcherContext>()))
				{
					var dialog = new Form();
					dialog.Visible = true;
					dialog.ShowDialog();
				}
			});
			await WithDefaultLatencyNetworkEffect.DisconnectProxyAsync();
			Assert.That(() => System.Windows.Forms.Application.OpenForms.Count, Is.EqualTo(0).After(5000, 500));

			ctx.RegisterDisposeAction(() =>
			{
				ctx.WinzorDispatcher.Dispose();
			});
		}
		ToxiProxyHelper.CleanupPageErrors();
	}
}
