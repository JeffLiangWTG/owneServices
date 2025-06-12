using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core;
using CargoWise.eHub.Products.GBCustoms.Core.Correlation;
using CargoWise.eHub.Products.GBCustoms.Core.Correlation.Handlers;
using CargoWise.eHub.Shared.Mime;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.GBCustoms.TL.BT.Helpers
{
	public static class OrchestrationHelpers
	{
		internal static Func<IClientRegistrationAccessor> ClientRegistrationAccessor = () => new ClientRegistrationAccessor();
		internal static Func<DataModelAccessor> DataModelAccessor = () => new DataModelAccessor();
		internal static Func<eHubTransactionsContext> ContextFactory = () => new eHubTransactionsContext();
		internal static Func<DateTime> Now = () => DateTime.UtcNow;
		internal static Func<Guid> NewGuid = () => Guid.NewGuid();
		const string GBSubscriptionType = "GBCTID";
		const string DateFormat = "yyyy-MM-ddTHH:mm:ss";
		const string EmcsQualifier = "";
		private static readonly IReadOnlyDictionary<string, string> ProviderCodeMapping = new Dictionary<string, string>
			{
				{ "EMCS", "FirstMessage"},
			};

		public static string ProcessHttpResponseAndReturnMessage(ILog logger, string originalRecipient, string serviceName, string originalSender, string responseXml, HttpHeader[] headers,
			string outgoingMessage, bool isSuccessResponse, string eHubMessageTrackingId, string serviceReference, string requestId, string correlationID = "")
		{
			var returnMessage = "";
			var provider = originalRecipient.Substring(originalRecipient.LastIndexOf('-') + 1);
			
			switch (serviceName)
			{
				case "GetPushData":
				case "Polling":
				case "Unsolicited":
					returnMessage = CreateGBCustomsBusinessResponse(logger, originalRecipient, serviceName, originalSender, responseXml, headers, outgoingMessage, isSuccessResponse, eHubMessageTrackingId, serviceReference, requestId, provider, correlationID);
					break;
				default:
					List<GBCustomsCorrelationIdentifier> correlationIdentifiers = new List<GBCustomsCorrelationIdentifier>();

					correlationIdentifiers = ExtractCorrelationIDs(logger, originalRecipient, serviceName, responseXml, headers);
					var subscribedCorrelations = correlationIdentifiers.Where(i => i.Type == CorrelationType.Subscribed && !string.IsNullOrEmpty(i.Value))
						.Select(i => i.Value);

					if (isSuccessResponse && subscribedCorrelations.Count() > 0)
					{
						var subscriptionContent = ExtractSubscriptionContent(logger, provider, outgoingMessage);
						AddSubscription(logger, originalRecipient, originalSender, subscribedCorrelations.First(), subscriptionContent, provider);
					}

					var jobNumber = string.Empty;
					try
					{
						var outgoingMessageXml = XDocument.Parse(outgoingMessage);
						jobNumber = outgoingMessageXml.XPathSelectElement("/*/*[local-name()='Header']/*[local-name()='GBCustomsRequest']/*[local-name()='JobNumber']").Value;
					}
					catch (XmlException) { }

					returnMessage = CreateUniversalEvent(logger, originalRecipient, originalSender, provider, jobNumber,
						isSuccessResponse, responseXml, eHubMessageTrackingId, correlationIdentifiers);

					break;
			}

			return returnMessage;
		}

		private static string ExtractSubscriptionContent(ILog logger, string provider, string outgoingMessage)
		{
			if (string.IsNullOrEmpty(provider) || provider != "CTCGB")
			{
				return outgoingMessage;
			}

			try
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(outgoingMessage);

				if (!IsLargeFileUpload(xmlDoc))
				{
					return outgoingMessage;
				}

				var headerParent = xmlDoc.SelectSingleNode("/*/*[local-name()='Header']")?.ParentNode;
				if (headerParent == null) return outgoingMessage;

				foreach (XmlNode child in headerParent.ChildNodes)
				{
					if (!child.Name.ToUpper().Equals("HEADER"))
					{
						headerParent.RemoveChild(child);
					}
				}
				return xmlDoc.InnerXml;
			}
			catch (Exception ex)
			{
				logger.Error($"{nameof(ExtractSubscriptionContent)} caught exception.", ex);
				return outgoingMessage;
			}
		}

		public static bool IsLargeFileUpload(XmlDocument message)
		{
			var node = message?.SelectSingleNode("/*[local-name()='GBCustoms']/*[local-name()='Header']/*[local-name()='GBCustomsRequest']/*[local-name()='LargeFile']/text()");

			if (node == null || string.IsNullOrEmpty(node.Value)) return false;

			return node.Value == "1" || node.Value.ToLower() == "true";
		}

		public static List<GBCustomsCorrelationIdentifier> ExtractCorrelationIDs(ILog logger, string originalRecipient, string service, string content, HttpHeader[] headers)
		{
			var correlationIds = new GBCustomsCorrelation(logger, originalRecipient.Substring(originalRecipient.LastIndexOf('-') + 1), service, GBCustomsSource.Synchronous.ToString(),
				content, !originalRecipient.Contains("Test"), ClientRegistrationAccessor()).GBCustomsCorrelationIdentifiers;

			if (correlationIds.FindAll(x => !string.IsNullOrEmpty(x.Value)).Count == 0 && headers != null)
			{
				var headersString = "";
				foreach (var header in headers)
				{
					headersString += string.Format("{0}: {1}\n", header.Key, header.Value);
				}

				if (!string.IsNullOrEmpty(headersString))
				{
					correlationIds = new GBCustomsCorrelation(logger, originalRecipient.Substring(originalRecipient.LastIndexOf('-') + 1), service, GBCustomsSource.Synchronous.ToString(),
					headersString, !originalRecipient.Contains("Test"), ClientRegistrationAccessor()).GBCustomsCorrelationIdentifiers;
				}
			}

			return correlationIds;
		}

		static void AddSubscription(ILog logger, string originalRecipient, string originalSender, string correlationId, string outgoingMessage, string provider)
		{
			string direction = null;
			if (provider == "CTCGB" || provider == "EMCS")
			{
				direction = GetDirection(outgoingMessage, provider);
			}

			DataModelAccessor().InsertSubscriptionValue(GBSubscriptionType, originalRecipient, originalSender, correlationId, outgoingMessage, direction);
			logger.Info($"GBCustomsOutboundOrchestration - Subscription Type of {GBSubscriptionType} is updated with {originalRecipient}, {originalSender}, {correlationId}, {direction ?? string.Empty}");
			if (logger.IsTraceEnabled) logger.TraceFormat("Original Mesasge:{0}", outgoingMessage);
		}

		public static void AddPollingSubscription(ILog logger, string originalRecipient, string originalSender, string correlationId, string direction)
		{
			DataModelAccessor().InsertSubscriptionValue(GBSubscriptionType, originalRecipient, originalSender, correlationId, null, direction);
			logger.Info($"GBCustomsPolling - Subscription Type of {GBSubscriptionType} is added with {originalRecipient}, {originalSender}, {correlationId}, {direction ?? string.Empty}");
		}

		public static string UpdateSubscription(ILog logger, string originalRecipient, string originalSender, DateTime pollingStart, bool isReceived = false)
		{
			if (string.IsNullOrEmpty(originalRecipient) || string.IsNullOrEmpty(originalSender))
			{
				logger.Error($"GBCustoms TL UpdateSubscription: Supplied parameters are invalid - originalRecipient [{originalRecipient ?? ""}] originalSender [{originalSender ?? ""}]");
				throw new ArgumentException($"GBCustoms TL UpdateSubscription: Supplied parameters are invalid - originalRecipient [{originalRecipient ?? ""}] originalSender [{originalSender ?? ""}]");
			}
			var currentUTC = Now().ToString(DateFormat);
			Guid originalRecipientPk, originalSenderPk, subscriptionTypePK;
			var referenceTypeValue = "";
			using (var context = ContextFactory())
			{
				subscriptionTypePK = context.eHubSubscriptionTypes.Where(_ => _.ST_ID == GBSubscriptionType).Select(_ => _.ST_PK).FirstOrDefault();
				originalRecipientPk = context.eHubClients.Where(c => c.CC_ID == originalRecipient).Select(c => c.CC_PK).FirstOrDefault();
				originalSenderPk = context.eHubClients.Where(c => c.CC_ID == originalSender).Select(c => c.CC_PK).FirstOrDefault();
				var now = Now();

				var subscriptions = context.eHubSubscriptionValues.Where(
					x => x.SV_ST == subscriptionTypePK &&
					x.SV_CC_Sender == originalSenderPk &&
					x.SV_CC_Recipient == originalRecipientPk &&
					!(x.SV_ReferenceType ?? "").StartsWith("Received") &&
					(x.SV_ExpiryUTC == null || x.SV_ExpiryUTC > now) &&
					x.SV_SubscribedUTC < pollingStart
					);

				foreach (var subscription in subscriptions)
				{
					referenceTypeValue = isReceived
						? "Received"
						: ((subscription.SV_ReferenceType?.StartsWith("New") ?? false) || (subscription.SV_ReferenceType?.StartsWith("Polling") ?? false))
							? "Polling"
							: "New";

					logger.Trace($"GBCustoms TL UpdateSubscription: Subscription [SV_PK {subscription.SV_PK}] SV_ReferenceType change from [{subscription.SV_ReferenceType ?? ""}] to [{string.Concat(referenceTypeValue, currentUTC)}]");
					subscription.SV_ReferenceType = string.Concat(referenceTypeValue, "|", currentUTC);
				}

				context.SaveChanges();
			}

			return currentUTC;
		}

		static string CreateGBCustomsBusinessResponse(ILog logger, string originalRecipient, string serviceName, string originalSender, string responseXml, HttpHeader[] headers,
			string outgoingMessage, bool isSuccessResponse, string eHubMessageTrackingId, string serviceReference, string requestId, string provider, string correlationID)
		{
			logger.Debug($"GBCustomsOutboundOrchestration - Successful HTTP Response: {isSuccessResponse}");
			var xmlString = "";
			if (string.IsNullOrEmpty(eHubMessageTrackingId))
			{
				eHubMessageTrackingId = NewGuid().ToString();
			}

			#region Check for errors
			var error = CheckForError(logger, responseXml, isSuccessResponse);
			if (!isSuccessResponse || error.isError())
			{
				var eventType = isSuccessResponse ? "MSN" : "MRJ";
				var xmlErrorString = $@"
          <Context>
            <Type>Error</Type>
            <Value>{error.Error}</Value>
          </Context>
          <Context>
            <Type>ErrorSummary</Type>
            <Value>{error.ErrorDescription}</Value>
          </Context>
          <Context>
            <Type>ResponseText</Type>
            <Value>{error.ErrorResponseContent}</Value>
          </Context>";

				xmlString = $@"<ns0:UniversalInterchange xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID />
    <RecipientID />
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	  <Event>
        <EventTime>{XmlConvert.ToString(Now(), "yyyy-MM-ddTHH:mm:sszzz")}</EventTime>
        <EventType>{eventType}</EventType>
		<DataContext>
          <DataSource>
            <DataProvider>{provider}</DataProvider>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{eHubMessageTrackingId}</Value>
          </Context>{xmlErrorString}
        </ContextCollection>
      </Event>
    </UniversalEvent>
  </Body>
</ns0:UniversalInterchange>";

				return CreateCompositeMessage(xmlString, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", originalRecipient, originalSender);
			}
			#endregion

			#region Create GBCustomsBusinessResponse
			if (serviceName == "Polling")
			{
				switch (provider)
				{
					case "ICSGB":
						xmlString = $@"<GBCustomsBusinessResponse>
  <ResponseHeader Provider=""{provider}"">
    <eHubTrackingID>{eHubMessageTrackingId}</eHubTrackingID>
    <CorrelationID>{correlationID}</CorrelationID>
  </ResponseHeader>
  <ResponseBody ContentType=""XML"" Encoding=""none"">
    {responseXml}
  </ResponseBody>
</GBCustomsBusinessResponse>";
						break;
					case "EMCS":
						var responseEncoded = IsBase64Encoded(responseXml)
							? responseXml
							: Convert.ToBase64String(Encoding.UTF8.GetBytes(responseXml));

						xmlString = $@"<GBCustomsBusinessResponse>
  <ResponseHeader Provider=""{provider}"">
    <eHubTrackingID>{eHubMessageTrackingId}</eHubTrackingID>
    <JobNumber>{requestId}</JobNumber>
    <ServiceReference>{correlationID}</ServiceReference>
  </ResponseHeader>
  <ResponseBody ContentType=""XML"" Encoding=""Base64"">
    {responseEncoded}
  </ResponseBody>
</GBCustomsBusinessResponse>";
						break;
				}
			}
			else
			{
				switch (provider)
				{
					case "CTCGB":
						if (serviceName == "GetPushData")
						{
							responseXml = ExtractBodyXML(logger, responseXml);
						}

						xmlString = $@"<GBCustomsBusinessResponse>
  <ResponseHeader Provider=""{provider}"">
    <RequestID>{requestId}</RequestID>
    <ServiceReference>{serviceReference}</ServiceReference>
  </ResponseHeader>
  <ResponseBody ContentType=""XML"" Encoding=""none"">
    {responseXml}
  </ResponseBody>
</GBCustomsBusinessResponse>";
						break;
					case "EMCS":
						var responseEncoded = IsBase64Encoded(responseXml)
							? responseXml
							: Convert.ToBase64String(Encoding.UTF8.GetBytes(responseXml));

						xmlString = $@"<GBCustomsBusinessResponse>
  <ResponseHeader Provider=""{provider}"">
    <eHubTrackingID>{eHubMessageTrackingId}</eHubTrackingID>
    <JobNumber>{requestId}</JobNumber>
    <ServiceReference>{serviceReference}</ServiceReference>
  </ResponseHeader>
  <ResponseBody ContentType=""XML"" Encoding=""Base64"">
    {responseEncoded}
  </ResponseBody>
</GBCustomsBusinessResponse>";
						break;
				}
			}
			#endregion

			return CreateCompositeMessage(xmlString, "http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", originalRecipient, originalSender);
		}

		static string CreateUniversalEvent(ILog logger, string originalRecipient, string originalSender, string provider, string jobNumber, bool isSuccessResponse, string responseXml,
			string eHubMessageTrackingId, List<GBCustomsCorrelationIdentifier> correlationIdentifiers)
		{
			logger.Debug($"GBCustomsOutboundOrchestration (CreateUniversalEvent) - Successful HTTP Response: {isSuccessResponse}");
			var error = CheckForError(logger, responseXml, isSuccessResponse, provider);

			#region UniversalEvent wrapped in UniversalInterchange
			var eventType = isSuccessResponse
				? provider != "EMCS"
					? "MSN"
					: "MRS"
				: "MRJ";
			var xmlErrorString = (isSuccessResponse || !error.isError()) ? "" : CreateErrorContextXML(error);

			logger.Trace($"GBCustomsOutboundOrchestration (CreateUniversalEvent) - xmlErrorString: {xmlErrorString}");

			var xmlCorrelationString = "";
			foreach (var correlationString in correlationIdentifiers?.Where(i => !string.IsNullOrEmpty(i.Value)).Select(i => $@"
				<Context>
				<Type>{i.Name}</Type>
				<Value>{i.Value}</Value>
			  </Context>"))
			{
				xmlCorrelationString += correlationString;
			}

			logger.Trace($"GBCustomsOutboundOrchestration (CreateUniversalEvent) - xmlCorrelationString: {xmlCorrelationString}");

			if (string.IsNullOrEmpty(xmlErrorString) && string.IsNullOrEmpty(xmlCorrelationString) && provider != "EMCS")
			{
				var originalResponse = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseXml));

				var noCorrelationError = new MessageError()
				{
					Error = "No Correlation Identifiers",
					ErrorDescription = "No Correlation Identifiers could be identified",
					ErrorResponseContent = originalResponse
				};

				eventType = "MRJ";
				xmlErrorString = CreateErrorContextXML(noCorrelationError);
			}

			var jobDataContext = BuildJobData(provider, jobNumber);


			var xmlString = $@"<ns0:UniversalInterchange xmlns:ns0=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID />
    <RecipientID />
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	  <Event>
		{jobDataContext}
        <EventTime>{XmlConvert.ToString(Now(), "yyyy-MM-ddTHH:mm:sszzz")}</EventTime>
        <EventType>{eventType}</EventType>
		<DataContext>
          <DataSource>
            <DataProvider>{provider}</DataProvider>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{eHubMessageTrackingId}</Value>
          </Context>{xmlErrorString}{xmlCorrelationString}
        </ContextCollection>
      </Event>
    </UniversalEvent>
  </Body>
</ns0:UniversalInterchange>";

			logger.Trace($"GBCustomsOutboundOrchestration (CreateUniversalEvent) - xmlString: {xmlString}");
			#endregion

			return CreateCompositeMessage(xmlString, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", originalRecipient, originalSender);
		}

		private static string BuildJobData(string provider, string jobNumber)
		{
			var jobType = string.Empty;
			switch(provider)
			{
				case "CTCGB":
				case "CTCNI":
					jobType = "NctsHeader";
					break;
				case "EMCS":
					jobType = "EMCSJobDeclaration";
					break;
				case "ICSGB":
				case "ICSNI":
					jobType = "AsycudaManifest";
					break;
				case "GVMS":
					jobType = "GvmsAsycudaManifestHeader";
					break;
			}

			return !string.IsNullOrEmpty(jobType) ? $@"        <DataContext>
			       <DataTargetCollection>
			         <DataTarget>
			           <Type>{jobType}</Type>
			           <Key>{jobNumber}</Key>
					</DataTarget>
			       </DataTargetCollection>
			     </DataContext>" : string.Empty;
		}


		static MessageError CheckForError(ILog logger, string responseXml, bool isSuccessResponse, string provider = null)
		{
			string error = string.Empty, errorDescription = string.Empty, errorResponseContent = string.Empty;

			try
			{
				logger.Trace($"CheckForError() - Response XML: {responseXml}");

				var xmlStr = responseXml;
				var extractedHTML = Core.BT.Helpers.OrchestrationHelpers.ExtracAndRemoveHTMLcontentFromErrorResponse(ref xmlStr);
				errorResponseContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(extractedHTML));

				var xml = XDocument.Parse(xmlStr);
				if (xml.Root.Name.LocalName == "ErrorResponse")
				{
					error = string.Join("; ", from e in xml.Descendants() where e.Name.LocalName == "Type" select e.Value);

					StringBuilder builder = new StringBuilder();
					var errorElements = from e in xml.Descendants() where e.Name.LocalName == "Error" select e;
					foreach (var errorElement in errorElements)
					{
						builder.Append(string.Join(", ", from e in errorElement.Descendants() where e.Name.LocalName == "Text" select e.Value));
						if (errorElements.Last() != errorElement)
						{
							builder.Append("; ");
						}
					}
					errorDescription = builder.ToString();

					if (string.IsNullOrEmpty(errorResponseContent))
					{
						var responseElements = from e in xml.Descendants() where e.Name.LocalName == "ResponseText" select e;
						logger.DebugFormat("responseElements: {0}", string.Join("|", from e in responseElements select e.Value));
						if (responseElements.Count() >= 1)
						{
							var originalResponseContent = string.Empty;

							if ((provider ?? "") == "EMCS")
							{
								originalResponseContent = Core.BT.Helpers.OrchestrationHelpers.GetInnerXmlFromXelement(xml.XPathSelectElement("ErrorResponse/ResponseText/Text"));
								if (!string.IsNullOrEmpty(originalResponseContent))
								{
									errorDescription = "Message not accepted";
								}
							}
							else
							{
								originalResponseContent = string.Join(", ", from e in responseElements.First().Descendants()
																				where e.Name.LocalName == "Text"
																				select e.Value);
							}
							var originalResponseBytes = Encoding.UTF8.GetBytes(originalResponseContent);
							errorResponseContent = Convert.ToBase64String(originalResponseBytes);
							logger.DebugFormat("originalResponseContent: {0}", originalResponseContent.ToString());
						}
					}
				}
			}
			catch (XmlException) { }

			if (!isSuccessResponse && string.IsNullOrEmpty(errorDescription)) errorDescription = responseXml;

			return new MessageError()
			{
				Error = error,
				ErrorDescription = errorDescription,
				ErrorResponseContent = errorResponseContent
			};
		}

		static string CreateCompositeMessage(string xmlString, string messageType, string originalRecipient, string originalSender)
		{
			using (var stream = new MemoryStream())
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xmlString);
				xmlDoc.Save(stream);
				stream.Seek(0, SeekOrigin.Begin);

				XNamespace nsEHubInboxMessage = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage";
				XNamespace nsSqlTableOps = "http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo";
				XNamespace nsEHubInboxXmlContent = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent";
				XNamespace nsEHubOutboxMessage = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage";

				var inboxPk = NewGuid();
				string originalRecipientPk, originalSenderPk, universalInterchangePk;
				using (var context = ContextFactory())
				{
					originalRecipientPk = context.eHubClients.Where(c => c.CC_ID == originalRecipient).Select(c => c.CC_PK.ToString()).FirstOrDefault() ?? string.Empty;
					originalSenderPk = context.eHubClients.Where(c => c.CC_ID == originalSender).Select(c => c.CC_PK.ToString()).FirstOrDefault() ?? string.Empty;
					universalInterchangePk = context.eHubMessageTypes.Where(m => m.DT_Code == messageType).Select(m => m.DT_PK.ToString()).FirstOrDefault() ?? "12F1150E-4F3F-410F-990C-BAAB866EDC29";
				}
				var encodedContent = stream.CompressAndEncode().ReadToEnd();
				var applicationCode = messageType == "http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse" ? "CDS" : "UDM";

				var queryXml = new XDocument(
					new XElement("CompositeOperation",
						new XElement(nsEHubInboxMessage + "Insert",
							new XElement(nsEHubInboxMessage + "Rows",
								new XElement(nsSqlTableOps + "eHubInboxMessage",
									new XElement(nsSqlTableOps + "EI_PK", inboxPk),
									new XElement(nsSqlTableOps + "EI_MessageTrackingID", inboxPk),
									new XElement(nsSqlTableOps + "EI_EnvelopeTrackingID", Guid.Empty),
									new XElement(nsSqlTableOps + "EI_CC_Sender", originalRecipientPk),
									new XElement(nsSqlTableOps + "EI_CC_Recipient", originalSenderPk),
									new XElement(nsSqlTableOps + "EI_MessageType", messageType),
									new XElement(nsSqlTableOps + "EI_IsFlatFile", false),
									new XElement(nsSqlTableOps + "EI_ApplicationCode", applicationCode),
									new XElement(nsSqlTableOps + "EI_Status", 2),
									new XElement(nsSqlTableOps + "EI_Content", encodedContent)))),
						new XElement(nsEHubInboxXmlContent + "Insert",
							new XElement(nsEHubInboxXmlContent + "Rows",
								new XElement(nsSqlTableOps + "eHubInboxXmlContent",
									new XElement(nsSqlTableOps + "EX_EI_Inbox", inboxPk),
									new XElement(nsSqlTableOps + "EX_XmlContent", xmlString),
									new XElement(nsSqlTableOps + "EX_DT_Source", universalInterchangePk),
									new XElement(nsSqlTableOps + "EX_UncompressedLength", stream.Length)))),
						new XElement(nsEHubOutboxMessage + "Insert",
							new XElement(nsEHubOutboxMessage + "Rows",
								new XElement(nsSqlTableOps + "eHubOutboxMessage",
									new XElement(nsSqlTableOps + "OI_PK", NewGuid()),
									new XElement(nsSqlTableOps + "OI_MessageTrackingID", inboxPk),
									new XElement(nsSqlTableOps + "OI_EI_InboxPK", inboxPk),
									new XElement(nsSqlTableOps + "OI_CC_Sender", originalRecipientPk),
									new XElement(nsSqlTableOps + "OI_CC_Recipient", originalSenderPk),
									new XElement(nsSqlTableOps + "OI_DT_Target", universalInterchangePk),
									new XElement(nsSqlTableOps + "OI_Status", 0),
									new XElement(nsSqlTableOps + "OI_Content", encodedContent),
									new XElement(nsSqlTableOps + "OI_XmlContent", xmlString))))));
				return queryXml.ToString();
			}

		}

		public static string GetStatusType(ILog logger, string status)
		{
			return SplitStatus(logger, status)[0];
		}

		public static DateTime GetStatusDate(ILog logger, string status)
		{
			DateTime statusDate;
			try
			{
				statusDate = DateTime.Parse(SplitStatus(logger, status)[1]);
			}
			catch (Exception ex)
			{
				logger.Error($"GBCustoms TL GetStatusDate: Supplied parameters are invalid - Status [{status ?? ""}] - Exception Message: {ex.Message}");
				throw new ArgumentException($"GBCustoms TL GetStatusDate: Supplied parameters are invalid - Status [{status ?? ""}] - Exception Message: {ex.Message}");
			}
			return statusDate;
		}

		public static bool HasTimeLapsed(ILog logger, DateTime lastUpdated, int timeInterval)
		{
			var now = Now();
			var hasLapsed = lastUpdated.AddMinutes(timeInterval) < now;
			logger.Trace($"GBCustoms TL HasTimeLapsed - LastUpdated [{lastUpdated.ToString(DateFormat)}], CurrentUTC [{now.ToString(DateFormat)}], Interval [{timeInterval}], HasLapsed [{hasLapsed}]");
			return hasLapsed;
		}

		public static int GetXmlMessageCount(ILog logger, string message, string countPath)
		{
			var xmlMessage = XDocument.Parse(message);
			return xmlMessage.XPathSelectElements(countPath).Count();
		}

		public static string GetXmlMessageValue(ILog logger, string message, string path, int messageItemNumber)
		{
			var xmlMessage = XDocument.Parse(message);
			try
			{
				return xmlMessage.XPathSelectElements(path).ElementAt(messageItemNumber - 1).Value;
			}
			catch
			{
				logger.Error($"Error in GetXmlMessageValue: message = {message}, path = {path}, messageItemNumber = {messageItemNumber}");
				throw;
			}
		}

		public static string GetXmlMessageAttribute(ILog logger, string message, string path, string attribute)
		{
			var xmlMessage = XDocument.Parse(message);
			return xmlMessage.XPathSelectElement(path).Attribute(attribute).Value;
		}


		internal static string[] SplitStatus(ILog logger, string status)
		{
			if (status == null)
			{
				status = $"New|{Now()}";
			}
			var statusParts = status.Split('|');
			if (statusParts.Count() != 2 || string.IsNullOrEmpty(statusParts[0]) || string.IsNullOrEmpty(statusParts[1]))
			{
				logger.Error($"GBCustoms TL SplitStatus: Supplied parameters are invalid - Status [{status ?? ""}]");
				throw new ArgumentException($"GBCustoms TL SplitStatus: Supplied parameters are invalid - Status [{status ?? ""}]");
			}
			return statusParts;
		}

		public static void SaveConfiguration(EMCSConfig emcsConfig, string clientId, string registrationType = "GBCustoms-EMCS")
		{
			try
			{
				var configXml = string.Empty;
				using (StringWriter textWriter = new StringWriter())
				{
					XmlSerializer xmlSerializer = new XmlSerializer(emcsConfig.GetType());
					xmlSerializer.Serialize(textWriter, emcsConfig);
					configXml = textWriter.ToString();
				}
				if (!ClientRegistrationAccessor().Exists(clientId, registrationType, EmcsQualifier))
				{
					ClientRegistrationAccessor().Insert(clientId, registrationType, EmcsQualifier, string.Empty, 1, configXml, string.Empty, string.Empty);
				}
				else
				{
					ClientRegistrationAccessor().UpdateFlag1AndConfigXml(clientId, registrationType, EmcsQualifier, 1, configXml);
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error executing method SaveConfiguration" + Environment.NewLine + ex.Message);
			}
		}

		public static DateTime GetFormattedUTC()
		{
			return Convert.ToDateTime(System.DateTime.UtcNow.ToString());
		}

		public static void MarkConfigurationAsCompleted(EMCSConfig emcsConfig, string clientId, string registrationType = "GBCustoms-EMCS")
		{
			try
			{
				ClientRegistrationAccessor().UpdateFlag1(clientId, registrationType, EmcsQualifier, string.Empty, 0);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error executing method MarkConfigurationAsCompleted" + Environment.NewLine + ex.Message);
			}

		}

		private static string CreateErrorContextXML(MessageError messageError)
		{
			return $@"
          <Context>
            <Type>Error</Type>
            <Value>{messageError.Error}</Value>
          </Context>
          <Context>
            <Type>ErrorSummary</Type>
            <Value>{messageError.ErrorDescription}</Value>
          </Context>
          <Context>
            <Type>ResponseText</Type>
            <Value>{messageError.ErrorResponseContent}</Value>
          </Context>";
		}

		private static string ExtractBodyXML(ILog logger, string inputJson)
		{
			var jsonExtractor = new JSONBodyExtractionHandler();
			var bodyValue = jsonExtractor.Extract(logger, inputJson, "body");
			return string.IsNullOrEmpty(bodyValue)
				? inputJson
				: OrchestrationHelper.RemoveInvalidCharactersFromXmlContent(bodyValue);
		}

		internal static string CreateIRValue(XmlDocument xmlDocument)
		{
			var transform = new XmlDsigC14NTransform();
			transform.LoadInput(xmlDocument);
			var stream = (Stream)transform.GetOutput(typeof(Stream));

			byte[] hash = null;
			using (var sha1 = new SHA1CryptoServiceProvider())
			{
				stream.Position = 0;
				hash = sha1.ComputeHash(stream);
			}

			return Convert.ToBase64String(hash);
		}

		public static EMCSConfig CreateConfig(ILog logger, string messageXml, string clientId, MessageType messageType, HttpHeader[] requestHeaders, HttpHeader[] responseHeaders, string credentialsKey, string contentType, string registrationType = "GBCustoms-EMCS")
		{
			string rootPath;

			switch (messageType)
			{
				case MessageType.EMCSXmlConfig:
					rootPath = "//EMCSConfig";
					break;
				case MessageType.GBCustomsRequest:
					rootPath = "//GBCustomsRequest";
					break;
				default:
					throw new Exception("MessageType not provided");
			}

			try
			{
				string existingLastPollDate = null;
				var xmlMessage = XDocument.Parse(messageXml);

				var registation = ClientRegistrationAccessor().ReadRegistrations(clientId, registrationType, qualifier: EmcsQualifier);
				if (registation != null && registation.Count() == 1)
				{
					var existingConfig = registation.First()?["CX_ConfigXml"]?.ToString() ?? string.Empty;
					if (!string.IsNullOrEmpty(existingConfig))
					{
						try
						{
							var existingConfigXml = XDocument.Parse(existingConfig);
							if (existingConfigXml != null)
							{
								try
								{
									existingLastPollDate = Convert.ToDateTime(existingConfigXml.XPathSelectElement("EMCSConfig/LastPollingDate")?.Value).ToString();
									logger.Debug($"Updating ClientRegistration with existing LastPollingDate {existingLastPollDate}");
								}
								catch
								{
									existingLastPollDate = null;
								}
							}
						}
						catch (XmlException) { }
					}
				}

				return new EMCSConfig()
				{
					Service = xmlMessage.XPathSelectElement($"{rootPath}/Service")?.Value ?? string.Empty,
					Recipient = xmlMessage.XPathSelectElement($"{rootPath}/Recipient")?.Value ?? string.Empty,
					OutboundDate = Convert.ToDateTime(xmlMessage.XPathSelectElement($"{rootPath}/OutboundDate")?.Value ?? Now().ToString()),
					LastPollingDate = Convert.ToDateTime(xmlMessage.XPathSelectElement($"{rootPath}/LastPollingDate")?.Value ?? existingLastPollDate ?? Now().AddYears(-1).ToString()),
					ErrorCount = Convert.ToInt32(xmlMessage.XPathSelectElement($"{rootPath}/ErrorCount")?.Value ?? "0"),
					RequestHeaders = requestHeaders,
					ResponseHeaders = responseHeaders,
					CredentialsKey = credentialsKey,
					ContentType = contentType,
				};
			}
			catch (XmlException ex)
			{
				throw new XmlException($"Invalid XML format.{Environment.NewLine}Message [{messageXml}]{Environment.NewLine}Exception: [{ex.Message}]");
			}
		}

		public static EMCSConfig ExtractConfig(string emcsXml)
		{
			try
			{
				var xDoc = XDocument.Parse(emcsXml);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(EMCSConfig));

				using (var reader = xDoc.Root.CreateReader())
				{
					return (EMCSConfig)xmlSerializer.Deserialize(reader);
				}
			}
			catch (XmlException ex)
			{
				throw new XmlException($"Invalid Config XML.{Environment.NewLine}Message [{emcsXml}]{Environment.NewLine}Exception: [{ex.Message}]");
			}
		}

		public static HttpHeader[] SetTokenHeader(HttpHeader[] httpHeaders, string token)
		{
			httpHeaders.FirstOrDefault(x => x.Key == "Authorization").Value = $"Bearer {token}";
			return httpHeaders;
		}

		public static string GetQueryString(EMCSConfig eMCSConfig)
		{
			var queryString = "?traderType=consignee";

			if (!(eMCSConfig.LastPollingDate == null || eMCSConfig.LastPollingDate == new DateTime(0)))
			{
				queryString += $"&updatedSince={eMCSConfig.LastPollingDate.ToString("yyyy-MM-ddTHH:mm:ssZ")}";
			}

			return queryString;
		}

		internal static XmlDocument ExtractBodyForIrMark(XmlDocument xml, XmlNamespaceManager ns)
		{
			const string stringBodyForIrMark = @"<s:Body xmlns:s=""http://www.w3.org/2003/05/soap-envelope""></s:Body>";
			var bodyXdoc = XmlDocToXDoc(xml).Root;

			var xDocForIrMark = XDocument.Parse(stringBodyForIrMark);
			xDocForIrMark.XPathSelectElement("/s:Body", ns)?.Add(bodyXdoc);

			return XDocToXmlDoc(xDocForIrMark);
		}

		internal static string GetDirection(string outgoingMessage, string provider)
		{
			try
			{
				var outgoingMessageXml = new XmlDocument();
				outgoingMessageXml.LoadXml(outgoingMessage);
				var service = GetService(outgoingMessageXml);

				switch (provider)
				{
					case "CTCGB":
						return Regex.IsMatch(service, "arriv", RegexOptions.IgnoreCase) ? "Arrive"
								: Regex.IsMatch(service, "depart", RegexOptions.IgnoreCase) ? "Depart"
								: null;
					case "EMCS":
						return service;
					default:
						return null;
				}
			}
			catch(XmlException)
			{
				return null;
			}
		}

		public static string SplitAction(string inputString)
		{
			return inputString.Split('|')[0];
		}

		static string GetService(XmlDocument input)
		{
			return GetValueFromXml(input, "//*[local-name()='GBCustoms']/*[local-name()='Header']/*[local-name()='GBCustomsRequest']/*[local-name()='Service']");
		}

		public static Int32 GetMessageCount(string bodyXml)
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(bodyXml);
			return Convert.ToInt32(GetValueFromXml(xmlDocument, "//*[local-name()='GetNewMessagesResponse']/*[local-name()='NewMessagesDataResponse']/*[local-name()='CountOfMessagesAvailable']"));
		}

		static string GetValueFromXml(XmlDocument input, string xpath)
		{
			var nodes = input.SelectNodes(xpath);
			return nodes?[0]?.InnerXml ?? string.Empty;
		}

		static XDocument XmlDocToXDoc(XmlDocument xmlDoc)
		{
			using (var nodeReader = new XmlNodeReader(xmlDoc))
			{
				nodeReader.MoveToContent();
				return XDocument.Load(nodeReader);
			}
		}

		static XmlDocument XDocToXmlDoc(XDocument xDoc)
		{
			var xmlDoc = new XmlDocument();
			using (var xmlReader = xDoc.CreateReader())
			{
				xmlDoc.Load(xmlReader);
			}
			return xmlDoc;
		}

		static XmlDocument AddXmlDeclaration(XmlDocument xmlDoc)
		{
			var xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
			xmlDoc.InsertBefore(xmldecl, xmlDoc.DocumentElement);
			return xmlDoc;
		}

		public static void SubscribePreSubmissionCorrelations(ILog logger, string outgoingMessage, string originalRecipient, string originalSender)
		{
			var correlationPaths = new List<string>()
			{
				"//*[local-name()='ComAccRefMES21']",
			};

			var provider = originalRecipient.Substring(originalRecipient.LastIndexOf('-') + 1);
			var xml = new XmlDocument();
			try
			{
				xml.LoadXml(outgoingMessage);
			}
			catch (XmlException) { }

			foreach (var path in correlationPaths)
			{
				var correlationValue = GetValueFromXml(xml, path);
				if (!string.IsNullOrEmpty(correlationValue))
				{
					AddSubscription(logger, originalRecipient, originalSender, correlationValue, outgoingMessage, provider);
				}
			}
		}

		public static Stream CreateLargeFileMessage(ILog logger, string fields, string contentType, string boundary, string body)
		{
			try
			{
				var largeFileContentHeaders = SplitFormData(fields);
				largeFileContentHeaders.Add("file", body);

				var mimeMessage = new MimePart.Multipart(contentType, boundary)
				{
					Headers =
					{
						{ "Message-ID", string.Format("<{0}>", boundary) },
						{ "MIME-Version", "1.0" },
						{ "Content-Type", contentType }
					}
				};

				foreach (var keyValuePair in largeFileContentHeaders)
				{
					mimeMessage.Parts.Add(new MimePart.Content(keyValuePair.Key == "file" ? "application/xml" : "text/xml")
					{
						Headers =
								{
									{ "Content-Disposition", "form-data; name=\"" + keyValuePair.Key + "\"" }
								},
						Contents = new MemoryStream(Encoding.UTF8.GetBytes(keyValuePair.Value))

					});
				}

				if (logger.IsTraceEnabled)
				{
					using (var streamForLog = mimeMessage.Format())
					using (var reader = new StreamReader(streamForLog, Encoding.UTF8))
					{
						streamForLog.Position = 0;
						logger.Trace($"CreateLargeFileMessage - Request message: {reader.ReadToEnd()}");
					}
				}


				return mimeMessage.Format();
			}
			catch (Exception ex)
			{
				logger.Error($"Error in CreateLargeFileMessage - [{ex.Message}]", ex);
				throw;
			}
		}

		public static string GetCodeMappingService(ILog logger, string provider, string serviceName, string serviceReference)
		{
			provider = provider.Substring(provider.LastIndexOf('-') + 1);
			if (string.IsNullOrEmpty(serviceReference) && ProviderCodeMapping.TryGetValue(provider, out string mappedService))
			{
				logger.Info($"GetCodeMappingService - Service changed from [{serviceName}] to [{mappedService}] for provider [{provider}] for CodeMappings");
				return mappedService;
			}

			return serviceName;
		}

		public static int GetJsonArrayCount(string json)
		{
			return ParseJsonArray(json).Count;
		}

		public static string GetJsonValueFromArray(string json, int recordNumber, string elementName)
		{
			return ParseJsonArray(json)[recordNumber].Value<string>(elementName);
		}

		public static TypedPollingArray DeserializeICSParameters(XmlDocument xml)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(TypedPolling));
			using (StringReader reader = new StringReader(xml.OuterXml))
			{
				var result = (TypedPolling)serializer.Deserialize(reader);
				return new TypedPollingArray { PollingResults = result.TypedPollingResultSet0.ToArray() };
			}
		}

		public static int GetICSParameterCount(TypedPollingResultSet0[] array)
		{
			return array?.Length ?? 0;
		}

		private static JArray ParseJsonArray(string json)
		{
			return JArray.Parse(json);
		}

		private static bool IsBase64Encoded(string input)
		{
			try
			{
				var data = Convert.FromBase64String(input);
				return input.Replace(" ", "").Length % 4 == 0;
			}
			catch
			{
				return false;
			}
		}

		internal static Dictionary<string, string> SplitFormData(string formData)
		{
			return JsonConvert.DeserializeObject<Dictionary<string, string>>(formData);
		}

		public struct MessageError
		{
			public string Error { get; set; }
			public string ErrorDescription { get; set; }
			public string ErrorResponseContent { get; set; }

			public bool isError() => !(string.IsNullOrEmpty(Error) && string.IsNullOrEmpty(ErrorDescription) && string.IsNullOrEmpty(ErrorResponseContent));
		}
	}
}
