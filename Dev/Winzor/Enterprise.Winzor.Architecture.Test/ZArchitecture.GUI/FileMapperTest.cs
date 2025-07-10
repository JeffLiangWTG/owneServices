using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

public class FileMapperTest
{
	static IEnumerable TestPaths()
	{
		yield return new TestCaseData(Path.Combine(FileService.UploadRoot, "a"), true) { TestName = "{m}0" };
		yield return new TestCaseData(Path.Combine(FileService.UploadRoot, "a/"), true) { TestName = "{m}1" };
		yield return new TestCaseData(Path.Combine(FileService.UploadRoot, "a/b"), true) { TestName = "{m}2" };
		yield return new TestCaseData(Path.Combine(FileService.UploadRoot, "./a/./b"), true) { TestName = "{m}3" };
		yield return new TestCaseData(Path.Combine(FileService.UploadRoot, "./a/../b"), true) { TestName = "{m}4" };
		yield return new TestCaseData(Path.Combine(FileService.UploadRoot, "../a"), false) { TestName = "{m}5" };
		yield return new TestCaseData(Path.Combine(FileService.UploadRoot, "../a/"), false) { TestName = "{m}6" };
		yield return new TestCaseData(Path.Combine(FileService.UploadRoot, "../a/b"), false) { TestName = "{m}7" };
		yield return new TestCaseData("c:/a", false) { TestName = "{m}8" };
		yield return new TestCaseData("/a", false) { TestName = "{m}9" };
		yield return new TestCaseData(Path.Combine(Path.GetDirectoryName(FileService.UploadRoot), "a"), false) { TestName = "{m}10" };
		yield return new TestCaseData("a", false) { TestName = "{m}11" };
		yield return new TestCaseData("a/", false) { TestName = "{m}12" };
		yield return new TestCaseData("a/b", false) { TestName = "{m}13" };
	}

	[TestCaseSource(nameof(TestPaths))]
	public void TestIsLocalFile(string path, bool expected)
	{
		Assert.That(FileMapper.IsLocalFile(path), Is.EqualTo(expected));
	}

