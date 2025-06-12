using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting; 

using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.INCustoms.Transform.UniversalShipment_CMCHI01;

namespace CargoWise.eHub.Products.INCustoms.Tests
{
	[TestClass]
	public class UniversalShipment_CMCHI01_Test
	{
        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_TestCMCHI01()
		{

			string sourceFile = "TestFiles.UxmlInputCmchi01.xml";
			string outputFile = "TestFiles.Cmchi01Output01.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), ComparerToExcludeDateTimeNodes);
			mapTester.Execute<UxmlToCMCHI01>(sourceFile, outputFile);

		}

		ICompare ComparerToExcludeDateTimeNodes
		{
			get
			{
				if (comparer == null)
				{
					var exclusionXpaths = new List<string>();
					exclusionXpaths.Add("/*[local-name()='CMCHI01']/*[local-name()='HREC']/*[local-name()='MessageDate']");
					exclusionXpaths.Add("/*[local-name()='CMCHI01']/*[local-name()='HREC']/*[local-name()='MessageTime']");
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}

		ICompare comparer;
	}
}