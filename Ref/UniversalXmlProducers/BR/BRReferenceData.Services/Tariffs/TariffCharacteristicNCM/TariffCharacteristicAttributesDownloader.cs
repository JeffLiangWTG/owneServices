using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffCharacteristicAttributesDownloader
	{
		public TariffCharacteristicAttributesDownloader(bool isProduction)
		{
			this.isProduction = isProduction;
		}
		readonly bool isProduction;

		string AttributesUri => ConfigurationProvider.Configuration.GetSection(isProduction ? "URL_TARIFF_ATTRIBUTE" : "URL_TARIFF_ATTRIBUTE_TEST").Value;

		public (IEnumerable<Atributo> Attributes, string Filename) Download(HttpClient client)
		{
			var filename = string.Empty;
			var attributes = new List<Atributo>();
			using (var message = new HttpRequestMessage(HttpMethod.Get, AttributesUri))
			using (var response = client.GetAsyncEx(AttributesUri)?.Result)
			{
				Contract.Assume(response != null);
				Contract.Assume(response.IsSuccessStatusCode);
				var sZip = response.Content.ReadAsStreamAsync()?.Result;
				Contract.Assume(sZip != null);

				using (var archive = new ZipArchive(sZip))
				{
					Contract.Assume(archive != null);
					foreach (var entry in archive.Entries)
					{
						if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
						{
							filename = entry.FullName;
							using (var stream = new StreamReader(entry.Open()))
							{
								var content = stream.ReadToEnd().Replace("\\u001A", string.Empty);
								var attrList = JsonConvert.DeserializeObject<Attributes>(content);
								attributes.AddRange(attrList.atributos);
							}
						}
					}
				}
			}
			return (attributes, filename);
		}
	}
}
