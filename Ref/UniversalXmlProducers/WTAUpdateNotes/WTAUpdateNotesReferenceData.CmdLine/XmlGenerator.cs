using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using WiseTechAcademy.UpdateNotesContract;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.WTAUpdateNotesReferenceData.CmdLine
{
	public class XmlGenerator(DateTime? currentTime = null) : IXmlGenerator
	{
		public static IXmlGenerator instance { get; set; } = new XmlGenerator();

		readonly HttpClient Client = new();

		public async Task<IEnumerable<UpdateNoteData>> RunGenerator(UpdateRunType runType)
		{
			var accessToken = await CreateToken();

			var jsonBody = "{" + $"\"token\":\"{accessToken}\", \"runType\":\"{runType}\"" + "}";

			var apiResponse = await CallApi(jsonBody);

			return UpdateNoteDataHandler.DeserialiseUpdateNoteData(apiResponse);
		}

		public async Task<string> CreateToken()
		{
			var accessToken = await OAuthClientAssertion.GetClientAccessTokenAsync(AppConfig.TokenEndpoint, AppConfig.PrivateKey, AppConfig.Certificate, AppConfig.Azp, AppConfig.Aud, CreateClient).ConfigureAwait(false);
			return accessToken;
		}
		public async Task<string> CallApi(string jsonBody)
		{
			using var client = new HttpClient();

			using var content = new StringContent(jsonBody, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
			var postURI = new Uri($"{AppConfig.LMSSiteUrl}/{ContractConstants.UpdateNotesRelativeEndpoint}");
			var response = await client.PostAsync(postURI, content);

			var responseContent = await response.Content.ReadAsStringAsync();
			return responseContent;
		}
		public void ManageXmlCreation(IEnumerable<UpdateNoteData> collection, UpdateRunType updateType)
		{
			var currentDateTime = currentTime ?? DateTime.Now;

			if (updateType == UpdateRunType.Full)
			{
				CreateXml(collection, UpdateType.Full, currentDateTime);
			}
			else if (updateType == UpdateRunType.Partial)
			{
				CreateXml(collection, UpdateType.Partial, currentDateTime);//do the inserts
				CreateXml(collection, UpdateType.Deletion, currentDateTime.AddMinutes(-1));//do the deletions
			}
		}
		public void CreateXml(IEnumerable<UpdateNoteData> collection, UpdateType updateType, DateTime pubTime)
		{
			var updateNoteCollection = GetReleaseNotesFromUpdateNotes(collection, updateType);
			if (updateNoteCollection == null || updateNoteCollection.Count == 0)
			{
				Console.WriteLine($"Did not create {updateType} xml as collection is empty.");
				return;
			}

			var xmlWriter = new XmlWriter(GetXmlWriterConfiguration());
			xmlWriter.SetDataSource(DataSource);
			xmlWriter.SetPublicationTime(pubTime);
			xmlWriter.SetUpdateType(updateType);

			var outputFileName = GetXmlFileName(pubTime, updateType);
			var outputFilePath = Path.Combine(AppConfig.OutputPath, outputFileName);
			ExportToXml(xmlWriter, updateNoteCollection, outputFilePath);
		}

		public static string GetXmlFileName(DateTime pubTime, UpdateType updateType) => $"{DataSource}-{pubTime:yyyyMMdd_HHmmss_fff}-{updateType}.xml";
		public static string DataSource => "RefGlbReleaseNote";

		public List<RefGlbReleaseNote> GetReleaseNotesFromUpdateNotes(IEnumerable<UpdateNoteData> collection, UpdateType updateType)
		{
			var result = new List<RefGlbReleaseNote>();

			foreach (var receivedData in collection)
			{
				if ((updateType == UpdateType.Partial && receivedData.IsInsertion == false)
					|| (updateType == UpdateType.Deletion && receivedData.IsInsertion == true))
				{
					continue;
				}
				var newReleaseNote = new RefGlbReleaseNote()
				{
					ZGF_QuickStartPK = receivedData.GF_QS_PK,
					ZGF_URL = receivedData.GF_URL,
					ZGF_Section = receivedData.GF_Section,
					ZGF_Summary = receivedData.GF_Summary,
					ZGF_ReleaseNoteDate = receivedData.GF_ReleaseNoteDate,
					ZGF_MinVersion = receivedData.GF_MinVersion ?? "",
					ZGF_RN_NKCountryForReleaseNote = "",
					ZGF_Category = "",
					ZGF_IsValid = true,
				};
				result.Add(newReleaseNote);
			}
			return result;
		}

		public void ExportToXml(XmlWriter xmlWriter, IEnumerable<RefGlbReleaseNote> collection, string filePath)
		{
			foreach (var data in collection)
			{
				xmlWriter.PopulateData(data);
			}
			xmlWriter.SaveXml(filePath);
		}

		public XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefGlbReleaseNote>(true);
			entityConfig.IncludeColumn(x => x.ZGF_QuickStartPK, true);
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZGF_IsValid, false, true);
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZGF_Category, false, "");
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZGF_RN_NKCountryForReleaseNote, false, "");
			entityConfig.IncludeColumn(x => x.ZGF_Summary);
			entityConfig.IncludeColumn(x => x.ZGF_URL);
			entityConfig.IncludeColumn(x => x.ZGF_ReleaseNoteDate);
			entityConfig.IncludeColumn(x => x.ZGF_Section);
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZGF_MinVersion, false, "");
			writerConfiguration.IncludeEntityTypeConfiguration(entityConfig);

			return writerConfiguration;
		}

		public HttpClient CreateClient()
		{
			return Client;
		}

		public void Dispose()
		{
			Client.Dispose();
		}
	}
}
