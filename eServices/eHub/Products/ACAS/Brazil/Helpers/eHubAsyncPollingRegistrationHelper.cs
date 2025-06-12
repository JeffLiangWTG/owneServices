using System;
using System.Linq;
using System.Xml;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Products.ACAS.BR.Helpers
{
	public class eHubAsyncPollingRegistrationHelper
	{
		private const string AsyncPollingRegistrationType = "ACAS_BRProtocol";
		public static string InsertEHubAsyncPollingRegistration(string clientID,
			string recipientID, string staffID, string cnpj, string contextKey, string contextType,
			string referenceValue, string ushipNamespace, string processingUTCTime)
		{
			using (var context = InternalContextFactory())
			{
				var registration = context.eHubRegistrationTypes.SingleOrDefault(x => x.RT_ID == AsyncPollingRegistrationType);
				MandatoryField(registration, "eHubRegistrationType", AsyncPollingRegistrationType);
				var client = context.eHubClients.SingleOrDefault(x => x.CC_ID == clientID);
				MandatoryField(client, "eHubClient", clientID);
				var clientSystemID = clientID.Substring(0, 3) + clientID.Substring(6, 3);
				var clientSystem = context.eHubClientSystems.SingleOrDefault(x => x.EH_ID == clientSystemID);
				MandatoryField(staffID, "staffID");
				MandatoryField(recipientID, "recipientID");
				MandatoryField(cnpj, "cnpj");
				MandatoryField(contextKey, "contextKey");
				MandatoryField(contextType, "contextType");
				MandatoryField(referenceValue, "referenceValue");
				MandatoryField(ushipNamespace, "ushipNamespace");
				MandatoryField(processingUTCTime, "processingUTCTime");
				var utc = DateTime.Parse(processingUTCTime);
				TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Central Brazilian Standard Time");
				var brazillianTime = TimeZoneInfo.ConvertTimeFromUtc(utc, tzi).ToString("yyyy-MM-dd");

				var newRecord = new eHubAsyncPollingRegistration()
				{
					PR_PK = InternalGuidFactory(),
					eHubClient = client,
					eHubClientSystem = clientSystem,
					eHubRegistrationType = registration,
					PR_Text = staffID
				};
				newRecord.PR_XML = string.Format(@"<Config xmlns=""http://wisetechglobal.com/ehub/acas/br/AsyncPolling"">
	<PK>{0}</PK>
	<CNPJ>{1}</CNPJ>
	<ProtocolNumber></ProtocolNumber>
	<BrazilianDataProcessing>{2}</BrazilianDataProcessing>
	<ContextKey>{3}</ContextKey>
	<ContextType>{4}</ContextType>
	<ReferenceValue>{5}</ReferenceValue>
	<SenderID>{6}</SenderID>
	<RecipientID>{7}</RecipientID>
	<StaffID><![CDATA[{8}]]></StaffID>
	<State></State>
	<ExpiredDateUTC></ExpiredDateUTC>
	<UShipmentNamespace>{9}</UShipmentNamespace>
</Config>", newRecord.PR_PK, cnpj, brazillianTime, contextKey, contextType, referenceValue, clientID, recipientID,
					 staffID,
					ushipNamespace);
				context.eHubAsyncPollingRegistrations.Add(newRecord);
				context.SaveChanges();
				return newRecord.PR_PK.ToString(); // Promote to EmailSubject
			}
		}

		public static string GetAsyncPollingConfig(XmlDocument doc, string nodeName)
		{
			var xmlNode =
				doc.SelectSingleNode(string.Format("/*[local-name()='Config']/*[local-name()='{0}']", nodeName));

			if (xmlNode == null || String.IsNullOrEmpty(xmlNode.InnerText))
			{
				throw new ApplicationException(string.Format("{0} is missing from Config. \r\n{1}", nodeName,
					doc.OuterXml));
			}

			return xmlNode.InnerText;
		}

		public static void SetAsyncPollingConfig(XmlDocument doc, string nodeName, string value)
		{
			var xmlNode =
				doc.SelectSingleNode(string.Format("/*[local-name()='Config']/*[local-name()='{0}']", nodeName));

			if (xmlNode == null)
			{
				throw new ApplicationException(string.Format("{0} is missing from Config. \r\n{1}", nodeName,
					doc.OuterXml));
			}

			xmlNode.InnerText = value;
		}

		private static void MandatoryField(object dbRecord, string tableName, string fieldValue)
		{
			if (dbRecord == null)
				throw new InvalidOperationException(
					string.Format("{0} is mandatory but cannot find matching value with '{1}'.", tableName,
						fieldValue));
		}

		private static void MandatoryField(string record, string argumentName)
		{
			if (string.IsNullOrWhiteSpace(record))
				throw new InvalidOperationException(string.Format("{0} is mandatory but it is null or empty.",
					argumentName));
		}

		internal static Func<eHubTransactionsContext> InternalContextFactory = () => new eHubTransactionsContext();

		internal static  Func<Guid> InternalGuidFactory = () => Guid.NewGuid();
	}
}
