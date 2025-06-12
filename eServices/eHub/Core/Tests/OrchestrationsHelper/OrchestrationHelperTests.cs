using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.eHub.Core.Orchestrations.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests.OrchestrationsHelper
{
	[TestClass]
	public class OrchestrationHelperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void RemoveXmlDeclaration()
		{
			var xmlStringWithDeclaration = @"<?xml version='1.0' encoding='utf-8'?><SystemInterchange />";
			var xmlStringWithDeclarationRemoved = CargoWise.eHub.Core.Orchestrations.Helper.OrchestrationHelper.RemoveXmlDeclaration(xmlStringWithDeclaration);
			Assert.AreEqual("<SystemInterchange />", xmlStringWithDeclarationRemoved);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRemoveInvalidCharInXml()
		{
			var errorDescription = "Reason: '', hexadecimal value 0x04, is an invalid character.";
			var expectedString = "Reason: '', hexadecimal value 0x04, is an invalid character.";
			var processedString = OrchestrationHelper.RemoveInvalidCharactersFromXmlContent(errorDescription);
			Assert.AreEqual(expectedString, processedString);
		}

		[TestMethod]
		public void TestReplaceXmlReservedCharacters()
		{
			var errorDescription = "<&>";
			var expectedString = "&lt;&amp;&gt;";
			var processedString = OrchestrationHelper.RemoveInvalidCharactersFromXmlContent(errorDescription);
			Assert.AreEqual(expectedString, processedString);
		}
	}
}
