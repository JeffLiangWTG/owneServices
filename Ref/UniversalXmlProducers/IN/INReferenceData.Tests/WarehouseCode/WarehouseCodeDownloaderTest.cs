using System;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests;

[TestFixture]
public class WarehouseCodeDownloaderTest
{
	[Test]
	public void TestDownloadData()
	{
		using var goodResponse = new HttpResponseMessage(HttpStatusCode.OK);
		goodResponse.Content = new StringContent("FOO");
		var mockHttp = new Mock<IHttpClient>();
		mockHttp.Setup(x => x.Get(new Uri($"{AppConfig.WarehouseCode.CodeUrl}?captchaValue=FOO"))).Returns(goodResponse);
		var downloader = new WarehouseCodeDownloaderForTest { MockClient = mockHttp.Object };

		Assert.AreEqual("FOO", downloader.DownloadData());
	}

	[Test]
	public void TestDownloadData_RequestFail()
	{
		using var failResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
		var mockHttp = new Mock<IHttpClient>();
		mockHttp.Setup(x => x.Get(new Uri(AppConfig.WarehouseCode.CodeUrl))).Returns(failResponse);
		var downloader = new WarehouseCodeDownloaderForTest { MockClient = mockHttp.Object };

		Assert.Throws<UnhandledApplicationException>(() => downloader.DownloadData(), $"Failed to load warehouse codes from the source after {AppConfig.WarehouseCode.MaxRetry} retries");
	}
	
	[Test]
	public void TestDownloadData_WrongCaptcha()
	{
		using var wrongCaptchaResponse = new HttpResponseMessage(HttpStatusCode.OK);
		wrongCaptchaResponse.Content = new StringContent("Invalid Captcha!");
		var mockHttp = new Mock<IHttpClient>();
		mockHttp.Setup(x => x.Get(new Uri(AppConfig.WarehouseCode.CaptchaUrl))).Returns(wrongCaptchaResponse);

		var downloader = new WarehouseCodeDownloaderForTest { MockClient = mockHttp.Object };
		Assert.Throws<UnhandledApplicationException>(() => downloader.DownloadData(), $"Failed to load warehouse codes from the source after {AppConfig.WarehouseCode.MaxRetry} retries");
	}

	class WarehouseCodeDownloaderForTest : WarehouseCodeDownloader
	{
		protected override string HandleCaptcha(IHttpClient client) => "FOO";

		public IHttpClient MockClient { get; init; }

		protected override IHttpClient GetHttpClient() => MockClient;

		protected override int SleepInterval => 1;
	}
}
