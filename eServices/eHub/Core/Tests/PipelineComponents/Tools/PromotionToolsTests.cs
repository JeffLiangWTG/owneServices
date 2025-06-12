using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Core.PipelineComponents.Tools;

namespace CargoWise.eHub.Core.Tests.PipelineComponents.Tools
{
	[TestClass]
	public class PromotionToolsTests : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCallMethod_SubstringFrom()
		{
			var message = MessageFactory.CreateMessage();
			message.Context.Write("OverrideFilename", "http://test", "C:\\Temp\\IPS_DelNoteInfo_802040876.xml");
			object result = PromotionTools.CallMethod(message, "MethodCall:SubstringFrom@http://test#OverrideFilename@IPS_");
			Assert.AreEqual("IPS_DelNoteInfo_802040876.xml", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCallMethod_SubstringFrom_NotFound()
		{
			var message = MessageFactory.CreateMessage();
			message.Context.Write("OverrideFilename", "http://test", "C:\\Temp\\IPS_DelNoteInfo_802040876.xml");
			object result = PromotionTools.CallMethod(message, "MethodCall:SubstringFrom@http://test#OverrideFilename@TST");
			Assert.AreEqual("C:\\Temp\\IPS_DelNoteInfo_802040876.xml", result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCallMethod_SubstringFrom_Exception()
		{
			var message = MessageFactory.CreateMessage();
			try
			{
				object result = PromotionTools.CallMethod(message, "MethodCall:SubstringFrom@1@2@3");
				Assert.Fail("FormatException should be thrown before");
			}
			catch (FormatException ex)
			{
				StringAssert.Contains(ex.Message, "Method call is not correct - Expected number of parameters 2 : (MethodCall:SubstringFrom@1@2@3)");
			}
		}
	}
}
