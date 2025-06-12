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
	public class TestAsySAD
	{
		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUxml2AsySAD()
		{
			string sourceFile = "TestFiles.AsycudaDeclaration_Sea.xml";
			string outputFile = "TestFiles.AsySADOutput_Sea.xml";
			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<Uxml2AsySAD>(sourceFile, outputFile);

            sourceFile = "TestFiles.AsycudaDeclaration_Air.xml";
            outputFile = "TestFiles.AsySADOutput_Air.xml";
            mapTester = new MapTester(Assembly.GetExecutingAssembly());
            mapTester.ExecuteCompiled<Uxml2AsySAD>(sourceFile, outputFile);
		}
	}
}
