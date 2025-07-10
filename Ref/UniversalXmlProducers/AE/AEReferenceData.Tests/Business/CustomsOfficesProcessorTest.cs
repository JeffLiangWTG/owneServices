using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using System.IO;
using NUnit.Framework;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class CustomsOfficesProcessorTest : ExcelProcessorAbstractTest<CustomsOffice, RefCusCodeList>
{
	protected override CustomsOfficesProcessor Processor => new CustomsOfficesProcessor(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AETestFiles\TestOutputFiles\CustomsOffices"), "RefCustomsOfficesCodesZZ_AED.xml");

	protected override string ExpectXml => @"AETestFiles\RefCustomsOfficesCodesZZ_AED.xml";
}
