using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;

namespace Enterprise.Freight.Business
{
	public sealed class VesselMovementsUrlGenerator : IVesselMovementsUrlGenerator
	{
		const int UrlLifetimeInMinutes = 10;
		readonly IAuthTokenProvider tokenProvider = ObjectFactory.Get<IAuthTokenProvider>();

		public async Task<VesselMovementsAuthTokenResult> GetTokenAsync(string correlationId, OrgContact contact = null, CancellationToken ct = default)
		{
			if (FreightDataRegistry.Instance.EnableRouteVisualizerMyAccountLogin.Value)
			{
				if (!Uri.TryCreate(FreightDataRegistry.Instance.RouteVisualizerUrl.Value, UriKind.Absolute, out var baseUrl))
				{
					return new VesselMovementsAuthTokenResult { ErrorMessage = Res.GetString("8ea5166c-6681-41a2-b16d-7f02d79e99e2", "Invalid registry item: Freight > Global Tracking > Route Visualizer > Route Visualizer URL.") };
				}

				var userPortalClient = ObjectFactory.Get<IUserPortalClient>();
				var result = await userPortalClient.OAuthAutoLoginAsync(baseUrl);
				if (!result.Success)
				{
					var message = string.Join(",", result.Messages.Select(m => m.Code + ": " + m.Message));
					return new VesselMovementsAuthTokenResult { ErrorMessage = message };
				}

				// MyAccount needs redirection to portal when something requires user attention (eg. pending user email verification)
				if (result.Response.RedirectUrl != null && !result.Response.RedirectUrl.ToString().Contains(baseUrl.AbsoluteUri))
				{
					return new VesselMovementsAuthTokenResult { RedirectUrl = result.Response.RedirectUrl };
				}

				var token = new VesselMovementsAuthToken(VesselMovementsAuthTokenType.MyAccount, result.Response.Token);

				return new VesselMovementsAuthTokenResult { Token = token };
			}
			else
			{
				var (authToken, tokenValidationMessage) = tokenProvider.GetToken(correlationId, UrlLifetimeInMinutes * 60, OverrideLoginInfoWithContactDetails, ct, true);
				if (!string.IsNullOrEmpty(tokenValidationMessage))
				{
					return new VesselMovementsAuthTokenResult { ErrorMessage = Res.GetString("42adf837-ffd7-4f4c-acf5-d328f2459bcf", "Unable to get permission to show map: {0}", tokenValidationMessage) };
				}

				if (string.IsNullOrEmpty(authToken))
				{
					throw new InvalidOperationException($"{nameof(authToken)} must not be null or empty");
				}

				var token = new VesselMovementsAuthToken(VesselMovementsAuthTokenType.Rating, authToken);

				return new VesselMovementsAuthTokenResult { Token = token };
			}

			void OverrideLoginInfoWithContactDetails(LoginInfo loginInfo)
			{
				if (contact != null)
				{
					loginInfo.UserFullName = contact.OC_ContactName;
					loginInfo.UserEmail = contact.OC_Email;
					loginInfo.ClientCompanyCode = contact.ParentOrg.OH_Code;
					loginInfo.ClientCompanyName = contact.ParentOrg.OH_FullName;
				}
			}
		}

		public (Uri url, string errorMessage) Generate(VesselMovementsAuthToken token, IVesselMovementsUrlSupporter supporter, VesselMovementsUrlGeneratorOptions options)
		{
			_ = supporter ?? throw new ArgumentNullException(nameof(supporter));

			if (!Uri.TryCreate(FreightDataRegistry.Instance.RouteVisualizerUrl.Value, UriKind.Absolute, out var baseUrl))
			{
				return (null, Res.GetString("8ea5166c-6681-41a2-b16d-7f02d79e99e2", "Invalid registry item: Freight > Global Tracking > Route Visualizer > Route Visualizer URL."));
			}

			var (model, errorMessage) = GetUrlModelAndValidate(supporter);
			if (model == null)
			{
				return (null, errorMessage);
			}

			var url = BuildRouteVisualizerUrl(baseUrl, token, model, options);

			return (url, null);
		}

