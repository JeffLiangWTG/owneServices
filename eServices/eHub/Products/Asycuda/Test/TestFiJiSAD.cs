using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.Products.AsycudaCustoms.Transform.Uxml_2_AsySAD;
using CargoWise.BizTalk.UnitTestFX;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.AsycudaCustoms.Test
{
	[TestClass]
	public class TestFJSAD
	{
		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFijianSAD()
		{
			var sourceFile = "TestFiles.FijianInput.xml";
			var outputFile = "TestFiles.FijianOutput.xml";
			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<Uxml2AsySAD>(sourceFile, outputFile);
		}
	}
}
