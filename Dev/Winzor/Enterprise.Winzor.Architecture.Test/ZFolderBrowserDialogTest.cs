using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;

namespace Enterprise.Winzor.Architecture.Test;
public class ZFolderBrowserDialogTest
{
	[Test]
	public async Task ZOpenFolderDialogCallsInvokesFolderOk()
	{
		using var ctx = new EnterpriseTestContext();

		Form form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());

		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock.Setup(i => i.ShowDirectoryPickerAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(() => Task.FromResult("LocalFolderName"));
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		var folderName = string.Empty;
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var dialog = new ZFolderBrowserDialog();
			dialog.ShowDialog();
			folderName = dialog.MappedSelectedPath;
		});

		rendered.WaitForAssertion(() => Assert.That(folderName, Is.EqualTo("LocalFolderName")));
	}
}
