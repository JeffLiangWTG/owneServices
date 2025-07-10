using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class ConsentingBodyCodeListParserTest
	{
		[Test]
		public void TestCustomsConsentingBodyGeneration()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeList_BR_CUSCB.xml"))
			using (var expectedTypeStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeType_BR_CUSCB.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.OrgaoAnuente.xml"))
			{
				var publicationDate = new DateTime(2022, 02, 22, 14, 58, 19);
				var parser = new ConsentingBodyCodeListParser("BR Consenting Body Code List");

				using (var outputStream = new MemoryStream())
				using (var typeOutputStream = new MemoryStream())
				{
					parser.ExportToStream(inputStream, outputStream, typeOutputStream, publicationDateTime: publicationDate);
					StreamCompareHelper.CompareStreamContent(expectedStream, outputStream);
					StreamCompareHelper.CompareStreamContent(expectedTypeStream, typeOutputStream);
				}
			}
		}
	}
}
