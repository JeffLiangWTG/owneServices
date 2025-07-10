using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests;

[TestFixture]
public class WarehouseCodeXmlProducerTest
{
	[Test]
	public void TestExportToXml()
	{
		using var producer = new WarehouseCodeXmlProducerForTest("IN CUS Warehouse Code List");
		var input = TestHelper.ReadContentString(@"WarehouseCode\INTestFiles\Input\code.html");
		var warehouseCodes = WarehouseCodeParser.ParseResponse(input);
		producer.ExportToXml(warehouseCodes);

		var xmlFilePath = producer.GetOutputFilePath();
		Assert.True(File.Exists(xmlFilePath));
		var expectedXmlFileContent = TestHelper.ReadContentString(@"WarehouseCode\INTestFiles\Output\RefWarehouseCodeZZ_IN.xml");
		Assert.AreEqual(expectedXmlFileContent, TestHelper.RemoveIgnoredArgsFromXml(File.ReadAllText(xmlFilePath)));
	}

	class WarehouseCodeXmlProducerForTest(string dataSource) : WarehouseCodeXmlProducer(dataSource), IDisposable
	{
		public new string GetOutputFilePath()
		{
			return base.GetOutputFilePath();
		}

		public void Dispose()
		{
			DeleteOutputFiles();
		}

		public new void ExportToXml(List<RefCusCodeList> warehouseCodes)
		{
			base.ExportToXml(warehouseCodes);
		}
	}

}
