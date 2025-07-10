using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;

namespace Enterprise.TransportConsignment.Business.Common
{
	public static class GlowHelper
	{
		public enum EntityName
		{
			Consignment,
			RunSheet
		}

		const string ModuleCodeLTP = "LTP";
		const string ModuleCodeCST = "CST";
		const string Glow1Name = "Glow1";
		const string Glow2Name = "Glow2";
		const string BPMEndPointGetModuleInfo = "api/bpm/module/GetModuleInfo";

		public static async Task<GenerateGotoGlowUrlResult> GenerateGotoGlowUrlForExistingEntityAsync(EntityName entityName, ZGuid entityPK)
		{
			return await GenerateGotoGlowUrlAsync(entityName, entityPK).ConfigureAwait(false);
		}

		public static async Task<GenerateGotoGlowUrlResult> GenerateGotoGlowUrlForNewEntityAsync(EntityName entityName)
		{
			return await GenerateGotoGlowUrlAsync(entityName, entityPK: null, aliasPrefix: (NoResString)"New").ConfigureAwait(false);
		}

		static async Task<GenerateGotoGlowUrlResult> GenerateGotoGlowUrlAsync(EntityName entityName, ZGuid? entityPK, string aliasPrefix = "")
		{
			using (Db.DisposableActionForDbConnection())
			{
				var result = new GenerateGotoGlowUrlResult();
				var portalUrl = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				var serviceUri = GlowRegistry.Instance.GlowServiceUri;

				result.ErrorMessage = Validate(entityPK, serviceUri, portalUrl);

				if (result.ErrorMessage != null)
				{
					return result;
				}

				var isLTPModuleEnabled = await GetLTPModuleEnabledAsync().ConfigureAwait(false);
				var aliasName = $"{aliasPrefix}{entityName}{(isLTPModuleEnabled ? Glow2Name : Glow1Name)}";
				var additionalQueryStrings = entityPK.HasValue ? new[] { ("entityPK", entityPK.ToString()) } : null;

				result.Uri = UrlBuilder.GenerateURL(new Uri(portalUrl), $"Goto/{aliasName}", additionalQueryStrings: additionalQueryStrings);

				return result;
			}
		}

		static ResourceString Validate(ZGuid? entityPK, string serviceUri, string portalUrl)
		{
			if (entityPK is { IsEmpty: true })
			{
				return ResString.GetMultilingualString("0da619fb-1405-4110-9a67-7121a5f872ed", "Invalid entity PK.");
			}

			if (string.IsNullOrEmpty(portalUrl))
			{
				return ResString.GetMultilingualString("32079ea4-e3ef-4059-9008-8e96b9e58d5c", "Glow portal URL has not been configured for this client. Registry: {0}/{1}.", GlowRegistry.Instance.GlowPortalsUri.Category, GlowRegistry.Instance.GlowPortalsUri.Caption);
			}

			if (string.IsNullOrEmpty(serviceUri) || serviceUri == "/")
			{
				return ResString.GetMultilingualString("38d54a92-c5a6-4589-9da8-b7071b6d6dee", "Glow service URL has not been configured for this client. Registry: {0}/{1}.", GlowRegistry.Instance.GlowServiceUriRegistryItem.Category, GlowRegistry.Instance.GlowServiceUriRegistryItem.Caption);
			}

			return null;
		}

		static async Task<bool> GetLTPModuleEnabledAsync()
		{
			var restrictedPortalsOverride = GlowRegistry.GetRestrictedPortalsOverride();
			var isLTPInRestrictedOverride = restrictedPortalsOverride.Contains(ModuleCodeLTP);

			if (isLTPInRestrictedOverride)
			{
				return true;
			}

			var isCSTInRestrictedOverride = restrictedPortalsOverride.Contains(ModuleCodeCST);

			if (!isCSTInRestrictedOverride)
			{
				return true;
			}

			var isLTPModulePublishedAndUnrestricted = await ValidateIfLTPModuleIsPublishedAndUnrestrictedAsync().ConfigureAwait(false);

			return isLTPModulePublishedAndUnrestricted;
		}

		static async Task<bool> ValidateIfLTPModuleIsPublishedAndUnrestrictedAsync()
		{
			const string getModuleInfoEndpoint = $"{BPMEndPointGetModuleInfo}?code={ModuleCodeLTP}";

			using var client = CreateGlowHttpClient();
			using var response = await client.GetAsync(getModuleInfoEndpoint).ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var result = JsonConvert.DeserializeObject<GlowModuleInformation>(content);

			return result.Publish && !result.IsRestricted;
		}

		static IGlowServiceClient CreateGlowHttpClient()
		{
			var serviceUri = GlowRegistry.Instance.GlowServiceUri;
			return ObjectFactory.Get<IGlowServiceClientFactory>().Create(new Uri(serviceUri));
		}

		class GlowModuleInformation
		{
			public bool Publish { get; set; }
			public bool IsRestricted { get; set; } = true;
		}

		public class GenerateGotoGlowUrlResult
		{
			public Uri Uri { get; set; }
			public ResourceString ErrorMessage { get; set; }
		}
	}
}
