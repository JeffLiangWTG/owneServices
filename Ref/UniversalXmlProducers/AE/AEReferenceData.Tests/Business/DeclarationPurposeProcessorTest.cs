using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class DeclarationPurposeProcessorTest : ExcelProcessorAbstractTest<DeclarationPurpose, RefCusCodeList>
{
	protected override DeclarationPurposeProcessor Processor => new DeclarationPurposeProcessor(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AETestFiles\TestOutputFiles\DeclarationPurpose"), "RefDeclarationPurposeCodesZZ_AED.xml");

	protected override string ExpectXml => @"AETestFiles\RefDeclarationPurposeCodesZZ_AED.xml";
}


