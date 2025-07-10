using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.Foundation.Http;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Helpers
{
	public class CarrierMessagingBussClientProvider
	{
		readonly BusinessObjectFactory factory;
		readonly IHttpClientFactory httpClientFactory;
		const string BookingRequestEndpoint = "BookingRequest";

		public CarrierMessagingBussClientProvider()
		{
			factory = new BusinessObjectFactory() { NameForDebugging = "CarrierMessagingBussClientProvider Factory" };
			httpClientFactory = ObjectFactory.Get<IHttpClientFactory>();
		}

		public async Task SendBookingRequest(Guid consignmentPK)
		{
			var carrierBooking = factory.Load<DtbCarrierBookingConsignment>(consignmentPK) ?? throw new InvalidOperationException("Unable to find Consignment");
			var domesticCarrierCode = GetDomesticCarrierCodeFromCarrierBookingAsync(carrierBooking);
			var universalInterchangeHelper = new UniversalInterchangeHelper();
			universalInterchangeHelper.PopulateCarrierBooking(carrierBooking);
			HttpClient httpClient = httpClientFactory.Create();
			var requestMessage = GetRequestMessage(domesticCarrierCode, BookingRequestEndpoint, universalInterchangeHelper.GetUniversalInterchangeXml());
			var response = await httpClient.SendAsync(requestMessage);
			var responseBody = await response.Content.ReadAsStringAsync();
			XDocument doc = XDocument.Parse(responseBody);
			if(doc.Document?.Elements().FirstOrDefault(e => e.Name.LocalName == "Status").Value != "ERR")
			{
				DocumentHelper.AddDocumentFromUniversalResponse(responseBody, carrierBooking);
				carrierBooking.LTC_Status = "BKD";
				factory.Save();
			}
		}
		string GetDomesticCarrierCodeFromCarrierBookingAsync(DtbCarrierBookingConsignment dtbCarrierBookingConsignment)
		{
			var factory = new BusinessObjectFactory() { NameForDebugging = "Carrier Booking Factory" };
			var carrierAccount = factory.Load<OrgCarrierAccount>(dtbCarrierBookingConsignment.LTC_OAN_CarrierAccount) ?? throw new InvalidOperationException("Unable to find Carrier Account");
			var carrierOrg = factory.Load<OrgHeader>(carrierAccount.OAN_OH_Carrier) ?? throw new InvalidOperationException("Unable to find Carrier");
			var domesticCarrierCode = carrierOrg.CustomsCodes.Where(x => x.OK_CodeType == CodeTypes.DomesticCarrierCode)
				.Select(x => x.OK_CustomsRegNo)
				.FirstOrDefault();
			if (string.IsNullOrEmpty(domesticCarrierCode))
			{
				throw new InvalidOperationException("Unable to find Domestic Carrier Code");
			}
			return domesticCarrierCode; 
		}

		HttpRequestMessage GetRequestMessage(string domesticCarrierCode, string endPoint, string interchangeMessage)
		{
			var baseUri = new Uri(TransportRegistry.Instance.CarrierMessagingBussServerAddress.Value);
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/oauth2/token")
			{
				RequestUri = new Uri($"{baseUri}api/carrier/execution/{domesticCarrierCode}/latest/{endPoint}")
			};
#pragma warning disable CW1161 // Authentication
			requestMessage.Headers.Authorization = GetAuthenticationHeaderValue();
#pragma warning restore CW1161 // Authentication

			requestMessage.Content = new StringContent(interchangeMessage, Encoding.UTF8, "application/xml");
			return requestMessage;
		}

		AuthenticationHeaderValue GetAuthenticationHeaderValue()
		{
			var queryLine = new ZQuery(StmDataSchema.SD_Name, "FreightNotesRegistration");
			var encryptedKey = factory.LoadTop1<StmData>(queryLine).SD_BinaryValue;
			var registration = ProductRegistrationSerializer.GetRegistrationKey(encryptedKey);
			var companyCode = GlbBranch.GetCurrentBranch(factory).GB_Code;
			var authCode = $"{registration?.EnterpriseCode}{companyCode}{registration?.ServerCode}:{registration?.Password}";
			return new AuthenticationHeaderValue(Res.GetString("8859e304-3f5c-4cd2-bbf2-c1c444529e28", "Basic"), Convert.ToBase64String(Encoding.UTF8.GetBytes(authCode)));
		}
	}
}
