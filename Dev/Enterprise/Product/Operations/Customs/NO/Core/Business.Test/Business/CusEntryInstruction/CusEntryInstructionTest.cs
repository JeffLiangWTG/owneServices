using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.NO;
using Enterprise.Customs.Universal.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryInstruction))]
sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
{
	#region ViewModel

	public void TestModelView()
	{
		AssertEquals(typeof(AutoNOCusEntryInstruction), typeof(CusEntryInstruction).BaseType);
	}

	[ExpectNoExceptions]
	public void TestAllAddInfoColumnsAreInModelView()
		=> ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(instruction,
			"NOCusEntryInstruction",
			schemaTypeName: nameof(AutoNOCusEntryInstruction.Schema));

	#endregion

	public void TestSupportsClone() => Assert(instruction.SupportsClone());

	public void TestLookups_Cached()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportCusEntryInstructionLookups>(instruction.Lookups);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportCusEntryInstructionLookups>(instruction.Lookups);
	}

	public void TestHasDigitollGoodsNumber() => CombineAssertions(() =>
	{
		var standaloneInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals("When instruction is stand-alone (CEI_JE is empty)", expected: false, standaloneInstruction.HasDigitollGoodsNumber);

		var declarationMock = Factory.NewMoq<JobDeclaration>();
		instruction.CEI_JE = declarationMock.Object.PK;
		declarationMock.SetupGetHasDigitollGoodsNumber(true);
		AssertEquals("When declaration HasDigitollGoodsNumber is true", expected: true, instruction.HasDigitollGoodsNumber);

		declarationMock.SetupGetHasDigitollGoodsNumber(false);
		AssertEquals("When declaration HasDigitollGoodsNumber is false", expected: false, instruction.HasDigitollGoodsNumber);
	});

	public void TestCEI_ProcedureMaxLength()
	{
		AssertEquals(4, instruction.CEI_ProcedureInfo.MaxLength);
	}

	public void TestDescriptionIsSetOnlyIfEmpty()
	{
		var manualDescription = "Manual Desc";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		instruction.CEI_Style = "7";
		instruction.CEI_Procedure = "7041";

		CombineAssertions(() =>
		{
			AssertEquals("Description is empty, should be set from CEI_Style", "Innlegg på tollager", instruction.CEI_Description);

			instruction.CEI_Description = manualDescription;
			instruction.CEI_Style = "4";
			instruction.CEI_Procedure = "4050";
			AssertEquals("Description is set, should not be set from CEI_Style", manualDescription, instruction.CEI_Description);
		});
	}

	public void TestDescriptionIsNotSetIfNotEmpty()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		instruction.CEI_Style = "7";
		instruction.CEI_Procedure = "7041";
		AssertEquals("Description should be set from CEI_Style", "Innlegg på tollager", instruction.CEI_Description);
	}

	public void TestValidationOfGoodsnumberAndPosition()
	{
		CombineAssertions(() =>
		{
			declaration.JE_GoodsNumber = "111";
			declaration.JE_Position = "222";
			AssertEquals(instruction.CEI_GoodsNumber, "111");
			AssertEquals(instruction.CEI_Position, "222");
		});
	}

	public void TestValidationOfGoodsSubPosition()
	{
		var instruction = Factory.New<CusEntryInstructionForTest>();
		instruction.CEI_SubPosition = "111";
		AssertEquals(instruction.GSPCusEntryNumberForTest.EntryNumber, "111");
	}

	public void TestValidationOfGoodsSubPosition_EmptyString()
	{
		var instruction = Factory.New<CusEntryInstructionForTest>();
		instruction.CEI_SubPosition = ZString.Empty;
		Assert(!instruction.GSPCusEntryNumberForTest.ExistsCusEntryNumber);
	}

	public void TestGetCustomsEntryHeaders()
	{
		AssertType<CusEntryHeaderCollection<CusEntryHeader>>(declaration.CustomsEntryHeaders);
	}

	public void TestEntryHeader()
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		AssertType<CusEntryHeader>(instruction.EntryHeader);
	}

	public void TestCEI_GoodsNumber_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(instruction.CEI_GoodsNumberInfo);
		AssertEquals("Goods Number", resourceStringData.Caption);
	}

	public void TestCEI_Position_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(instruction.CEI_PositionInfo);
		AssertEquals("Position", resourceStringData.Caption);
	}

	public void TestCEI_SubPosition_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(instruction.CEI_SubPositionInfo);
		AssertEquals("Sub Position", resourceStringData.Caption);
	}

	public void TestCEI_SubPositionMaxLength()
	{
		AssertEquals(5, instruction.CEI_SubPositionInfo.MaxLength);
	}

	public void TestInvoicesShouldBeLinkedToCorrectInstruction()
	{
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = ZGuid.Empty;
		invoiceLine2.JI_CEI = ZGuid.Empty;

		CombineAssertions(() =>
		{
			AssertEquals("No invoices", 0, instruction.Invoices.Count);

			invoiceLine1.JI_CEI = instruction.PK;
			AssertEquals("One invoice", 1, instruction.Invoices.Count);
			AssertEquals("One invoice, Invoice PK", invoiceHeader1.PK, instruction.Invoices.ElementAt(0).PK);

			invoiceLine2.JI_CEI = instruction.PK;
			AssertEquals("Two invoices", 2, instruction.Invoices.Count);
			AssertNotNull("Two invoices, Invoice 1", instruction.Invoices.SingleOrDefault(x => x.PK == invoiceHeader1.PK));
			AssertNotNull("Two invoices, Invoice 2", instruction.Invoices.SingleOrDefault(x => x.PK == invoiceHeader2.PK));

			var invoiceHeader3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = ZGuid.Empty;

			AssertEquals("Three invoices, only two is linked to instruction", 2, instruction.Invoices.Count);
		});
	}

	public void TestCEI_PackageCount_DefaultValue()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TotalNoOfPieces = 20;

			var ins1 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("First Instruction", 20.00m, ins1.CEI_PackageCount);

			var ins2 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("Second Instruction", 0.00m, ins2.CEI_PackageCount);

			ins1.CEI_PackageCount = 10.00m;
			ins2.CEI_PackageCount = 5.00m;

			var ins3 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("Third Instruction", 5.00m, ins3.CEI_PackageCount);
		});
	}

	public void TestSetDefaultsForNewChild_Argument()
	{
		AssertArgumentExceptionThrown<ArgumentNullException>("declaration", () => instruction.SetDefaultsForNewChild(null));
	}

	public void TestSetDefaultsForNewChild_SubStyle() => CombineAssertions(() =>
	{
		var standaloneInstruction = Factory.New<CusEntryInstruction>();
		standaloneInstruction.SetDefaultsForNewChild(declaration);
		AssertEquals("CEI_SubStyle default when declaration has NO CopyStatus", "N", standaloneInstruction.CEI_SubStyle);

		instruction.CEI_SubStyle = NODeclarationCopyStatus.Codes.Recalculation;
		standaloneInstruction.SetDefaultsForNewChild(declaration);
		AssertEquals("CEI_SubStyle default when declaration has CopyStatus", "REC", standaloneInstruction.CEI_SubStyle);
	});

	public void TestCEI_SubStyle_DefaultValue()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertEquals("Default declaration sub type", "N", entryInstruction.CEI_SubStyle);
	}

	public void TestCEI_SubStyle_ShouldSetCopyStatusToAllSiblings() => CombineAssertions(() =>
	{
		var sibling = declaration.CustomsEntryInstructions.AddNew();
		sibling.CEI_SubStyle = ImportDeclarationSubTypes.Codes.N;
		instruction.CEI_SubStyle = ImportDeclarationSubTypes.Codes.P;
		AssertEquals("CEI_SubStyle after assigning sibling instruction (value is NOT CopyStatus)", "N", sibling.CEI_SubStyle);

		instruction.CEI_SubStyle = NODeclarationCopyStatus.Codes.Recalculation;
		AssertEquals("CEI_SubStyle after assigning sibling instruction (value is CopyStatus)", "REC", sibling.CEI_SubStyle);
	});

	public void TestImportProcedure_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryInstruction>()
			.HasProperty(x => x.CEI_DateForDuty)
			.WithCaption("Date for Duty")
			.WithFullDescription("Date for Duty regulates the lookup of duty and fees in the tariff and also the currency exchange rate. When blank today's date is used."));

	public void TestPreviousDocuments() => CombineAssertions(() =>
	{
		AssertEquals("PreviousDocument Collection Type", typeof(CusSupportingInfoCollection<PreviousDocument>), instruction.PreviousDocuments.GetType());

		var previousDocument = instruction.PreviousDocuments.AddNew();
		AssertType<PreviousDocument>("PreviousDocument Type", previousDocument);
	});

	public void TestICusSupportingInfoTypeSupporter_GetCusSupportingInfoTypes() => CombineAssertions(() =>
	{
		ICusSupportingInfoTypeSupporter supporter = instruction;
		var supportingInfoTypes = supporter.GetCusSupportingInfoTypes();

		AssertEquals("Previous Document Type", typeof(PreviousDocument), supportingInfoTypes.GetValueSafe(CusSupportingInfoTypeList.Codes.PreviousDocument));
	});

	public void TestICusSupportingInfoTypeSupporter_GetFetchStrategies() => CombineAssertions(() =>
	{
		ICusSupportingInfoTypeSupporter supporter = instruction;
		var fetchStrategiesTypes = supporter.GetFetchStrategies().Select(s => s.GetType());

		var expectedTypes = new [] { typeof(CusSupportingInfoTypeSupporterFetchStrategy) };

		AssertContainsExactElementsInAnyOrder("Fetch Strategy Types", expectedTypes, fetchStrategiesTypes);
	});

	public void TestPreviousDocumentMaster()
	{
		AssertType<PreviousDocumentMaster>(instruction.PreviousDocumentMaster);
	}

	public void TestIPreviousDocumentsProvider_PreviousDocuments()
	{
		IPreviousDocumentsProvider provider = instruction;
		AssertSame(instruction.PreviousDocuments, provider.PreviousDocuments);
	}

	public void TestIsProcedureCodeWithOutOfWarehouse()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CreateRefCusProcedureCodeDataForTest();
		CombineAssertions(() =>
		{
			instruction.CEI_Procedure = ZString.Empty;
			AssertEquals("When Procedure Code is Empty", false, instruction.IsProcedureCodeWithOutOfWarehouse);

			instruction.CEI_Procedure = "0471";
			AssertEquals("When Procedure Code having OutOfWarehouse=True", true, instruction.IsProcedureCodeWithOutOfWarehouse);

			instruction.CEI_Procedure = "5710";
			AssertEquals("When Procedure Code having OutOfWarehouse=False", false, instruction.IsProcedureCodeWithOutOfWarehouse);
		});
	}

	public void TestUpdatePreviousDocumentMasterCSI_CodeWithGoodsLocationAndPreviousProcedureCode()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_LocationOfGoods = "A8";
		var documentMaster = instruction.PreviousDocumentMaster;
		CreateRefCusProcedureCodeDataForTest();
		CombineAssertions(() =>
		{
			instruction.CEI_Procedure = ZString.Empty;
			AssertEquals("When Procedure Code is Empty", ZString.Empty, documentMaster.CSI_Procedure);

			instruction.CEI_Procedure = "0471";
			AssertEquals("When Procedure Code having OutOfWarehouse=True", "71A8", documentMaster.CSI_Procedure);

			declaration.JE_LocationOfGoods = "A";
			AssertEquals("When Procedure Code having OutOfWarehouse=True and LocationOfGoods Changed", "71A8", documentMaster.CSI_Procedure);

			instruction.CEI_Procedure = "0571";
			AssertEquals("When Procedure Code Changed having OutOfWarehouse=True", "71A", documentMaster.CSI_Procedure);

			instruction.CEI_Procedure = "5710";
			AssertEquals("When Procedure Code having OutOfWarehouse=False", ZString.Empty, documentMaster.CSI_Procedure);
		});
	}

	public void TestProcedureShouldNotThrowOnStandaloneEntryInstruction()
	{
		var standaloneInstruction = Factory.New<CusEntryInstruction>();
		AssertNoExceptionThrown(() => standaloneInstruction.CEI_Procedure = UniversalReferenceConstants.ChargeTypes.VGE_ProcedureCode);
	}

	void CreateRefCusProcedureCodeDataForTest()
	{
		var referenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
		_ = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "04",
			previousProcedureCode: "71",
			concession: ZString.Empty,
			description: "Description1",
			shipmentType: "IMP",
			outOfWarehouse: true);

		_ = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "57",
			previousProcedureCode: "10",
			concession: ZString.Empty,
			description: "Description 2",
			shipmentType: "IMP",
			outOfWarehouse: false);

		_ = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "05",
			previousProcedureCode: "71",
			concession: ZString.Empty,
			description: "Description3",
			shipmentType: "IMP",
			outOfWarehouse: true);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		instruction = declaration.CustomsEntryInstructions.AddNew();
	}
	JobDeclaration declaration;
	CusEntryInstruction instruction;
}

static class CusEntryInstructionHelper
{
	public static void SetupGetHasDigitollGoodsNumber(this Mock<CusEntryInstruction> mock, bool hasDigitollGoodsNumber)
	{
		mock.Protected().Setup<ZBool>("GetHasDigitollGoodsNumber").Returns(hasDigitollGoodsNumber);
	}
}

class CusEntryInstructionForTest : CusEntryInstruction
{
	public CusEntryInstructionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}
	public CusEntryNumberWrapper GSPCusEntryNumberForTest => GSPCusEntryNumber;
}
