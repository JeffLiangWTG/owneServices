using System;
using CargoWise.Common;
using Newtonsoft.Json;

namespace Enterprise.Rating.Business.WiseRates
{
	public class UniversalToWiseRateErrorReporter : IUniversalToWiseRateErrorReporter
	{
		public UniversalToWiseRateErrorReporter(string correlationId)
		{
			this.correlationId = correlationId;
		}

		public void ReportMappingError(string message, string functionName, params (string name, object obj)[] sourceObjects)
		{
			sourceObjects ??= Array.Empty<(string, object)>();

			using (ErrorReporter.GatherAdditionalInformation())
			{
				ErrorReporter.SetAdditionalInfo(UrsMappingCategory, "CorrelationID", correlationId);

				foreach (var (name, obj) in sourceObjects)
				{
					string jsonObj;
					try
					{
						jsonObj = JsonConvert.SerializeObject(obj);
					}
					catch (Exception ex)
					{
						jsonObj = $"Error during serialisation: {ex.Message}";
					}

					ErrorReporter.SetAdditionalInfo(UrsMappingCategory, name, jsonObj);
				}

				var exceptionKey = $"URSMappingError|{functionName}";
				var errorMessage = $"Error mapping URS object during {functionName}: {message}";
				ErrorReporter.ReportOnceWithAdditionalInfo(exceptionKey, errorMessage, UrsMappingCategory);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Internal ID for error reporting")]
		const string UrsMappingCategory = "URS Mapping";

		readonly string correlationId;
	}
}
