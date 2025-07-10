using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

class ZRichEditOleObjectInserterTest
{
	[Test, MaxTime(10000)]
	public async Task ZRichTextBoxShouldNotPasteImageAsRawUrl()
	{
		using var ctx = new EnterpriseTestContext();
		ZRichTextBox richTextBox = null!;
		var imageInsertedFired = new TaskCompletionSource<string[]>();
		ZForm form = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new ZForm();
			form.PlugIns.Add(DummyControllerIDs.eDocsPlugInForTesting);
			form.DataObjectPasted += (object sender, DataObjectPastedEventArgs e) =>
			{
				if (e.DataToPaste.GetDataPresent(DataFormats.FileDrop))
				{
					var fileNames = (string[])e.DataToPaste.GetData(DataFormats.FileDrop);
					foreach (var fileName in fileNames)
					{
						e.AddPastedFile(fileName, Path.GetFileName(fileName), string.Empty, Guid.NewGuid(), Guid.NewGuid(), true);
					}
				}
			};
			richTextBox = new ZRichTextBox()
			{
				Width = 500,
				Height = 500,
			};
			form.Controls.Add(richTextBox);
			return form;
		});
		using var tempFile = TempFile.New();
		var fileName = Path.GetFileName(tempFile.Filename);
		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(() =>
		{
			return Task.FromResult<string[]>([tempFile.Filename]);
		});
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;
		await richTextBox.RichEdit.OnInsertImagesAsync(new BrowserFile[] { new BrowserFile() { Name = fileName } }, 0, 0);
		var fileLinkPasted = new TaskCompletionSource<string>();
		Assert.That(richTextBox.RichEdit.SelectedHtml, Does.Contain($"File {Path.GetFileName(fileName)} () added to eDocs tab."));
	}
}
