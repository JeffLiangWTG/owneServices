using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class AmendCancelReasonProcessorTest : ExcelProcessorAbstractTest<AmendCancelReason, RefCusCodeList>
{
	protected override ExcelProcessor<AmendCancelReason, RefCusCodeList> Processor => new AmendCancelReasonProcessor(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AETestFiles\TestOutputFiles\AmendCancelReason"), "RefAmendCancelReasonZZ_AED.xml");

	protected override string ExpectXml => @"AETestFiles\RefAmendCancelReasonZZ_AED.xml";
}
