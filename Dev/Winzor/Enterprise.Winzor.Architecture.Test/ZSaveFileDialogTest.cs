using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

internal sealed class ZSaveFileDialogTest
{
	[Test]
	public async Task ZSaveFileDialogCallsInvokesFileOk()
	{
		using var ctx = new EnterpriseTestContext();
		ctx.MockCargoWiseClientServices.FileService.Setup(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(Mock.Of<IWritableBrowserFile>()));
		var fileOk = new TaskCompletionSource();

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form());

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var dialog = new ZSaveFileDialog();
			dialog.FileOk += (_, _) => fileOk.SetResult();
			dialog.ShowDialog();
		});

		Assert.That(await fileOk.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test]
	public async Task ZSaveFileDialogOpenFileWritesToBrowserFileOnDispose()
	{
		using var ctx = new EnterpriseTestContext();

		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		writableBrowserFileMock.Setup(x => x.FileName).Returns("TestFileName.txt");
		fileServiceMock.Setup(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));

		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);
		Stream fileStream = null;

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var zSaveFileDialog = new ZSaveFileDialog();
			var dialogResult = zSaveFileDialog.ShowDialog();
			Assert.That(dialogResult, Is.EqualTo(DialogResult.OK));
			fileStream = ZSaveFileDialog.OpenFile("TestFileName.txt");
		});

		fileServiceMock.Verify(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
		writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Never());
		Assert.That(fileStream, Is.Not.Null);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			using (var writer = new StreamWriter(fileStream))
			{
				writer.WriteLine("Text Text");
			}
		});

		fileServiceMock.Verify(f => f.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
		writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Once());
	}
}
