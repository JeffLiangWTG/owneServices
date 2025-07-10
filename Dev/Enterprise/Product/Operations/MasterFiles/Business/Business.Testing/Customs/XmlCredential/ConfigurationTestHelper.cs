using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing
{
	public static class ConfigurationTestHelper
	{
		public static IEDIInterchange GetLatestEHubConfigurationInterchange(this BusinessObjectFactory factory, string interchangeType = EDIInterchangeTypeList.Codes.Configuration)
			=> GetQueuedEHubConfigurationInterchangeMessages(factory, interchangeType).FirstOrDefault();

		public static IEDIInterchange GetLatestDxTConfigurationInterchange(this BusinessObjectFactory factory, string interchangeType = EDIInterchangeTypeList.Codes.Configuration)
			=> GetQueuedDxTConfigurationInterchangeMessages(factory, interchangeType).FirstOrDefault();

		public static IReadOnlyCollection<IEDIInterchange> GetQueuedEHubConfigurationInterchangeMessages(this BusinessObjectFactory factory, string interchangeType = EDIInterchangeTypeList.Codes.Configuration)
			=> GetQueuedConfigurationInterchangeMessagesCore(factory, interchangeType, CredentialRecipient.eHub);

		public static IReadOnlyCollection<IEDIInterchange> GetQueuedDxTConfigurationInterchangeMessages(this BusinessObjectFactory factory, string interchangeType = EDIInterchangeTypeList.Codes.Configuration)
			=> GetQueuedConfigurationInterchangeMessagesCore(factory, interchangeType, CredentialRecipient.DirectxT);

		static IReadOnlyCollection<IEDIInterchange> GetQueuedConfigurationInterchangeMessagesCore(this BusinessObjectFactory factory, string interchangeType = EDIInterchangeTypeList.Codes.Configuration, CredentialRecipient credentialRecipient = CredentialRecipient.eHub)
		{
			var isDirectxTRecipient = credentialRecipient == CredentialRecipient.DirectxT;

			var query = new ZDBOnlyQuery(typeof(IEDIInterchange));
			query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, isDirectxTRecipient ? ApplicationCodeList.Codes.XHCredentialConfig : ApplicationCodeList.Codes.eHub);
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, interchangeType);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_From, ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier);
			query.AddToFilter(EDIInterchangeSchema.EI_To, isDirectxTRecipient ? Constants.Configuration.XHRecipient : Constants.Configuration.EHubRecipient);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, isDirectxTRecipient ? EDIInterchangeStatusList.Codes.Queued : EDIInterchangeStatusList.Codes.eHubQueued);
			query.AddToFilter(EDIInterchangeSchema.EI_TransportType, isDirectxTRecipient ? EDIInterchangeTransportTypeList.Codes.xT : EDIInterchangeTransportTypeList.Codes.eHub);
			query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			return factory.Load<IEDIInterchange>(query).ToArray();
		}
	}
}
