using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class ExchangeHedgeMethodofPaymentCodeListParserTest
	{
		[Test]
		public void TestMethodOfPaymentXMLGeneration()
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeList_BR_EXMOP.xml"))
			using (var expectedTypeStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Output.RefCusCodeType_BR_EXMOP.xml"))
			using (var inputStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.ModalidadePagamento.xml"))
			{
				var publicationDate = new DateTime(2022, 02, 22, 14, 58, 19);
				var parser = new ExchangeHedgeMethodofPaymentCodeListParser("BR Exchange Hedge Method of Payment Code List");

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
