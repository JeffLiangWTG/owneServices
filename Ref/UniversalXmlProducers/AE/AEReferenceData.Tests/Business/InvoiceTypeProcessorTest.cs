using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.AEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class InvoiceTypeProcessorTest : ExcelProcessorAbstractTest<InvoiceType, RefCusCodeList>
{
	protected override InvoiceTypeProcessor Processor => new InvoiceTypeProcessor(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AETestFiles\TestOutputFiles\InvoiceType"), "RefInvoiceTypeZZ_AED.xml");

	protected override string ExpectXml => @"AETestFiles\RefInvoiceTypeZZ_AED.xml";
}
