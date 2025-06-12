using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.CACustoms.Transformations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.IO;
using System.Xml.XPath;

namespace CargoWise.eHub.Products.CACustoms.Tests.Transformations
{
	[TestClass]
	public class ZDNOTICE2UniversalTransactionBatchTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ZDNOTICE2UniversalTransactionBatchTests_1()
		{
			string sourceFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Input.input_1.XML";
			string outputFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Output.output_1.XML";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<CargoWise.eHub.Products.CACustoms.Transformations.CanadianCustomsXMLReply2UniversalTransactionBatch.CanadianCustomsXMLReply2UniversalTransactionBatch>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ZDNOTICE2UniversalTransactionBatchTests_2()
		{
			string sourceFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Input.input_2.XML";
			string outputFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Output.output_2.XML";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<CargoWise.eHub.Products.CACustoms.Transformations.CanadianCustomsXMLReply2UniversalTransactionBatch.CanadianCustomsXMLReply2UniversalTransactionBatch>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ZDNOTICE2UniversalTransactionBatchTests_3()
		{
			string sourceFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Input.input_3.XML";
			string outputFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Output.output_3.XML";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<CargoWise.eHub.Products.CACustoms.Transformations.CanadianCustomsXMLReply2UniversalTransactionBatch.CanadianCustomsXMLReply2UniversalTransactionBatch>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ZDNOTICE2UniversalTransactionBatchTests_4()
		{
			string sourceFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Input.input_4.XML";
			string outputFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Output.output_4.XML";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<CargoWise.eHub.Products.CACustoms.Transformations.CanadianCustomsXMLReply2UniversalTransactionBatch.CanadianCustomsXMLReply2UniversalTransactionBatch>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ZDNOTICE2UniversalTransactionBatchTests_5()
		{
			string sourceFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Input.input_5.XML";
			string outputFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Output.output_5.XML";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<CargoWise.eHub.Products.CACustoms.Transformations.CanadianCustomsXMLReply2UniversalTransactionBatch.CanadianCustomsXMLReply2UniversalTransactionBatch>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ZDNOTICE2UniversalTransactionBatchTests_6()
		{
			string sourceFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Input.input_6.XML";
			string outputFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Output.output_6.XML";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<CargoWise.eHub.Products.CACustoms.Transformations.CanadianCustomsXMLReply2UniversalTransactionBatch.CanadianCustomsXMLReply2UniversalTransactionBatch>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ZDNOTICE2UniversalTransactionBatchTests_6_1()
		{
			string sourceFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Input.input_6_1.XML";
			string outputFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Output.output_6_1.XML";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<CargoWise.eHub.Products.CACustoms.Transformations.CanadianCustomsXMLReply2UniversalTransactionBatch.CanadianCustomsXMLReply2UniversalTransactionBatch>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ZDNOTICE2UniversalTransactionBatchTests_7()
		{
			string sourceFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Input.input_7.XML";
			string outputFile = "Transformations.TestFiles.ZDNOTICE2UniversalTransactionBatch_Output.output_7.XML";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.Execute<CargoWise.eHub.Products.CACustoms.Transformations.CanadianCustomsXMLReply2UniversalTransactionBatch.CanadianCustomsXMLReply2UniversalTransactionBatch>(sourceFile, outputFile);
		}
	}
}

