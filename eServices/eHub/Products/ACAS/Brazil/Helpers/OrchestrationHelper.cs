using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ACAS.BR.Schemas.StatusUpdate;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.ACAS.BR.Helpers
{
    public class OrchestrationHelper
    {
        private const string UIMessageType = "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange";
        public static XmlDocument GetWcfInsertInboxAndUpdateAsyncPollingRegistration(
            string pollingRegistrationPk, string clientID, string acasClientId, string inboxContent, XmlDocument pollingRegistrationXml)
        {
            if (string.IsNullOrWhiteSpace(clientID)) throw new ArgumentNullException("clientID");
            if (string.IsNullOrWhiteSpace(pollingRegistrationPk)) throw new ArgumentNullException("pollingRegistrationPk");
            if (string.IsNullOrWhiteSpace(acasClientId)) throw new ArgumentNullException("acasClientId");

            XNamespace nsEHubAsyncPollingRegistration = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubAsyncPollingRegistration";
            XNamespace nsSqlTableOps = "http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo";

            var wcfSqlUpdateAsyncPollingRegistration =
                new XElement(nsEHubAsyncPollingRegistration + "Update",
                    new XElement(nsEHubAsyncPollingRegistration + "Rows",
                        new XElement(nsEHubAsyncPollingRegistration + "RowPair",
                            new XElement(nsEHubAsyncPollingRegistration + "After",
                                new XElement(nsSqlTableOps + "PR_XML", pollingRegistrationXml.InnerXml)
                                    ),
                            new XElement(nsEHubAsyncPollingRegistration + "Before",
                                new XElement(nsSqlTableOps + "PR_PK", pollingRegistrationPk)
                            )
                        )
                    )
                );
            var wcfSqlInsertInboxElement = WcfSqlOperationMessages.GetWcfSqlInsertInbox(acasClientId, clientID, UIMessageType, "UDM", inboxContent);

            var response = WcfSqlOperationMessages.CombineWcfSqlOps(wcfSqlInsertInboxElement, wcfSqlUpdateAsyncPollingRegistration);
            return response;
        }

        public static XmlDocument GetWcfInsertInbox(string clientID, string acasClientId, string inboxContent, Guid responseEHubTrackingID)
        {
            if (string.IsNullOrWhiteSpace(clientID)) throw new ArgumentNullException("clientID");
            if (string.IsNullOrWhiteSpace(acasClientId)) throw new ArgumentNullException("acasClientId");

            var wcfSqlInsertInboxElement = WcfSqlOperationMessages.GetWcfSqlInsertInbox(acasClientId, clientID, UIMessageType, "UDM", inboxContent, responseEHubTrackingID);

            var response = WcfSqlOperationMessages.CombineWcfSqlOps(wcfSqlInsertInboxElement);
            return response;
        }

        public static XmlDocument GetWcfInsertPollingXMLAndUpdateAsyncPollingRegistration(
            string pollingRegistrationPk, string clientID, string acasClientId, XmlDocument pollingRegistrationXml, string errorDescription)
        {
            if (!String.IsNullOrEmpty(errorDescription))
            {
                var stateDescription = pollingRegistrationXml.CreateElement("StateDescription");
                stateDescription.InnerText = errorDescription;
                pollingRegistrationXml.DocumentElement.AppendChild(stateDescription);
            }
            return GetWcfInsertInboxAndUpdateAsyncPollingRegistration(pollingRegistrationPk, clientID, acasClientId, pollingRegistrationXml.OuterXml, pollingRegistrationXml);
        }

        public static XmlDocument GetWcfInsertInboxAndDeleteAsyncPollingRegistration(
	        string pollingRegistrationPk, string cw1SystemId, string acasClientId, string inboxContent)
        {
	        if (string.IsNullOrWhiteSpace(cw1SystemId)) throw new ArgumentNullException("cw1SystemId");
	        if (string.IsNullOrWhiteSpace(pollingRegistrationPk)) throw new ArgumentNullException("pollingRegistrationPk");
	        if (string.IsNullOrWhiteSpace(acasClientId)) throw new ArgumentNullException("acasClientId");

			var wcfSqlUpdateAsyncPollingRegistration = GetWcfXElementSqlDeleteAsyncPollingRegistration(pollingRegistrationPk);

	        var wcfSqlInsertInboxElement = WcfSqlOperationMessages.GetWcfSqlInsertInbox(acasClientId, cw1SystemId, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", "UDM", inboxContent);

	        var response = WcfSqlOperationMessages.CombineWcfSqlOps(wcfSqlInsertInboxElement, wcfSqlUpdateAsyncPollingRegistration);
	        return response;
        }

        public static XmlDocument GetWcfDeleteAsyncPollingRegistration(string pollingRegistrationPk)
        {
	        return WcfSqlOperationMessages.CombineWcfSqlOps(GetWcfXElementSqlDeleteAsyncPollingRegistration(pollingRegistrationPk));
        }

        static XElement GetWcfXElementSqlDeleteAsyncPollingRegistration(string pollingRegistrationPk)
        {
	        XNamespace nsEHubAsyncPollingRegistration =
		        "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubAsyncPollingRegistration";
	        XNamespace nsSqlTableOps = "http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo";

	        var wcfSqlUpdateAsyncPollingRegistration =
		        new XElement(nsEHubAsyncPollingRegistration + "Delete",
			        new XElement(nsEHubAsyncPollingRegistration + "Rows",
				        new XElement(nsSqlTableOps + "eHubAsyncPollingRegistration",
					        new XElement(nsSqlTableOps + "PR_PK", pollingRegistrationPk)
				        )
			        )
		        );
	        return wcfSqlUpdateAsyncPollingRegistration;
        }

        public static string GetResponseSenderID(string acasID)
        {
	        return acasID.EndsWith("_TST") ? "ACAS_BRTest" : "ACAS_BR";
        }

        public static XmlDocument GetXmlResponse(string cctResponse)
        {
            var schema = new CheckStatusSchema();
            var rootName = schema.RootNodes[0];
            var nspace = schema.Schema.TargetNamespace;

			if (cctResponse.TrimStart().StartsWith("{") && cctResponse.TrimEnd().EndsWith("}")) cctResponse = $"[{cctResponse}]";

			var nodes = JsonConvert.DeserializeObject<List<JObject>>(cctResponse, new JsonSerializerSettings
			{
				DateParseHandling = DateParseHandling.None
			});
			
			var doc = new XmlDocument();
            var root = doc.CreateElement(rootName);
            doc.AppendChild(root);
            foreach (var node in nodes)
	        {
				var childDoc = DeserializeXmlNode(node.ToString(Newtonsoft.Json.Formatting.None), DateParseHandling.None, rootName);

                var childNode = doc.ImportNode(childDoc.DocumentElement, true);
                doc.DocumentElement.AppendChild(childNode);
	        }

            doc.DocumentElement.SetAttribute("xmlns", nspace);

            return doc;
        }

		public static string GetValueFromJson(string cctResponseString, string key)
        {
	        var jsonCCTResponse = JObject.Parse(cctResponseString);
	        var value = jsonCCTResponse[$"{key}"].ToString();

	        return value;
        }

        private const string ErrorDescriptionText = "Error Description:";
        public static string GetViewableErrorDescription(string error)
        {
			var errorDescriptionIndex = error.IndexOf(ErrorDescriptionText, StringComparison.InvariantCulture);
	        if (errorDescriptionIndex > 0)
				return error.Substring(errorDescriptionIndex + ErrorDescriptionText.Length).Trim();
	        return error;
        }

		public static string GetValueFromErrorResponseMsg(string responseMsg, string key)
		{
			string value = string.Empty;
			if (responseMsg.StartsWith("{"))
			{
				value = GetValueFromJson(responseMsg, key);
			}
			else
			{
				value = GetValueFromXmlString(responseMsg, $"//*[local-name()='error']/*[local-name()='{key}']/text()");
			}

			if (string.IsNullOrEmpty(value))
			{
				throw new InvalidOperationException($"error/{key} is invalid.");
			}

			return value;
		}

		public static bool IsNonXmlResponse(string cctResponseString)
		{
			try
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(cctResponseString);
				return false;
			}
			catch (XmlException)
			{
				return true;
			}
		}

		private static string GetValueFromXmlString(string xmlString, string xPath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlString);
			var node = doc.SelectSingleNode(xPath);

			return (node == null) ? string.Empty : node.Value;
		}

		private static XmlDocument DeserializeXmlNode(string json, DateParseHandling dateParseHandling, string deserializeRootElementName)
		{
			var settings = new JsonSerializerSettings
			{
				Converters =
				{
					new Newtonsoft.Json.Converters.XmlNodeConverter()
					{
						DeserializeRootElementName = deserializeRootElementName
					}
				},
				DateParseHandling = dateParseHandling,
			};

			return JsonConvert.DeserializeObject<XmlDocument>(json, settings);
		}

		internal static Func<eHubTransactionsContext> InternalContextFactory = () => new eHubTransactionsContext();
        internal static Func<Guid> InternalNewGuid = Guid.NewGuid;
    }
}
