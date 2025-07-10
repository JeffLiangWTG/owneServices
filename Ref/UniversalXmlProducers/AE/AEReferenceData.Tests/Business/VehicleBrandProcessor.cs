using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class VehicleBrandProcessorTest : ExcelProcessorAbstractTest<VehicleBrand, RefCusCodeList>
{
	protected override ExcelProcessor<VehicleBrand, RefCusCodeList> Processor => new VehicleBrandProcessor(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AETestFiles\TestOutputFiles\VehicleBrand"), "RefVehicleBrandsZZ_AED.xml");

	protected override string ExpectXml => @"AETestFiles\RefVehicleBrandsZZ_AED.xml";
}
