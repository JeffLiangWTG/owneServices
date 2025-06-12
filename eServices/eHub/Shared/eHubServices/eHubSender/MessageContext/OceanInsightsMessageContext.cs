using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

using CargoWise.eHub.Share.eHubServices.eHubSender.Extensions;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.MessageContext
{
	public class OceanInsightsMessageContext : CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext.MessageContext
	{
		public OceanInsightsMessageContext(string message, string eHubSenderId, string eHubRecipientId)
			: base(message, eHubSenderId, eHubRecipientId)
		{
		}

		public override void Load()
		{
			var xmlElement = XElement.Parse(Message);

			var eventTypeElement = xmlElement.XPathSelectElement("//*[local-name()='Event']/*[local-name()='EventType']");

			if (eventTypeElement == null || string.IsNullOrEmpty(eventTypeElement.Value) || eventTypeElement.Value.ToUpper() != "SBR")
			{
				throw new OceanInsighsSubscriptionValidationException("Message is not a subscription. Event Type should be 'SBR'.");
			}

			var referenceElement = xmlElement.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context']/*[local-name()='Type'][text() = 'Reference']/../*[local-name()='Value']");
			if (referenceElement != null && !string.IsNullOrWhiteSpace(referenceElement.Value))
			{
				AddContext("Reference", referenceElement.Value);
			}
			else
			{
				throw new OceanInsighsSubscriptionValidationException("Subscription doesn't have CSS Reference or CSS Reference is empty in ContextCollection. Subscription rejected.");
			}

			var subscriptionTypeElement = xmlElement.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context']/*[local-name()='Type'][text() = 'SubscriptionType']/../*[local-name()='Value']");
			if (subscriptionTypeElement != null && !string.IsNullOrWhiteSpace(subscriptionTypeElement.Value))
			{
				string[] subscriptionTypes = new[] { "CARRIERBOOKINGREFERENCE", "MASTERBILLNUMBER", "CONTAINERNUMBER" };
				if (subscriptionTypes.Contains(subscriptionTypeElement.Value.ToUpper()))
				{
					AddContext("SubscriptionType", subscriptionTypeElement.Value.ToUpper());
				}
				else
				{
					throw new OceanInsighsSubscriptionValidationException("SubscriptionType is not one of allowed values: CarrierBookingReference, MasterBillNumber, ContainerNumber. Subscription rejected.");
				}
			}
			else
			{
				throw new OceanInsighsSubscriptionValidationException("SubscriptionType is empty in ContextCollection. Subscription rejected.");
			}

			var carrierCodeElement = xmlElement.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context']/*[local-name()='Type'][text() = 'CarrierCode']/../*[local-name()='Value']");
			if (carrierCodeElement != null && !string.IsNullOrWhiteSpace(carrierCodeElement.Value))
			{
				AddContext("CarrierCode", carrierCodeElement.Value.ToUpper());
			}
			else
			{
				throw new OceanInsighsSubscriptionValidationException("Subscription doesn't have CarrierCode or CarrierCode is empty in ContextCollection. Subscription rejected.");
			}

			if (subscriptionTypeElement.Value.ToUpper() == "CARRIERBOOKINGREFERENCE")
			{
				var carriersBookingReference = xmlElement.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context']/*[local-name()='Type'][text() = 'CarriersBookingReference']/../*[local-name()='Value']");
				if (carriersBookingReference != null && !string.IsNullOrWhiteSpace(carriersBookingReference.Value))
				{
					AddContext("CarriersBookingReference", carriersBookingReference.Value.ToUpper());
				}
				else
				{
					throw new OceanInsighsSubscriptionValidationException("SubscriptionType = 'CarriersBookingReference',  CarriersBookingReference is empty in ContextCollection. Subscription rejected.");
				}
			}

			if (subscriptionTypeElement.Value.ToUpper() == "MASTERBILLNUMBER")
			{
				var masterBillNumber = xmlElement.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context']/*[local-name()='Type'][text() = 'MBOLNumber']/../*[local-name()='Value']");
				if (masterBillNumber != null && !string.IsNullOrWhiteSpace(masterBillNumber.Value))
				{
					AddContext("MasterBillNumber", masterBillNumber.Value.ToUpper());
				}
				else
				{
					throw new OceanInsighsSubscriptionValidationException("SubscriptionType = 'MasterBillNumber',  MasterBillNumber is empty in ContextCollection. Subscription rejected.");
				}
			}

			if (subscriptionTypeElement.Value.ToUpper() == "CONTAINERNUMBER")
			{
				var containerNumber = xmlElement.XPathSelectElement("//*[local-name()='ContextCollection']/*[local-name()='Context']/*[local-name()='Type'][text() = 'ContainerNumber']/../*[local-name()='Value']");
				if (containerNumber != null && !string.IsNullOrWhiteSpace(containerNumber.Value))
				{
					AddContext("ContainerNumber", containerNumber.Value.ToUpper());
				}
				else
				{
					throw new OceanInsighsSubscriptionValidationException("SubscriptionType = 'ContainerNumber',  ContainerNumber is empty in ContextCollection. Subscription rejected.");
				}
			}
		}
	}
}