using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class VehicleTypeProcessorTest : ExcelProcessorAbstractTest<VehicleType, RefCusCodeList>
{
	protected override ExcelProcessor<VehicleType, RefCusCodeList> Processor => new VehicleTypeProcessor(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AETestFiles\TestOutputFiles\VehicleType"), "RefVehicleTypesZZ_AED.xml");

	protected override string ExpectXml => @"AETestFiles\RefVehicleTypesZZ_AED.xml";
}
