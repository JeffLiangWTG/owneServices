using CargoWise.RefDbRepo.AEReferenceData.Business;
using System.IO;
using NUnit.Framework;
using System.Reflection;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class CustomsResponseProcessorProcessorTest : ExcelProcessorAbstractTest<CustomsResponseStatus, RefCusCodeList>
{
	protected override CustomsResponseProcessor Processor => new CustomsResponseProcessor(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AETestFiles\TestOutputFiles\CustomsResponse"), "RefCustomsResponsesZZ_AED.xml");

	protected override string ExpectXml => @"AETestFiles\RefCustomsResponsesZZ_AED.xml";
}
