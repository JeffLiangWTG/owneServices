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
	class DeltaPromotedValue : PromotedValue
	{
		public DeltaPromotedValue()
		{
			Add("SenderPIMA", "/*[local-name()='Delta' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.DeltaReplyMessage']/*[local-name()='Header2' and namespace-uri()='']/*[local-name()='SenderPIMA' and namespace-uri()='']");
			Add("RecipientPIMA", "/*[local-name()='Delta' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.DeltaReplyMessage']/*[local-name()='Header1' and namespace-uri()='']/*[local-name()='RecipientPIMA' and namespace-uri()='']");
			Add("InternalMessage", "/*[local-name()='Delta' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.DeltaReplyMessage']/*[local-name()='Data' and namespace-uri()='']");
		}
	}
}
