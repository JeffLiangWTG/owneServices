using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public class CO2eApiClient : ICO2eApiClient
	{
		readonly IApiClient client;

		public CO2eApiClient()
		{
			client = ObjectFactory.Get<IApiClient>("HttpClient", FreightDataRegistry.Instance.CO2eApiUrl.Value.Trim());
			client.Timeout = TimeSpan.FromSeconds(FreightDataRegistry.Instance.CO2eApiRequestsTimeout.Value);
			client.MediaType = "application/xml";
			client.Accept = new List<string> { "application/json", "application/xml" };
			client.AccessToken = ((NoResString)"Basic", Env.CurrentCompany.GetEHubAuthValue());
			client.ContentSerializer = new EmissionSerializer();
		}

		public CO2eApiClient(IApiClient client)
		{
			this.client = client;
		}

		public void Dispose()
		{
			client?.Dispose();
		}

		public async Task<EmissionResult> GetEmissionAsync(BusinessObject bizo, CancellationToken cancellationToken, ICO2eCalculationSupporter hostSupporter = null)
		{
			var request = GetRequest(bizo, hostSupporter);

			var response = await client.PostAsync<EmissionResult>(request.Endpoint, request.InterchangeString, cancellationToken);
			await response.EnsureSuccessStatusCodeAsync(true);

			var result = response.Content;
			if (result != null)
			{
				result.Request = request;
			}
			return result ?? new EmissionResult();
		}

		public EmissionResult GetEmission(BusinessObject bizo, ICO2eCalculationSupporter hostSupporter)
		{
			var request = GetRequest(bizo, hostSupporter);

			var response = client.PostAsync<EmissionResult>(request.Endpoint, request.InterchangeString, CancellationToken.None).GetAwaiter().GetResult();
			response.EnsureSuccessStatusCodeAsync(true).GetAwaiter().GetResult();

			var result = response.Content;
			if (result != null)
			{
				result.Request = request;
			}
			return result ?? new EmissionResult();
		}

		EmissionRequest GetRequest(BusinessObject bizo, ICO2eCalculationSupporter hostSupporter)
		{
			var request = new EmissionRequest();
			request.Endpoint = GetRequestEndpoint(bizo);
			request.InterchangeString = GetRequestBody(bizo, hostSupporter);
			return request;
		}

#if DEBUG
		public
#endif
		string GetRequestEndpoint(BusinessObject bizo)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var recipientId = registrationKey.EnterpriseCode + registrationKey.ServerCode;

			var manager = bizo.GetUniversalDataContextManager();
			var jobType = Uri.EscapeDataString(manager.DataContextType.ToString());
			var jobNumber = Uri.EscapeDataString(manager.DataContextKey);

			return $"v1/clients/{recipientId}/{jobType}/{jobNumber}/emission";
		}

		string GetRequestBody(BusinessObject bizo, ICO2eCalculationSupporter hostSupporter = null)
		{
			ITopLevelDataObject dataObject = null;
			if (hostSupporter is null)
			{
				dataObject = CO2eHelper.GetCO2eRequestDataObjectWriter(bizo).GetDataObject(bizo);
			}
			else
			{
				dataObject = CO2eHelper.GetCO2eRequestDataObjectWriter(bizo, hostSupporter).GetDataObject(bizo);
			}
			PopulateDataContext(dataObject);
			return GetDataObjectString(dataObject);
		}

		void PopulateDataContext(ITopLevelDataObject dataObject)
		{
			dataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			dataObject.DataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				EventUser = Staff.New(GlbStaff.CurrentUser),
				EventBranch = Branch.New(GlbBranch.CurrentBranch),
				EventDepartment = Department.New(GlbDepartment.CurrentDepartment),
				TriggerType = TriggerType.Manual,
				TriggerDescription = (NoResString)"Calculate: CO2e Greenhouse Gas Emission",
				EventReference = (NoResString)"User Action"
			});
		}

		string GetDataObjectString(ITopLevelDataObject dataObject)
		{
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new CO2eXmlWriter().WriteXML(dataObject, stream, UniversalXmlInfo.Namespace_2012_11);
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return WrapInInterchange(reader.ReadToEnd());
				}
			}
		}

		string WrapInInterchange(string universalXml)
		{
			if (string.IsNullOrWhiteSpace(universalXml))
			{
				return null;
			}

			var senderId = Env.CurrentCompany.GetLicenceCode();

			var payloadXml = XElement.Parse(universalXml);

			XNamespace ns = UniversalXmlInfo.Namespace_2011_11;

			#region SuppressResourceStringsCheckRegion

			var header = new XElement(ns + "Header",
					new XElement(ns + "SenderID", senderId),
					new XElement(ns + "RecipientID", CO2eHelper.CO2eCalculationID));

			var interchange = new XElement(ns + "UniversalInterchange",
				header,
				new XElement(ns + "Body", payloadXml));

			#endregion

			return interchange.ToString();
		}
	}
}
