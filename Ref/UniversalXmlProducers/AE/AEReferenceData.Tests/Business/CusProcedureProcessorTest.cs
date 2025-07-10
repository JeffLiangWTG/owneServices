using System.IO;
using NUnit.Framework;
using System.Reflection;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class CusProcedureProcessorTest : ExcelProcessorAbstractTest<CusProcedure, RefCusProcedure>
{
	protected override CusProcedureProcessor Processor => new CusProcedureProcessor(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AETestFiles\TestOutputFiles\CusProcedure"), "RefCusProcedureZZ_AED.xml");

	protected override string ExpectXml => @"AETestFiles\RefCusProcedureZZ_AED.xml";
}
