using System.IO;
using System.Reflection;
using CargoWise.eHub.Products.AsycudaCustoms.Transform.Uxml_2_AsyPlusPlus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;

namespace CargoWise.eHub.Products.AsycudaCustoms.Test
{
	[TestClass]
	public class TestPLUSPLUS : AsycudaTestCase<UxmlToAsycudaPlusPlus>
	{

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test01_PG()
		{
			RunTest("Test01_PNG_Result.xml", "PG");
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test02_Vanuatu()
		{
			RunTest("Test02_Vanuatu_Result.xml", "VU");
		}

		[TestMethod]
		public void TestXmlToCarMap()
		{
			var xmlFile = "VuAsyPlusPlus.xml";
			var xmlText = ReadFromFile(xmlFile);
			var carFile = "VuAsyPlusPlus.car";
			var carText = ReadFromFile(carFile);
			var result = CargoWise.eHub.Products.AsycudaCustoms.Common.CWConvertXmlToCar.ConvertXml2Car(xmlText);
			Assert.AreEqual(carText, result);
		}

        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test06_OldStyleContainersOnBills_PlusPlus()
		{
			string sourceFile = "TestFiles.GenericSourceOldStyleWithContainersAtBill.xml";
			string outputFile = "TestFiles.Test06_OldStyleContainersOnBills_Result_PlusPlus.xml";
			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<UxmlToAsycudaPlusPlus>(sourceFile, outputFile);
		}		

		string ReadFromFile(string fileName)
		{
			using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.eHub.Products.AsycudaCustoms.Test.TestFiles." + fileName))
			{
				using (var sr = new StreamReader(s))
				{
					return sr.ReadToEnd();
				}
			}

		}
	}
}
