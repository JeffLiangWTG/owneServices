using System;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eServices.eHub.Common;
using CargoWise.eServices.eHub.Service.BizTalk_Cargowise_eHub_Reference;

namespace CargoWise.eServices.eHub.Service
{
	public class eHubService : IeHubService
	{
		#region IeHubService Members

		public SendResponse Send(SendRequest request)
		{
			var interchage = new Interchange();
			interchage.InterchangeHeader = new InterchangeInterchangeHeader();
			interchage.InterchangeHeader.InterchangeID = request.RequestID;
			interchage.InterchangeHeader.SenderID = request.Envelope.InterchangeDetails.SenderID;
			interchage.InterchangeHeader.RecipientID = request.Envelope.InterchangeDetails.RecipientID;
			interchage.InterchangeHeader.InterchangeVersion = request.Envelope.InterchangeDetails.InterchangeVersion;
			interchage.InterchangeHeader.SenderApplicationVersion = request.Envelope.InterchangeDetails.SenderApplicaitonVersion;

			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(
				new XElement("{http://CargoWise.eServices.eHub.Schema}Document", new XAttribute(XNamespace.Xmlns + "ns0", "http://CargoWise.eServices.eHub.Schema"),
					new XAttribute("DocumentType", request.Envelope.Document.DocumentType),
					new XElement("DocumentContent", request.Envelope.Document.DocumentContent)).ToString());
			interchage.Payload = xmlDoc.DocumentElement;
			return GetSendResponse(Client.SendMessage(interchage));
		}

		public RetrieveResponse Retrieve(RetrieveRequest request)
		{
			var bizTalk_RetrieveRequest = new CargoWise.eServices.eHub.Service.BizTalk_Cargowise_eHub_Reference.RetrieveRequest();
			bizTalk_RetrieveRequest.RecipientID = request.RecipientID;
			bizTalk_RetrieveRequest.RequestID = request.RequestID;

			return GetRetrieveRequest(Client.RetrieveMessages(bizTalk_RetrieveRequest));
		}

		#endregion

		RetrieveResponse GetRetrieveRequest(CargoWise.eServices.eHub.Service.BizTalk_Cargowise_eHub_Reference.RetrieveResponse retrieveResponse)
		{
			var result = new RetrieveResponse() { HasError = retrieveResponse.HasErrors, RequestID = retrieveResponse.RequestID, ErrorDescription = retrieveResponse.ErrorMessage };
			result.Envelopes = retrieveResponse.Interchange != null ? (from interchage in retrieveResponse.Interchange select GetEnvelope(interchage)).ToArray() : new Envelope[0];
			return result;
		}

		Envelope GetEnvelope(Interchange interchage)
		{
			var result = new Envelope();
			result.InterchangeDetails.InterchangeID = interchage.InterchangeHeader.InterchangeID;
			result.InterchangeDetails.InterchangeVersion = interchage.InterchangeHeader.InterchangeVersion;
			result.InterchangeDetails.RecipientID = interchage.InterchangeHeader.RecipientID;
			result.InterchangeDetails.SenderID = interchage.InterchangeHeader.SenderID;
			result.InterchangeDetails.SenderApplicaitonVersion = interchage.InterchangeHeader.SenderApplicationVersion;
			var xPayload = XElement.Parse(interchage.Payload.OuterXml);
			result.Document.DocumentType = xPayload.Attributes("DocumentType").FirstOrDefault().Value;
			result.Document.DocumentContent = xPayload.Value;

			return result;
		}

		SendResponse GetSendResponse(CargoWise.eServices.eHub.Service.BizTalk_Cargowise_eHub_Reference.SendResponse bizTalk_Response)
		{
			return new SendResponse() { HasError = bizTalk_Response.Status.Equals(Constants.SendResponseMessageStatus.Failure), ErrorDescription = bizTalk_Response.ErrorMessage };
		}

		WcfService_CargoWise_eServices_eHub_RoutingClient Client
		{
			get
			{
				return client ?? (client = new WcfService_CargoWise_eServices_eHub_RoutingClient(
					new WSHttpBinding() { MaxReceivedMessageSize = 2147483647,  MaxBufferPoolSize = 2147483647, SendTimeout = new TimeSpan(0, 0, 60) }, new EndpointAddress(ServiceURL)));
			}
		}
		WcfService_CargoWise_eServices_eHub_RoutingClient client;

		const string ServiceURL = "http://syd-wpat-1.corporate.cargowise.com/CargoWise.eHub/WcfService_CargoWise_eServices_eHub_Routing.svc?wsdl";
	}
}
