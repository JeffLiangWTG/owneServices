using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class ExitPointProcessorTest : ExcelProcessorAbstractTest<ExitPoint, RefCusCodeList>
{
	protected override ExitPointProcessor Processor => new ExitPointProcessor(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AETestFiles\TestOutputFiles\ExitPoint"), "RefExitCodesZZ_AED.xml");

	protected override string ExpectXml => @"AETestFiles\RefExitCodesZZ_AED.xml";
}
