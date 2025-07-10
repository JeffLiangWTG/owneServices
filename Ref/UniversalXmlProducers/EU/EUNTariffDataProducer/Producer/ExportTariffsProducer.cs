using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Net.Http;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Model;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class ExportTariffsProducer
	{
		public static ICompositeKeyGeneratorResult DownloadUXmlFileAndLoadTariffs(string downloadFileUrl, string xmlFileName, string outputPath)
		{
			string xml = DownloadFile(downloadFileUrl, xmlFileName, outputPath);

			UniversalReferenceData universalRefData;
			var serializer = new XmlSerializer(typeof(UniversalReferenceData));
			using (var stream = new FileStream(xml, FileMode.Open))
			using (var reader = XmlReader.Create(stream))
			{
				universalRefData = (UniversalReferenceData)serializer.Deserialize(reader);
			}
			CompositeKeyGeneratorResult result = new CompositeKeyGeneratorResult();
			foreach (RefCusTariff tariff in universalRefData.RefCusTariff.ToList())
			{
				result.Tariffs.Add(tariff);
			}
			return result;
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public static string DownloadFile(string downloadFileUrl, string xmlFileName, string outputPath)
		{
			var fileContent = string.Empty;
			var xmlSavedFileFullPath = Path.Combine(outputPath, xmlFileName);

			try
			{
				var tempFileName = Path.GetTempFileName();
				if (File.Exists(tempFileName))
				{
					File.Delete(tempFileName);
				}

				try
				{
					using (var client = FileDownloaderHttpClientFactory.CreateHttpClient())
					{
						Console.WriteLine("Downloading from {0} ...", downloadFileUrl);

						using (HttpResponseMessage response = client.GetAsync(new Uri(downloadFileUrl)).Result)
						using (Stream streamToReadFrom = response.Content.ReadAsStreamAsync().Result)
						using (var fileStream = File.Create(tempFileName))
						{
							streamToReadFrom.CopyTo(fileStream);
						}

						Console.WriteLine("Finish downloading.");
					}

					using (var reader = new StreamReader(tempFileName))
					{
						fileContent = reader.ReadToEnd();
					}
				}
				finally
				{
					if (File.Exists(tempFileName))
					{
						File.Delete(tempFileName);
					}
					if (!string.IsNullOrEmpty(xmlSavedFileFullPath))
					{
						if (File.Exists(xmlSavedFileFullPath))
						{
							File.Delete(xmlSavedFileFullPath);
						}
						using (var stream = new StreamWriter(xmlSavedFileFullPath))
						{
							stream.Write(fileContent);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
			}

			return fileContent;
		}
	}
}
