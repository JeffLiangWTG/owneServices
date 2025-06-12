using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.Products.AsycudaCustoms.Transform.Uxml_2_AsySAD;
using CargoWise.BizTalk.UnitTestFX;
using System.Reflection;

namespace CargoWise.eHub.Products.AsycudaCustoms.Test
{
	[TestClass]
	public class TestVUSAD
	{
		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestVanuatuSAD()
		{
			var sourceFile = "TestFiles.VanuatuInput.xml";
			var outputFile = "TestFiles.VanuatuOutput.xml";
			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<Uxml2AsySAD>(sourceFile, outputFile);
		}
	}
}
