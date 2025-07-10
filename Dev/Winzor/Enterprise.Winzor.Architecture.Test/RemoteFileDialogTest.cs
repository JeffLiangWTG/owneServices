using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

class RemoteFileDialogTest : ShortcutCreator
{
	[Test]
	public async Task RemoteFileDialogSaveFileTest()
	{
		using var ctx = new EnterpriseTestContext();

		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock.Setup(f => f.SaveFileByPathAsync(It.IsAny<string>(), It.IsAny<byte[]>())).Returns(() => Task.FromResult(true));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			Assert.DoesNotThrow(() =>
			{
				RemoteFileDialog.SaveFile(string.Empty, Array.Empty<byte>());
			});
		});

		fileServiceMock.Verify(f => f.SaveFileByPathAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once());
	}
}
