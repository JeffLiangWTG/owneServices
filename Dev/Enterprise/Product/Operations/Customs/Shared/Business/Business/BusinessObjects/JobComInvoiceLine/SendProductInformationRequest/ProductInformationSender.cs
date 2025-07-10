using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.Business
{
	public class ProductInformationSender
	{
		public ProductInformationSender(ProductInformation information)
		{
			ProductInformation = Argument.NotNull(information, nameof(information));
		}

		ProductInformation ProductInformation { get; }
		BusinessObjectFactory Factory => ProductInformation.Factory;

		public SendResult TrySend()
		{
			var (errorMessage, contexts) = ProductInformation.GetDeliveryContext();
			if (!string.IsNullOrEmpty(errorMessage))
			{
				return new SendResult.Failure(errorMessage);
			}

			var  postUrl = GetUrl();
			if (string.IsNullOrEmpty(postUrl))
			{
				return new SendResult.Failure(Res.GetString("Enterprise.Customs.GUI.SendProductInformationRequestForm|CannotfindPostUrl", "The system does not have a post URL, please contact your administrator."));
			}

			var audienceID = GetAudienceID();
			if (string.IsNullOrEmpty(audienceID))
			{
				return new SendResult.Failure(Res.GetString("Enterprise.Customs.GUI.SendProductInformationRequestForm|CannotfindAudienceId", "The system does not have an Audience ID, please contact your administrator."));
			}

			string token;
			try
			{
				token = GetTokenService(audienceID).GetAccessToken();
			}
			catch (TokenAuthOnboardingApiException ex)
			{
				return new SendResult.Failure(ex.Message + "\r\n" + Res.GetString("Enterprise.Customs.GUI.SendProductInformationRequestForm|InvalidCertificate", "Please update the certificate via service task 'TCM'. If this issue persists, please contact your administrator."));
			}

			var failedSendMessages = new List<string>();
			var httpClient = GetPostWebClient(postUrl, token);

			foreach (var context in contexts)
			{
				var jsonString = JsonSerializer.Serialize(context, new JsonSerializerOptions { IncludeFields = true });
				HttpContent httpContent = new StringContent(jsonString, Encoding.UTF8, mediaType);
				var response = httpClient
					.PostAsync(postUrl, httpContent)
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();

				if (!response.IsSuccessStatusCode)
				{
					//var responseContent = response.Content?.ReadAsStringAsync().Result; // for debug purposes.  Might want to log this somewhere.
					failedSendMessages.Add($"[{context.context.supplier}]: {response.ReasonPhrase} ({response.StatusCode})");
				}
			}

			if (failedSendMessages.Count > 0)
			{
				return new SendResult.Failure(Res.GetString("Enterprise.Customs.GUI.SendProductInformationRequestForm|MessageSendError", "Below message(s) send failed:{0}", System.Environment.NewLine + string.Join(System.Environment.NewLine, failedSendMessages)));
			}

			return new SendResult.Success();
		}

		protected virtual HttpClient GetPostWebClient(string baseAddress, string token)
		{
			var defaultTimeOutInSeconds = TimeSpan.FromSeconds(5);
			var res = SystemDataRegistry.Instance.IdentityProviderTimeoutInSeconds.Value;
			if (res > 0 && res < int.MaxValue)
			{
				defaultTimeOutInSeconds = TimeSpan.FromSeconds(res);
			}

			var httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri(baseAddress);
			httpClient.Timeout = defaultTimeOutInSeconds;
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Bearer, token);
			return httpClient;
		}

		protected internal string GetUrl()
		{
			var configCode = IsTestSystem ? RefSysConfigKey_HSAsstUrlTest : RefSysConfigKey_HSAsstUrlProd;
			return RefSysConfigLoader.GetStringValue(configCode);
		}

		protected internal string GetAudienceID()
		{
			var configCode = IsTestSystem ? RefSysConfigKey_HSAsstAudTest : RefSysConfigKey_HSAsstAudProd;
			return RefSysConfigLoader.GetStringValue(configCode);
		}

		protected virtual ClassificationAssistantTokenService GetTokenService(string aud)
		{
			return new ClassificationAssistantTokenService(aud);
		}

		bool IsTestSystem => eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.Value;

		RefSysConfig.Loader RefSysConfigLoader => refSysConfigLoader ??= new RefSysConfig.Loader(Factory);
		RefSysConfig.Loader refSysConfigLoader;

#pragma warning disable CW1161 // Res.GetString Analyzer
		const string Bearer = "Bearer";
#pragma warning restore CW1161 // Res.GetString Analyzer
		const string mediaType = "application/json";
		public const string RefSysConfigKey_HSAsstUrlTest = "HSAsstUrlT";
		public const string RefSysConfigKey_HSAsstUrlProd = "HSAsstUrlP";
		public const string RefSysConfigKey_HSAsstAudTest = "HSAsstAudT";
		public const string RefSysConfigKey_HSAsstAudProd = "HSAsstAudP";
	}

	public abstract class SendResult
	{
		public sealed class Success : SendResult;
		public sealed class Failure(string errorMessage) : SendResult
		{
			public string ErrorMessage { get; } = errorMessage;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "clarity")]
	public class ClassificationAssistantTokenService : SystemToSystemTrustService
	{
		// TODO: Replace SystemToSystemTrustService base class with a function (when ITokenServicesFactory is available)

		//public string GetAccessToken()
		//{
		//	var tokenServicesFactory = ObjectFactory.Get<ITokenServicesFactory>();
		//	var tokenService = tokenServicesFactory.GetTokenService();
		//	var audience = GetAudienceCode();
		//	var token = await tokenService.GenerateCw1TokenAsync(audience, cancellationToken);
		//	return token;
		//}

		public ClassificationAssistantTokenService(string aud)
		{
			this.aud = aud;
		}
		readonly internal string aud;

		protected override string GetAud() => aud;
	}
}
