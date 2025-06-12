using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.AsycudaCustoms.Transform.Uxml_2_Asycuda_ESAD;
using CargoWise.BizTalk.UnitTestFX;
using System.Reflection;

namespace CargoWise.eHub.Products.AsycudaCustoms.Test
{
	[TestClass]
	public class TestAsycudaESAD
	{
		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUxml2AsycudaESAD()
		{
			string sourceFile = "Uxml_2_Asycuda_ESAD.TestFiles.AsycudaESADInput_Sea.xml";
			string outputFile = "Uxml_2_Asycuda_ESAD.TestFiles.AsycudaESADOutput_Sea.xml";
			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<Uxml_2_Asycuda_ESAD>(sourceFile, outputFile);
		}
	}
}
