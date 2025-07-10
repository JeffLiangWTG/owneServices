using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffCharacteristicNCMDownloader
	{
		public TariffCharacteristicNCMDownloader(bool isProduction)
		{
			this.isProduction = isProduction;
		}
		readonly bool isProduction;

		string NcmAttributesUri => ConfigurationProvider.Configuration.GetSection(isProduction ? "URL_TARIFF_NCM_ATTRIBUTE" : "URL_TARIFF_NCM_ATTRIBUTE_TEST").Value;

		public (IEnumerable<NCM> Ncms, string Filename) Download(HttpClient client, bool isExpOnly)
		{
			var filename = string.Empty;
			var ncms = new List<NCM>();

			using (var response = client.GetAsyncEx(NcmAttributesUri)?.Result)
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
							var sb = new StringBuilder();
							using (var sr = new StreamReader(entry.Open()))
							using (var reader = new JsonTextReader(sr))
							{
								reader.Read();
								Contract.Assume(reader.TokenType == JsonToken.StartObject);

								reader.Read();
								Contract.Assume(reader.TokenType == JsonToken.PropertyName && reader.Value?.ToString() == nameof(NCMs.versao));
								reader.Read();

								reader.Read();
								Contract.Assume(reader.TokenType == JsonToken.PropertyName && reader.Value?.ToString() == nameof(NCMs.listaNcm));

								ncms.AddRange(ReadNCMList(reader, isExpOnly).Where(x => x.listaAtributos?.Any() ?? false));
							}
						}
					}
				}
			}
			return (ncms, filename);
		}

		static IEnumerable<NCM> ReadNCMList(JsonTextReader reader, bool isExpOnly)
		{
			return ReadList<NCM>(reader, (obj, propertyName) =>
			{
				switch (propertyName)
				{
					case nameof(NCM.codigoNcm):
						obj.codigoNcm = reader.ReadAsString();
						break;
					case nameof(NCM.listaAtributos):
						obj.listaAtributos = new List<Atributo>();
						obj.listaAtributos.AddRange(ReadAtributoList(reader).Where(x => !isExpOnly || x.modalidade.StartsWith("EXP", StringComparison.OrdinalIgnoreCase)).DistinctBy(x => x.codigo));
						break;
				}
			});
		}

		static IEnumerable<Atributo> ReadAtributoList(JsonTextReader reader)
		{
			return ReadList<Atributo>(reader, (obj, propertyName) =>
			{
				switch (propertyName)
				{
					case nameof(Atributo.codigo):
						obj.codigo = reader.ReadAsString();
						break;
					case nameof(Atributo.modalidade):
						obj.modalidade = reader.ReadAsString();
						break;
					case nameof(Atributo.obrigatorio):
						obj.obrigatorio = reader.ReadAsBoolean().GetValueOrDefault();
						break;
					case nameof(Atributo.dataInicioVigencia):
						obj.dataInicioVigencia = reader.ReadAsString();
						break;
				}
			});
		}

		static IEnumerable<T> ReadList<T>(JsonTextReader reader, Action<T, string> setValues) where T : class, new()
		{
			reader.Read();
			Contract.Assume(reader.TokenType == JsonToken.StartArray);

			var depth = 0;
			T obj = null;
			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.StartObject)
				{
					if (depth == 0)
					{
						obj = new T();
					}
					depth++;
				}
				else if (reader.TokenType == JsonToken.EndObject)
				{
					depth--;
					if (depth == 0)
					{
						Contract.Assume(obj != null);
						yield return obj;

						obj = null;
						continue;
					}
				}
				else if (depth == 0 && reader.TokenType == JsonToken.EndArray)
				{
					break;
				}
				else if (depth == 1 && reader.TokenType == JsonToken.PropertyName && reader.Value != null)
				{
					setValues(obj, reader.Value?.ToString());
				}
			}
		}
	}
}
