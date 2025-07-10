using System;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.INReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	public class DBKTariffXmlProducerTest
	{
		[Test]
		public void TestEndToEnd()
		{
			using (var stringWriter = new StringWriter())
			{
				Console.SetOut(stringWriter);

				var pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DBKTariff\INTestFiles\Input\csnt-2023-77 dt 20Oct2023 - Principal DBK Schedule.pdf");
				DBKTariffProgram.Run(new string[] { "DBKTariff", pdfPath, "2023-10-30" });
				var actualXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\UXmlFiles\RefCusTariff_IN_DBK_20231030.xml");
				using (var actualXmlStream = new FileStream(actualXmlPath, FileMode.Open))
				using (var expectedXmlStream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DBKTariff\INTestFiles\Output\RefCusTariff_IN_DBK_20231030.xml"), FileMode.Open))
				{
					var actualXml = new XmlDocument();
					actualXml.Load(actualXmlStream);

					var expectedXml = new XmlDocument();
					expectedXml.Load(expectedXmlStream);

					Assert.AreEqual(expectedXml.InnerXml, actualXml.InnerXml);
				}
				File.Delete(actualXmlPath);

				var output = stringWriter.ToString();
				StringAssert.Contains("Processing completed.", output);
			}
		}

		[Test]
		public void TestProcessHasStartDatePdf()
		{
			using (var stringWriter = new StringWriter())
			{
				Console.SetOut(stringWriter);
				Console.SetError(stringWriter);
				var pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DBKTariff\INTestFiles\Input\hasStartDate.pdf");
				DBKTariffProgram.Run(new string[] { "DBKTariff", pdfPath });
				var output = stringWriter.ToString();
				StringAssert.Contains("No data found in PDF", output);
			}
		}

		[Test]
		public void TestProcessEmptyPdf()
		{
			using (var stringWriter = new StringWriter())
			{
				Console.SetOut(stringWriter);
				Console.SetError(stringWriter);
				var pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DBKTariff\INTestFiles\Input\empty.pdf");
				DBKTariffProgram.Run(new string[] { "DBKTariff", pdfPath });
				var output = stringWriter.ToString();
				StringAssert.Contains("The effective time cannot be parsed out within the PDF file.", output);
			}
		}

		[Test]
		public void TestProcessInvalidPdf()
		{
			using (var stringWriter = new StringWriter())
			{
				Console.SetOut(stringWriter);
				Console.SetError(stringWriter);
				var pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DBKTariff\INTestFiles\Input\invalid.pdf");
				DBKTariffProgram.Run(new string[] { "DBKTariff", pdfPath });
				var output = stringWriter.ToString();
				StringAssert.Contains("The number of table columns in the current PDF is not 5.", output);
			}
		}
	}
}
