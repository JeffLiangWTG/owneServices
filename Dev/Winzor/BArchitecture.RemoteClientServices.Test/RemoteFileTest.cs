using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Client.Integration.Files.Requests;
using CargoWise.Blazor.Client.Integration.Files.Responses;
using CargoWise.Blazor.Client.Integration.Messaging;
using Enterprise.Winzor.Architecture.Test;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework.RemoteClientServices.Test
{
	internal class RemoteFileTest
	{
		[Test]
		public async Task IdIsNotEmptyAsync()
		{
			var name = "Sample";
			var bytes = Encoding.UTF8.GetBytes(name);
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			using var clientFile = new CargoWiseClientFile(name, bytes, false, false);
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			Assert.That(clientFile.Id, Is.Not.Empty);
		}

		[Test]
		public async Task TestOpenMethodAsync([Values] bool readOnly, [Values] bool callOnExit)
		{
			var name = "Sample";
			var bytes = Encoding.UTF8.GetBytes(name);
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			using var clientFile = new CargoWiseClientFile(name, bytes, readOnly, callOnExit);
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				_ = clientFile.Open();
			});

			if (callOnExit)
			{
				ctx.MockCargoWiseClientServices.RemoteFileService.Verify(x => x.OpenAsync(
					It.Is<OpenFileRequest>(f => f.Id == clientFile.Id && f.FileName == name && f.Data.SequenceEqual(bytes) && f.ReadOnly == readOnly),
					It.IsNotNull<Action<FileChangedResponse>>(),
					It.IsNotNull<Action<ProcessExitResponse>>()));
			}
			else
			{
				ctx.MockCargoWiseClientServices.RemoteFileService.Verify(x => x.OpenAsync(
					It.Is<OpenFileRequest>(f => f.Id == clientFile.Id && f.FileName == name && f.Data.SequenceEqual(bytes) && f.ReadOnly == readOnly),
					It.IsNotNull<Action<FileChangedResponse>>(),
					null));
			}
		}

		[Test]
		public async Task TestIsOpenMethodAsync()
		{
			var name = "Sample";
			var bytes = Encoding.UTF8.GetBytes(name);
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			using var clientFile = new CargoWiseClientFile(name, bytes, false, false);
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				_ = clientFile.GetIsOpenStatus();
			});

			ctx.MockCargoWiseClientServices.RemoteFileService.Verify(x => x.IsOpenAsync(It.IsAny<IsFileOpenRequest>()));
		}

		[Test]
		public async Task TestDoesExistsMethodAsync()
		{
			var name = "Sample";
			var bytes = Encoding.UTF8.GetBytes(name);
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			using var clientFile = new CargoWiseClientFile(name, bytes, false, false);
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				_ = clientFile.GetDoesExistStatus();
			});

			ctx.MockCargoWiseClientServices.RemoteFileService.Verify(x => x.DoesExistsAsync(It.IsAny<FileExistsRequest>()));
		}

		[Test]
		public async Task TestFetchDataMethodAsync()
		{
			var name = "Sample";
			var bytes = Encoding.UTF8.GetBytes(name);
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			using var clientFile = new CargoWiseClientFile(name, bytes, false, false);
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				_ = clientFile.FetchFileData();
			});

			ctx.MockCargoWiseClientServices.RemoteFileService.Verify(x => x.FetchDataAsync(It.IsAny<FetchFileDataRequest>()));
		}

		[Test, WithPlaywrightPage]
		public async Task TestFetchLargeFileAsync()
		{
			var fileData = JsonConvert.SerializeObject(new FetchFileDataResponse(Guid.NewGuid(), new byte[20000000]));

			var form = default(Form);
			var clientServiceProvider = new MockCargoWiseClientSeviceProvider() { MockRemoteFileService = null };
			await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);
			var url = await ctx.InitializeFormAsync(() =>
			{
				form = new Form();
				return form;
			});

			var page = await PageHelper.GetPageAsync();
			await page.AddInitScriptAsync(@$"
				window.{JavascriptNames.CargoWiseClientName} = {{
					{RemoteFile.FetchDataFunctionName} : () => '{fileData}'
				}};
			");

			var response = await page.GotoAsync(url.ToString());
			Assert.That(response.Ok);
			await page.Locator(".form").WaitForAsync();

			var reqest = new FetchFileDataRequest(Guid.NewGuid());

			var fileResponse = await form.CargoWiseClientServices.RemoteFileService.FetchDataAsync(reqest);

			Assert.That(fileResponse.FileData, Has.Length.EqualTo(20000000));
			await page.CloseAsync();
		}

		[Test]
		public async Task TestDisposeMethodAsync()
		{
			var name = "Sample";
			var bytes = Encoding.UTF8.GetBytes(name);
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			using var clientFile = new CargoWiseClientFile(name, bytes, false, false);
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				Assert.That(() => clientFile.Dispose(), Throws.Nothing);
			});

			ctx.MockCargoWiseClientServices.RemoteFileService.Verify(x => x.DisposeAsync(It.IsAny<DisposeFileRequest>()));
		}

		[Test]
		public async Task TestDisposeAfterCloseMethodAsync()
		{
			var name = "Sample";
			var bytes = Encoding.UTF8.GetBytes(name);
#pragma warning disable CA2000 // Dispose objects before losing scope, the test disposes of the clientFile in a different thread, disposal is not possible using a "using" as it requires an open form which is closed as part of the test
			var clientFile = new CargoWiseClientFile(name, bytes, false, false);
#pragma warning restore CA2000 // Dispose objects before losing scope
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			await form.InvokeWinzorDispatcherAsync(form.Close);

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				Assert.That(() => clientFile.Dispose(), Throws.InvalidOperationException);
			});

			ctx.MockCargoWiseClientServices.RemoteFileService.Verify(x => x.DisposeAsync(new DisposeFileRequest(clientFile.Id)), Times.Never);
		}

		[Test]
		public async Task CallbackIsReceivedOnChangedAndExitAsync()
		{
			var name = "Sample";
			var bytes = Encoding.UTF8.GetBytes(name);
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			using var clientFile = new CargoWiseClientFile(name, bytes, false, true);
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			ctx.MockCargoWiseClientServices.RemoteFileService
				.Setup(x => x.OpenAsync(
					It.Is<OpenFileRequest>(f => f.Id == clientFile.Id && f.FileName == name && f.Data.SequenceEqual(bytes) && !f.ReadOnly),
					It.IsAny<Action<FileChangedResponse>>(),
					It.IsAny<Action<ProcessExitResponse>>()))
				.Callback((OpenFileRequest request, Action<FileChangedResponse> changed, Action<ProcessExitResponse> onExit) =>
				{
					changed.Invoke(new FileChangedResponse(Guid.Empty, true));
					onExit.Invoke(new ProcessExitResponse(0));
				})
				.ReturnsAsync(RequestSentResult.MessageSent);

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				int counter = 0;
				int exitCounter = 3;
				clientFile.FileChanged += (s, e) => { counter = 6; };
				clientFile.ProcessExited += (s, e) => { exitCounter = -1; };
				_ = clientFile.Open();
				Assert.That(counter, Is.EqualTo(6));
				Assert.That(exitCounter, Is.EqualTo(-1));
			});
		}
	}
}
