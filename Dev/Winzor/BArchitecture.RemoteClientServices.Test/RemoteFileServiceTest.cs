using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Client.Integration.DependencyInjection;
using Enterprise.Winzor.Architecture.Test;
using NUnit.Framework;

namespace WinzorFramework.RemoteClientServices.Test
{
	internal class RemoteFileServiceTest
	{
		[Test]
		public async Task TestRemoteFileServicesAreInitializedAsync()
		{
			var control = default(Control);
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderEntryPointComponent(
				() =>
				{
					var form = new Form();
					control = new Control();
					form.Controls.Add(control);
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			await control.InvokeWinzorDispatcherAsync(() =>
			{
				Assert.That(WinzorDispatcher.Current.CurrentContext.Form.CargoWiseClientServices.RemoteFileService, Is.Not.Null);
			});
		}

		[Test]
		public async Task TestIsSupportedMethodAsync([Values] bool expectedValue)
		{
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			ctx.JSInterop
				.Setup<bool>("checkFunctionExists", JavascriptNames.CargoWiseClientName, RemoteFile.OpenFunctionName)
				.SetResult(expectedValue);
			ctx.Services.AddCargoWiseClient();
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			var result = default(bool);
			await form.InvokeWinzorDispatcherAsync(() =>
			{
				result = CargoWiseClientInvoker.Invoke(async (cws) => await cws.RemoteFileService.IsRemoteFileSupportedAsync());
			});

			Assert.That(result, Is.EqualTo(expectedValue));
		}
	}
}
