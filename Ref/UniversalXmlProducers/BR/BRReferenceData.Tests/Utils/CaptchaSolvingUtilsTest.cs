using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using WTG.LS.CaptchaSolver.CaptchaSolvingProviders.TwoCaptcha;
using WTG.LS.CaptchaSolver.Exceptions;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	sealed class CaptchaSolvingUtilsTest
	{
		[Test]
		public void TestGetLoginRespone()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_LOGIN"]).Respond("application/html", LoginGetStream());
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_LOGIN"]).Respond("application/html", LoginPostStream());

				mockHttp.When(HttpMethod.Post, "https://2captcha.com/in.php?").Respond(req => HtmlResponseWithKey());
				mockHttp.When(HttpMethod.Get, "https://2captcha.com/res.php?key=b74c65642682527f64ce535bda23f436&action=get&id=128500&json=1").Respond(req => HtmlResponseWithCaptcha());

				var mokcHttpFactory = new Mock<IHttpClientFactory>();
				using (var client = mockHttp.ToHttpClient())
				{
					mokcHttpFactory.Setup(x => x.CreateClient(string.Empty)).Returns(() => mockHttp.ToHttpClient());
					CaptchaSolvingUtils.Instance.SetDefaultHttpClientFactory(mokcHttpFactory.Object);
					Assert.IsTrue(CaptchaSolvingUtils.Instance.GetLoginResponse(client).IsSuccessStatusCode);
				}
			}
		}

		[TestCase(false)]
		[TestCase(true)]
		public void TestGetLoginRespone_OnFailure(bool validateException)
		{
			HttpResponseMessage ResponseMessageLoginGet() => new()
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(LoginGetStream()),
			};
			HttpResponseMessage ResponseMessageLogin() => new()
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Content = new StreamContent(LoginPostStream()),
			};

			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_LOGIN"]).Respond(req => ResponseMessageLoginGet());
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_LOGIN"]).Respond(req => ResponseMessageLogin());
				mockHttp.When(HttpMethod.Post, "https://2captcha.com/in.php?").Respond(req => HtmlResponseWithKey());
				mockHttp.When(HttpMethod.Get, "https://2captcha.com/res.php?key=b74c65642682527f64ce535bda23f436&action=get&id=128500&json=1").Respond(req => validateException? HtmlResponseWithCaptchaAggregateException() : HtmlResponseWithCaptcha());

				var mokcHttpFactory = new Mock<IHttpClientFactory>();
				using (var client = mockHttp.ToHttpClient())
				{
					mokcHttpFactory.Setup(x => x.CreateClient(string.Empty)).Returns(() => mockHttp.ToHttpClient());
					CaptchaSolvingUtils.Instance.SetDefaultHttpClientFactory(mokcHttpFactory.Object);
					CaptchaSolvingUtils.Instance.SetDefaultRetryLimit(1);
					Assert.AreEqual(HttpStatusCode.InternalServerError, CaptchaSolvingUtils.Instance.GetLoginResponse(client).StatusCode);
				}
			}
		}

		[Test]
		public void TestGetLoginRespone_Exception()
		{
			HttpResponseMessage ResponseMessageLoginGet() => new()
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StreamContent(LoginGetStream()),
			};
			HttpResponseMessage ResponseMessageLogin() => new()
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Content = new StreamContent(LoginPostStream()),
			};

			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_LOGIN"]).Respond(req => ResponseMessageLoginGet());
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_LOGIN"]).Respond(req => ResponseMessageLogin());
				mockHttp.When(HttpMethod.Post, "https://2captcha.com/in.php?").Respond(req => HtmlResponseWithKey());
				mockHttp.When(HttpMethod.Get, "https://2captcha.com/res.php?key=b74c65642682527f64ce535bda23f436&action=get&id=128500&json=1").Respond(req => HtmlResponseWithCaptchaException());

				var mokcHttpFactory = new Mock<IHttpClientFactory>();
				using (var client = mockHttp.ToHttpClient())
				{
					mokcHttpFactory.Setup(x => x.CreateClient(string.Empty)).Returns(() => mockHttp.ToHttpClient());
					CaptchaSolvingUtils.Instance.SetDefaultHttpClientFactory(mokcHttpFactory.Object);
					CaptchaSolvingUtils.Instance.SetDefaultRetryLimit(1);
					var exception = Assert.Throws<AggregateException>(() => CaptchaSolvingUtils.Instance.GetLoginResponse(client));
					Assert.True(exception.Message.Contains("Test error"));
				}
			}
		}

		Stream LoginGetStream() => Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Utils.TestFiles.LOGIN_GET.html");
		Stream LoginPostStream() => Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Utils.TestFiles.LOGIN_POST.html");
		HttpResponseMessage HtmlResponseWithKey() => new()
		{
			StatusCode = HttpStatusCode.OK,
			Content = new StringContent("OK|128500"),
		};
		TwoCaptchaApiResponse ResponseWithCaptcha() => new()
		{
			Request = "663oit",
			Status = 1,
		};
		HttpResponseMessage HtmlResponseWithCaptcha() => new()
		{
			StatusCode = HttpStatusCode.OK,
			Content = new StringContent(JsonConvert.SerializeObject(ResponseWithCaptcha())),
		};

		HttpResponseMessage HtmlResponseWithCaptchaAggregateException() => throw new AggregateException(new CaptchaSolverException("Workers could not solve the Captcha"));

		HttpResponseMessage HtmlResponseWithCaptchaException() => throw new Exception("Test error");

		[SetUp]
		[TearDown]
		public void TearDown()
		{
			CaptchaSolvingUtils.Instance.SetDefaultHttpClientFactory(null);
			CaptchaSolvingUtils.Instance.SetDefaultRetryLimit(5);
			CaptchaSolvingUtils.Instance.SetResultToGetLoginResponse(null);
		}
	}
}
