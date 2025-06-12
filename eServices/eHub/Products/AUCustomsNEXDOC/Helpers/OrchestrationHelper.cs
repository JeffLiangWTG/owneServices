using System;
using System.Linq;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Common.Logging;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.AUCustomsNEXDOC.Helpers
{
	public class OrchestrationHelper
	{
		public static Subscription GetSubscription(string title, string clientId, string recipientId, ILog logger)
		{
			var rexNumber = GetRexNumber(title);

			var subscription = DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
			{
				using (var context = GetContext())
				{
					Guid clientPK;
					Guid recipientPK;
					string reference;
					var sub = context.eHubSubscriptionValues.FirstOrDefault(r => r.eHubSubscriptionType.ST_ID == SubscriptionTypeCode &&
																							r.SV_Value == rexNumber &&
																							r.eHubClient_Provider.CC_ID == recipientId);
					if (sub == null)
					{
						clientPK = context.eHubClients.FirstOrDefault(x => x.CC_ID == clientId).CC_PK;
						recipientPK = context.eHubClients.FirstOrDefault(x => x.CC_ID == recipientId).CC_PK;
						reference = "Unsolicited Notification";
					}
					else
					{
						clientPK = sub.eHubClient_Subscriber.CC_PK;
						recipientPK = sub.eHubClient_Provider.CC_PK;
						reference = sub.SV_Reference;
					}
					return new Subscription(recipientPK, clientPK, rexNumber, reference);
				}
			}, logger);
			return subscription;
		}

		public static string GetClientToken(string clientId, string registrationTypeCode, ILog logger)
		{
			var clientToken = DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
			{
				var systemId = clientId;
				if (systemId.Length == 9)
				{
					systemId = clientId.Substring(0, 3) + clientId.Substring(6);
				}
				using (var context = GetContext())
				{
					var token = context.eHubClientSystemRegistrations.FirstOrDefault(r => r.eHubRegistrationType.RT_ID == registrationTypeCode && r.eHubClientSystem.EH_ID == systemId && r.CD_Flag1 == 1);
					if (token == null)
						throw new InvalidOperationException("Unable to find system registration for Type: " + registrationTypeCode + "; Client: " + clientId + ", System: " + systemId);
					return token.CD_Code;
				}
			}, logger);
			return clientToken;
		}

		public static void InvalidateClientToken(string clientToken, ILog logger)
		{
			DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
			{
				using (var context = GetContext())
				{
					var clientReg = context.eHubClientSystemRegistrations.FirstOrDefault(r =>
						r.CD_Code == clientToken);

					if (clientReg == null)
						throw new InvalidOperationException("Unable to find system registration for ClientToken: " + clientToken);

					clientReg.CD_Flag1 = 0;
					context.SaveChanges();
				}
			}, logger);
		}

		public static string GetRexNumber(string title)
		{
			return Regex.Match(title, @"((REX)([A-Za-z0-9\-]+))(\s|)").Groups[1].ToString();
		}

		public static string GetEDNNumber(string text)
		{
			return Regex.Match(text, @"(EDN: )([A-Za-z0-9\-]+)").Groups[2].ToString();
		}

		public static string GetJobNumber(string title)
		{
			return Regex.Match(title, @"(Exporter Reference: ([A-Za-z0-9\-]+))").Groups[2].ToString();
		}

		public static string GetServiceType(string messageType)
		{
			switch (messageType)
			{
				case "OrderRex":
				case "LodgeRex":
				case "AmendRex":
				case "WithdrawalRex":
				case "CancelRex":
					return "rexsubmission";
				case "ReadCertificate":
					return "readCertificate";
				case "ReissueCertificate":
				case "ReplaceCertificate":
					return "modifyCertificate";
				case "TransferRexEDN":
				case "CancelRexEDN":
					return "customs";
				case "ReadRex":
					return "readrex";
				default:
					throw new InvalidOperationException($"Cannot identify NEXDOC service basing on message type {messageType}");
			}
		}

		public static string ExtractConsigneeInformation(XmlDocument responseMessage, string certificateConsignee = "")
		{
			var responseSupercertificates = responseMessage.SelectSingleNode("//*[local-name()='ReadCertificateResponse']/*[local-name()='superCertificates']");
			var consignee = responseSupercertificates.SelectSingleNode("//*[local-name()='consignee']");
			var consigneeAddress = consignee.SelectSingleNode("//*[local-name()='consigneeAddress']");

			certificateConsignee += CreateContextXML(consignee, "//*[local-name()='consigneeName']", "consigneeName");

			var rowCount = 0;
			foreach (XmlNode address in consigneeAddress.SelectNodes("//*[local-name()='streetAddress']/*[local-name()='streetLine']"))
			{
				rowCount++;
				certificateConsignee += CreateContextXML(address, ".", $"consigneeStreetAddress{rowCount}");
			}

			certificateConsignee += CreateContextXML(consigneeAddress, "//*[local-name()='city']", "consigneeCity");
			certificateConsignee += CreateContextXML(consigneeAddress, "//*[local-name()='state']", "consigneeState");
			certificateConsignee += CreateContextXML(consigneeAddress, "//*[local-name()='country']", "consigneeCountry");
			certificateConsignee += CreateContextXML(consigneeAddress, "//*[local-name()='postalCode']", "consigneePostalCode");
			certificateConsignee += CreateContextXML(consignee, "//*[local-name()='consigneePhoneNumber']", "consigneePhoneNumber");
			certificateConsignee += CreateContextXML(consignee, "//*[local-name()='consigneeRepresentative']", "consigneeRepresentative");
			certificateConsignee += CreateContextXML(responseSupercertificates, "//*[local-name()='departureDate']", "departureDate");

			return certificateConsignee;
		}

		public static string ExtractCertificatePayload(XmlDocument responseMessage, string copyType, string certificatePayload = "", string rexNumber = "")
		{
			var certificates = responseMessage.SelectSingleNode("//*[local-name()='ReadCertificateResponse']/*[local-name()='superCertificates']/*[local-name()='certificates']");
			var isPreview = copyType == "PREVIEW";
            var i = 0;

            foreach (XmlNode certificate in certificates.ChildNodes)
			{
                var certificateName = isPreview
                    ? $"{rexNumber}_{i + 1}"
                    : certificate.SelectNodes("//*[local-name()='certificateNumber']")?[i]?.InnerText ?? "";

                var certificateCode = isPreview
                    ? "QPP"
                    : "QRP";

                var certificateDescription = isPreview
                    ? "Quarantine Print Preview"
                    : "Quarantine Remote Print";

                certificatePayload += $@"
                <AttachedDocument>
                    <FileName>{certificateName}.pdf</FileName>
                    <Type>
                        <Code>{certificateCode}</Code>
                        <Description>{certificateDescription}</Description>
                    </Type>
                    <ImageData>{certificate.SelectNodes("//*[local-name()='certificatePayloads']/*[local-name()='certificatePayload' and @copyType='" + copyType + "']")?[i]?.InnerText ?? ""}</ImageData>
                    <IsPublished>{(!isPreview).ToString().ToLower()}</IsPublished>
                </AttachedDocument>";

                i++;
            }

            return certificatePayload;
		}

		public static string ExtractXmlValue(XmlDocument xmlDoc, string path)
		{

			return xmlDoc.SelectSingleNode(path)?.InnerText ?? "";
		}

		public static bool CheckExceptionForContent(Exception ex, string content)
		{
			return ex.Message.Contains(content)
				? true
				: ex.InnerException?.Message?.Contains(content) ?? false;
		}
		
		private static string CreateContextXML(XmlNode node, string path, string contextType)
		{
			return $@"
            <Context>
                <Type>{contextType}</Type>
                <Value>{System.Security.SecurityElement.Escape(node.SelectSingleNode(path)?.InnerText ?? "")}</Value>
            </Context>";
		}

		internal static Func<eHubTransactionsContext> GetContext = () => new eHubTransactionsContext();
		private static readonly string SubscriptionTypeCode = "NEXDOC";

		private static readonly List<string> ErrorFilterPatterns = new List<string>
		{
			"System.ServiceModel.CommunicationException: An error (The request was aborted: The request was canceled.) occurred while transmitting data over the HTTP channel"
		};

		public static bool ShouldFilterOutError(string errorMessage)
		{
			return (errorMessage != null) && ErrorFilterPatterns.Any(errorMessage.Contains);
		}

		[Serializable]
		public struct Subscription
		{
			public Subscription(Guid sender, Guid recipient, string value, string reference)
				: this()
			{
				Sender = sender.ToString();
				Recipient = recipient.ToString();
				Value = value;
				Reference = reference;
			}
			public string Sender { get; private set; }
			public string Recipient { get; private set; }
			public string Value { get; private set; }
			public string Reference { get; private set; }
		}
	}
}
