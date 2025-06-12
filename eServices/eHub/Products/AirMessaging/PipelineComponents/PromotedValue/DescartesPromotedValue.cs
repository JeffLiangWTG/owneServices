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
	class DescartesPromotedValue : PromotedValue
	{
		public DescartesPromotedValue()
		{
			Add("InternalMessage", "/*[local-name()='CargoIMP']");
		}
	}
}
