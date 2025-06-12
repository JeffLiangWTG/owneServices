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
	class CCNPromotedValue : PromotedValue
	{
		public CCNPromotedValue()
		{
			Add("SenderPIMA", "/*[local-name()='CargoIMP' and namespace-uri()='http://ccn.com/cargoimp/201109']/*[local-name()='SenderPIMA' and namespace-uri()='http://ccn.com/cargoimp/201109']");
			Add("RecipientPIMA", "/*[local-name()='CargoIMP' and namespace-uri()='http://ccn.com/cargoimp/201109']/*[local-name()='RecipientPIMA' and namespace-uri()='http://ccn.com/cargoimp/201109']");
			Add("InternalMessage", "/*[local-name()='CargoIMP' and namespace-uri()='http://ccn.com/cargoimp/201109']/*[local-name()='Body' and namespace-uri()='http://ccn.com/cargoimp/201109']");
		}
	}
}
