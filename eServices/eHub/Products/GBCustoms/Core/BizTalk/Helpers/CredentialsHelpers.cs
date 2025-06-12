using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using eServices.Configuration.Schemas;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core.ConfigurationHandler;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers
{
	public class CredentialsHelpers
	{
		internal static Func<eHubTransactionsContext> ContextFactory = () => new eHubTransactionsContext();
		internal static Func<Guid> GuidFactory = Guid.NewGuid;

		public static string GetAccessToken(string key)
		{
			using (var context = ContextFactory())
			{
				return context.eHubClientSystemRegistrations
						   .Where(r => r.eHubRegistrationType.RT_ID == "GBCustomsAccessToken"
									   && r.eHubClientSystem.EH_ID + "." + r.CD_Code == key
									   && (r.CD_Flag1 == 0 || r.CD_Flag1 == 3))
						   .Select(r => r.CD_Attr1).FirstOrDefault() ?? string.Empty;
			}
		}

		public static string GetAccessToken(string clientId, string provider)
		{
			if (clientId.Length != 9)
			{
				return string.Empty;
			}

			if (string.IsNullOrEmpty(provider))
			{
				provider = "  ";
			}

			var clientSystem = $"{clientId.Substring(0, 3)}{clientId.Substring(6, 3)}";

			List<eHubClientSystemRegistration> registrations;
			using (var context = ContextFactory())
			{
				registrations = context.eHubClientSystemRegistrations
						   .Where(r => r.eHubRegistrationType.RT_ID == "GBCustomsAccessToken"
									   && r.eHubClientSystem.EH_ID == clientSystem
									   && (r.CD_Flag1 == 0)
							)
						   .ToList();
			}

			var tokenRegistration = registrations
				.Where(x => x.CD_Code.Split('.')[1].PadRight(3).Substring(0, 2).ToUpper() == provider.Substring(0, 2).ToUpper())
				.FirstOrDefault();

			if(tokenRegistration == null)
			{
				tokenRegistration = registrations.FirstOrDefault();
			}

			return tokenRegistration?.CD_Attr1 ?? string.Empty;
		}

		public static string GetEoriBadge(string key)
		{
			return key.Substring(key.IndexOf('.') + 1);
		}

		public static void RequestAccessTokenRefresh(string key, string oldToken)
		{
			using (var context = ContextFactory())
			{
				var token = context.eHubClientSystemRegistrations
					.FirstOrDefault(r => r.eHubRegistrationType.RT_ID == "GBCustomsAccessToken"
										 && r.eHubClientSystem.EH_ID + "." + r.CD_Code == key
										 && (r.CD_Flag1 == 0 || r.CD_Flag1 == 3)
										 && r.CD_Attr1 == oldToken);
				if (token == null)
					return;
				token.CD_Flag1 = 1;
				context.SaveChanges();
			}
		}

		public static string GetSqlXmlWhenAccessTokenIsFailedToBeObtained(string key, string error, string cdPk, string cdRv, string configSenderPk, string configRecipientPk, string configMsgTypePk)
		{
            var configMessage = new GBCustomsConfigurationHandler().GetConfigurationMessageWhenAccessTokenIsFailedToBeObtained(key, error);
			var configStream = ConfigurationMessage.SerializeToStream(configMessage);
			var configXml = configStream.ReadToEnd();
			var configCompressed = configStream.CompressAndEncode().ReadToEnd();

			var eiPk = GuidFactory();
			var oiPk = GuidFactory();
			XNamespace nsEHubInboxMessage = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage";
			XNamespace nsEHubInboxXmlContent = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxXmlContent";
			XNamespace nsEHubOutboxMessage = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubOutboxMessage";
			XNamespace nsEHubClientSystemRegistration = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubClientSystemRegistration";
			XNamespace nsSqlTableOps = "http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo";

			var responseXml = new XDocument(
				new XElement("CompositeOperation",
					new XElement(nsEHubInboxMessage + "Insert",
						new XElement(nsEHubInboxMessage + "Rows",
							new XElement(nsSqlTableOps + "eHubInboxMessage",
								new XElement(nsSqlTableOps + "EI_PK", eiPk),
								new XElement(nsSqlTableOps + "EI_MessageTrackingID", eiPk),
								new XElement(nsSqlTableOps + "EI_EnvelopeTrackingID", string.Empty),
								new XElement(nsSqlTableOps + "EI_CC_Sender", configSenderPk),
								new XElement(nsSqlTableOps + "EI_CC_Recipient", configRecipientPk),
								new XElement(nsSqlTableOps + "EI_MessageType",
									"http://www.wisetechglobal.com/Schemas/Configuration#Configuration"),
								new XElement(nsSqlTableOps + "EI_IsFlatFile", false),
								new XElement(nsSqlTableOps + "EI_ApplicationCode", "HUB"),
								new XElement(nsSqlTableOps + "EI_Status", 2),
								new XElement(nsSqlTableOps + "EI_Content", configCompressed)
							)
						)
					),
					new XElement(nsEHubInboxXmlContent + "Insert",
						new XElement(nsEHubInboxXmlContent + "Rows",
							new XElement(nsSqlTableOps + "eHubInboxXmlContent",
								new XElement(nsSqlTableOps + "EX_EI_Inbox", eiPk),
								new XElement(nsSqlTableOps + "EX_XmlContent", configXml),
								new XElement(nsSqlTableOps + "EX_DT_Source", configMsgTypePk),
								new XElement(nsSqlTableOps + "EX_UncompressedLength", configStream.Length)
							)
						)
					),
					new XElement(nsEHubOutboxMessage + "Insert",
						new XElement(nsEHubOutboxMessage + "Rows",
							new XElement(nsSqlTableOps + "eHubOutboxMessage",
								new XElement(nsSqlTableOps + "OI_PK", oiPk),
								new XElement(nsSqlTableOps + "OI_MessageTrackingID", eiPk),
								new XElement(nsSqlTableOps + "OI_EI_InboxPK", eiPk),
								new XElement(nsSqlTableOps + "OI_CC_Sender", configSenderPk),
								new XElement(nsSqlTableOps + "OI_CC_Recipient", configRecipientPk),
								new XElement(nsSqlTableOps + "OI_DT_Target", configMsgTypePk),
								new XElement(nsSqlTableOps + "OI_Status", 0),
								new XElement(nsSqlTableOps + "OI_Content", configCompressed),
								new XElement(nsSqlTableOps + "OI_XmlContent", configXml)
							)
						)
					),
					new XElement(nsEHubClientSystemRegistration + "Update",
						new XElement(nsEHubClientSystemRegistration + "Rows",
							new XElement(nsEHubClientSystemRegistration + "RowPair",
								new XElement(nsEHubClientSystemRegistration + "After",
                                    new XElement(nsSqlTableOps + "CD_Flag1", 255)
								),
								new XElement(nsEHubClientSystemRegistration + "Before",
                                    new XElement(nsSqlTableOps + "CD_PK", cdPk),
                                    new XElement(nsSqlTableOps + "CD_RV", cdRv)
								)
							)
						)
					)
				)
			);

			return responseXml.ToString();
		}
	}
}
