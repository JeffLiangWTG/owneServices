using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class CustomsOfficeDepartmentsXmlWriterTest
	{
		[Test]
		public void TestDownloadAndConvertToXML()
		{
			Setup();
			var dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedFilePath = Path.Combine(dirPath, @"CustomsOfficeDepartments\\TestFiles\\Output\\ExpectedRefCusCodeList_JP_Departments.xml");
			var expectedXmlAsString = File.ReadAllText(expectedFilePath);

			new CustomsOfficeDepartmentsXMLWriter().WriteXml(mockHttpClientHelper.Object);
			var actualXmlAsString = File.ReadAllText(ActualOutputFilePath);
			Assert.That(actualXmlAsString, Is.EqualTo(expectedXmlAsString));
		}

		void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var basePagePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CustomsOfficeDepartments\TestFiles\Input\CustomsOfficeDearptmentBaseHtml.html");
			var inputFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CustomsOfficeDepartments\TestFiles\Input\bumon1.pdf");
			var inputFilePath2 = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CustomsOfficeDepartments\TestFiles\Input\bumon2.pdf");
			var inputFilePath3 = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CustomsOfficeDepartments\TestFiles\Input\bumon5.pdf");
			var inputFilePath4 = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CustomsOfficeDepartments\TestFiles\Input\bumon9.pdf");

			mockHttpClientHelper = new Mock<IHttpClientHelper>();
			mockBasePageFileStream = new FileStream(basePagePath, FileMode.Open);
			mockPDFFileStream = new FileStream(inputFilePath, FileMode.Open);
			mockPDFFileStream2 = new FileStream(inputFilePath2, FileMode.Open);
			mockPDFFileStream3 = new FileStream(inputFilePath3, FileMode.Open);
			mockPDFFileStream4 = new FileStream(inputFilePath4, FileMode.Open);

			string htmlContent;
			using (var reader = new StreamReader(mockBasePageFileStream))
			{
				htmlContent = reader.ReadToEnd();
			}

			mockHttpClientHelper.Setup(x => x.GetAsync(@"https://bbs.naccscenter.com/naccs/dfw/web/data/code/hanyo/bumon1.pdf")).Returns(Task.FromResult<Stream>(mockPDFFileStream));
			mockHttpClientHelper.Setup(x => x.GetAsync(@"https://bbs.naccscenter.com/naccs/dfw/web/data/code/hanyo/bumon2.pdf")).Returns(Task.FromResult<Stream>(mockPDFFileStream2));
			mockHttpClientHelper.Setup(x => x.GetAsync(@"https://bbs.naccscenter.com/naccs/dfw/web/data/code/hanyo/bumon5.pdf")).Returns(Task.FromResult<Stream>(mockPDFFileStream3));
			mockHttpClientHelper.Setup(x => x.GetAsync(@"https://bbs.naccscenter.com/naccs/dfw/web/data/code/hanyo/bumon9.pdf")).Returns(Task.FromResult<Stream>(mockPDFFileStream4));
			mockHttpClientHelper.Setup(x => x.GetWebPageAsync(AppConfig.NACCS.CodeLists.CustomsOfficeDepartmentsFileDownloadParentUrl)).Returns(Task.FromResult(htmlContent));
		}

		[TearDown]
		public void Teardown()
		{
			mockBasePageFileStream?.Close();
			mockPDFFileStream?.Close();
			mockPDFFileStream2?.Close();

			if (File.Exists(ActualOutputFilePath))
			{
				File.Delete(ActualOutputFilePath);
			}
		}

		string ActualOutputFilePath => Path.Combine(AppConfig.Shared.OutputDirectory, "RefCusCodeList_JP_Departments.xml");
		Mock<IHttpClientHelper> mockHttpClientHelper;
		FileStream mockBasePageFileStream;
		FileStream mockPDFFileStream;
		FileStream mockPDFFileStream2;
		FileStream mockPDFFileStream3;
		FileStream mockPDFFileStream4;
	}
}
