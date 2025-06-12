using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT.Helpers
{
	public class OrchestrationHelper
	{
		public static Func<eHubTransactionsContext> InternalContextFactory = () => new eHubTransactionsContext();

		public static XmlDocument CreateRequestMessage(string username, string password, string checkpointValue)
		{
			var xml = new XmlDocument();
			if (string.IsNullOrEmpty(username))
			{
				throw new ApplicationException("Username is empty.");
			}

			if (string.IsNullOrEmpty(password))
			{
				throw new ApplicationException("password is empty.");
			}

			var checkpoint = string.IsNullOrEmpty(checkpointValue) ? @"<checkpoint xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:nil=""true""/>" : $"<checkpoint>{checkpointValue}</checkpoint>";

			var xmlString = $@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Header>
    <wsse:Security xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" mustUnderstand=""1""> 
      <wsse:UsernameToken xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" wsu:Id=""UsernameToken-1""> 
        <wsse:Username>{username}</wsse:Username> 
        <wsse:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">{password}</wsse:Password> 
      </wsse:UsernameToken> 
    </wsse:Security> 
  </SOAP-ENV:Header> 
  <SOAP-ENV:Body> 
    <por:checkpointAndRetrieveMessages xmlns:por=""http://portcommunity.haven.antwerpen.be/""> 
      {checkpoint} 
      <criteria> 
        <maxNumberOfMessages>20</maxNumberOfMessages> 
        <maxSize>9000000</maxSize> 
      </criteria> 
    </por:checkpointAndRetrieveMessages> 
  </SOAP-ENV:Body> 
</SOAP-ENV:Envelope>";

			xml.LoadXml(xmlString);
			return xml;
		}

		public static string GetXpathValue(string xmlString, string xpath)
		{
			var collector = new XPathValueCollector(xpath);
			return collector.Collect(xmlString).ToArray()[0].Item3;
		}

		public static XmlDocument GetWcfSqlUpdateCheckPoint(string messageType,string currentCheckpoint, string finalCheckpoint)
		{
			if (string.IsNullOrWhiteSpace(messageType))
				throw new ArgumentNullException("messageType");
			if (string.IsNullOrWhiteSpace(currentCheckpoint))
				throw new ArgumentNullException("currentCheckpoint");
			if (string.IsNullOrWhiteSpace(finalCheckpoint))
				throw new ArgumentNullException("finalCheckpoint");

			using (var context = InternalContextFactory())
			{
				var eHubCodeMapValues = context.eHubCodeMapValues.FirstOrDefault(r => r.eHubCodeMapKey.CK_PK == r.CV_CK && r.eHubCodeSetResult.CR_PK == r.CV_CR &&
				                                                                      r.eHubCodeMapKey.CK_Key1Value == messageType && r.CV_OutputCode == currentCheckpoint &&
				                                                                      r.eHubCodeSetResult.CR_Name == "Checkpoint");

				XNamespace nsEHubCodeMapValue = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubCodeMapValue";
				XNamespace nsSqlTableOps = "http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo";


				var responseXml = new XDocument(
					new XElement("CompositeOperation",
						new XElement(nsEHubCodeMapValue + "Update",
							new XElement(nsEHubCodeMapValue + "Rows",
								new XElement(nsEHubCodeMapValue + "RowPair",
									new XElement(nsEHubCodeMapValue + "After",
										new XElement(nsSqlTableOps + "CV_OutputCode", finalCheckpoint)
									),
									new XElement(nsEHubCodeMapValue + "Before",
										new XElement(nsSqlTableOps + "CV_CK", eHubCodeMapValues.CV_CK),
										new XElement(nsSqlTableOps + "CV_CR", eHubCodeMapValues.CV_CR)
									)
								)
							)
						)
					)
				);
				var response = new XmlDocument();
				using (var rdr = responseXml.CreateReader())
					response.Load(rdr);
				return response;
			}
		}

		public static string GetMoreMessagesValue(XmlDocument responseMessage)
		{
			var stringPath = $"/*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='checkpointAndRetrieveMessagesResponse']/*[local-name()='batch']/*[local-name()='moreMessages']";
			var moreMessages = GetXpathValue(responseMessage.InnerXml, stringPath);
			if (moreMessages == null) throw new ArgumentNullException(nameof(moreMessages));
			return moreMessages;
		}

		public static bool IsKnownError(string fault)
		{
			foreach (var error in KnownErrors)
			{
				if (fault.Contains(error))
					return true;
			}

			return false;
		}

		static string[] KnownErrors = new string[] 
		{
			"java.net.NoRouteToHostException: No route to host",
			"SC_INTERNAL_SERVER_ERROR"
		};
	}
}
