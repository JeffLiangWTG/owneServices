using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using CargoWise.Blazor.Client.Integration.Files;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
internal class DataImportWizardFormTest
{
	[Test]
	public async Task DataImportWizardFormBrowseFilePreviewListRendered()
	{
		DataImportWizardForm form = null;
		TempFile tempFile = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var collectionInfo = new ImportCollectionInfoImpl(new DummyNonPersistentBusinessObjectCollection(null));
			form = new DataImportWizardForm(collectionInfo, "");
			return form;
		});
		var fileDialogOpened = new TaskCompletionSource<bool>();
		var fileServiceMock = new Mock<IFileService>();
		var fileName = "LocalFileName1";
		fileServiceMock.Setup(i => i.ShowOpenFilePickerAsync(It.IsAny<bool>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() =>
		{
			fileDialogOpened.SetResult(true);
			return Task.FromResult(new [] { new BrowserFile() { Name = fileName } } );
		});
		fileServiceMock.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(async () =>
		{
			tempFile = TempFile.New();
			using (var writer = new StreamWriter(tempFile.Filename))
			{
				await writer.WriteLineAsync("COL11,COL12\r\nROW12,ROW22\r\nROW13,ROW32\r\n");
			}
			return [tempFile.Filename];
		});

		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		var clientFileApi = new Mock<IClientFileApi>();
		clientFileApi.Setup(s => s.ReadFileAsync(It.IsAny<string>()))
			.ReturnsAsync((string path) => new MemoryStream(File.ReadAllBytes(path)));
		form.CargoWiseClientServices.ClientFileApi = clientFileApi.Object;

		var browseButton = rendered.Find("button[data-name='BrowseButton']");
		browseButton.Click();
		Assert.That(await fileDialogOpened.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
		Assert.That(() => rendered.FindAll("div[data-name='PreviewListView'] tbody tr").Count, Is.EqualTo(3).After(2000, 500));
		tempFile.Dispose();
	}
}
