using System;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.INCustoms.Transform;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.INCustoms.Tests
{
	[TestClass]
	public class UniversalShipment_CMCHI21_Test
	{
        [TestMethod]
  		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_CMCHI21_Test1()
		{

			string sourceFile = "TestFiles.UxmlInput01.xml";
			string outputFile = "TestFiles.Cmchi21Output01.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), ComparerToExcludeDateTimeNodes);
			mapTester.Execute<UxmlToCMCHI21>(sourceFile, outputFile);
		}

		ICompare ComparerToExcludeDateTimeNodes
		{
			get
			{
				if (comparer == null)
				{
					var exclusionXpaths = new List<string>();
					exclusionXpaths.Add("/*[local-name()='CMCHI21']/*[local-name()='HREC']/*[local-name()='MessageDate']");
					exclusionXpaths.Add("/*[local-name()='CMCHI21']/*[local-name()='HREC']/*[local-name()='MessageTime']");
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}

		ICompare comparer;
	}
}