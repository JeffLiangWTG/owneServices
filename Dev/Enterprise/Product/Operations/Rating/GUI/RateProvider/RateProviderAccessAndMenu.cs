using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WiseRates.Api.Client;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI
{
	public class RateProviderAccessAndMenu
	{
		public RateProviderAccessAndMenu(ZMenuItem parentMenu)
			: this(parentMenu, null)
		{
		}

		public RateProviderAccessAndMenu(ZMenuItem parentMenu, RatingHeader header)
		{
			ratingHeader = header;
			CreateManageRatesServiceMenuItems(parentMenu);
		}

		bool CanAccessRatesService(BooleanRegistryItem registryItem, string subscriptionName, string transportMode, out string reason)
		{
			if (!DataRegistryRating.Instance.IsLicenseForRatesServiceValid(out reason))
			{
				return false;
			}

			if (!DataRegistryRating.Instance.IsRateServiceSubscriptionEnabled(transportMode, out reason))
			{
				return false;
			}

			if (!registryItem.Value)
			{
				reason = ResString.GetMultilingualString("471537c4-4848-477c-8b5b-3cf9736b9098", @"{0} integration is disabled in the registry:
{1}", subscriptionName, registryItem.Location());

				return false;
			}

			reason = string.Empty;
			return true;
		}

		void CreateManageRatesServiceMenuItems(ZMenuItem parentMenu)
		{
			if (parentMenu == null)
			{
				return;
			}

			var cargoguideRateSearchMenu = new ZMenuItem(ResString.GetMultilingualString("1696ac3d-c798-49d3-935d-8fdefef1ef3d", "Cargoguide - Air Rates"), CargoguideRateSearch_Click);

			var cargoSphereMenu = new ZMenuItem(ResString.GetMultilingualString("0a56b4a0-ec9b-4e07-bdf3-770eeb457c6c", "CargoSphere - Ocean Rates"));
			var cargoSphereRateSearchMenu = new ZMenuItem(ResString.GetMultilingualString("d4429e04-8d4c-4a7c-8682-c7b5ab0c88a8", "Rate Search"), CargoSphereRateSearch_Click);
			var cargoSphereContractManagementMenu = new ZMenuItem(ResString.GetMultilingualString("f87b5599-990f-4651-8463-2feefddb7f32", "SUDS"), CargoSphereContractManagement_Click);
			cargoSphereMenu.MenuItems.Add(cargoSphereRateSearchMenu);
			cargoSphereMenu.MenuItems.Add(cargoSphereContractManagementMenu);

			parentMenu.MenuItems.Add(cargoguideRateSearchMenu);
			parentMenu.MenuItems.Add(cargoSphereMenu);
		}

		static string CargoSphereSubscription => ResString.GetMultilingualString("abbfbd7f-3340-45ce-acfe-d86b4f8efee4", "CargoSphere ocean rates management");
		static string CargoguideSubscription => ResString.GetMultilingualString("a454f4e7-5676-4f0f-a377-d3ec1730a97d", "Cargoguide air rates management");

		void ShowAccessToRatesServiceMessage(string accessToRatesServiceMessage, string subscriptionName)
		{
			Globals.Message.Show(accessToRatesServiceMessage,
				subscriptionName,
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		void ShowNoEmailAddressOfCurrentUserMessage(string providerName)
		{
			Globals.Message.ShowInformation(ResString.GetMultilingualString("20365F75-6544-47BA-8933-1ACAF8A8D3A0", "Email address of current user in staff details is mandatory for accessing {0}", providerName));
		}

		public void CargoguideRateSearch()
		{
			if (!CanAccessRatesService(DataRegistryRating.Instance.CargoguideIntegrationEnabled, CargoguideSubscription, Core.Constants.TransportModes.Air, out string reason))
			{
				ShowAccessToRatesServiceMessage(reason, CargoguideSubscription);
				return;
			}

			var checkpoint = Env.Security.WiseRatesCargoguideRateSearch;

			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			var url = DataRegistryRating.Instance.CargoguideRateSearchUrl.Value;
			if (string.IsNullOrEmpty(url))
			{
				url = RatesServiceConfiguration?.Cargoguide?.SiteSearchUrl;
				if (string.IsNullOrEmpty(url))
				{
					Globals.Message.Show(
						Res.GetString("b6df9303-4d9b-42a9-850b-f34bcfdbd368", "Cargoguide Rate Search URL setting could not be found. Please raise an eRequest."),
						CargoguideSubscription,
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					return;
				}
			}

			if (string.IsNullOrEmpty(GlbStaff.CurrentUser.GS_EmailAddress))
			{
				ShowNoEmailAddressOfCurrentUserMessage((NoResString)"Cargoguide"); // Constant string
				return;
			}

			using (new ZWaitCursorChanger())
			{
				var openUrlResult = OpenUrlWithTokenInWebBrowser(url);

				if (!string.IsNullOrEmpty(openUrlResult))
				{
					Globals.Message.ShowInformation(Res.GetString("5a5d97b3-d433-46d2-8cbe-3108063804da", "Cannot access Cargoguide Rate Search page due to {0}", openUrlResult));
				}
			}
		}

		void CargoguideRateSearch_Click(object sender, EventArgs e)
		{
			CargoguideRateSearch();
		}

		public void CargoSphereRateSearch()
		{
			if (!CanAccessRatesService(DataRegistryRating.Instance.CargoSphereIntegrationEnabled, CargoSphereSubscription, Core.Constants.TransportModes.Sea, out string reason))
			{
				ShowAccessToRatesServiceMessage(reason, CargoSphereSubscription);
				return;
			}

			var checkpoint = Env.Security.WiseRatesCargoSphereRateSearch;

			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			var url = DataRegistryRating.Instance.CargoSphereRateSearchUrl.Value;
			if (string.IsNullOrEmpty(url))
			{
				url = RatesServiceConfiguration?.CargoSphere?.SiteSearchUrl;
				if (string.IsNullOrEmpty(url))
				{
					Globals.Message.Show(
						Res.GetString("26e62c34-d591-4d23-943b-6305b36e2f0a", "CargoSphere Rate Search URL setting could not be found. Please raise an eRequest."),
						CargoSphereSubscription,
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					return;
				}
			}

			if (string.IsNullOrEmpty(GlbStaff.CurrentUser.GS_EmailAddress))
			{
				ShowNoEmailAddressOfCurrentUserMessage("CargoSphere"); // Constant string
				return;
			}

			using (new ZWaitCursorChanger())
			{
				var openUrlResult = OpenUrlWithTokenInWebBrowser(url);

				if (!string.IsNullOrEmpty(openUrlResult))
				{
					Globals.Message.ShowInformation(Res.GetString("7e977de1-2504-4d5c-a6f4-942ac2d2f815", "Cannot access CargoSphere Rate Search page due to {0}", openUrlResult));
				}
			}
		}

		void CargoSphereRateSearch_Click(object sender, EventArgs e)
		{
			CargoSphereRateSearch();
		}

		public void CargoSphereContractManagement()
		{
			if (!CanAccessRatesService(DataRegistryRating.Instance.CargoSphereIntegrationEnabled, CargoSphereSubscription, Core.Constants.TransportModes.Sea, out string reason))
			{
				ShowAccessToRatesServiceMessage(reason, CargoSphereSubscription);
				return;
			}

			var checkpoint = Env.Security.WiseRatesCargoSphereContractManagement;

			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			var url = DataRegistryRating.Instance.CargoSphereSUDSUrl.Value;
			if (string.IsNullOrEmpty(url))
			{
				url = RatesServiceConfiguration?.CargoSphere?.SiteSudsUrl;
				if (string.IsNullOrEmpty(url))
				{
					Globals.Message.Show(
						Res.GetString("c8eddccb-5a87-4810-9d4b-dc3c9be032bd", "CargoSphere SUDS URL setting could not be found. Please raise an eRequest."),
						CargoSphereSubscription,
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					return;
				}
			}

			if (string.IsNullOrEmpty(GlbStaff.CurrentUser.GS_EmailAddress))
			{
				ShowNoEmailAddressOfCurrentUserMessage("CargoSphere"); // Constant string
				return;
			}

			using (new ZWaitCursorChanger())
			{
				NameValueCollection scacQuery = null;
				if (!ratingHeader?.Header?.SCACCode.IsEmpty ?? false)
				{
					scacQuery = new NameValueCollection();
					scacQuery[UrlQueryKey.scac] = ratingHeader.Header.SCACCode;
				}

				var openUrlResult = OpenUrlWithTokenInWebBrowser(url, scacQuery);

				if (!string.IsNullOrEmpty(openUrlResult))
				{
					Globals.Message.ShowInformation(Res.GetString("669333e2-b3f4-4a58-9f38-4828a824a10b", "Cannot access CargoSphere SUDS page due to {0}", openUrlResult));
				}
			}
		}

		void CargoSphereContractManagement_Click(object sender, EventArgs e)
		{
			CargoSphereContractManagement();
		}

		string OpenUrlWithTokenInWebBrowser(string url, NameValueCollection query = null)
		{
			var tuple = GetToken();

			if (string.IsNullOrEmpty(tuple.Token))
			{
				return tuple.ValidationMessage;
			}

			var urlBuilder = new UriBuilder(url) { Port = -1 };
			var collection = HttpUtility.ParseQueryString(urlBuilder.Query);
			collection[UrlQueryKey.token] = tuple.Token;

			if (query != null)
			{
				foreach (var key in query.Cast<string>().Where(key => !string.IsNullOrEmpty(query[key])))
				{
					collection[key] = query[key];
				}
			}

			urlBuilder.Query = collection.ToString();
			SafeStartProcess(urlBuilder.ToString());

			return string.Empty;
		}

		public static class UrlQueryKey
		{
			public const string token = nameof(token);
			public const string scac = nameof(scac);
		}

		void SafeStartProcess(string url)
		{
			try
			{
				WebUrlLauncher.Launch(url);
			}
			catch (Win32Exception ex)
			{
				ErrorReporter.ReportOnce("RateProviderAccessAndMenu|SafeStartProcess", "Failed to open web address for: " + url, ex);
				Globals.Message.ShowInformation(Res.GetString("9b084c2f-d214-4dda-9eaf-bca5829a6246", "The web address '{0}' could not be opened. Please try copy and pasting it into your web browser instead.", url));
			}
		}

		(string Token, string ValidationMessage) GetToken()
		{
			(string Token, string ValidationMessage) tuple = (null, "");

			try
			{
				var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DataRegistryRating.Instance.RatesServiceRateSearchRequestTimeout.Value));
				var requestID = WiseRatesClient.GenerateTraceID();

				authTokenProvider ??= new WTGAuthTokenProviderForRating();
				tuple = authTokenProvider.GetToken(requestID, 30, cancellationToken: cts.Token);
			}
			catch (HttpRequestException ex)
			{
				ErrorReporter.ReportOnce("RateProviderAccessAndMenu.GetToken", ex);
				tuple.ValidationMessage = Res.GetString("cfa51027-be91-4ba8-be9f-de6cb8e4c135", "connection failure");
			}

			return tuple;
		}

		protected IAuthTokenProvider authTokenProvider;
		readonly RatingHeader ratingHeader;

		protected virtual IWiseRatesClient CreateWiseRatesClient(string traceID)
		{
			var clientFactory = ObjectFactory.Get<IWiseRatesClientFactory>();

			var (client, _) = clientFactory.TryCreate(traceID);
			return client;
		}

		ProvidersConfig RatesServiceConfiguration
		{
			get
			{
				var traceID = WiseRatesClient.GenerateTraceID();
				var client = CreateWiseRatesClient(traceID);
				return client?.GetConfiguration(traceID)?.Providers;
			}
		}
	}
}
