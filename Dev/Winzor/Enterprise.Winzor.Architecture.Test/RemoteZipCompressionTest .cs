using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

sealed class RemoteZipCompressionTest
{
	[Test]
	public async Task TestZipFiles()
	{
		using var ctx = new EnterpriseTestContext();

		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		fileServiceMock.Setup(f => f.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));

		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			using (var tempDir = new TempDirectory())
			{
				string file1 = Path.Combine(tempDir, "file1.txt");
				File.WriteAllText(file1, new string('x', 10000));
				var unmappedPath = Path.Combine(EnvProxy.Instance.TempPath, "file.zip");

				Assert.That(RemoteZipCompression.Zip(tempDir, unmappedPath), Is.EqualTo(true));

				fileServiceMock.Verify(f => f.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
				writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Once());

				TempFile.TryDelete(unmappedPath, out _);
			}
		});
	}

	[Test]
	public async Task TestZipFilesWhenExist()
	{
		using var ctx = new EnterpriseTestContext();

		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		fileServiceMock.Setup(f => f.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));

		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			using (var tempDir = new TempDirectory())
			using (var tempDir2 = new TempDirectory())
			{
				// Not throw exception when file already exist
				var unmappedPath = Path.Combine(EnvProxy.Instance.TempPath, "file.zip");

				File.WriteAllText(unmappedPath, new string('x', 10000));

				string fileForTest = Path.Combine(tempDir, "fileForTest.txt");
				File.WriteAllText(fileForTest, new string('x', 10000));

				Assert.That(File.Exists(unmappedPath), Is.EqualTo(true));
				Assert.That(RemoteZipCompression.Zip(tempDir2, unmappedPath), Is.EqualTo(true));

				fileServiceMock.Verify(f => f.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>()), Times.Once());
				writableBrowserFileMock.Verify(f => f.WriteAsync(It.IsAny<Stream>()), Times.Once());

				TempFile.TryDelete(unmappedPath, out _);
			}
		});
	}

	[Test]
	public async Task TestZipInvalidPath()
	{
		using var ctx = new EnterpriseTestContext();

		var fileServiceMock = new Mock<IFileService>();
		var writableBrowserFileMock = new Mock<IWritableBrowserFile>();
		fileServiceMock.Setup(f => f.SaveFileToDirectoryAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Dictionary<string, string[]>>>())).Returns(() => Task.FromResult(writableBrowserFileMock.Object));

		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: ctx.JSInterop.JSRuntime, fileService: fileServiceMock.Object);

		Form form = null;
		await ctx.RenderFormAsync(() => form = new Form(), cargowiseClientServices);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			using (var tempDir = new TempDirectory())
			{
				string file1 = Path.Combine(tempDir, "file1.txt");
				File.WriteAllText(file1, new string('x', 10000));
				var unmappedPath = @"~!@#$%^&*()_+-={}[]/|?\><,.:;'//file.zip";

				// Zip process should fail for invalid path
				Assert.That(RemoteZipCompression.Zip(tempDir, unmappedPath), Is.EqualTo(false));
			}
		});
	}
}
