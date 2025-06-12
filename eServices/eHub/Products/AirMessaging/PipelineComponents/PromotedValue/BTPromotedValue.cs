using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.XPath;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	class BTPromotedValue : PromotedValue
	{
		public BTPromotedValue()
		{
			Add("SenderPIMA", "/*[local-name()='BT' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.BTReplyMessage']/*[local-name()='Message' and namespace-uri()='']/*[local-name()='Header' and namespace-uri()='']/*[local-name()='Line2' and namespace-uri()='']/*[local-name()='SenderPIMA' and namespace-uri()='']");
			Add("RecipientPIMA", "/*[local-name()='BT' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.BTReplyMessage']/*[local-name()='Message' and namespace-uri()='']/*[local-name()='Header' and namespace-uri()='']/*[local-name()='Line1' and namespace-uri()='']/*[local-name()='RecipientPIMA' and namespace-uri()='']");
			Add("AirlinePIMA", "/*[local-name()='BT' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.BTReplyMessage']/*[local-name()='Message' and namespace-uri()='']/*[local-name()='Header' and namespace-uri()='']/*[local-name()='Line2' and namespace-uri()='']/*[local-name()='AirlinePIMA' and namespace-uri()='']");
			Add("InternalMessage", "/*[local-name()='BT' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.BTReplyMessage']/*[local-name()='Message' and namespace-uri()='']/*[local-name()='BodyAndFooter' and namespace-uri()='']/*[local-name()='Body' and namespace-uri()='']");
		}
	}
}
