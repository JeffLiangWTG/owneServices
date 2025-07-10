using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public class CredentialChangeRequestResponseProvider : IRequestResponseProvider<CredentialChangesRequest, UniversalEventWrapper>
	{
		public CredentialChangesRequest GetRequestMessage(EDIInterchange outgoingInterchange)
		{
			using var reader = new StringReader(outgoingInterchange.EI_BodyText);
			var serializer = new XmlSerializer(typeof(CredentialChanges));
			var xmlObject = serializer.Deserialize(reader) as CredentialChanges;
			return new CredentialChangesRequest(xmlObject?.CredentialChange);
		}

		public UniversalEventWrapper ParseResponseMessage(EDIMessage message)
		{
			var xDocument = XDocument.Parse(message.EM_MessageText);
			var universalEventElement = xDocument.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals(Constant.UniversalEvent, StringComparison.InvariantCultureIgnoreCase));
			return new UniversalEventWrapper(universalEventElement.ToString());
		}

		public object GetLinkedObjectFromRequest(EDIInterchange outgoingInterchange)
		{
			return null;
		}
	}

	public class CredentialChangesRequest : ICFGMessageRequest
	{
		public CredentialChangesRequest(CredentialChangesCredentialChange request)
		{
			Request = request;
		}
		public CredentialChangesCredentialChange Request;
	}
}
