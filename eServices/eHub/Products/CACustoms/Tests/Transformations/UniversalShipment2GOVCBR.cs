using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Products.CACustoms.Transformations.UniversalShipmentToGOVCBR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.CACustoms.Tests.Transformations
{
	[TestClass]
	public class UniversalShipment2GOVCBRTests
	{
		[TestMethod]
		public void OnlyOneSEQ1WhenTheLastInvoiceLineOfFirstInvoiceHasDFOThatEvisceratedIsTrueAndHasNoProcessor()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.Last Line Of First Invoice Has DFO that Eviscerated Is True And Has No Processor.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.Last Line Of First Invoice Has DFO that Eviscerated Is True And Has No Processor.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.Last Line Of First Invoice Has DFO that Eviscerated Is True And Has No Processor.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessage3Invoices9InvoiceLines1PGAPerInvoiceLinexml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.3 invoices - 9 invoice lines - 1 PGA per invoice line.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.3 invoices - 9 invoice lines - 1 PGA per invoice line.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.3 invoices - 9 invoice lines - 1 PGA per invoice line.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessage9PGAsIn1InvoiceLinexml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.9 PGAs in 1 invoice line.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.9 PGAs in 1 invoice line.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.9 PGAs in 1 invoice line.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGACSteel1CustomsQuantityTNE()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GACSteel1.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GACSteel1.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GACSteel1.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGACSteel1CustomsQuantityMG()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GACSteel2.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GACSteel2.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GACSteel2.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageInvQtyAsWholeNumber()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.InvQtyAsWholeNumber.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.InvQtyAsWholeNumber.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.InvQtyAsWholeNumber.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGACSteel3CustomsQuantityNoLPCO()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GACSteel3.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GACSteel3.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GACSteel3.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void ShouldSendOnly99SG9Segments()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.3lines-TC.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.3lines-TC.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.3lines-TC.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void ShouldSendOnly99SG121Segments()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.3lines-HCCFIACFIA.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.3lines-HCCFIACFIA.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.3lines-HCCFIACFIA.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageCFIAOnlyxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.CFIA Only.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.CFIA Only.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.CFIA Only.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageCNSCOnlyxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.CNSC Only.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.CNSC Only.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.CNSC Only.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageCFIAAndDFOxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.DFO CFIA.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.DFO CFIA.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.DFO CFIA.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageDFOOnlyxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.DFO Only.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.DFO Only.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.DFO Only.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageECCCManufacturerxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.ECCC Manufacturer.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.ECCC Manufacturer.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.ECCC Manufacturer.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageECCCVersion42xml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.ECCC Version42.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.ECCC Version42.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.ECCC Version42.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageECCCOnlyxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.ECCC Only.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.ECCC Only.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.ECCC Only.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageEmptyDeclarationxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.Empty Declaration.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.Empty Declaration.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.Empty Declaration.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGACOnlyxml()
		{
			// Set CustomsQuantity with decimal
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GAC Only.xml";
			// Confirm it's rounded on CNT block
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GAC Only.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GAC Only.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGACDuplicateCNTSegmentsxml()
		{
			var sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GAC Duplicate CNT Segments.xml";
			var expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GAC Duplicate CNT Segments.xml";
			var expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GAC Duplicate CNT Segments.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGACCustomsQuantityUnitInWeightxml()
		{
			var sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GAC Customs Quantity Unit - Weight.xml";
			// CustomsQuantity should be rounded
			var expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GAC Customs Quantity Unit - Weight.xml";
			var expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GAC Customs Quantity Unit - Weight.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageCFIAAndGACxml()
		{
			var sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.CFIA And GAC.xml";
			var expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.CFIA And GAC.xml";
			var expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.CFIA And GAC.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGAGI1xml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GAGI 1.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GAGI 1.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GAGI 1.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGAGI2xml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GAGI 2.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GAGI 2.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GAGI 2.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGAGI3xml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GAGI 3.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GAGI 3.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GAGI 3.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGAGI4xml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GAGI 4.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GAGI 4.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GAGI 4.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageGAGI5xml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.GAGI 5.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.GAGI 5.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.GAGI 5.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageHCOnlyxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.HC Only.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.HC Only.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.HC Only.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessageHCWithEachCategoryxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.HC MDE I88 C99.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.HC MDE I88 C99.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.HC MDE I88 C99.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

        [TestMethod]
        public void IIDMessageIIDVersionxml()
        {
            string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.IID Version.xml";
            string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.IID Version.xml";
            string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.IID Version.xml";
            AssertMappingU2G(sourceFile, expectedFile);
            AssertMappingG2G(expectedFile, expectedFinalFile);
        }

		[TestMethod]
		public void IIDMessageNRCANOnlyxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.NRCAN Only.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.NRCAN Only.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.NRCAN Only.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void IIDMessagePHACOnlyxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.PHAC Only.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.PHAC Only.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.PHAC Only.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

        [TestMethod]
        public void IIDMessageSameModelMakeModelYearECCCAndTCxml()
        {
            string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.Same Model Make Model Year in ECCC and TC.xml";
            string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.Same Model Make Model Year in ECCC and TC.xml";
            string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.Same Model Make Model Year in ECCC and TC.xml";
            AssertMappingU2G(sourceFile, expectedFile);
            AssertMappingG2G(expectedFile, expectedFinalFile);
        }

		[TestMethod]
		public void IIDMessageTCOnlyxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.TC Only.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.TC Only.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.TC Only.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

        [TestMethod]
		public void IIDMessageSupplierForTCWithoutTPRorVPR()
        {
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.TC Without TPRorVPR.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.TC Without TPRorVPR.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.TC Without TPRorVPR.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
        }

		[TestMethod]
		public void Testxml()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.Test.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.Test.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.Test.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void TestIIDUniqueRefNumber()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.2 Declarations.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.2 Declarations.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.2 Declarations.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void TestIIDUniqueRefNumber_MQTest()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.2 Declarations - MQ - Test.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.2 Declarations - MQ - Test.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.2 Declarations - MQ - Test.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void TestIIDUniqueRefNumber_MQTest2()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.2 Declarations - MQ - Test2.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.2 Declarations - MQ - Test2.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.2 Declarations - MQ - Test2.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void TestIIDUniqueRefNumber_MQProduction()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.2 Declarations - MQ - Production.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.2 Declarations - MQ - Production.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.2 Declarations - MQ - Production.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void TestIIDContact()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.ApplicantContact.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.ApplicantContact.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.ApplicantContact.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void TestCASUALEntries()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.CASUAL Entries.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.CASUAL Entries.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.CASUAL Entries.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void TestDummyHSCodeCasualImportLineExcluded()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.AutoDummyHSCodeCasualImportLine.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.AutoDummyHSCodeCasualImportLine.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.AutoDummyHSCodeCasualImportLine.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void TestWI00411058()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.WI00411058.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.WI00411058.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.WI00411058.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		[TestMethod]
		public void TestSubGroupCommercialInvoices_Processed()
		{
			string sourceFile = "Transformations.TestFiles.IID_GOVCBR.Input.SubGroupInvoices.xml";
			string expectedFile = "Transformations.TestFiles.IID_GOVCBR.Output.SubGroupInvoices.xml";
			string expectedFinalFile = "Transformations.TestFiles.IID_GOVCBR.OutputFinal.SubGroupInvoices.xml";
			AssertMappingU2G(sourceFile, expectedFile);
			AssertMappingG2G(expectedFile, expectedFinalFile);
		}

		void AssertMappingU2G(string sourceFile, string expectedFile)
		{
			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='UNB4.1']");
			exclusionXpaths.Add("//*[local-name()='UNB4.2']");
			exclusionXpaths.Add("//*[local-name()='UNG4.1']");
			exclusionXpaths.Add("//*[local-name()='UNG4.2']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			var ctx = InitialiseTestingMessageContext_ProdEnv();

			var mockDateMapper = MockRepository.GeneratePartialMock<DateMapper>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockDateMapper.Stub(x => x.CurrentDateTimeUTC("yyyyMMdd")).Return("20171218");
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper(
				Arg<string>.Is.Equal("GetStateCode"),
				Arg<string>.Is.Equal("@Code"),
				Arg<string>.Is.Equal("@FullName"),
				Arg<string>.Is.Anything))
				.Return(null)
				.WhenCalled(X =>
				{
					X.ReturnValue = X.Arguments[3];
				});
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper(
				Arg<string>.Is.Equal("GetStateFromUNLOCO"),
				Arg<string>.Is.Equal(""),
				Arg<string>.Is.Equal("@UNLOCO"),
				Arg<string>.Is.Anything)).Return("KJH");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS5", mockCodeMapper }
			};
			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer, extensionObjects);
			mapTester.ExecuteCompiled<UniversalShipment2GOVCBR>(sourceFile, expectedFile);
		}

		void AssertMappingG2G(string sourceFile, string expectedFile)
		{
			var ctx = InitialiseTestingMessageContext_ProdEnv();
			var mapTester = new MapTester(Assembly.GetExecutingAssembly());
			mapTester.ExecuteCompiled<GOVCBR2GOVCBR>(sourceFile, expectedFile);
		}

		TestingMessageContext InitialiseTestingMessageContext_ProdEnv()
		{
			var ctx = new TestingMessageContext();
			ctx.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "CACustoms");
			return ctx;
		}
	}
}
