using System;
using System.Configuration;
using System.ServiceModel;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider;
using CargoWise.eHub.Share.eHubServices.eHubSender.MessageContext;
using CargoWise.eHub.Share.eHubServices.eHubSender.ReplyMessageBuilder;
using Common.Logging;
using System.Net;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.ServiceProvider
{
	[SupportedRecipient("USDIS")]
	public class USDISServiceProvider : CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider.ServiceProvider
	{
		const string TestRecipientID = "USDISTest";
		const string RegistryApplicationCode = "USD";
		const string RegistryEntryName = "Entry Filer Code";
		readonly ILog logger;

		public USDISServiceProvider(ILog logger, string senderId, string recipientId, string message)
			: base(logger, senderId, recipientId, message)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
		}

		protected override CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext.MessageContext CeateMessageContext(string senderId, string recipientId, string message)
		{
			var eHubRegistryAccessor = DataAccess.Integration.DataAccessFactories.NewRegistryAccessorInstance();
			var isProd = eHubRegistryAccessor.SelectRegistryIsProd(senderId, RegistryApplicationCode, RegistryEntryName);
			var recipientID = isProd?recipientId:TestRecipientID;
			logger.InfoFormat("Route {0} for {1}", senderId, recipientID);
			return new USDISMessageContext(message, senderId, recipientID);
		}

		protected override ServiceReply CallService()
		{
            //Remove SecurityProcol settings when breaking up USDIS : WI00158924 
            //Replace with config settings
            var currentSecurityProtocol = ServicePointManager.SecurityProtocol;
            var TLSVersionForCurrentRecipient = Context.eHubRecipientId == TestRecipientID ? 
                ConfigurationManager.AppSettings["TLSVersionForUSDISCertification"] : 
                ConfigurationManager.AppSettings["TLSVersionForUSDISProduction"];
            if (!string.IsNullOrWhiteSpace(TLSVersionForCurrentRecipient))
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)Convert.ToInt32(ConfigurationManager.AppSettings[TLSVersionForCurrentRecipient]);
            }
            try
            {
                var client = new USDISSubmit.DocumentSubmissionServicePortTypeClient();
                client.ClientCredentials.UserName.UserName = ConfigurationManager.AppSettings["USDIS_UserName"];
                client.ClientCredentials.UserName.Password = ConfigurationManager.AppSettings["USDIS_Password"];
                client.Endpoint.Address = GetEndpointAddress();
                string reply = client.SubmitDocument("", Context.Message);
                return new ServiceReply(ServiceReply.Action.Success, reply);
            }
            finally
            {
                ServicePointManager.SecurityProtocol = currentSecurityProtocol;
            }
		}

		protected override CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder.ReplyMessageBuilder CreateReplyMessageBuilder(ServiceReply reply)
		{
			return new USDISReplyMessageBuilder(Context, reply);
		}

		protected override void SendReplyToBiztalk(string replyText)
		{
			if(!string.IsNullOrEmpty(replyText)) throw new FaultException(replyText);
		}

		protected override string ModifyMessage(string text)
		{
			var xmlElement = XElement.Parse(text);
			var transmitterID = xmlElement.XPathSelectElement("//*[local-name()='MessageHeader']/*[local-name()='TransmitterID']");
			if (transmitterID == null) throw new ArgumentException("Invalid Xml. Can't find field TransmitterID in MessageHeader.");
			transmitterID.Value = ConfigurationManager.AppSettings["USDIS_CargoWiseTransmitterID"];

			var transmitterSiteCode = xmlElement.XPathSelectElement("//*[local-name()='MessageHeader']/*[local-name()='TransmitterSiteCode']");
			if (transmitterSiteCode == null) throw new ArgumentException("Invalid Xml. Can't find field TransmitterSiteCode in MessageHeader.");
			transmitterSiteCode.Value = ConfigurationManager.AppSettings["USDIS_CargoWiseTransmitterSiteCode"];

			return xmlElement.ToString();
		}

		protected override System.Collections.Specialized.NameValueCollection GetAppSettings()
		{
			return ConfigurationManager.AppSettings;
		}
	}
}