		static Uri BuildRouteVisualizerUrl(Uri baseUrl, VesselMovementsAuthToken token, VesselMovementsUrlModel model, VesselMovementsUrlGeneratorOptions options)
		{
			var result = new UriBuilder(baseUrl);
			result.Path = "/movements/" + model.LloydsNumber;
			result.Query = GetQuery(token, model, options).ToString();

			return result.Uri;
		}

		static (VesselMovementsUrlModel model, string errorMessage) GetUrlModelAndValidate(IVesselMovementsUrlSupporter supporter)
		{
			var (model, errorMessage) = supporter.GetVesselMovementsUrlModel();
			if (model == null)
			{
				return (null, errorMessage);
			}

			if (string.IsNullOrEmpty(model.LloydsNumber) || !IMOIsValid(model.LloydsNumber))
			{
				return (null, Res.GetString("8f571946-88ae-4ea9-b3c3-e93866af6656", "Routing leg must have a vessel with valid IMO Number."));
			}

			if ((!model.DepartureTime.IsValid && !model.ArrivalTime.IsValid) || (model.ArrivalTime < model.DepartureTime))
			{
				return (null, Res.GetString("7f8a1f29-4ad8-479b-838b-486c4e846a3d", "Routing leg must have a valid departure or arrival time."));
			}

			return (model, null);
		}

		static bool IMOIsValid(string lloydsNumber)
		{
			var imoValidator = new LloydsNumberValidation();
			imoValidator.Validate(lloydsNumber);

			return imoValidator.IsValid;
		}

		static QueryString GetQuery(VesselMovementsAuthToken token, VesselMovementsUrlModel model, VesselMovementsUrlGeneratorOptions options)
		{
			var query = new QueryString();

			switch (token.Type)
			{
				case VesselMovementsAuthTokenType.Rating:
					query["token"] = token.Value;
					break;
				case VesselMovementsAuthTokenType.MyAccount:
					query["myaccount_token"] = token.Value;
					break;
				default:
					throw new InvalidOperationException($"Unexpected VesselMovementsAuthToken type {token.Type}");
			}

			if (model.DepartureTime.IsValid)
			{
				query["departureTime"] = model.DepartureTime.ToString(GetUrlDateFormat(model.DepartureTime), CultureInfo.InvariantCulture);
			}

			if (model.ArrivalTime.IsValid)
			{
				query["arrivalTime"] = model.ArrivalTime.ToString(GetUrlDateFormat(model.ArrivalTime), CultureInfo.InvariantCulture);
			}

			if (!string.IsNullOrEmpty(model.CarrierCode) && !string.IsNullOrEmpty(model.VoyageNumber))
			{
				query["carrierCode"] = model.CarrierCode;
				query["voyageNumber"] = model.VoyageNumber;
			}

			if (!string.IsNullOrEmpty(model.DeparturePortUnloco))
			{
				query["departurePortUnloco"] = model.DeparturePortUnloco;
			}

			if (!string.IsNullOrEmpty(model.ArrivalPortUnloco))
			{
				query["arrivalPortUnloco"] = model.ArrivalPortUnloco;
			}

			if ((options & VesselMovementsUrlGeneratorOptions.CollapseInfoPanel) != 0)
			{
				query["collapseInfoPanel"] = true.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
			}

			if ((options & VesselMovementsUrlGeneratorOptions.CollapsePortCallsPanel) != 0)
			{
				query["collapsePortCallsPanel"] = true.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
			}

			return query;
		}

		static string GetUrlDateFormat(ZDateTime dt)
		{
			if (dt.TimeOfDay == TimeSpan.Zero)
			{
				return "yyyyMMdd";
			}

			if (dt.Second == 0)
			{
				return "yyyyMMddTHHmm";
			}

			return "yyyyMMddTHHmmss";
		}
	}
}
