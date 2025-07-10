using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using Enterprise.DeniedPartyScreening.Common;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DeniedPartyScreening.Business
{
	[CodeAlive("Will be used in a future WI.")]
	class DpsRequestHeaderValidator : IValidator<DpsRequestHeader>
	{
		public void Validate(DpsRequestHeader requestHeader)
		{
			var violations = new List<string>();

			const int maxPayloadSize = 3_000_000;
			const int maxCombinedCandidates = 10_000;

			var jsonString = JsonConvert.SerializeObject(requestHeader);
			var httpMessage = new HttpRequestMessage
			{
				Content = new StringContent(jsonString)
			};

			if (httpMessage.Content.Headers.ContentLength > maxPayloadSize)
			{
				violations.Add($"Payload size exceeds {maxPayloadSize} Bytes");
			}

			var totalCandidates = (requestHeader.DpsNameCandidates?.Count() ?? 0) + (requestHeader.DpsCountryCandidates?.Count() ?? 0) + (requestHeader.DpsRegistrationCodeCandidates?.Count() ?? 0) + (requestHeader.DpsAddressCandidates?.Count() ?? 0);
			if (totalCandidates > maxCombinedCandidates)
			{
				violations.Add($"Total number of candidates in request header exceeds maximum allowed limit of {maxCombinedCandidates} candidates");
			}

			if (violations.Any())
			{
				throw new ValidationException($"{nameof(DpsRequestHeader)} validation failed: {string.Join(",", violations)}");
			}
		}
	}
}
