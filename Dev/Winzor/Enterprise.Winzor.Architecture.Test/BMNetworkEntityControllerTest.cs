using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
namespace Enterprise.Winzor.Architecture.Test;

class BMNetworkEntityControllerTest
{
	[Test]
	public async Task TestEmptyClipboard()
	{
		using var ctx = new EnterpriseTestContext();
		await SafeClipboard.FetchClipboardDataAsync(ctx.JSInterop.JSRuntime);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new ZForm();
			var refresher = new JobNetworkRefresher();
			var controller = new BMNetworkEntityController(refresher, form);
			Assert.DoesNotThrow(() => controller.GetJobsFromClipboard(new BusinessObjectFactory()));
		});
	}
}
