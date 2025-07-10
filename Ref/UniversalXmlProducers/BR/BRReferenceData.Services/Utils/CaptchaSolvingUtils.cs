using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using WTG.LS.CaptchaSolver.CaptchaSolvingProviders.TwoCaptcha;
using WTG.LS.CaptchaSolver.CaptchaSolvingProviders.TwoCaptcha.CaptchaTypes;
using WTG.LS.CaptchaSolver.CaptchaSolvingProviders.TwoCaptcha.Model;
using WTG.LS.CaptchaSolver.Exceptions;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class CaptchaSolvingUtils
	{
		static readonly CaptchaSolvingUtils captchaSolvingUtils = new CaptchaSolvingUtils();

		CaptchaSolvingUtils() { }

		public static CaptchaSolvingUtils Instance => captchaSolvingUtils;

		public void SetResultToGetLoginResponse(Func<HttpResponseMessage> value)
		{
			resultToGetLoginResponse = value;
		}

		Func<HttpResponseMessage> resultToGetLoginResponse;

		public void SetDefaultHttpClientFactory(IHttpClientFactory defaultHttpClientFactory)
		{
			fDefaultHttpClientFactory = defaultHttpClientFactory;
		}
		IHttpClientFactory fDefaultHttpClientFactory;

		IHttpClientFactory DefaultHttpClientFactory => fDefaultHttpClientFactory ?? new DefaultHttpClientFactory();

		public void SetDefaultRetryLimit(int DefaultRetryLimit)
		{
			fDefaultRetryLimit = DefaultRetryLimit;
		}
		int fDefaultRetryLimit = 5;

		int DefaultRetryLimit => fDefaultRetryLimit;

		async Task<CaptchaResponse> HCaptchaAsync()
		{
			var siteKey = ConfigurationProvider.Configuration.GetSection("URL_TABELAS_ADUANEIRAS_SITE_KEY").Value;
			var twoCaptcha = new TwoCaptchaHCaptcha(siteKey, LoginPage);
			using (var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug)))
			{
				var twoCaptchaProvider = new TwoCaptchaProvider(DefaultHttpClientFactory, loggerFactory.CreateLogger("BR"), string.Empty, ConfigurationProvider.Configuration.GetSection("CAPTCHA_SOLVER_API_KEY").Value);
				return await twoCaptchaProvider.SolveCaptchaAsync(twoCaptcha);
			}
		}

		public HttpResponseMessage GetLoginResponse(HttpClient client)
		{
			var loginResponse = resultToGetLoginResponse?.Invoke();
			if (loginResponse != null)
			{
				return loginResponse;
			}

			SetupDefaultHeaderRequest(client);
			var loginTries = 0;
			bool ShouldKeepTrying(Exception ex)
			{
				if ((ex is AggregateException && ex.Data[0] is CaptchaSolverException captchaSolverException) || (ex.Message?.Contains("Workers could not solve the Captcha") ?? false))
				{
					loginTries++;
					return true;
				}

				return false;
			}

			while (loginTries <= DefaultRetryLimit)
			{
				using (var httpResponse = client.GetAsyncEx(LoginPage))
				{
					var loginpage = httpResponse?.Result;
					Contract.Assume(loginpage != null);

					try
					{
						var captchaResult = HCaptchaAsync()?.Result;

						Contract.Assume(!string.IsNullOrEmpty(captchaResult.CaptchaText));

						loginResponse = DoPostLogin(client, loginpage, captchaResult.CaptchaText);

						if (!loginResponse?.IsSuccessStatusCode ?? false && loginTries < DefaultRetryLimit)
						{
							loginTries++;
							continue;
						}
					}
					catch (Exception ex)
					{
						if (ShouldKeepTrying(ex))
						{
							continue;
						}
						else
						{
							throw;
						}
					}

					Contract.Assume(loginResponse.IsSuccessStatusCode, nameof(loginResponse.IsSuccessStatusCode));

					return loginResponse;
				}
			}

			return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
		}

		static HttpResponseMessage DoPostLogin(HttpClient client, HttpResponseMessage response, string captchaResult)
		{
			SetupDefaultCookies(client, response.Headers);

			var p = new Dictionary<string, string>
			{
				{ "j_id11", "j_id11" },
				{ "uniqueToken", "" },
				{ "g-recaptcha-response", captchaResult },
				{ "h-captcha-response", captchaResult },
				{ "j_id11:j_id16", "Entrar" },
				{ "javax.faces.ViewState", GetViewId(response.Content.ReadAsStringAsync()?.Result) }
			};
			using var postBody = new FormUrlEncodedContent(p);
			var loginResponse = client.PostSyncEx(postBody, LoginPage);
			Contract.Assume(loginResponse != null);

			return loginResponse;
		}

		static string SetupDefaultCookies(HttpClient client, HttpResponseHeaders headers)
		{
			foreach (var header in headers)
			{
				if (header.Key.Equals("Set-Cookie", StringComparison.Ordinal))
				{
					var sessionId = ((string[])header.Value)[0].Split(';')[0];
					client.DefaultRequestHeaders.Add("Cookie", sessionId);
					return sessionId;
				}
			}
			return string.Empty;
		}

		static void SetupDefaultHeaderRequest(HttpClient client)
		{
			if (client.DefaultRequestHeaders.Contains("Host"))
			{
				return;
			}
			client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
			client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br, zstd");
			client.DefaultRequestHeaders.Add("Accept-Language", "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7");
			client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
			client.DefaultRequestHeaders.Add("Connection", "keep-alive");
			client.DefaultRequestHeaders.Add("Host", "www35.receita.fazenda.gov.br");
			client.DefaultRequestHeaders.Add("Origin", "https://www35.receita.fazenda.gov.br");
			client.DefaultRequestHeaders.Add("Referer", "https://www35.receita.fazenda.gov.br/tabaduaneiras-web/public/pages/security/login_publico.jsf");
			client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", " document");
			client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", " navigate");
			client.DefaultRequestHeaders.Add("Sec-Fetch-Site", " same-origin");
			client.DefaultRequestHeaders.Add("Sec-Fetch-User", " ?1");
			client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36");
			client.DefaultRequestHeaders.Add("sec-ch-ua", "Not A(Brand\";v=\"8\", \"Chromium\";v=\"132\", \"Google Chrome\";v=\"132\"");
			client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
			client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "Windows");
		}

		public static string GetViewId(string html)
		{
			var page = new HtmlDocument();
			page.LoadHtml(html);
			return page.GetElementbyId("javax.faces.ViewState")?.GetAttributeValue("value", string.Empty);
		}

		protected static string LoginPage => ConfigurationProvider.Configuration.GetSection("URL_TABELAS_ADUANEIRAS_LOGIN").Value;
	}
}
