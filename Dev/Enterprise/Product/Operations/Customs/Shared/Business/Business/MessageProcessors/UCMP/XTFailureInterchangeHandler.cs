using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public static class XTFailureInterchangeHandler
	{
		public static IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIMessage outgoingMessage)
		{
			(bool isValid, string universalEvent) = IsValidUniversalEvent(interchange.EI_BodyText);
			return isValid
					? new EDIInterchangeUnpackerResult(new[] { EDIInterchangeUnPackerUtils.CreateReceivedEDIMessage(interchange, universalEvent) })
					: new EDIInterchangeUnpackerResult((NoResString)"The xT Failure interchange does not contain a valid Universal XML Event in the body text.");
		}

		public static (bool isValid, string universalEvent) IsValidUniversalEvent(ZString interchangePayload)
		{
			try
			{
				var xDocument = XDocument.Parse(interchangePayload);
				var universalEventElement = xDocument.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals(Constant.UniversalEvent, StringComparison.InvariantCultureIgnoreCase));
				return (universalEventElement != null, universalEventElement?.ToString());
			}
			catch
			{
				return (false, null);
			}
		}
	}
}
