using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class CUSDECMessageTextBuilderTest : TestCaseWithFactory
	{
		public void TestShouldOutputFinancialAccountNumber()
		{
			var zaTestHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("6");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("BND");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("CVI");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("RCV");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("DCV");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("ts5");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("ts6");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("ts7");
			Factory.Save();

			var testInput = new CUSDECMessageDataProviderForTest();
			CUSDECMessageTextBuilderForTest testBuilder;
			string result = "";
			CombineAssertions(() =>
			{
				FillHeaderLevelInformation(testInput);
				testInput.ShipmentType = "830";
				testInput.ShouldOutputFinancialAccountNumber = true;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("ShouldOutputFinancialAccountNumber is true", ExpectedOutput_HeaderLevelInformationForExportOnly, result.Replace("'", "'\n"));

				testInput.ShouldOutputFinancialAccountNumber = false;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("ShouldOutputFinancialAccountNumber is false", ExpectedOutput_HeaderLevelInformationForExportOnly.Replace("RFF+ABI:FINACCNUM'\r\n", "").Replace("UNT+53+<<MSGNO PLACEHOLDER>>'", "UNT+52+<<MSGNO PLACEHOLDER>>'"), result.Replace("'", "'\n"));
			});
		}

		public void TestEntryDocType()
		{
			var testInput = new CUSDECMessageDataProviderForTest();
			CUSDECMessageTextBuilderForTest testBuilder;

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				CombineAssertions(() =>
				{
					const string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM'
CST++:117:ZZZ'
FTX+LIN+++0'
TDT+20'
UNS+D'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
UNT+10+<<MSGNO PLACEHOLDER>>'
";
					testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
					var result = testBuilder.GenerateMessageBody();
					AssertMultilineASCIIEquals(expected, result.Replace("'", "'\n"));
				});
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				CombineAssertions(() =>
				{
					foreach (CodeDescriptionPair pair in new DeclarationTypeList())
					{
						testInput.DeclarationType = pair.Code;
						var expected = $@"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+:::{pair.Code}'
CST++:117:ZZZ'
FTX+LIN+++0'
TDT+20'
UNS+D'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
UNT+10+<<MSGNO PLACEHOLDER>>'
";
						testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
						var result = testBuilder.GenerateMessageBody();
						AssertMultilineASCIIEquals(expected, result.Replace("'", "'\n"));
					}
				});
			}
		}

		public void TestGenerateMessageBody()
		{
			var zaTestHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("6");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("BND");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("CVI");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("RCV");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("DCV");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("ts5");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("ts6");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("ts7");
			Factory.Save();

			var testInput = new CUSDECMessageDataProviderForTest();
			CUSDECMessageTextBuilderForTest testBuilder;
			string result = "";
			CombineAssertions(() =>
			{
				FillHeaderLevelInformation(testInput);
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("HeaderInformation Only", ExpectedOutput_HeaderLevelInformationOnly, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				testInput.InvoiceInformations = new InvoiceInformationForTest[] {
					new InvoiceInformationForTest() { InvoiceNumber = "InvN1", InvoiceDate = new ZDateTime(2014, 01, 01) },
					new InvoiceInformationForTest() { InvoiceNumber = "InvN2", InvoiceDate = new ZDateTime(2014, 01, 02) }
				};
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("With Invoice Info", ExpectedOutput_WithInvoiceInfo, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				testInput.Containers = new ContainerInformationForTest[]
				{
					new ContainerInformationForTest { ContainerMode = "4", ContainerNumber = "CONT1", FirstSealNumber = "SEAL1" },
					new ContainerInformationForTest { ContainerMode = "5", ContainerNumber = "CONT2", FirstSealNumber = "SEAL1", SecondSealNumber = "SEAL2"  },
					new ContainerInformationForTest { ContainerMode = "7", ContainerNumber = "CONT3", SecondSealNumber = "SEAL2" },
					new ContainerInformationForTest { ContainerMode = "8", ContainerNumber = "CONT4", FirstSealNumber = "SEAL1SEALASEALBSEALC" },
					new ContainerInformationForTest { ContainerMode = "", ContainerNumber = "CONT5", FirstSealNumber = "SEAL1SEALASEALBSEALC" }
				};
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("With Container Info", ExpectedOutput_WithContainerInfo, result.Replace("'", "'\n"));
			});

			CombineAssertions("Duties and Fees", () =>
			{
				testInput.LineLevelDetails = new LineLevelInformationForTest[]
				{
					new LineLevelInformationForTest {
						CustomsProcedureCode = "11",
						LineNumber = "1",
						PreviousProcedureCode = "00",
						TariffCode = "020110",
						DutiesAndFees = new DutyFeeInformationForTest[]
						{
							new DutyFeeInformationForTest { Code = "1P1", Value = 11 },
							new DutyFeeInformationForTest { Code = "12A", Value = 12.1 },
							new DutyFeeInformationForTest { Code = "12B", Value = 12.2 },
						},
						ProvisionalPayments = Array.Empty<DutyFeeInformationForTest>(),
						AdditionalInformations = Array.Empty<AdditionalInformationForTest>()
					}
				};
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("With No Zeros", ExpectedOutput_DutiesAndFeesNoZeros, result.Replace("'", "'\n"));

				testInput.LineLevelDetails = new LineLevelInformationForTest[]
				{
					new LineLevelInformationForTest {
						CustomsProcedureCode = "11",
						LineNumber = "1",
						PreviousProcedureCode = "00",
						TariffCode = "020110",
						DutiesAndFees = new DutyFeeInformationForTest[]
						{
							new DutyFeeInformationForTest { Code = "1P1", Value = 11 },
							new DutyFeeInformationForTest { Code = "12A", Value = 0 },
							new DutyFeeInformationForTest { Code = "12B", Value = 12.2 },
						},
						ProvisionalPayments = Array.Empty<DutyFeeInformationForTest>(),
						AdditionalInformations = Array.Empty<AdditionalInformationForTest>()
					}
				};
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("With Zeros", ExpectedOutput_DutiesAndFeesWithZeros, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				testInput.LineLevelDetails = new LineLevelInformationForTest[]
				{
					new LineLevelInformationForTest {
						Factory = Factory,
						ActualPrice = 200,
						CountryOfOrigin = "AU",
						CustomsProcedureCode = "11",
						CustomsQuantity = 101,
						CustomsUnitQty = "KG",
						AdditionalQuantity = 0,
						AdditionalUnitQty = "GJ",
						ClassificationQuantity = 303,
						ClassificationUnitQty = "CQ",
						WarehouseCountableQuantity = 404,
						WarehouseCountableUnitQty = "KG",
						CustomsValue = 88.88,
						GoodsDescription = "This is a long long long long long long long long long long long long long long long long long long long long long Description",
						LineNumber = "1",
						PreviousProcedureCode = "00",
						PreviousProcedureMRN = "PTA201604198464646",
						TariffCode = "020110",
						RebateUserCode = "WPC",
						ProcedureMeasure = "31506010002",
						DutiesAndFees = new DutyFeeInformationForTest[]
						{
							new DutyFeeInformationForTest { Code = "1P1", Value = 11 },
							new DutyFeeInformationForTest { Code = "12A", Value = 12.1 },
							new DutyFeeInformationForTest { Code = "12B", Value = 12.2 },
							new DutyFeeInformationForTest { Code = "13A", Value = 13.1 },
							new DutyFeeInformationForTest { Code = "13B", Value = 13.2 },
							new DutyFeeInformationForTest { Code = "13C", Value = 13.3 },
							new DutyFeeInformationForTest { Code = "13D", Value = 13.4 },
							new DutyFeeInformationForTest { Code = "15A", Value = 15.1 },
							new DutyFeeInformationForTest { Code = "15B", Value = 15.2 },
							new DutyFeeInformationForTest { Code = "1P8", Value = 18 },
							new DutyFeeInformationForTest { Code = "2P1", Value = 21 },
							new DutyFeeInformationForTest { Code = "2P2", Value = 22 },
							new DutyFeeInformationForTest { Code = "2P3", Value = 23 },
							new DutyFeeInformationForTest { Code = "VAT", Value = 44.44 },
							new DutyFeeInformationForTest { Code = "SUR", Value = 55.55 },
							new DutyFeeInformationForTest { Code = "DLA", Value = 77.77 },
						},
						ProvisionalPayments = new DutyFeeInformationForTest[]
						{
							new DutyFeeInformationForTest { Code = "PEN", Value = 66.66 },
							new DutyFeeInformationForTest { Code = "FOR", Value = 99.99 },
							new DutyFeeInformationForTest { Code = "PPA", Value = 99.88 },
						},
						AdditionalInformations = new AdditionalInformationForTest[]
						{
							new AdditionalInformationForTest { Code = "BND", Value = "123" },
							new AdditionalInformationForTest { Code = "CVI", Value = "201" },
							new AdditionalInformationForTest { Code = "RCV", Value = "456" },
							new AdditionalInformationForTest { Code = "DCV", Value = "402" },
							new AdditionalInformationForTest { Code = "ts5", Value = "valuets5" },
							new AdditionalInformationForTest { Code = "ts6", Value = "valuets6" },
							new AdditionalInformationForTest { Code = "ts7", Value = "valuets77777777777777777" },
						},
					}
				};
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("With zero additional quantity", ExpectedOutput_WithZeroAdditionalQuantity, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				testInput.LineLevelDetails = new LineLevelInformationForTest[]
				{
					new LineLevelInformationForTest {
						Factory = Factory,
						ActualPrice = 200,
						CountryOfOrigin = "AU",
						CustomsProcedureCode = "11",
						CustomsQuantity = 101,
						CustomsUnitQty = "KG",
						AdditionalQuantity = 202,
						AdditionalUnitQty = "M3",
						ClassificationQuantity = 303,
						ClassificationUnitQty = "CQ",
						WarehouseCountableQuantity = 404,
						WarehouseCountableUnitQty = "KG",
						CustomsValue = 88.88,
						GoodsDescription = "This is a long long long long long long long long long long long long long long long long long long long long long Description",
						LineNumber = "1",
						PreviousProcedureCode = "00",
						PreviousProcedureMRN = "PTA201604198464646",
						TariffCode = "020110",
						RebateUserCode = "WPC",
						ProcedureMeasure = "31506010002",
						DutiesAndFees = new DutyFeeInformationForTest[]
						{
							new DutyFeeInformationForTest { Code = "1P1", Value = 11 },
							new DutyFeeInformationForTest { Code = "12A", Value = 12.1 },
							new DutyFeeInformationForTest { Code = "12B", Value = 12.2 },
							new DutyFeeInformationForTest { Code = "13A", Value = 13.1 },
							new DutyFeeInformationForTest { Code = "13B", Value = 13.2 },
							new DutyFeeInformationForTest { Code = "13C", Value = 13.3 },
							new DutyFeeInformationForTest { Code = "13D", Value = 13.4 },
							new DutyFeeInformationForTest { Code = "15A", Value = 15.1 },
							new DutyFeeInformationForTest { Code = "15B", Value = 15.2 },
							new DutyFeeInformationForTest { Code = "1P8", Value = 18 },
							new DutyFeeInformationForTest { Code = "2P1", Value = 21 },
							new DutyFeeInformationForTest { Code = "2P2", Value = 22 },
							new DutyFeeInformationForTest { Code = "2P3", Value = 23 },
							new DutyFeeInformationForTest { Code = "VAT", Value = 44.44 },
							new DutyFeeInformationForTest { Code = "SUR", Value = 55.55 },
							new DutyFeeInformationForTest { Code = "DLA", Value = 77.77 },
						},
						ProvisionalPayments = new DutyFeeInformationForTest[]
						{
							new DutyFeeInformationForTest { Code = "PEN", Value = 66.66 },
							new DutyFeeInformationForTest { Code = "FOR", Value = 99.99 },
							new DutyFeeInformationForTest { Code = "PPA", Value = 99.88 },
						},
						AdditionalInformations = new AdditionalInformationForTest[]
						{
							new AdditionalInformationForTest { Code = "BND", Value = "123" },
							new AdditionalInformationForTest { Code = "CVI", Value = "201" },
							new AdditionalInformationForTest { Code = "RCV", Value = "456" },
							new AdditionalInformationForTest { Code = "DCV", Value = "402" },
							new AdditionalInformationForTest { Code = "ts5", Value = "valuets5" },
							new AdditionalInformationForTest { Code = "ts6", Value = "valuets6" },
							new AdditionalInformationForTest { Code = "ts7", Value = "valuets77777777777777777" },
						},
					}
				};
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("With Line Info", ExpectedOutput_WithLineInfo, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				testInput.Importer = new AddressInformationForTest
				{
					OrganizationCode = "importer1",
					PostCode = "PCI01",
					Address = "1A/11 Importer Street",
					City = "SYndey",
					Name = "TestImporter",
					VATRegistrationNo = "IMPVATREG"
				};
				testInput.Exporter = new AddressInformationForTest
				{
					OrganizationCode = "Exporter",
					PostCode = "PCE01",
					Address = "1A/11 Exporter Street",
					City = "Export",
					Name = "TestExporter",
					VATRegistrationNo = "EXPPVATREG"
				};
				testInput.UnregisteredTrader = new AddressInformationForTest
				{
					OrganizationCode = "TaxPartyIdentification",
					OrganizationCodeQualifier = "167",
					PostCode = "PCD01",
					Address = "1A/11 Declarant Street LONG1 LONG2 LONG3 LONG4 LONG5 LONG6 LONG7 LONG8 LONG9 LONG10 LONG11 LONG12 LONG13 LONG14 LONG15 LONG16 LONG17 LONG18 LONG19 LONG20 LONG21 LONG22 LONG23 LONG24 LONG25 LONG26 LONG27 LONG28 LONG29 LONG30 LONG31 LONG32 LONG33 LONG34 LONG35 LONG36 LONG37 LONG38 ",
					City = "Declarant",
					Name = "TestDeclarant",
					VATRegistrationNo = "DECPVATREG"
				};
				testInput.ImporterForExportJob = new AddressInformationForTest
				{
					OrganizationCode = "consignee1",
					PostCode = "PCC01",
					Address = "1A/11 Consignee Street",
					City = "Con City",
					Name = "TestConsignee",
					VATRegistrationNo = "CONVATREG"
				};
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("With Org Address Info", ExportedOutput_WithOrgAddress, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				testInput.ToWarehouse = ZString.Empty;
				testInput.CustomsOfficeCode = ZString.Empty;
				testInput.MRNToBeReplaced = ZString.Empty;
				testInput.FinancialAccountNumber = ZString.Empty;
				testInput.CaseNumber = ZString.Empty;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("With Mutex Value Output", ExportedOutput_MutexValue, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				testInput.HouseBill = ZString.Empty;
				testInput.OriginalMRN = ZString.Empty;
				testInput.MRNToBeReplaced = ZString.Empty;
				testInput.TransportDocumentNumber = ZString.Empty;
				testInput.FinancialAccountNumber = ZString.Empty;
				testInput.UniqueConsignmentReference = ZString.Empty;
				testInput.MessageNumber = ZString.Empty;
				testInput.CaseNumber = ZString.Empty;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("No SG1-SG2-SG3", ExportedOutput_NoSG1, result.Replace("'", "'\n"));
			});
		}

		public void TestJZ_InvoiceNumberForExport()
		{
			var zaTestHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("6");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("BND");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("CVI");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("RCV");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("DCV");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("ts5");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("ts6");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("ts7");
			Factory.Save();

			var testInput = new CUSDECMessageDataProviderForTest();
			CUSDECMessageTextBuilderForTest testBuilder;
			string result = "";
			CombineAssertions(() =>
			{
				FillHeaderLevelInformation(testInput);
				testInput.ShipmentType = "830";
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("HeaderInformation Only", ExpectedOutput_HeaderLevelInformationForExportOnly, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				testInput.InvoiceInformations = new InvoiceInformationForTest[] {
					new InvoiceInformationForTest() { InvoiceNumber = "InvN1", InvoiceDate = new ZDateTime(2014, 01, 01) },
					new InvoiceInformationForTest() { InvoiceNumber = "InvN2", InvoiceDate = new ZDateTime(2014, 01, 02) }
				};
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("With Invoice Info", ExpectedOutput_WithInvoiceInfoForExport, result.Replace("'", "'\n"));
			});
		}

		public void TestOutputOriginalMRN()
		{
			var testInput = new CUSDECMessageDataProviderForTest();
			CUSDECMessageTextBuilderForTest testBuilder;
			string result = "";
			string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM'
CST++:117:ZZZ'
FTX+LIN+++0'
RFF+IB:MRN'
TDT+20'
UNS+D'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
UNT+11+<<MSGNO PLACEHOLDER>>'
";

			CombineAssertions(() =>
			{
				testInput.MRNToBeReplaced = "MRN";
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("Header has original MRN", expected, result.Replace("'", "'\n"));
			});
		}

		public void TestGenerateEmptyTotalDutiesAndVAT()
		{
			var testInput = new CUSDECMessageDataProviderForTest();
			CUSDECMessageTextBuilderForTest testBuilder;
			string result = "";

			CombineAssertions(() =>
			{
				string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM'
CST++:117:ZZZ'
FTX+LIN+++0'
TDT+20'
UNS+D'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:0.00'
TAX+3+TVD:107:ZZZ'
MOA+161:0.00'
UNT+14+<<MSGNO PLACEHOLDER>>'";

				testInput.TotalDutiesDue = 0;
				testInput.TotalVATDue = 0;
				testInput.ShouldOutputDutiesDueWhenZero = true;
				testInput.ShouldOutputVATDueWhenZero = true;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("Header has Total Duties and Total VAT", expected, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM'
CST++:117:ZZZ'
FTX+LIN+++0'
TDT+20'
UNS+D'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:1000.00'
TAX+3+TVD:107:ZZZ'
MOA+161:0.00'
UNT+14+<<MSGNO PLACEHOLDER>>'";

				testInput.TotalDutiesDue = 1000;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("Header has Total Duties and Total VAT", expected, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM'
CST++:117:ZZZ'
FTX+LIN+++0'
TDT+20'
UNS+D'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:0.00'
TAX+3+TVD:107:ZZZ'
MOA+161:1000.00'
UNT+14+<<MSGNO PLACEHOLDER>>'";

				testInput.TotalDutiesDue = 0;
				testInput.TotalVATDue = 1000;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("Header has Total Duties and Total VAT", expected, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM'
CST++:117:ZZZ'
FTX+LIN+++0'
TDT+20'
UNS+D'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:1000.00'
UNT+12+<<MSGNO PLACEHOLDER>>'";

				testInput.TotalDutiesDue = 1000;
				testInput.TotalVATDue = 0;
				testInput.ShouldOutputDutiesDueWhenZero = false;
				testInput.ShouldOutputVATDueWhenZero = false;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("Header has Total Duties and Total VAT", expected, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM'
CST++:117:ZZZ'
FTX+LIN+++0'
TDT+20'
UNS+D'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TVD:107:ZZZ'
MOA+161:0.00'
UNT+12+<<MSGNO PLACEHOLDER>>'";

				testInput.TotalDutiesDue = 0;
				testInput.TotalVATDue = 0;
				testInput.ShouldOutputDutiesDueWhenZero = false;
				testInput.ShouldOutputVATDueWhenZero = true;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("Header has Total Duties and Total VAT", expected, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM'
CST++:117:ZZZ'
FTX+LIN+++0'
TDT+20'
UNS+D'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:1000.00'
TAX+3+TVD:107:ZZZ'
MOA+161:1000.00'
UNT+14+<<MSGNO PLACEHOLDER>>'";

				testInput.TotalDutiesDue = 1000;
				testInput.TotalVATDue = 1000;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("Header has Total Duties and Total VAT", expected, result.Replace("'", "'\n"));
			});

			CombineAssertions(() =>
			{
				string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM'
CST++:117:ZZZ'
FTX+LIN+++0'
TDT+20'
UNS+D'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:1000.00'
TAX+3+TVD:107:ZZZ'
MOA+161:1000.00'
UNT+14+<<MSGNO PLACEHOLDER>>'";

				testInput.IsIntoWarehouseWarehousing = true;
				testInput.TotalDutiesDue = 1000;
				testInput.TotalVATDue = 1000;
				testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				result = testBuilder.GenerateMessageBody();
				AssertMultilineASCIIEquals("Header has Total Duties and Total VAT", expected, result.Replace("'", "'\n"));
			});
		}

		public void TestProvisionalPaymentForDiamondLevyDeclaration()
		{
			var input = new CUSDECMessageDataProviderForTest
			{
				DeclarationType = DeclarationTypeList.Codes.RegularCompleteDeclarationDefault,
				ShipmentType = "EXP",
				TotalDutiesDue = 111.11m + 222.22m,

				LineLevelDetails = new[]
				{
					new LineLevelInformationForTest
					{
						DutiesAndFees = Array.Empty<DutyFeeInformationForTest>(),
						AdditionalInformations = new [] { new AdditionalInformationForTest { Code = "DLV", Value = "111" } },
						ProvisionalPayments = new[] { new DutyFeeInformationForTest { Code = "DLA", Value = 111.11 } }
					},
					new LineLevelInformationForTest
					{
						DutiesAndFees = Array.Empty<DutyFeeInformationForTest>(),
						AdditionalInformations = new [] { new AdditionalInformationForTest { Code = "DLV", Value = "222" } },
						ProvisionalPayments = new[] { new DutyFeeInformationForTest { Code = "DLA", Value = 222.22 } }
					}
				}
			};
			var builder = new CUSDECMessageTextBuilderForTest(input);

			var result = builder.GenerateMessageBody();

			string expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+EXP:::RCD'
CST++:117:ZZZ'
FTX+LIN+++0'
TDT+20'
UNS+D'
CST++:108:ZZZ'
FTX+ACB+++DLV111'
MOA+40:0'
TAX+1+DLA:107:ZZZ'
MOA+161:111.11'
CST++:108:ZZZ'
FTX+ACB+++DLV222'
MOA+40:0'
TAX+1+DLA:107:ZZZ'
MOA+161:222.22'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:333.33'
UNT+22+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals("Expecting 2xDLA & 1xTDD", expected, result.Replace("'", "'\n"));
		}

		public void TestAdditionalInformation_CodeOrder_ForDiamondLevyDeclaration()
		{
			#region Prepare Test Scenarios:

			var testScenarios = new List<AdditionalInformation_TestScenario>()
			{
				prepareAdditionalInformation_TestScenario_ForDiamondLevyDeclaration_01(),
				prepareAdditionalInformation_TestScenario_ForDiamondLevyDeclaration_02(),
				prepareAdditionalInformation_TestScenario_ForDiamondLevyDeclaration_03(),
				prepareAdditionalInformation_TestScenario_ForDiamondLevyDeclaration_04(),
				prepareAdditionalInformation_TestScenario_ForDiamondLevyDeclaration_05()
			};

			#endregion Prepare Test Scenarios.

			var testInput = new CUSDECMessageDataProviderForTest();

			Action<AdditionalInformation_TestScenario> testAction = scenario =>
			{
				testInput.LineLevelDetails = scenario.GetLineLevelDetails();
				var testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				var messageBody = testBuilder.GenerateMessageBody();
				CheckOrderOfAdditionalInformationCodes(scenario, messageBody);
			};

			testScenarios.ForEach(testAction);
		}

		public void TestAdditionalInformation_TwoLetterROOCode()
		{
			#region Prepare Test Scenarios:
			var zaTestHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("UK");
			zaTestHelper.CreateAdditionalInformationCusCodeEntry("PPR");
			Factory.Save();

			var testScenarios = new List<AdditionalInformation_TestScenario>()
			{
				prepareAdditionalInformation_TestScenario_ForROOCode(),
				prepareAdditionalInformation_TestScenario_ForNonROOCode(),
			};

			#endregion Prepare Test Scenarios.

			var testInput = new CUSDECMessageDataProviderForTest
			{
				DateOfAssessment = ZDateTime.Today
			};

			void testAction(AdditionalInformation_TestScenario scenario)
			{
				testInput.LineLevelDetails = scenario.GetLineLevelDetails(Factory);

				var testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				var messageBody = testBuilder.GenerateMessageBody();
				CheckOrderOfAdditionalInformationCodes(scenario, messageBody);
			}

			testScenarios.ForEach(testAction);
		}

		public void TestTotalTransactionValueDecimalPlaces()
		{
			CombineAssertions(() =>
			{
				var testInput = new CUSDECMessageDataProviderForTest();
				testInput.TotalTransactionValue = 123m;
				testInput.TotalTransactionValueCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
				var testBuilder = new CUSDECMessageTextBuilderForTest(testInput);
				var result = testBuilder.GenerateMessageBody();
				AssertContains("Output no decimal place", "TAX+3+TRN:107:ZZZ'MOA+161:123:ZAR'", result);

				testInput.TotalTransactionValue = 123.4m;
				result = testBuilder.GenerateMessageBody();
				AssertContains("Output one decimal place", "TAX+3+TRN:107:ZZZ'MOA+161:123.4:ZAR'", result);

				testInput.TotalTransactionValue = 123.45m;
				result = testBuilder.GenerateMessageBody();
				AssertContains("Output two decimal places", "TAX+3+TRN:107:ZZZ'MOA+161:123.45:ZAR'", result);
			});
		}

		public void TestPopulateInvoiceDetails()
		{
			var input = new CUSDECMessageDataProviderForTest
			{
				ShipmentType = ZAJobMessageTypeList.Codes.Import,
				ExchangeRateDateTime = new ZDateTime(2024, 8, 1),
				DateForDuty = new ZDateTime(2024, 8, 1),
				ShippedOnBoardDate = new ZDateTime(2024, 8, 21),
				PaymentTerms = "23",
				InvoiceHeaderInformations = new[]
				{
					new InvoiceHeaderInformationForTest
					{
						InvoiceNumber = "INV-1234-1",
						InvoiceDate = new ZDateTime(2024, 8, 1),
						InvoiceCurrencyCoded = Core.Constants.CurrencyCodes.SouthAfrica,
						TotalInvoiceAmount = 123.45m,
						TotalChargesInLocalCurrency = 23.45m,
						CommonFactor = 1234567.901234567m,
						ExchangeRate = 0.1234m,
						TermsOfDelivery = Core.Constants.IncoTerms.FreeOnBoard,
						NameOfIssuer = "SUPPLIER A FULL NAME",
						Address1 = "SUP A ADDRESS 1",
						Address2 = "SUP A ADDRESS 2",
						Address3 = "SUP A CITY",
						Address4 = "SUP A STATE",
						CountryOfIssuer = Core.Constants.CountryCodes.SouthAfrica,
						AdvancePaymentAmount = 12.34m,
						AdvancePaymentCurrencyCode = Core.Constants.CurrencyCodes.SouthAfrica,
						AdvancePaymentNotificationDetails = null,
						InvoiceChargeInformations = new []
						{
							new InvoiceChargeInformationForTest
							{
								MonetaryAmountChargeType = MonetaryAmountTypeQualifierList.OtherCosts.ToString(),
								ChargeAmount = 1.23m,
								ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica,
								ChargeCurrencyConversionRate = 1.0m
							},
							new InvoiceChargeInformationForTest
							{
								MonetaryAmountChargeType = MonetaryAmountTypeQualifierList.OtherCosts.ToString(),
								ChargeAmount = 2.34m,
								ChargeCurrency = Core.Constants.CurrencyCodes.UnitedStates,
								ChargeCurrencyConversionRate = 0.05602m,
								IsOtherCharge = true,
								ChargeDescription = "SPECIAL CHARGE FOR BRAND A"
							},
							new InvoiceChargeInformationForTest
							{
								MonetaryAmountChargeType = ZString.Empty,
								ChargeAmount = 3.45m,
								ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica,
								ChargeCurrencyConversionRate = 1.0m,
								ChargeDescription = "Unmapped Charge Type"
							}
						},
						InvoiceLineInformations = new []
						{
							new InvoiceLineInformationForTest
							{
								InvoiceLineNumber = 1,
								RelatedDeclarationLineNumber = 1,
								CommercialInvoiceItemDescription = "ITEM A",
								ProductCode = "PART A",
								Quantity = 11m,
								QuantityUnit = "KG",
								PriceDetails = 12.3m,
								ItemAmount = 135.3m,
								RateDetails = 11.1m,
								BrandName = "BRAND A",
								InvoiceLineChargeInformations = new []
								{
									new InvoiceLineChargeInformationForTest
									{
										MonetaryDiscountAmount = 1.23m,
										ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica
									}
								}
							}
						}
					},
					new InvoiceHeaderInformationForTest
					{
						InvoiceNumber = "INV-1234-2",
						InvoiceDate = new ZDateTime(2024, 8, 2),
						InvoiceCurrencyCoded = Core.Constants.CurrencyCodes.SouthAfrica,
						TotalInvoiceAmount = 234.56m,
						TotalChargesInLocalCurrency = 34.56m,
						CommonFactor = 2.345678901m,
						ExchangeRate = 0.2345m,
						TermsOfDelivery = Core.Constants.IncoTerms.ExWorks,
						NameOfIssuer = "SUPPLIER FULL NAME B",
						Address1 = "SUP B ADDRESS 1",
						Address2 = "SUP B ADDRESS 2",
						Address3 = "SUP B CITY",
						Address4 = "SUP B STATE",
						CountryOfIssuer = Core.Constants.CountryCodes.SouthAfrica,
						AdvancePaymentAmount = 23.45m,
						AdvancePaymentCurrencyCode = Core.Constants.CurrencyCodes.SouthAfrica,
						AdvancePaymentNotificationDetails = new ZString[] { "APN-21",  "APN-22" },
						InvoiceChargeInformations = new []
						{
							new InvoiceChargeInformationForTest
							{
								MonetaryAmountChargeType = MonetaryAmountTypeQualifierList.LandingCharges.ToString(),
								ChargeAmount = 3.45m,
								ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica,
								ChargeCurrencyConversionRate = 1.0m
							},
							new InvoiceChargeInformationForTest
							{
								MonetaryAmountChargeType = MonetaryAmountTypeQualifierList.OtherCosts.ToString(),
								ChargeAmount = 4.56m,
								ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica,
								ChargeCurrencyConversionRate = 1.0m,
								IsOtherCharge = true,
								ChargeDescription = "SPECIAL CHARGE FOR INVOICE 2"
							}
						},
						InvoiceLineInformations = new []
						{
							new InvoiceLineInformationForTest
							{
								InvoiceLineNumber = 1,
								RelatedDeclarationLineNumber = 2,
								CommercialInvoiceItemDescription = "VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG ITEM B",
								Quantity = 22m,
								QuantityUnit = "T",
								PriceDetails = 23.4m,
								ItemAmount = 514.8m,
								RateDetails = 22.2m,
								BrandName = "BRAND B"
							},
							new InvoiceLineInformationForTest
							{
								InvoiceLineNumber = 2,
								RelatedDeclarationLineNumber = 3,
								CommercialInvoiceItemDescription = "ITEM C",
								Quantity = 33m,
								QuantityUnit = "G",
								PriceDetails = 3.45m,
								ItemAmount = 113.85m,
								RateDetails = 33.3m,
								BrandName = "BRAND C",
								InvoiceLineChargeInformations = new []
								{
									new InvoiceLineChargeInformationForTest
									{
										MonetaryDiscountAmount = 3.33m,
										ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica
									}
								}
							}
						}
					}
				},
				ShouldOutputInvoiceDetails = true
			};
			var builder = new CUSDECMessageTextBuilderForTest(input);
			var result = builder.GenerateMessageBody();

			var expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+IMP'
CST++:117:ZZZ'
DTM+111:20240821:102'
FTX+LIN+++0'
TDT+20'
UNS+D'
DMS+INV-1234-1+380'
DTM+3:20240801:102'
MOA+39:123.45:ZAR'
CUX+++0.1234'
DTM+134:20240801:102'
MOA+259:23.45:ZAR'
CUX++5+1234567.9012345'
TOD+6++FOB'
NAD+II++SUPPLIER A FULL NAME:SUP A ADDRESS 1:SUP A ADDRESS 2:SUP A CITY:SUP A STATE++++++ZA'
PAT+1++9::D:23'
MOA+74:12.34:ZAR'
ALC+C'
MOA+160:1.23:ZAR'
CUX+1:ZAR++1.0000'
ALC+C++++:::SPECIAL CHARGE FOR BRAND A'
MOA+160:2.34:USD'
CUX+1:USD++0.0560'
LIN+1+++:1'
PIA+5+PART A'
QTY+47:11.0000:KG'
PRI+INV:12.3000'
MOA+38:135.30'
ALC+A'
RTE+1:11'
MOA+52:1.2300:ZAR'
IMD+A++:::BRAND A'
FTX+IND+++ITEM A'
DMS+INV-1234-2+380'
DTM+3:20240802:102'
MOA+39:234.56:ZAR'
CUX+++0.2345'
DTM+134:20240801:102'
MOA+259:34.56:ZAR'
CUX++5+2.345678901'
TOD+6++EXW'
NAD+II++SUPPLIER FULL NAME B:SUP B ADDRESS 1:SUP B ADDRESS 2:SUP B CITY:SUP B STATE++++++ZA'
PAT+1++9::D:23'
MOA+74:23.45:ZAR'
FTX+PMT+++APN-21:APN-22'
ALC+C'
MOA+78:3.45:ZAR'
CUX+1:ZAR++1.0000'
ALC+C++++:::SPECIAL CHARGE FOR INVOICE 2'
MOA+160:4.56:ZAR'
CUX+1:ZAR++1.0000'
LIN+1+++:2'
PIA+5'
QTY+47:22.0000:T'
PRI+INV:23.4000'
MOA+38:514.80'
ALC+A'
RTE+1:22'
IMD+A++:::BRAND B'
FTX+IND+++VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :ITEM B'
LIN+2+++:3'
PIA+5'
QTY+47:33.0000:G'
PRI+INV:3.4500'
MOA+38:113.85'
ALC+A'
RTE+1:33'
MOA+52:3.3300:ZAR'
IMD+A++:::BRAND C'
FTX+IND+++ITEM C'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
UNT+75+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals(expected, result.Replace("'", "'\n"));
		}

		public void TestPopulateInvoiceDetailsWithCreditTermsAndPaymentTerms()
		{
			var input = new CUSDECMessageDataProviderForTest
			{
				ShipmentType = ZAJobMessageTypeList.Codes.Import,
				ExchangeRateDateTime = new ZDateTime(2024, 8, 1),
				DateForDuty = new ZDateTime(2024, 8, 1),
				ShippedOnBoardDate = new ZDateTime(2024, 8, 21),
				PaymentTerms = "23",
				CreditTerms = CreditTermsCodeList.Codes.NEP,
				InvoiceHeaderInformations = new[]
				{
					new InvoiceHeaderInformationForTest
					{
						InvoiceNumber = "INV-1234-1",
						InvoiceDate = new ZDateTime(2024, 8, 1),
						InvoiceCurrencyCoded = Core.Constants.CurrencyCodes.SouthAfrica,
						TotalInvoiceAmount = 123.45m,
						TotalChargesInLocalCurrency = 23.45m,
						CommonFactor = 1234567.901234567m,
						ExchangeRate = 0.1234m,
						TermsOfDelivery = Core.Constants.IncoTerms.FreeOnBoard,
						NameOfIssuer = "SUPPLIER A FULL NAME",
						Address1 = "SUP A ADDRESS 1",
						Address2 = "SUP A ADDRESS 2",
						Address3 = "SUP A CITY",
						Address4 = "SUP A STATE",
						CountryOfIssuer = Core.Constants.CountryCodes.SouthAfrica,
						AdvancePaymentAmount = 12.34m,
						AdvancePaymentCurrencyCode = Core.Constants.CurrencyCodes.SouthAfrica,
						AdvancePaymentNotificationDetails = null,
						InvoiceChargeInformations = new []
						{
							new InvoiceChargeInformationForTest
							{
								MonetaryAmountChargeType = MonetaryAmountTypeQualifierList.OtherCosts.ToString(),
								ChargeAmount = 1.23m,
								ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica,
								ChargeCurrencyConversionRate = 1.0m
							},
							new InvoiceChargeInformationForTest
							{
								MonetaryAmountChargeType = MonetaryAmountTypeQualifierList.OtherCosts.ToString(),
								ChargeAmount = 2.34m,
								ChargeCurrency = Core.Constants.CurrencyCodes.UnitedStates,
								ChargeCurrencyConversionRate = 0.05602m,
								IsOtherCharge = true,
								ChargeDescription = "SPECIAL CHARGE FOR BRAND A"
							},
							new InvoiceChargeInformationForTest
							{
								MonetaryAmountChargeType = ZString.Empty,
								ChargeAmount = 3.45m,
								ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica,
								ChargeCurrencyConversionRate = 1.0m,
								ChargeDescription = "Unmapped Charge Type"
							}
						},
						InvoiceLineInformations = new []
						{
							new InvoiceLineInformationForTest
							{
								InvoiceLineNumber = 1,
								RelatedDeclarationLineNumber = 1,
								CommercialInvoiceItemDescription = "ITEM A",
								ProductCode = "PART A",
								Quantity = 11m,
								QuantityUnit = "KG",
								PriceDetails = 12.3m,
								ItemAmount = 135.3m,
								RateDetails = 11.1m,
								BrandName = "BRAND A",
								InvoiceLineChargeInformations = new []
								{
									new InvoiceLineChargeInformationForTest
									{
										MonetaryDiscountAmount = 1.23m,
										ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica
									}
								}
							}
						}
					},
					new InvoiceHeaderInformationForTest
					{
						InvoiceNumber = "INV-1234-2",
						InvoiceDate = new ZDateTime(2024, 8, 2),
						InvoiceCurrencyCoded = Core.Constants.CurrencyCodes.SouthAfrica,
						TotalInvoiceAmount = 234.56m,
						TotalChargesInLocalCurrency = 34.56m,
						CommonFactor = 2.345678901m,
						ExchangeRate = 0.2345m,
						TermsOfDelivery = Core.Constants.IncoTerms.ExWorks,
						NameOfIssuer = "SUPPLIER FULL NAME B",
						Address1 = "SUP B ADDRESS 1",
						Address2 = "SUP B ADDRESS 2",
						Address3 = "SUP B CITY",
						Address4 = "SUP B STATE",
						CountryOfIssuer = Core.Constants.CountryCodes.SouthAfrica,
						AdvancePaymentAmount = 23.45m,
						AdvancePaymentCurrencyCode = Core.Constants.CurrencyCodes.SouthAfrica,
						AdvancePaymentNotificationDetails = new ZString[] { "APN-21",  "APN-22" },
						InvoiceChargeInformations = new []
						{
							new InvoiceChargeInformationForTest
							{
								MonetaryAmountChargeType = MonetaryAmountTypeQualifierList.LandingCharges.ToString(),
								ChargeAmount = 3.45m,
								ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica,
								ChargeCurrencyConversionRate = 1.0m
							},
							new InvoiceChargeInformationForTest
							{
								MonetaryAmountChargeType = MonetaryAmountTypeQualifierList.OtherCosts.ToString(),
								ChargeAmount = 4.56m,
								ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica,
								ChargeCurrencyConversionRate = 1.0m,
								IsOtherCharge = true,
								ChargeDescription = "SPECIAL CHARGE FOR INVOICE 2"
							}
						},
						InvoiceLineInformations = new []
						{
							new InvoiceLineInformationForTest
							{
								InvoiceLineNumber = 1,
								RelatedDeclarationLineNumber = 2,
								CommercialInvoiceItemDescription = "VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG ITEM B",
								Quantity = 22m,
								QuantityUnit = "T",
								PriceDetails = 23.4m,
								ItemAmount = 514.8m,
								RateDetails = 22.2m,
								BrandName = "BRAND B"
							},
							new InvoiceLineInformationForTest
							{
								InvoiceLineNumber = 2,
								RelatedDeclarationLineNumber = 3,
								CommercialInvoiceItemDescription = "ITEM C",
								Quantity = 33m,
								QuantityUnit = "G",
								PriceDetails = 3.45m,
								ItemAmount = 113.85m,
								RateDetails = 33.3m,
								BrandName = "BRAND C",
								InvoiceLineChargeInformations = new []
								{
									new InvoiceLineChargeInformationForTest
									{
										MonetaryDiscountAmount = 3.33m,
										ChargeCurrency = Core.Constants.CurrencyCodes.SouthAfrica
									}
								}
							}
						}
					}
				},
				ShouldOutputInvoiceDetails = true
			};
			var builder = new CUSDECMessageTextBuilderForTest(input);
			var result = builder.GenerateMessageBody();

			var expected = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+IMP'
CST++:117:ZZZ'
DTM+111:20240821:102'
FTX+LIN+++0:NEP'
TDT+20'
UNS+D'
DMS+INV-1234-1+380'
DTM+3:20240801:102'
MOA+39:123.45:ZAR'
CUX+++0.1234'
DTM+134:20240801:102'
MOA+259:23.45:ZAR'
CUX++5+1234567.9012345'
TOD+6++FOB'
NAD+II++SUPPLIER A FULL NAME:SUP A ADDRESS 1:SUP A ADDRESS 2:SUP A CITY:SUP A STATE++++++ZA'
PAT+1++9::D:0'
MOA+74:12.34:ZAR'
ALC+C'
MOA+160:1.23:ZAR'
CUX+1:ZAR++1.0000'
ALC+C++++:::SPECIAL CHARGE FOR BRAND A'
MOA+160:2.34:USD'
CUX+1:USD++0.0560'
LIN+1+++:1'
PIA+5+PART A'
QTY+47:11.0000:KG'
PRI+INV:12.3000'
MOA+38:135.30'
ALC+A'
RTE+1:11'
MOA+52:1.2300:ZAR'
IMD+A++:::BRAND A'
FTX+IND+++ITEM A'
DMS+INV-1234-2+380'
DTM+3:20240802:102'
MOA+39:234.56:ZAR'
CUX+++0.2345'
DTM+134:20240801:102'
MOA+259:34.56:ZAR'
CUX++5+2.345678901'
TOD+6++EXW'
NAD+II++SUPPLIER FULL NAME B:SUP B ADDRESS 1:SUP B ADDRESS 2:SUP B CITY:SUP B STATE++++++ZA'
PAT+1++9::D:0'
MOA+74:23.45:ZAR'
FTX+PMT+++APN-21:APN-22'
ALC+C'
MOA+78:3.45:ZAR'
CUX+1:ZAR++1.0000'
ALC+C++++:::SPECIAL CHARGE FOR INVOICE 2'
MOA+160:4.56:ZAR'
CUX+1:ZAR++1.0000'
LIN+1+++:2'
PIA+5'
QTY+47:22.0000:T'
PRI+INV:23.4000'
MOA+38:514.80'
ALC+A'
RTE+1:22'
IMD+A++:::BRAND B'
FTX+IND+++VERY LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :ITEM B'
LIN+2+++:3'
PIA+5'
QTY+47:33.0000:G'
PRI+INV:3.4500'
MOA+38:113.85'
ALC+A'
RTE+1:33'
MOA+52:3.3300:ZAR'
IMD+A++:::BRAND C'
FTX+IND+++ITEM C'
UNS+S'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
UNT+75+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals(expected, result.Replace("'", "'\n"));
		}

		void CheckOrderOfAdditionalInformationCodes(AdditionalInformation_TestScenario scenario, string messageBody)
		{
			var assertMsg = "Check order of additional information codes - for diamond levy declarations.";
			string ftxSegment = null;
			var start = messageBody.IndexOf("FTX+ACB");

			if (start > -1)
			{
				var endsAt = messageBody.IndexOf("'", start + 3);
				if (endsAt > -1)
				{
					ftxSegment = messageBody.Substring(start, endsAt - start);
				}
			}
			AssertEquals(assertMsg, scenario.ExpectedText, ftxSegment);
		}

		AdditionalInformation_TestScenario prepareAdditionalInformation_TestScenario_ForDiamondLevyDeclaration_01()
		{
			var scenario = new AdditionalInformation_TestScenario();

			scenario.AddAdditionalInformation("NUI", "N");              // New/Used Indicator
			scenario.AddAdditionalInformation("KBC", "za 1117950");     // Kimberley Certificate
			scenario.AddAdditionalInformation("DPX", "0435");           // Diamond Producer Exemption
			scenario.AddAdditionalInformation("DDL", "ap09255");        // Diamond Dealer License
			scenario.AddAdditionalInformation("DLV", "135305");         // Diamond Levy Value

			scenario.ExpectedText = "FTX+ACB+++KBCZA 1117950:DLV135305:NUIN:DPX0435:DDLAP09255";
			return scenario;
		}

		AdditionalInformation_TestScenario prepareAdditionalInformation_TestScenario_ForDiamondLevyDeclaration_02()
		{
			var scenario = new AdditionalInformation_TestScenario();

			scenario.AddAdditionalInformation("AAA", "VAL-A");
			scenario.AddAdditionalInformation("DLV", "123456");         // Diamond Levy Value
			scenario.AddAdditionalInformation("BBB", "VAL-B");
			scenario.AddAdditionalInformation("KBC", "kim-cert");       // Kimberley Certificate
			scenario.AddAdditionalInformation("CCC", "VAL-C");

			scenario.ExpectedText = "FTX+ACB+++KBCKIM-CERT:DLV123456:AAAVAL-A:BBBVAL-B:CCCVAL-C";
			return scenario;
		}

		AdditionalInformation_TestScenario prepareAdditionalInformation_TestScenario_ForDiamondLevyDeclaration_03()
		{
			var scenario = new AdditionalInformation_TestScenario();

			scenario.AddAdditionalInformation("AAA", "VAL-A");
			scenario.AddAdditionalInformation("BBB", "VAL-B");
			scenario.AddAdditionalInformation("KBC", "kim-cert");       // Kimberley Certificate

			scenario.ExpectedText = "FTX+ACB+++KBCKIM-CERT:AAAVAL-A:BBBVAL-B";
			return scenario;
		}

		AdditionalInformation_TestScenario prepareAdditionalInformation_TestScenario_ForDiamondLevyDeclaration_04()
		{
			var scenario = new AdditionalInformation_TestScenario();

			scenario.AddAdditionalInformation("AAA", "VAL-A");
			scenario.AddAdditionalInformation("DLV", "789123");     // Diamond Levy Value
			scenario.AddAdditionalInformation("BBB", "VAL-B");
			scenario.AddAdditionalInformation("CCC", "VAL-C");

			scenario.ExpectedText = "FTX+ACB+++DLV789123:AAAVAL-A:BBBVAL-B:CCCVAL-C";
			return scenario;
		}

		AdditionalInformation_TestScenario prepareAdditionalInformation_TestScenario_ForDiamondLevyDeclaration_05()
		{
			var scenario = new AdditionalInformation_TestScenario();

			scenario.AddAdditionalInformation("AAA", "VAL-A");
			scenario.AddAdditionalInformation("BBB", "VAL-B");
			scenario.AddAdditionalInformation("CCC", "VAL-C");
			scenario.AddAdditionalInformation("DDD", "VAL-D");
			scenario.AddAdditionalInformation("EEE", "VAL-E");

			scenario.ExpectedText = "FTX+ACB+++AAAVAL-A:BBBVAL-B:CCCVAL-C:DDDVAL-D:EEEVAL-E";
			return scenario;
		}

		AdditionalInformation_TestScenario prepareAdditionalInformation_TestScenario_ForROOCode()
		{
			var scenario = new AdditionalInformation_TestScenario();
			scenario.AddAdditionalInformation("UK", "123"); // two letter ROO code
			scenario.ExpectedText = "FTX+ACB+++UK 123";

			return scenario;
		}

		AdditionalInformation_TestScenario prepareAdditionalInformation_TestScenario_ForNonROOCode()
		{
			var scenario = new AdditionalInformation_TestScenario();
			scenario.AddAdditionalInformation("PPR", "123"); // 3 letter non ROO code
			scenario.ExpectedText = "FTX+ACB+++PPR123";

			return scenario;
		}

		void FillHeaderLevelInformation(CUSDECMessageDataProviderForTest testInput)
		{
			#region BGM
			testInput.AgentCode = "00626166";
			testInput.LocalReferenceNumber = "LRN";
			testInput.ShipmentType = "929";
			testInput.PartClearanceQuantity = 1;
			testInput.MessageType = "9";
			#endregion

			#region CST
			testInput.CustomsProcedureCategory = "A";
			#endregion

			#region LOC
			testInput.TransportDocumentIssuedAt = "POLOA";
			testInput.LocationOfGoods = "LOCGO";
			testInput.FromWarehouse = "EXWHS";
			testInput.ToWarehouse = "TOWHS";
			testInput.Consignee = "CONSIGNEE";
			testInput.PortOfExit = "POEXI";
			testInput.CountryOfExport = "AU";
			testInput.CountryOfDestination = "ZA";
			testInput.CustomsOfficeCode = "DOC";
			#endregion

			#region DTM
			testInput.DateOfAssessment = new ZDateTime(2016, 02, 19);
			testInput.DateOfArrival = new ZDateTime(2016, 02, 20);
			testInput.DateOfDepartureOrDateOfFlight = new ZDateTime(2016, 02, 18);
			#endregion

			#region GIS
			testInput.RelatedPartyIndicator = "N";
			testInput.ValuationCode = "1";
			testInput.PaymentMethod = "1";
			testInput.GrossWeightInKG = 55.567;
			#endregion

			#region MEA
			testInput.GrossWeightInKG = 5000.46;
			#endregion

			#region LIN
			testInput.TotalLineCount = 3;
			testInput.CreditTerms = "1";
			testInput.VATIndicator = "2";
			testInput.TransactionBankCode = "TRANBKCODE";
			testInput.ChangeAcknowledgementIndicator = "1";
			#endregion

			#region SG1
			testInput.HouseBill = "HBOL01";
			testInput.HouseBillIssuedDate = new ZDateTime(2015, 01, 01);
			testInput.OriginalMRN = "ORGMRN";
			testInput.MRNToBeReplaced = "MRNToBeRep";
			testInput.TransportDocumentNumber = "MBOL01";
			testInput.TransportDocumentDate = new ZDateTime(2015, 01, 02);
			testInput.ShouldOutputFinancialAccountNumber = ZBool.True;
			testInput.FinancialAccountNumber = "FINACCNUM";
			testInput.UniqueConsignmentReference = "UCR";
			testInput.MessageNumber = "JOBNUM";
			testInput.CaseNumber = "CASENUM";
			#endregion

			#region SG1/SG2
			testInput.TotalNoOfPacks = "200";
			#endregion

			#region SG1/SG2/SG3
			testInput.MarksAndNumbers = new ZString[] { "MARK1", "MARK2", "MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3MARK3" };
			#endregion

			#region SG4
			testInput.TransportMode = "4";
			testInput.VoyageFlightNo = "JQ841";
			testInput.RemovalTransportMode = "1";
			testInput.TransportName = "VESSEL";
			#endregion

			#region SG6
			testInput.AgentDualProfileCode = "TST";
			testInput.OwnerCode = "BOC";
			testInput.RemoverTransporterCode = "RTC";
			#endregion

			#region SG49
			testInput.TotalCIFCAmount = 200.202;
			testInput.TotalTransactionValue = 700.707;
			testInput.TotalDutiesDue = 300.303;
			testInput.TotalVATDue = 400.404;
			testInput.OverpaidExcise = 500.505;
			testInput.UnderpaidExcise = 600.606;
			testInput.TotalCustomsValue = 800.808;
			#endregion
		}

		#region Expected Output Message Text Strings:

		string ExpectedOutput_HeaderLevelInformationForExportOnly { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+830+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+IB:MRNTOBEREP'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+53+<<MSGNO PLACEHOLDER>>'
";

		string ExpectedOutput_HeaderLevelInformationOnly { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+IB:MRNTOBEREP'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+53+<<MSGNO PLACEHOLDER>>'
";

		string ExpectedOutput_WithInvoiceInfoForExport { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+830+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+IB:MRNTOBEREP'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+57+<<MSGNO PLACEHOLDER>>'
";

		string ExpectedOutput_WithInvoiceInfo { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+IB:MRNTOBEREP'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+57+<<MSGNO PLACEHOLDER>>'
";

		string ExpectedOutput_WithContainerInfo { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
EQD+CN+CONT1+:::SEAL1+++4'
EQD+CN+CONT2+:::SEAL1          SEAL2+++5'
EQD+CN+CONT3+:::               SEAL2+++7'
EQD+CN+CONT4+:::SEAL1SEALASEALB+++8'
EQD+CN+CONT5+:::SEAL1SEALASEALB'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+IB:MRNTOBEREP'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+62+<<MSGNO PLACEHOLDER>>'
";

		string ExpectedOutput_DutiesAndFeesNoZeros { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
EQD+CN+CONT1+:::SEAL1+++4'
EQD+CN+CONT2+:::SEAL1          SEAL2+++5'
EQD+CN+CONT3+:::               SEAL2+++7'
EQD+CN+CONT4+:::SEAL1SEALASEALB+++8'
EQD+CN+CONT5+:::SEAL1SEALASEALB'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+IB:MRNTOBEREP'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
CST+1+020110:108:ZZZ'
FTX+CCI+++11:00'
MOA+40:0'
TAX+1+1P1:107:ZZZ'
MOA+161:11.00'
TAX+1+12A:107:ZZZ'
MOA+161:12.10'
TAX+1+12B:107:ZZZ'
MOA+161:12.20'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+71+<<MSGNO PLACEHOLDER>>'";

		string ExpectedOutput_DutiesAndFeesWithZeros { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
EQD+CN+CONT1+:::SEAL1+++4'
EQD+CN+CONT2+:::SEAL1          SEAL2+++5'
EQD+CN+CONT3+:::               SEAL2+++7'
EQD+CN+CONT4+:::SEAL1SEALASEALB+++8'
EQD+CN+CONT5+:::SEAL1SEALASEALB'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+IB:MRNTOBEREP'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
CST+1+020110:108:ZZZ'
FTX+CCI+++11:00'
MOA+40:0'
TAX+1+1P1:107:ZZZ'
MOA+161:11.00'
TAX+1+12B:107:ZZZ'
MOA+161:12.20'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+69+<<MSGNO PLACEHOLDER>>'";

		string ExpectedOutput_WithLineInfo { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
EQD+CN+CONT1+:::SEAL1+++4'
EQD+CN+CONT2+:::SEAL1          SEAL2+++5'
EQD+CN+CONT3+:::               SEAL2+++7'
EQD+CN+CONT4+:::SEAL1SEALASEALB+++8'
EQD+CN+CONT5+:::SEAL1SEALASEALB'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+IB:MRNTOBEREP'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
CST+1+020110:108:ZZZ'
FTX+AAA+++THIS IS A LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :LONG LONG LONG LONG LONG LONG LONG LONG LONG DESCRIPTION'
FTX+ACB+++BND00000000000000000000000000000123:CVI00000000000000000000000000000201:RCV00000000000000000000000000000456:DCV00000000000000000000000000000402:TS5VALUETS5'
FTX+AAI+++TS6VALUETS6:TS7VALUETS77777777777777777'
FTX+CCI+++11:00:31506010002'
LOC+27+AU'
MEA+AAR++KG:101.00'
MEA+AAS++M3:202.00'
MEA+AAT++CQ:303.00'
MEA+AAF++KG:404'
NAD+WH+WPC'
MOA+38:200'
MOA+40:88'
RFF+WE:PTA201604198464646'
TAX+1+1P1:107:ZZZ'
MOA+161:11.00'
TAX+1+12A:107:ZZZ'
MOA+161:12.10'
TAX+1+12B:107:ZZZ'
MOA+161:12.20'
TAX+1+13A:107:ZZZ'
MOA+161:13.10'
TAX+1+13B:107:ZZZ'
MOA+161:13.20'
TAX+1+13C:107:ZZZ'
MOA+161:13.30'
TAX+1+13D:107:ZZZ'
MOA+161:13.40'
TAX+1+15A:107:ZZZ'
MOA+161:15.10'
TAX+1+15B:107:ZZZ'
MOA+161:15.20'
TAX+1+1P8:107:ZZZ'
MOA+161:18.00'
TAX+1+2P1:107:ZZZ'
MOA+161:21.00'
TAX+1+2P2:107:ZZZ'
MOA+161:22.00'
TAX+1+2P3:107:ZZZ'
MOA+161:23.00'
TAX+1+VAT:107:ZZZ'
MOA+161:44.44'
TAX+1+SUR:107:ZZZ'
MOA+161:55.55'
TAX+1+DLA:107:ZZZ'
MOA+161:77.77'
TAX+1+PEN:107:ZZZ'
MOA+161:66.66'
TAX+1+FOR:107:ZZZ'
MOA+161:99.99'
TAX+1+PPA:107:ZZZ'
MOA+161:99.88'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+114+<<MSGNO PLACEHOLDER>>'
";

		string ExportedOutput_WithOrgAddress { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
EQD+CN+CONT1+:::SEAL1+++4'
EQD+CN+CONT2+:::SEAL1          SEAL2+++5'
EQD+CN+CONT3+:::               SEAL2+++7'
EQD+CN+CONT4+:::SEAL1SEALASEALB+++8'
EQD+CN+CONT5+:::SEAL1SEALASEALB'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+IB:MRNTOBEREP'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+IM+IMPORTER1++TESTIMPORTER+1A/11 IMPORTER STREET+SYNDEY++PCI01'
RFF+VA:IMPVATREG'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+EX+EXPORTER++TESTEXPORTER+1A/11 EXPORTER STREET+EXPORT++PCE01'
RFF+VA:EXPPVATREG'
NAD+CN+CONSIGNEE1++TESTCONSIGNEE+1A/11 CONSIGNEE STREET+CON CITY++PCC01'
NAD+MS+TST'
NAD+BY+BOC'
NAD+DT+TAXPARTYIDENTIFICATION:167:ZZZ++TESTDECLARANT+1A/11 DECLARANT STREET LONG1 LONG2 :LONG3 LONG4 LONG5 LONG6 LONG7 LONG8: LONG9 LONG10 LONG11 LONG12 LONG13 +DECLARANT++PCD01'
UNS+D'
CST+1+020110:108:ZZZ'
FTX+AAA+++THIS IS A LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :LONG LONG LONG LONG LONG LONG LONG LONG LONG DESCRIPTION'
FTX+ACB+++BND00000000000000000000000000000123:CVI00000000000000000000000000000201:RCV00000000000000000000000000000456:DCV00000000000000000000000000000402:TS5VALUETS5'
FTX+AAI+++TS6VALUETS6:TS7VALUETS77777777777777777'
FTX+CCI+++11:00:31506010002'
LOC+27+AU'
MEA+AAR++KG:101.00'
MEA+AAS++M3:202.00'
MEA+AAT++CQ:303.00'
MEA+AAF++KG:404'
NAD+WH+WPC'
MOA+38:200'
MOA+40:88'
RFF+WE:PTA201604198464646'
TAX+1+1P1:107:ZZZ'
MOA+161:11.00'
TAX+1+12A:107:ZZZ'
MOA+161:12.10'
TAX+1+12B:107:ZZZ'
MOA+161:12.20'
TAX+1+13A:107:ZZZ'
MOA+161:13.10'
TAX+1+13B:107:ZZZ'
MOA+161:13.20'
TAX+1+13C:107:ZZZ'
MOA+161:13.30'
TAX+1+13D:107:ZZZ'
MOA+161:13.40'
TAX+1+15A:107:ZZZ'
MOA+161:15.10'
TAX+1+15B:107:ZZZ'
MOA+161:15.20'
TAX+1+1P8:107:ZZZ'
MOA+161:18.00'
TAX+1+2P1:107:ZZZ'
MOA+161:21.00'
TAX+1+2P2:107:ZZZ'
MOA+161:22.00'
TAX+1+2P3:107:ZZZ'
MOA+161:23.00'
TAX+1+VAT:107:ZZZ'
MOA+161:44.44'
TAX+1+SUR:107:ZZZ'
MOA+161:55.55'
TAX+1+DLA:107:ZZZ'
MOA+161:77.77'
TAX+1+PEN:107:ZZZ'
MOA+161:66.66'
TAX+1+FOR:107:ZZZ'
MOA+161:99.99'
TAX+1+PPA:107:ZZZ'
MOA+161:99.88'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+120+<<MSGNO PLACEHOLDER>>'
";

		string ExportedOutput_MutexValue { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+122+CONSIGNEE::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
EQD+CN+CONT1+:::SEAL1+++4'
EQD+CN+CONT2+:::SEAL1          SEAL2+++5'
EQD+CN+CONT3+:::               SEAL2+++7'
EQD+CN+CONT4+:::SEAL1SEALASEALB+++8'
EQD+CN+CONT5+:::SEAL1SEALASEALB'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+IM+IMPORTER1++TESTIMPORTER+1A/11 IMPORTER STREET+SYNDEY++PCI01'
RFF+VA:IMPVATREG'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+EX+EXPORTER++TESTEXPORTER+1A/11 EXPORTER STREET+EXPORT++PCE01'
RFF+VA:EXPPVATREG'
NAD+CN+CONSIGNEE1++TESTCONSIGNEE+1A/11 CONSIGNEE STREET+CON CITY++PCC01'
NAD+MS+TST'
NAD+BY+BOC'
NAD+DT+TAXPARTYIDENTIFICATION:167:ZZZ++TESTDECLARANT+1A/11 DECLARANT STREET LONG1 LONG2 :LONG3 LONG4 LONG5 LONG6 LONG7 LONG8: LONG9 LONG10 LONG11 LONG12 LONG13 +DECLARANT++PCD01'
UNS+D'
CST+1+020110:108:ZZZ'
FTX+AAA+++THIS IS A LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :LONG LONG LONG LONG LONG LONG LONG LONG LONG DESCRIPTION'
FTX+ACB+++BND00000000000000000000000000000123:CVI00000000000000000000000000000201:RCV00000000000000000000000000000456:DCV00000000000000000000000000000402:TS5VALUETS5'
FTX+AAI+++TS6VALUETS6:TS7VALUETS77777777777777777'
FTX+CCI+++11:00:31506010002'
LOC+27+AU'
MEA+AAR++KG:101.00'
MEA+AAS++M3:202.00'
MEA+AAT++CQ:303.00'
MEA+AAF++KG:404'
NAD+WH+WPC'
MOA+38:200'
MOA+40:88'
RFF+WE:PTA201604198464646'
TAX+1+1P1:107:ZZZ'
MOA+161:11.00'
TAX+1+12A:107:ZZZ'
MOA+161:12.10'
TAX+1+12B:107:ZZZ'
MOA+161:12.20'
TAX+1+13A:107:ZZZ'
MOA+161:13.10'
TAX+1+13B:107:ZZZ'
MOA+161:13.20'
TAX+1+13C:107:ZZZ'
MOA+161:13.30'
TAX+1+13D:107:ZZZ'
MOA+161:13.40'
TAX+1+15A:107:ZZZ'
MOA+161:15.10'
TAX+1+15B:107:ZZZ'
MOA+161:15.20'
TAX+1+1P8:107:ZZZ'
MOA+161:18.00'
TAX+1+2P1:107:ZZZ'
MOA+161:21.00'
TAX+1+2P2:107:ZZZ'
MOA+161:22.00'
TAX+1+2P3:107:ZZZ'
MOA+161:23.00'
TAX+1+VAT:107:ZZZ'
MOA+161:44.44'
TAX+1+SUR:107:ZZZ'
MOA+161:55.55'
TAX+1+DLA:107:ZZZ'
MOA+161:77.77'
TAX+1+PEN:107:ZZZ'
MOA+161:66.66'
TAX+1+FOR:107:ZZZ'
MOA+161:99.99'
TAX+1+PPA:107:ZZZ'
MOA+161:99.88'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+117+<<MSGNO PLACEHOLDER>>'
";

		string ExportedOutput_NoSG1 { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+122+CONSIGNEE::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
EQD+CN+CONT1+:::SEAL1+++4'
EQD+CN+CONT2+:::SEAL1          SEAL2+++5'
EQD+CN+CONT3+:::               SEAL2+++7'
EQD+CN+CONT4+:::SEAL1SEALASEALB+++8'
EQD+CN+CONT5+:::SEAL1SEALASEALB'
FTX+LIN+++3:1:2:TRANBKCODE:1'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+IM+IMPORTER1++TESTIMPORTER+1A/11 IMPORTER STREET+SYNDEY++PCI01'
RFF+VA:IMPVATREG'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+EX+EXPORTER++TESTEXPORTER+1A/11 EXPORTER STREET+EXPORT++PCE01'
RFF+VA:EXPPVATREG'
NAD+CN+CONSIGNEE1++TESTCONSIGNEE+1A/11 CONSIGNEE STREET+CON CITY++PCC01'
NAD+MS+TST'
NAD+BY+BOC'
NAD+DT+TAXPARTYIDENTIFICATION:167:ZZZ++TESTDECLARANT+1A/11 DECLARANT STREET LONG1 LONG2 :LONG3 LONG4 LONG5 LONG6 LONG7 LONG8: LONG9 LONG10 LONG11 LONG12 LONG13 +DECLARANT++PCD01'
UNS+D'
CST+1+020110:108:ZZZ'
FTX+AAA+++THIS IS A LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :LONG LONG LONG LONG LONG LONG LONG LONG LONG DESCRIPTION'
FTX+ACB+++BND00000000000000000000000000000123:CVI00000000000000000000000000000201:RCV00000000000000000000000000000456:DCV00000000000000000000000000000402:TS5VALUETS5'
FTX+AAI+++TS6VALUETS6:TS7VALUETS77777777777777777'
FTX+CCI+++11:00:31506010002'
LOC+27+AU'
MEA+AAR++KG:101.00'
MEA+AAS++M3:202.00'
MEA+AAT++CQ:303.00'
MEA+AAF++KG:404'
NAD+WH+WPC'
MOA+38:200'
MOA+40:88'
RFF+WE:PTA201604198464646'
TAX+1+1P1:107:ZZZ'
MOA+161:11.00'
TAX+1+12A:107:ZZZ'
MOA+161:12.10'
TAX+1+12B:107:ZZZ'
MOA+161:12.20'
TAX+1+13A:107:ZZZ'
MOA+161:13.10'
TAX+1+13B:107:ZZZ'
MOA+161:13.20'
TAX+1+13C:107:ZZZ'
MOA+161:13.30'
TAX+1+13D:107:ZZZ'
MOA+161:13.40'
TAX+1+15A:107:ZZZ'
MOA+161:15.10'
TAX+1+15B:107:ZZZ'
MOA+161:15.20'
TAX+1+1P8:107:ZZZ'
MOA+161:18.00'
TAX+1+2P1:107:ZZZ'
MOA+161:21.00'
TAX+1+2P2:107:ZZZ'
MOA+161:22.00'
TAX+1+2P3:107:ZZZ'
MOA+161:23.00'
TAX+1+VAT:107:ZZZ'
MOA+161:44.44'
TAX+1+SUR:107:ZZZ'
MOA+161:55.55'
TAX+1+DLA:107:ZZZ'
MOA+161:77.77'
TAX+1+PEN:107:ZZZ'
MOA+161:66.66'
TAX+1+FOR:107:ZZZ'
MOA+161:99.99'
TAX+1+PPA:107:ZZZ'
MOA+161:99.88'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+106+<<MSGNO PLACEHOLDER>>'
";

		string ExpectedOutput_WithZeroAdditionalQuantity { get; } = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+LRN::00001+9'
CST++A:117:ZZZ'
LOC+14+LOCGO::ZZZ'
LOC+18+EXWHS::ZZZ'
LOC+45+POEXI::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+DOC::ZZZ'
LOC+9+POLOA::5'
DTM+141:20160219:102'
DTM+132:20160220:102'
DTM+178:20160218:102'
GIS+N1:127:ZZZ'
GIS+1:134:ZZZ'
MEA+AAE+AAD+KGM:5000.46'
EQD+CN+CONT1+:::SEAL1+++4'
EQD+CN+CONT2+:::SEAL1          SEAL2+++5'
EQD+CN+CONT3+:::               SEAL2+++7'
EQD+CN+CONT4+:::SEAL1SEALASEALB+++8'
EQD+CN+CONT5+:::SEAL1SEALASEALB'
FTX+LIN+++3:1:2:TRANBKCODE:1'
RFF+BH:HBOL01'
DTM+137:20150101:102'
RFF+ABT:ORGMRN'
RFF+IB:MRNTOBEREP'
RFF+AAS:MBOL01'
DTM+137:20150102:102'
RFF+ABI:FINACCNUM'
RFF+UCN:UCR'
RFF+ACD:JOBNUM'
RFF+AAV:CASENUM'
PAC+200'
PCI++MARK1'
PCI++MARK2'
PCI++MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3MARK3MARK3:MARK3MARK3MARK3MARK3MARK3'
TDT+20+JQ841+4:1+++++:::VESSEL'
DOC+380+INVN1'
DTM+3:20140101:102'
DOC+380+INVN2'
DTM+3:20140102:102'
NAD+AG+00626166'
NAD+AF+RTC'
NAD+MS+TST'
NAD+BY+BOC'
UNS+D'
CST+1+020110:108:ZZZ'
FTX+AAA+++THIS IS A LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG LONG :LONG LONG LONG LONG LONG LONG LONG LONG LONG DESCRIPTION'
FTX+ACB+++BND00000000000000000000000000000123:CVI00000000000000000000000000000201:RCV00000000000000000000000000000456:DCV00000000000000000000000000000402:TS5VALUETS5'
FTX+AAI+++TS6VALUETS6:TS7VALUETS77777777777777777'
FTX+CCI+++11:00:31506010002'
LOC+27+AU'
MEA+AAR++KG:101.00'
MEA+AAS++GJ:0.00'
MEA+AAT++CQ:303.00'
MEA+AAF++KG:404'
NAD+WH+WPC'
MOA+38:200'
MOA+40:88'
RFF+WE:PTA201604198464646'
TAX+1+1P1:107:ZZZ'
MOA+161:11.00'
TAX+1+12A:107:ZZZ'
MOA+161:12.10'
TAX+1+12B:107:ZZZ'
MOA+161:12.20'
TAX+1+13A:107:ZZZ'
MOA+161:13.10'
TAX+1+13B:107:ZZZ'
MOA+161:13.20'
TAX+1+13C:107:ZZZ'
MOA+161:13.30'
TAX+1+13D:107:ZZZ'
MOA+161:13.40'
TAX+1+15A:107:ZZZ'
MOA+161:15.10'
TAX+1+15B:107:ZZZ'
MOA+161:15.20'
TAX+1+1P8:107:ZZZ'
MOA+161:18.00'
TAX+1+2P1:107:ZZZ'
MOA+161:21.00'
TAX+1+2P2:107:ZZZ'
MOA+161:22.00'
TAX+1+2P3:107:ZZZ'
MOA+161:23.00'
TAX+1+VAT:107:ZZZ'
MOA+161:44.44'
TAX+1+SUR:107:ZZZ'
MOA+161:55.55'
TAX+1+DLA:107:ZZZ'
MOA+161:77.77'
TAX+1+PEN:107:ZZZ'
MOA+161:66.66'
TAX+1+FOR:107:ZZZ'
MOA+161:99.99'
TAX+1+PPA:107:ZZZ'
MOA+161:99.88'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:200'
TAX+3+TRN:107:ZZZ'
MOA+161:700.70'
TAX+3+TDD:107:ZZZ'
MOA+161:300.30'
TAX+3+TVD:107:ZZZ'
MOA+161:400.40'
TAX+3+AOP:107:ZZZ'
MOA+161:500.50'
TAX+3+AUP:107:ZZZ'
MOA+161:600.60'
TAX+3+CUS:107:ZZZ'
MOA+161:800'
UNT+114+<<MSGNO PLACEHOLDER>>'
";
		#endregion

	}

	sealed class AdditionalInformation_TestScenario
	{
		public void AddAdditionalInformation(string code, string value)
		{
			var addInfo = new AdditionalInformationForTest()
			{
				Code = code,
				Value = value
			};
			addInfoList.Add(addInfo);
		}

		readonly List<AdditionalInformationForTest> addInfoList = new List<AdditionalInformationForTest>();

		public List<ILineLevelInformation> GetLineLevelDetails(BusinessObjectFactory factory = null)
		{
			var lineLevelInfo = new LineLevelInformationForTest()
			{
				Factory = factory,
				CustomsProcedureCode = "11",
				LineNumber = "1",
				PreviousProcedureCode = "00",
				TariffCode = "020110",
				DutiesAndFees = new DutyFeeInformationForTest[]
				{
					new DutyFeeInformationForTest { Code = "1P1", Value = 11 },
					new DutyFeeInformationForTest { Code = "12A", Value = 12.1 },
					new DutyFeeInformationForTest { Code = "12B", Value = 12.2 },
				},
				ProvisionalPayments = Array.Empty<DutyFeeInformationForTest>(),
				AdditionalInformations = addInfoList
			};
			return new List<ILineLevelInformation>() { lineLevelInfo };
		}

		public string ExpectedText { get; set; }
	}

	sealed class CUSDECMessageTextBuilderForTest
	{
		public CUSDECMessageTextBuilderForTest(ICUSDECMessageDataProvider dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		readonly ICUSDECMessageDataProvider dataProvider;
		public string GenerateMessageBody()
		{
			var result = new Edifact.D96B.Messages.CUSDEC.CUSDECMessage();
			CUSDECMessageTextBuilder.PopulateCUSDECMessage(result, dataProvider);
			return result.ToString(new Edifact.UNOACharacterSet());
		}
	}
}
