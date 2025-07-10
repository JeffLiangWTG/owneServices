using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.Business
{
	public static class PortMappingsDataLoader
	{
		static readonly string[] UNLOCOBlacklist = new[] { "NZAN3", "CNFCG" };

		public static async Task<List<PortMapping>> GetPortMappingsAsync(HttpClient client, string url)
		{
			var response = await client.GetAsync(new Uri(url));
			response.EnsureSuccessStatusCode();

			var data = await response.Content.ReadAsStringAsync();
			var portMappingsResponse = JsonConvert.DeserializeObject<PortMappingsResponse>(data);

			if (portMappingsResponse.items == null)
			{
				throw new InvalidDataException("Failed to deserialise port mappings: no \"items\" property.");
			}

			var result = new List<PortMapping>();
			foreach (var mapping in portMappingsResponse.items)
			{
				mapping.relatedUnlocos.RemoveAll(UNLOCOBlacklist.Contains);
				if (mapping.relatedUnlocos.Count > 1)
				{
					result.Add(mapping);
				}
			}

			return result;
		}

		class PortMappingsResponse
		{
			public PortMapping[] items { get; set; }
		}
	}
}
