using System.IO;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using NUnit.Framework;
using Org.XmlUnit.Builder;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
abstract class ExcelProcessorAbstractTest<T, TResult>
{
	[Test]
	public void TestProcessExcel()
	{
		var xls = ExcelHelper.LoadExcel(@"AETestFiles\Declaration_List_of_Values.xlsx");
		OutputPath = Processor.OutputPath;
		Processor.ProcessExcel(xls);
		var file = Path.Combine(OutputPath, Processor.FileName);
		Assert.That(File.Exists(file));

		var xml = File.ReadAllText(file);
		var expectXml = File.ReadAllText(ExpectXml);
		var mydiff = DiffBuilder.Compare(expectXml)
			.WithTest(xml)
			.WithNodeFilter(node => node.Name != "PublicationTime")
			.Build();
		Assert.That(!mydiff.HasDifferences(), mydiff.ToString());
	}

	[TearDown]
	public void Teardown()
	{
		if (Directory.Exists(OutputPath))
		{
			Directory.Delete(OutputPath, true);
		}
	}

	protected abstract ExcelProcessor<T, TResult> Processor { get; }

	protected abstract string ExpectXml { get; }

	string OutputPath;
}
