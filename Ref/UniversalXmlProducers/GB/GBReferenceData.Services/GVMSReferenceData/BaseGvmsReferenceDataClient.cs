using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models;
using Newtonsoft.Json.Linq;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
	public abstract class BaseGvmsReferenceDataClient
	{
		public static JObject GetGvmsApiResponse(IGvmsWebClient webClient, string gvmsApiURL)
		{
			if (webClient == null)
			{
				throw new ArgumentNullException(nameof(webClient));
			}
			string apiResponse;
			try
			{
				apiResponse = webClient.GetApiResponse(gvmsApiURL);
			}
			catch (Exception ex)
			{
				throw new ApplicationException($"Could not get API response for '{gvmsApiURL}'", ex);
			}

			if (string.IsNullOrWhiteSpace(apiResponse))
			{
				throw new ApplicationException("No data returned");
			}

			return JObject.Parse(apiResponse);
		}

		internal static List<T> GetReferenceDataByType<T>(JObject apiResponse, StringBuilder errorCollector, string lookupType) where T : IReferenceDataModel
		{
			var refData = new List<T>();
			List<JToken> tokens = apiResponse[lookupType].Children().ToList();
			foreach (JToken token in tokens)
			{
				var refDataType = token.ToObject<T>();
				if (refDataType.IsValid(errorCollector))
				{
					refData.Add(refDataType);
				}
			}
			return refData;
		}

		public static ReferenceData GetReferenceData(IGvmsWebClient webClient, string gvmsApiURL)
		{
			ReferenceData referenceData = new ReferenceData();
			StringBuilder errorCollector = new StringBuilder();
			try
			{
				JObject response = GetGvmsApiResponse(webClient, gvmsApiURL);
				referenceData.Carriers = GetReferenceDataByType<Carrier>(response, errorCollector, "carriers");
				referenceData.Ports = GetReferenceDataByType<Port>(response, errorCollector, "ports");
				referenceData.Routes = GetReferenceDataByType<Route>(response, errorCollector, "routes");
				referenceData.RuleFailures = GetReferenceDataByType<RuleFailure>(response, errorCollector, "ruleFailures");
				referenceData.InspectionLocations = GetReferenceDataByType<InspectionLocation>(response, errorCollector, "locations");
				referenceData.InspectionTypes = GetReferenceDataByType<InspectionType>(response, errorCollector, "inspectionTypes");
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Could not convert content to Reference Data object", ex);
			}

			referenceData.ErrorCollector = errorCollector.ToString();
			Console.WriteLine(referenceData.ErrorCollector);

			return referenceData;
		}

		public ReferenceData GetReferenceData(string gvmsApiURL)
		{
			return GetReferenceData(GetGvmsWebClient(), gvmsApiURL);
		}

		public abstract IGvmsWebClient GetGvmsWebClient();
	}

	public class GVMSReferenceDataClient : BaseGvmsReferenceDataClient
	{
		public override IGvmsWebClient GetGvmsWebClient() => new GvmsWebClient();
	}
}
