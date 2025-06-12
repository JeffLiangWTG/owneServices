using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.AirMessaging.Tests;
using CargoWise.eHub.Products.AirMessaging.Schemas;

namespace CargoWise.eHub.Products.AirMessaging.CCSJ.Tests
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class CCNSchemaTests : BaseSchemaTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CCNReplyMessageSchema_FSU()
		{
		    var xml = new XmlDocument();
            xml.Load(GetEmbeddedResource(@"CCN.TestFiles.CCNFSU.xml")); 
			xml.Schemas.Add(XmlSchema.Read(new StringReader(new CCNReplyMessage().XmlContent), null));
            xml.Validate(null);
		}
	}
}
