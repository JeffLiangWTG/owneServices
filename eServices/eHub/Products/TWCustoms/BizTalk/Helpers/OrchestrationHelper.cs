using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using eServices.Configuration.Framework;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.TWCustoms.Configuration;
using static System.String;

namespace CargoWise.eHub.Products.TWCustoms.Orchestrations.Helpers
{
	public static class OrchestrationHelper
	{

		internal static Func<ClientRegistrationAccessor> GetClientRegistrationAccessor =
			() => new ClientRegistrationAccessor();

		private const string RegistrationType = "TWCustomsAccount";
		private const string ForwarderAndLicensingRegistrationType = "TWCustomsForwarderAndLicensing";
		private const string UInterchangeNS = "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange";

		public static OrchestrationDictionary GetRegistration(string clientId, string qualifier, string code)
		{
			var registrationType = RegistrationType;
			var qualifierStr = qualifier + "|" + code;

			if (string.IsNullOrEmpty(qualifier))
			{
				registrationType = ForwarderAndLicensingRegistrationType;
				qualifierStr = code;
			}

			var registration = GetClientRegistrationAccessor()
				.ReadRegistrations(clientId, registrationType, qualifierStr).FirstOrDefault();

			if (registration == null)
				throw new FatalMessageProcessingException(
					string.Format("Could not find client registration with ClientID {0}, Qualifier {1}, Code {2}", clientId,
						qualifier, code));

			return new OrchestrationDictionary(registration);
		}

		public static string GetUserName(string configXml)
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(configXml);
			var userName = xmlDocument.SelectSingleNode("//*[local-name()='Credential']/*[local-name()='UserName']")?.InnerText;

			if (string.IsNullOrEmpty(userName))
			{
				throw new ConfigurationException("UserName is missing from Configuration message.");
			}
			return userName;
		}

		public static void UpdateStatusToInvalid(string clientId, string qualifier, string code, string errorDescription, int recAutoFlag)
		{
			new TWCustomsConfigurationHandler().UpdateConfigStatus(clientId, qualifier, code, "INV", errorDescription, recAutoFlag);
		}

		public static string GetConfigStatus(string clientId, string qualifier, string code)
		{
			return new TWCustomsConfigurationHandler().GetConfigStatus(clientId, qualifier, code);
		}

		public static string GetWcfInsertInboxOutboxAndUpdateRegistrationQueries(string senderID, string recipientID, string messageTrackingID, string messageBody, string cx_pk)
		{
			var inboxPK = Guid.NewGuid();
			var trackingGuid = Guid.Parse(messageTrackingID);
			return WcfSqlOperationMessages.CombineWcfSqlOps(
					WcfSqlOperationMessages.GetWcfSqlInsertInbox(senderID, recipientID, UInterchangeNS, "UDM", messageBody, trackingGuid, inboxPK),
					WcfSqlOperationMessages.GetWcfSqlInsertOutbox(senderID, recipientID, UInterchangeNS, messageBody, trackingGuid),
					FormatRegistrationTypeUpdate(cx_pk, 0)
				).OuterXml;
		}

		public static string GetWcfInsertInboxOutboxQueries(string senderID, string recipientID, string messageTrackingID, string messageBody)
		{
			var messageXml = new XmlDocument();
			messageXml.LoadXml(messageBody);

			var messageCode = GetMessageCode(messageXml);
			var wrapperNS = "http://cargowise.com/ehub/products/TWCustoms#TWCustomsResponse";
			switch (messageCode)
			{
				case "N5108":
					wrapperNS = "http://cargowise.com/ehub/products/TWCustomsForwarderManifest#TWCustomsForwarderManifest";
					break;
				case "NX903":
					wrapperNS = "http://cargowise.com/ehub/products/TWCustomsLicensing#TWCustomsLicensing";
					break;
			}

			var inboxPK = Guid.NewGuid();
			var trackingGuid = Guid.Parse(messageTrackingID);
			return WcfSqlOperationMessages.CombineWcfSqlOps(
				WcfSqlOperationMessages.GetWcfSqlInsertInbox(senderID, recipientID, wrapperNS, "TWC", messageBody, trackingGuid, inboxPK),
				WcfSqlOperationMessages.GetWcfSqlInsertOutbox(senderID, recipientID, wrapperNS, messageBody, inboxPK, trackingGuid)
			).OuterXml;
		}

		public static string GetMessageCode(XmlDocument responseMessage)
		{
			var responseNode = responseMessage.SelectSingleNode("/*[local-name()='Response']");
			if (responseNode == null) return Empty;
			var namespaceUri = responseNode.NamespaceURI;

			// 21 is the length of 'urn:wco:datamodel:TW:'
			var messageCode = namespaceUri.Substring(21, 5);
			return messageCode;
		}

		private static XElement FormatRegistrationTypeUpdate(string cx_pk, int cx_flag1_value)
		{
			XNamespace nsEHubRegistrationType = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubClientRegistration";
			XNamespace nsSqlTableOps = "http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo";

			return new XElement(nsEHubRegistrationType + "Update",
				new XElement(nsEHubRegistrationType + "Rows",
					new XElement(nsEHubRegistrationType + "RowPair",
						new XElement(nsEHubRegistrationType + "After",
							new XElement(nsSqlTableOps + "CX_Flag1", cx_flag1_value)
						),
						new XElement(nsEHubRegistrationType + "Before",
							new XElement(nsSqlTableOps + "CX_PK", cx_pk)
						)
					)
				)
			);
		}
	}
}
