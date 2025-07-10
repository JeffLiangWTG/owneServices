using System;
using System.IO;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor.Test
{
	[TestFixture]
	public class XMLMessageProcessorFixture : BaseMessageProcessorFixture
	{
		[Test]
		public void ProcessXML_CUSCOM_ALL()
		{
			AssertProcess("SGCLASET XML_CUSCOM_ALL.xml", AssertXML_CUSCOM_ALL);
		}

		void AssertXML_CUSCOM_ALL(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(33, noOfUpdates);
			AssertSameDataAsDirectory("XML_CUSTOMS_ALL", xmlFiles);
		}

		[Test]
		public void ProcessXML_CUSCOM_PRTCODE()
		{
			AssertProcess("SGCLASET XML_CUSCOM_PRTCODE.xml", AssertXML_CUSCOM_PRTCODE);
		}

		void AssertXML_CUSCOM_PRTCODE(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(3, noOfUpdates);
			AssertSameDataAsDirectory("XML_CUSCOM_PRTCODE", xmlFiles);
		}

		[Test]
		public void ProcessXML_CUSCOM_LOCCODE()
		{
			AssertProcess("SGCLASET XML_CUSCOM_LOCCODE.xml", AssertXML_CUSCOM_LOCCODE);
		}

		void AssertXML_CUSCOM_LOCCODE(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(4, noOfUpdates);
			AssertSameDataAsDirectory("XML_CUSCOM_LOCCODE", xmlFiles);
		}

		[Test]
		public void ProcessXML_CUSCOM_CHSCODE()
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"TestData\SGCLASET XML_CUSCOM_CHSCODE.xml");
			var content = File.ReadAllText(path);

			ProcessInvalidDataCore(content, StatusProvider.GetMERStatus());
		}

		[Test]
		public void ProcessXML_CUSCOM_CTYCODE()
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"TestData\SGCLASET XML_CUSCOM_CTYCODE.xml");
			var content = File.ReadAllText(path);

			ProcessInvalidDataCore(content, StatusProvider.GetMERStatus());
		}

		[Test]
		public void ProcessXML_CUSCOM_CPCCODE()
		{
			AssertProcess("SGCLASET XML_CUSCOM_CPCCODE.xml", AssertXML_CUSCOM_CPCCODE);
		}

		void AssertXML_CUSCOM_CPCCODE(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(37, noOfUpdates);
			AssertSameDataAsDirectory("XML_CUSCOM_CPCCODE", xmlFiles);
		}

		[Test]
		public void ProcessXML_CUSCOM_CPCCODE_WithoutDescriptionNode()
		{
			AssertProcess("SGCLASET XML_CUSCOM_CPCCODE_NO_DES.xml", AssertXML_CUSCOM_CPCCODE_WithoutDescriptionNode);
		}

		void AssertXML_CUSCOM_CPCCODE_WithoutDescriptionNode(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(4, noOfUpdates);
			AssertSameDataAsDirectory("SGCLASET XML_CUSCOM_CPCCODE_NO_DES", xmlFiles);
		}

		protected override string ContentType => DataSourceConstants.ContentType.XML;
	}
}