	[Test]
	public async Task TestGetFolderPath()
	{
		using var ctx = new EnterpriseTestContext();
		ctx.MockCargoWiseClientServices.ClientFileApi
			.Setup(s => s.GetFolderPathAsync(System.Environment.SpecialFolder.MyDocuments))
			.ReturnsAsync("MyDocuments");
		using var rendered = await ctx.RenderFormAsync(() => new Form());

		var fileMapper = new FileMapper();
		var path = fileMapper.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		Assert.That(path, Is.EqualTo("MyDocuments"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestGetFolderPathCallsClientJS()
	{
		var clientSeviceProvider = new MockCargoWiseClientSeviceProvider() { MockClientFileApi = null };
		await using var ctx = new InMemoryAppServerTestContext(clientSeviceProvider);
		var page = await ctx.LoadFormAsync(() => new Form());
		await page.EvaluateAsync($"() => window.cargoWiseClient = {{'{JavascriptNames.GetFolderPathFunctionName}': () => 'MyDocuments'}};");

		var fileMapper = new FileMapper();
		var path = fileMapper.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		Assert.That(path, Is.EqualTo("MyDocuments"));
	}

	[Test]
	public async Task TestOpenRead()
	{
		using var ctx = new EnterpriseTestContext();
		var data = new byte[] { 1, 2, 3 };
		ctx.MockCargoWiseClientServices.ClientFileApi
			.Setup(s => s.ReadFileAsync(It.IsAny<string>()))
			.ReturnsAsync(new MemoryStream(data));
		using var rendered = await ctx.RenderFormAsync(() => new Form());

		var fileMapper = new FileMapper();
		var stream = fileMapper.OpenRead("path");
		Assert.That(stream.Length, Is.EqualTo(3));
		var buffer = new byte[3];
#pragma warning disable VSTHRD103 // Call async methods when in an async method
		stream.Read(buffer, 0, 3);
#pragma warning restore VSTHRD103 // Call async methods when in an async method
		Assert.That(buffer, Is.EqualTo(data));
	}

	[Test]
	public async Task TestOpenReadLocalFile()
	{
		using var ctx = new EnterpriseTestContext();
		using var rendered = await ctx.RenderFormAsync(() => new Form());

		var fileMapper = new FileMapper();
		var file = Path.Combine(FileService.UploadRoot, Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(FileService.UploadRoot);
			File.WriteAllText(file, "TestOpenReadLocalFile");
			var stream = fileMapper.OpenRead(file);
			using var reader = new StreamReader(stream);
			var content = await reader.ReadToEndAsync();
			Assert.That(content, Is.EqualTo("TestOpenReadLocalFile"));
		}
		finally
		{
			Directory.Delete(FileService.UploadRoot, true);
		}
		ctx.MockCargoWiseClientServices.ClientFileApi
			.Verify(s => s.ReadFileAsync(It.IsAny<string>()), Times.Never());
	}

	[Test, WithPlaywrightPage]
	public async Task TestOpenReadCallsClientJS()
	{
		var clientSeviceProvider = new MockCargoWiseClientSeviceProvider() { MockClientFileApi = null };
		await using var ctx = new InMemoryAppServerTestContext(clientSeviceProvider);
		var page = await ctx.LoadFormAsync(() => new Form());
		await page.EvaluateAsync($"() => window.cargoWiseClient = {{'{JavascriptNames.ReadFileFunctionName}': () => new Uint8Array([1, 2, 3])}};");

		var fileMapper = new FileMapper();
		var path = fileMapper.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		var stream = fileMapper.OpenRead("path");
		Assert.That(stream.Length, Is.EqualTo(3));
		var buffer = new byte[3];
#pragma warning disable VSTHRD103 // Call async methods when in an async method
		stream.Read(buffer, 0, 3);
#pragma warning restore VSTHRD103 // Call async methods when in an async method
		Assert.That(buffer, Is.EqualTo(new byte[] { 1, 2, 3 }));
	}

	[Test, WithPlaywrightPage]
	public async Task TestOpenReadThrowException()
	{
		var clientSeviceProvider = new MockCargoWiseClientSeviceProvider() { MockClientFileApi = null };
		await using var ctx = new InMemoryAppServerTestContext(clientSeviceProvider);
		var page = await ctx.LoadFormAsync(() => new Form());
		await page.EvaluateAsync($"() => window.cargoWiseClient = {{'{JavascriptNames.ReadFileFunctionName}': () => {{throw new Error('failed');}} }};");

		var fileMapper = new FileMapper();
		var path = fileMapper.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		var exception = Assert.Throws<IOException>(() => fileMapper.OpenRead("path"));
		Assert.That(exception.InnerException, Is.Not.Null);
		Assert.That(exception.InnerException.Message, Contains.Substring("failed"));
	}

	[Test]
	public async Task TestOpenWrite()
	{
		using var ctx = new EnterpriseTestContext();
		using var rendered = await ctx.RenderFormAsync(() => new Form());
		var fileMapper = new FileMapper();
		var data = new byte[] { 1, 2, 3 };
		using (var stream = fileMapper.OpenWrite("path"))
		{
			await stream.WriteAsync(data);
		}

		ctx.MockCargoWiseClientServices.ClientFileApi.Verify(s => s.WriteFileAsync("path", data));
	}

	[Test, WithPlaywrightPage]
	public async Task TestOpenWriteCallsClientJS()
	{
		var clientSeviceProvider = new MockCargoWiseClientSeviceProvider() { MockClientFileApi = null };
		await using var ctx = new InMemoryAppServerTestContext(clientSeviceProvider);
		var page = await ctx.LoadFormAsync(() => new Form());
		await page.EvaluateAsync($"() => window.cargoWiseClient = {{'{JavascriptNames.WriteFileFunctionName}': (path, data) => {{window.writeFilePath = path; window.writeFileData = data;}} }};");

		var fileMapper = new FileMapper();
		var data = new byte[] { 1, 2, 3 };
		using (var stream = fileMapper.OpenWrite("path"))
		{
			await stream.WriteAsync(data);
		}

		var path = await page.EvaluateAsync<string>("() => window.writeFilePath");
		Assert.That(path, Is.EqualTo("path"));

		var writeFileData = await page.EvaluateAsync<byte[]>("() => window.writeFileData");
		Assert.That(writeFileData, Is.EqualTo(data));
	}

	[Test, WithPlaywrightPage]
	public async Task TestOpenWriteThrowException()
	{
		var clientSeviceProvider = new MockCargoWiseClientSeviceProvider() { MockClientFileApi = null };
		await using var ctx = new InMemoryAppServerTestContext(clientSeviceProvider);
		var page = await ctx.LoadFormAsync(() => new Form());
		await page.EvaluateAsync($"() => window.cargoWiseClient = {{'{JavascriptNames.WriteFileFunctionName}': () => {{throw new Error('failed');}} }};");

		var fileMapper = new FileMapper();
		var data = new byte[] { 1, 2, 3 };
		try
		{
			using (var stream = fileMapper.OpenWrite("path"))
			{
				await stream.WriteAsync(data);
			}
		}
		catch (IOException ex)
		{
			Assert.That(ex.InnerException, Is.Not.Null);
			Assert.That(ex.InnerException.Message, Contains.Substring("failed"));
			return;
		}

		Assert.Fail("Expected IOException was not thrown");
	}
}
