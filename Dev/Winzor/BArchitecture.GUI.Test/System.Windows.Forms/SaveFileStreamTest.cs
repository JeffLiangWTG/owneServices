using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace System.Windows.Forms;

internal sealed class SaveFileStreamTest
{
	[Test]
	public async Task SaveFileStreamWithoutFileReferenceWritesToBrowserFileOnDispose()
	{
		using var ctx = new WinzorTestContext();

		SaveFileDialog.RemoveFileReference("TestFileName.txt");
		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		writableBrowserFileMock.Setup(x => x.FileName).Returns("TestFileName.txt");
		fileServiceMock.Setup(f => f.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		SaveFileStream fileStream = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			fileStream = new SaveFileStream("TestFileName.txt");
			using (var writer = new StreamWriter(fileStream))
			{
				writer.WriteLine("Text Text");
			}
		});

		Assert.That(() => fileStream, Is.Not.Null.After(2000, 100));
		Assert.That(() => fileStream.IsDisposed, Is.True.After(2000, 100));
		fileServiceMock.Verify(f => f.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
		writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Once());
	}
}
