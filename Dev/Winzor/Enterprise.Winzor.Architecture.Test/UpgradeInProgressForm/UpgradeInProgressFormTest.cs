using System.Linq;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Windows.UI;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test.UpgradeInProgressForm;
class UpgradeInProgressFormTest
{
	[Test]
	public async Task TestUpgradeInProgressFormInvokeEnableRestartOnDisconnect()
	{
		using var ctx = new EnterpriseTestContext();
		var renderFormAsync = await ctx.RenderFormAsync(() => new Environment.UpgradeInProgressForm());
		var form = renderFormAsync.GetForm();
		form.Show();
		ctx.JSInterop.VerifyInvoke("enableRestartOnDisconnect", 1);
	}

	[Test]
	public async Task TestUpgradeInProgressFormExitButton()
	{
		using var ctx = new EnterpriseTestContext();
		var formRender = await ctx.RenderFormAsync(() => new Environment.UpgradeInProgressForm());
		var form = formRender.GetForm();
		form.Show();
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var exitButton = form.Controls.Find("exitButton", true).FirstOrDefault() as KButton;
			Assert.That(exitButton, Is.Not.Null);
			exitButton.PerformClick();
			Mock.Get(form.CargoWiseClientServices.LifecycleService).Verify(x => x.RequestApplicationShutDownAsync(), Times.Once());
		});
	}
}
