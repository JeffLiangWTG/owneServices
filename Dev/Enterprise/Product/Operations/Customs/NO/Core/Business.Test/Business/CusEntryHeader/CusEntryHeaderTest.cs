using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.NO;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryHeader))]
sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
{
	public void TestICommonGoodsItemsIntegratorProvider_CommonGoodsItemsIntegrator()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertType<CommonGoodsItemsIntegrator>(entryHeader.CommonGoodsItemsIntegrator);
	}

	new JobDeclaration ImportJobDeclaration
	{
		get
		{
			var declaration = base.ImportJobDeclaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return declaration as JobDeclaration;
		}
	}

	protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

	protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

	public void TestLookups()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertType<CusEntryHeaderLookups>(entryHeader.Lookups);
	}

	public void TestCH_ReCalcReplyMessage_Caption()
	{
		var cusEntryHeader = Factory.New<CusEntryHeader>();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusEntryHeader.CH_ReCalcReplyMessageInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Message from customs", resourceStringData.Caption);
			AssertEquals("Message from customs. Basis for decision.", resourceStringData.FullDescription);
		});
	}

	public void TestCH_ReCalcReason_Caption()
	{
		var cusEntryHeader = Factory.New<CusEntryHeader>();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusEntryHeader.CH_ReCalcReasonInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Reason", resourceStringData.Caption);
			AssertEquals("Free text description of the reason. Extra references if needed.", resourceStringData.FullDescription);
		});
	}

	public void TestCH_ReCalcOrigDecl_Caption()
	{
		var cusEntryHeader = Factory.New<CusEntryHeader>();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusEntryHeader.CH_ReCalcOrigDeclInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Original Id", resourceStringData.Caption);
			AssertEquals("Original declaration id", resourceStringData.FullDescription);
		});
	}

	public void TestCH_ReCalcCaseCode_Caption()
	{
		var cusEntryHeader = Factory.New<CusEntryHeader>();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusEntryHeader.CH_ReCalcCaseCodeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Case code", resourceStringData.Caption);
			AssertEquals("Case code for re-calculation of existing customs declaration. The case code denotes the nature and reason for the re-calculation.", resourceStringData.FullDescription);
		});
	}

	public void TestCH_ReCalcDeclType_Caption()
	{
		var cusEntryHeader = Factory.New<CusEntryHeader>();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusEntryHeader.CH_ReCalcDeclTypeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Select type", resourceStringData.Caption);
			AssertEquals("The type of declaration selected for re-calculation or statistics.", resourceStringData.FullDescription);
		});
	}

	public void TestPhaseStatusDescription_Caption()
	{
		_ = AssertEntity<CusEntryHeader>().HasProperty(h => h.PhaseStatusDescription).WithCaption("Phase Status Description");
	}

	public void TestCustomsDutyAmount()
	{
		var declaration = Factory.New<JobDeclaration>();
		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew()
			.AddCustomsDutyFee(1m)
			.AddAgricultureFee(10m)
			.AddExciseDutyFee(300m)
			.AddVatFee(3000m);
		AssertEquals("Macro CustomsDutyAmount should sum TL and RT fees", 11m, cusEntryHeader.CustomsDutyAmount);
	}

	public void TestCustomsDutyAmount_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryHeader>()
			.HasProperty(x => x.CustomsDutyAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 0, because: "value is rounded")
			.WithAttribute<DecimalPrecisionAttribute>(x => x.DecimalPrecision == 16)
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly, because: "underlying values are calculated during merge"));

	public void TestExciseDutyAmount()
	{
		var declaration = Factory.New<JobDeclaration>();
		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew()
			.AddCustomsDutyFee(1m)
			.AddAgricultureFee(10m)
			.AddExciseDutyFee(300m)
			.AddVatFee(3000m);
		AssertEquals("Macro ExciseDutyAmount should sum all but TL, RT and MV fees", 300m, cusEntryHeader.ExciseDutyAmount);
	}

	public void TestExciseDutyAmount_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryHeader>()
			.HasProperty(x => x.ExciseDutyAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 0, because: "value is rounded")
			.WithAttribute<DecimalPrecisionAttribute>(x => x.DecimalPrecision == 16)
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly, because: "underlying values are calculated during merge"));

	public void TestVatAmount_WhenIsLandedCostOnly()
	{
		AssertLandedCostOnly(1m, 2m, 1m, 0m, iSLandedCostOnly: true);
	}

	public void TestVatAmount_WhenNotIsLandedCostOnly()
	{
		AssertLandedCostOnly(1m, 2m, 3m, 2m, iSLandedCostOnly: false);
	}

	void AssertLandedCostOnly(ZDecimal tLAmount, ZDecimal mVAmount, ZDecimal sumTotal, ZDecimal sumMva, ZBool iSLandedCostOnly) => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
		var line = cusEntryHeader.MergedLines.AddNew();
		var fee1 = line.Fees.AddNew();
		fee1.CF_ChargeAmount = tLAmount;
		fee1.CF_ChargeType = "TL";
		var fee2 = line.Fees.AddNew();
		fee2.CF_ChargeAmount = mVAmount;
		fee2.CF_ChargeType = "MV";
		fee2.CF_IsLandedCostOnly = iSLandedCostOnly;
		AssertEquals("Macro VatAmount should sum MV fees", sumMva, cusEntryHeader.VatAmount);
		AssertEquals("Macro TotalAmount should sum all fees", sumTotal, cusEntryHeader.TotalAmount);
	});

	public void TestVatAmount_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryHeader>()
			.HasProperty(x => x.VatAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 0, because: "value is rounded")
			.WithAttribute<DecimalPrecisionAttribute>(x => x.DecimalPrecision == 16)
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly, because: "underlying values are calculated during merge"));

	public void TestTotalAmount()
	{
		var declaration = Factory.New<JobDeclaration>();
		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew()
			.AddCustomsDutyFee(1m)
			.AddAgricultureFee(10m)
			.AddExciseDutyFee(300m)
			.AddVatFee(3000m);
		AssertEquals("Macro TotalAmount should sum all fees", 3_311m, cusEntryHeader.TotalAmount);
	}

	public void TestTotalAmount_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryHeader>()
			.HasProperty(x => x.TotalAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 0, because: "value is rounded")
			.WithAttribute<DecimalPrecisionAttribute>(x => x.DecimalPrecision == 16)
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly, because: "underlying values are calculated during merge"));

	public void TestTotalDutyAmount_Attributes() => CombineAssertions(() =>
		AssertEntity<CusEntryHeader>()
			.HasProperty(x => x.TotalDutyAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 0, because: "value is rounded")
			.WithAttribute<DecimalPrecisionAttribute>(x => x.DecimalPrecision == 16)
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly, because: "underlying values are calculated during merge"));

	public void TestTotalPaymentAmount() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var importer = Factory.New<OrgHeader>();
		declaration.JE_OH_Importer = importer.PK;

		AssertTotalPaymentsAmount("When importer is NOT MVA-registered, VAT should be included", 3_311m, declaration);

		importer.AsMVARegistered();
		AssertTotalPaymentsAmount("When importer is MVA-registered, VAT should be excluded", 311m, declaration);

		static void AssertTotalPaymentsAmount(string message, decimal expected, JobDeclaration declaration)
		{
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew()
				.AddCustomsDutyFee(1m)
				.AddAgricultureFee(10m)
				.AddExciseDutyFee(300m)
				.AddVatFee(3000m);
			AssertEquals(message, expected, cusEntryHeader.TotalPaymentAmount);
		}
	});

	public void TestHasPayments() => CombineAssertions(() =>
	{
		var entryHeaderWithNoPaymentsMock = Factory.NewMoq<CusEntryHeader>();
		_ = entryHeaderWithNoPaymentsMock.Protected().Setup<ZDecimal>("GetTotalPaymentAmount").Returns(0);
		var entryHeaderWithNoPayments = entryHeaderWithNoPaymentsMock.Object;
		AssertEquals("When entry header has no payments", expected: false, entryHeaderWithNoPayments.HasPayments);

		var entryHeaderWithPaymentsMock = Factory.NewMoq<CusEntryHeader>();
		_ = entryHeaderWithPaymentsMock.Protected().Setup<ZDecimal>("GetTotalPaymentAmount").Returns(420);
		var entryHeaderWithPayments = entryHeaderWithPaymentsMock.Object;
		AssertEquals("When entry header has payments", expected: true, entryHeaderWithPayments.HasPayments);
	});

	public void TestFeesGrouping()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var line1 = entryHeader.AllEntryLines.AddNew();
		var line2 = entryHeader.AllEntryLines.AddNew();

		line1.Fees.AddNew("TL100", 3m);
		line2.Fees.AddNew("TL100", 5m);

		line1.Fees.AddNew("MV1", 9m);
		line1.Fees.AddNew("MV2", 14m);
		line2.Fees.AddNew("MV1", 22m);

		CombineAssertions(() =>
		{
			var fees = entryHeader.Fees;
			AssertEquals(2, fees.Count);

			var fee = fees.FirstOrDefault(c => c.Duty == "TL");
			AssertNotNull("TL", fee);
			AssertEquals("TL Amount", 8m, fee?.Amount);

			fee = fees.FirstOrDefault(c => c.Duty == "MV");
			AssertNotNull("MV", fee);
			AssertEquals("MV Amount", 45m, fee?.Amount);

			line2.Fees.AddNew("MV2", 5m);
			AssertEquals("MV Amount (after adding 5)", 50m, entryHeader.Fees.FirstOrDefault(c => c.Duty == "MV").Amount);
		});
	}

	public void TestFeesSorting()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.AllEntryLines.AddNew();
		var entryLine2 = entryHeader.AllEntryLines.AddNew();

		entryLine1.Fees.AddNew("TL1", 1m);
		entryLine1.Fees.AddNew("RT100", 10m);
		entryLine1.Fees.AddNew("OL720", 100m);
		entryLine1.Fees.AddNew("MV1", 1000m);

		entryLine2.Fees.AddNew("TL100", 2m);
		entryLine2.Fees.AddNew("RT100", 20m);
		entryLine2.Fees.AddNew("MA100", 200m);
		entryLine2.Fees.AddNew("MV2", 2000m);

		var expectedFeeCodes = new[] { "TL", "RT", "MA", "OL", "MV" };
		var actualFeeCodes = entryHeader.Fees.Select<string>(x => x.Duty).ToArray();
		AssertSequencesEqual("Should sort alphabetically and grouped in order TL, RT, Excise, MV", expectedFeeCodes, actualFeeCodes);
	}

	[TestDate(2007, 12, 12, 12, 12, 12)]
	public override void TestFOBAndCIFFigures()
	{
		var declaration = ImportJobDeclaration;
		var setup = GetChargesCurrencyTestSetup();
		setup.SetupJobDecWithOFTAndCIFCharges(declaration, declaration.LocalCurrencyCode);
		DoMerge(declaration);

		CombineAssertions(() =>
		{
			AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Overseas Freight for the entry does not include ForeignInlandFreight", 500m, entryHeader.OverseasFreight.Amount);
			AssertEquals("FOB for the entry", 10300m, entryHeader.FOB.Amount);
			AssertEquals("CIF for the entry", 10800m, entryHeader.CIF.Amount);
		});
	}

	[TestDate(2007, 12, 12, 12, 12, 12)]
	public override void TestFOBAndCIFInLocalCurrency()
	{
		var newCurrency = RefCurrency.New(Factory);
		newCurrency.RX_Code = "MDD";
		ZDateTime from = new ZDateTime(2007, 6, 1);
		ZDateTime to = new ZDateTime(2007, 12, 30);
		newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);

		var declaration = ImportJobDeclaration;
		var setup = GetChargesCurrencyTestSetup();
		setup.SetupJobDecWithOFTAndCIFCharges(declaration, newCurrency.RX_Code);
		DoMerge(declaration);

		CombineAssertions(() =>
		{
			AssertEquals("Precondition: EntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("FOB in local currency", 20600.0m, entryHeader.FOBInLocalCurrency.Amount);
			AssertEquals("CIF in local currency", 21600.0m, entryHeader.CIFInLocalCurrency.Amount);
		});
	}

	public void TestDefaultPaymentMethod()
	{
		var declaration = ImportJobDeclaration;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var importerNotMVARegistered = Factory.NewWithValidTestData<OrgHeader>();
		var importerMVARegistered = Factory.NewWithValidTestData<OrgHeader>().AsMVARegistered();
		CombineAssertions(() =>
		{
			declaration.JE_OH_Importer = importerMVARegistered.PK;
			AssertDefaultPaymentMethod(
				"no duties to be paid and importer is MVA/VAT-registered",
				NOPaymentMethodCodeList.Codes.NoDutiesOrVatPayable,
				vatAmount: 420m
			);

			AssertDefaultPaymentMethod(
				"the importer has not deferred duties account and there are duties to be paid",
				NOPaymentMethodCodeList.Codes.ForwardersDayCredit,
				customsDutyAmount: 69m,
				vatAmount: 420m
			);

			AssertDefaultPaymentMethod(
				"the importer has no deferred duties account and there are duties to be paid and entry type is 5",
				NOPaymentMethodCodeList.Codes.Cash,
				customsDutyAmount: 69m,
				vatAmount: 420m,
				entryType: "5"
			);

			importerMVARegistered.AsDeferredDutiesAccount();
			AssertDefaultPaymentMethod(
				"the importer has deferred duties account and there are duties to be paid",
				NOPaymentMethodCodeList.Codes.ImportersDeferred,
				customsDutyAmount: 69m,
				vatAmount: 420m
			);

			AssertDefaultPaymentMethod(
				"no VAT is payable at all",
				NOPaymentMethodCodeList.Codes.NoDutiesOrVatPayable
			);

			declaration.JE_OH_Importer = importerNotMVARegistered.PK;
			AssertDefaultPaymentMethod(
				"no VAT is payable at all and importer is not MVA/VAT-registered",
				NOPaymentMethodCodeList.Codes.NoDutiesOrVatPayable
			);

			AssertDefaultPaymentMethod(
				"the importer has not deferred duties account and no VAT payable and importer is not MVA/VAT-registered",
				NOPaymentMethodCodeList.Codes.ForwardersDayCredit,
				customsDutyAmount: 690m
			);

			importerNotMVARegistered.AsDeferredDutiesAccount();
			AssertDefaultPaymentMethod(
				"the importer has deferred duties account and no VAT payable and importer is not MVA/VAT-registered",
				NOPaymentMethodCodeList.Codes.ImportersDeferred,
				customsDutyAmount: 690m
			);
		});
		void AssertDefaultPaymentMethod(string message, string expected, decimal customsDutyAmount = 0m, decimal vatAmount = 0m, string entryType = "")
		{
			var entryheader = declaration.CustomsEntryHeaders.AddNew()
				.AddCustomsDutyFee(customsDutyAmount)
				.AddVatFee(vatAmount);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryheader.CH_CEI_Instruction = entryInstruction.PK;
			entryheader.EntryInstruction.CEI_Style = entryType;
			entryheader.DefaultPaymentMethod();
			AssertEquals(message, expected, entryheader.CH_PaymentMethod);
		}
	}

	public void TestMessages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertEquals("Precondition: EntryHeaders.Count", 0, entryHeader.Messages.Count);
		CombineAssertions(() =>
		{
			var message = entryHeader.Messages.AddNew();
			AssertEquals("Should be 1", 1, entryHeader.Messages.Count);
			AssertType<NOEDIMessageCollection>(entryHeader.Messages);
			AssertType<NOEDIMessage>(message);
		});
	}

	public void TestEntryNumberType()
	{
		var entryHeader = Factory.New<CusEntryHeaderForTest>();
		AssertEquals("CER", entryHeader.EntryNumberType_Exposed);
	}

	public void TestEntryReleaseNumber()
	{
		const string ernNum = "NO123";
		var ernIssueDate = new ZDateTime(2024, 1, 1, 0, 0, 0);
		var ernExpiryDate = new ZDateTime(2024, 10, 1, 0, 0, 0);
		const string ernEntryStatus = "123";
		var newFactory = new BusinessObjectFactory();

		var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

		CombineAssertions(() =>
		{
			AssertEntryReleaseNumber("When all parameters are empty");

			cusEntryHeader.SetEntryReleaseNumber(entryReleaseNumber: ernNum);
			AssertEntryReleaseNumber("When IssueDate, ExpiryDate and EntryStatus are empty", entryNumber: ernNum);

			cusEntryHeader.SetEntryReleaseNumber(entryReleaseNumber: ernNum, issueDate: ernIssueDate);
			AssertEntryReleaseNumber("When ExpiryDate and EntryStatus are empty", entryNumber: ernNum, issueDate: ernIssueDate);

			cusEntryHeader.SetEntryReleaseNumber(entryReleaseNumber: ernNum, issueDate: ernIssueDate, expiryDate: ernExpiryDate);
			AssertEntryReleaseNumber("When EntryStatus is empty", entryNumber: ernNum, issueDate: ernIssueDate, expiryDate: ernExpiryDate);

			cusEntryHeader.SetEntryReleaseNumber(entryReleaseNumber: ernNum, issueDate: ernIssueDate, expiryDate: ernExpiryDate, entryStatus: ernEntryStatus);
			AssertEntryReleaseNumber("When No parameter is empty", entryNumber: ernNum, issueDate: ernIssueDate, expiryDate: ernExpiryDate, entryStatus: ernEntryStatus);
		});

		void AssertEntryReleaseNumber(string message, ZString? entryNumber = null, ZDateTime? issueDate = null, ZDateTime? expiryDate = null, ZString? entryStatus = null)
		{
			var ernNumber = entryNumber ?? ZString.Empty;
			var ernIssueDate = issueDate ?? ZDateTime.Empty;
			var ernExpiryDate = expiryDate ?? ZDateTime.Empty;
			var ernEntryStatus = entryStatus ?? ZString.Empty;

			AssertEquals($"{message} EntryReleaseNumber", ernNumber, cusEntryHeader.EntryReleaseNumber);
			AssertEquals($"{message} EntryReleaseNumberIssueDate", ernIssueDate, cusEntryHeader.EntryReleaseNumberIssueDate);
			AssertEquals($"{message} EntryReleaseNumberExpiryDate", ernExpiryDate, cusEntryHeader.EntryReleaseNumberExpiryDate);
			AssertEquals($"{message} EntryReferenceNumberEntryStatus", ernEntryStatus, cusEntryHeader.EntryReferenceNumberEntryStatus);
			Factory.Save();

			var entryHeaderReloaded = newFactory.Load<CusEntryHeader>(cusEntryHeader.PK);
			AssertEquals($"{message} EntryReleaseNumber Reloaded", ernNumber, entryHeaderReloaded.EntryReleaseNumber);
			AssertEquals($"{message} EntryReleaseNumberIssueDate Reloaded", ernIssueDate, entryHeaderReloaded.EntryReleaseNumberIssueDate);
			AssertEquals($"{message} EntryReleaseNumberExpiryDate Reloaded", ernExpiryDate, entryHeaderReloaded.EntryReleaseNumberExpiryDate);
			AssertEquals($"{message} EntryReferenceNumberEntryStatus Reloaded", ernEntryStatus, entryHeaderReloaded.EntryReferenceNumberEntryStatus);
		}
	}

	public void TestEntryReleaseDate()
	{
		var releaseDate = new ZDateTime(2024, 1, 1, 0, 0, 0);
		var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

		cusEntryHeader.SetEntryReleaseDate(releaseDate);

		AssertEquals("Can set CH_EntryReleaseDate", releaseDate, cusEntryHeader.CH_EntryReleaseDate);
	}

	public void TestRandomEntryLine() => CombineAssertions(() =>
	{
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		var entryLine = entryHeader.MergedLines.AddNew();
		var randomLine = entryHeader.RandomEntryLine;
		AssertType<CusEntryLine>(randomLine);
		AssertEquals("Random Line and added entry line should be same", entryLine, randomLine);
	});

	public void TestInvoiceHeaders() => CombineAssertions(() =>
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var invoiceHeaders = entryHeader.InvoiceHeaders;
		AssertType<JobComInvoiceHeader[]>(invoiceHeaders);
		AssertEquals("Invoice Headers Count", 1 , invoiceHeaders.Length);
		AssertSame("Invoice Header", invoice, invoiceHeaders.FirstOrDefault());
	});

	public void TestTotalNetWeight()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var invoiceOne = declaration.Invoices.AddNew();
		var lineOne = invoiceOne.InvoiceLines.AddNew();
		lineOne.JI_NetWeight = 10.23m;
		var lineTwo = invoiceOne.InvoiceLines.AddNew();
		lineTwo.JI_NetWeight = 12m;

		var invoiceTwo = declaration.Invoices.AddNew();
		var lineThree = invoiceTwo.InvoiceLines.AddNew();
		lineThree.JI_NetWeight = 5.32m;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		lineOne.JI_CL = entryLine.PK;
		lineTwo.JI_CL = entryLine.PK;
		lineThree.JI_CL = entryLine.PK;

		AssertEquals("Total Net Weight", 27.55m, entryHeader.TotalNetWeight);
	}

	public void TestTotalGrossWeight()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var invoiceOne = declaration.Invoices.AddNew();
		var lineOne = invoiceOne.InvoiceLines.AddNew();
		lineOne.JI_Weight = 22m;

		var invoiceTwo = declaration.Invoices.AddNew();
		var lineTwo = invoiceTwo.InvoiceLines.AddNew();
		lineTwo.JI_Weight = 8.77m;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		lineOne.JI_CL = entryLine.PK;
		lineTwo.JI_CL = entryLine.PK;

		AssertEquals("Total Gross Weight", 30.77m, entryHeader.TotalGrossWeight);
	}

	public void TestTotalFreightInLocalCurrency()
	{
		_ = new RefCurrencyTestHelper(Factory).USDCurrency;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceHeaderOne = CreateInvoiceHeaderWithFreightCharge(500, 10);
		var invoiceHeaderTwo = CreateInvoiceHeaderWithFreightCharge(200, 42);
		var invoiceLineOne = invoiceHeaderOne.JobComInvoiceLines.AddNew();
		var invoiceLineTwo = invoiceHeaderTwo.JobComInvoiceLines.AddNew();
		invoiceLineOne.JI_LinePrice = 500m;
		invoiceLineTwo.JI_LinePrice = 200m;
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLineOne.JI_CL = entryLine.PK;
		invoiceLineTwo.JI_CL = entryLine.PK;

		AssertEquals("TotalFreightInLocalCurrency", 650m, entryHeader.TotalFreightInLocalCurrency);

		JobComInvoiceHeader CreateInvoiceHeaderWithFreightCharge(decimal invoiceAmount, decimal freightCharge)
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = invoiceAmount;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, freightCharge, Core.Constants.CurrencyCodes.UnitedStates);
			return invoiceHeader;
		}
	}

	public void TestIsFeePaidByBroker()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		var entryLineFee = entryLine.Fees.AddNew();
		var logger = new DetailedLoggerForTest();

		CombineAssertions("Fee payment by broker tests", () => {
			entryHeader.CH_PaymentMethod = "";
			entryLineFee.CF_IsLandedCostOnly = false;
			var result = entryHeader.IsFeePaidByBroker("", "", logger);
			AssertEquals("When feeCode and payment method is empty", false, result);

			entryHeader.CH_PaymentMethod = "D";
			entryLineFee.CF_IsLandedCostOnly = false;
			result = entryHeader.IsFeePaidByBroker("", "D", logger);
			AssertEquals("When payment method is 'D' and landed cost only is true", true, result);

			entryLineFee.CF_IsLandedCostOnly = true;
			result = entryHeader.IsFeePaidByBroker("", "D", logger);
			AssertEquals("When payment method is 'D' but landed cost only is false", false, result);
		});
	}

	#region Overseas carges (OFT/ONS) are dutiable in NO

	protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new ChargesCurrencyTestSetup();

	sealed class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
	{
		void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
		{
			declaration.AutoCreateChargesBasedOnIncoTerm = false;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			if (declaration.IsDeclarationIntegrated)
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
			}

			var nonDutiableCharge = invoiceHeader.Charges.AddNew();
			nonDutiableCharge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.ForeignInlandFreight;
			nonDutiableCharge.J7_Amount = 200m;
			nonDutiableCharge.J7_IsDutiable = false;
			nonDutiableCharge.J7_IsIncludedInITOT = true;

			var oft = invoiceHeader.Charges.AddNew();
			oft.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasFreight;
			oft.J7_Amount = 500m;
			oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
		}

		ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10300m;
		ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 10800m;
	}

	#endregion

	protected override void SetInvoicesToResultInTwoEntries(BaseJobComInvoiceLine line1, BaseJobComInvoiceLine line2)
	{
		line1.JI_PrimaryPreference = "A";
		SetValuationMethodOnLineInvoiceHeader(line1, "1");
		line2.JI_PrimaryPreference = "B";
		SetValuationMethodOnLineInvoiceHeader(line2, "2");

		void SetValuationMethodOnLineInvoiceHeader(BaseJobComInvoiceLine line, string valuationMethod)
		{
			if (line?.InvoiceHeader is JobComInvoiceHeader header)
			{
				header.JZ_ValuationMethod = valuationMethod;
			}
		}
	}

	public void TestGetEmmaMessageGenerationActionProcessor()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertType<EmmaMessageGeneratorProcessor>(((IEmmaMessageGenerationProcessorProvider)entryHeader).GetEmmaMessageGenerationActionProcessor(entryHeader));
	}

	public void TestProcessHandlingInfo()
	{
		IProcessHandlingInfoProvider entryHeader = Factory.New<CusEntryHeader>();
		AssertType<CusEntryHeaderProcessHandlingInfo>(entryHeader?.ProcessHandlingInfo);
	}

	public void TestCreateStmProcessQueueProcessor()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		IBaseAutoSendingMessageSupporter messageSendingMessageSupporter = entryHeader;
		AssertType<CustomsStmProcessQueueCreatorProcessor>(messageSendingMessageSupporter?.CreateStmProcessQueueProcessor(entryHeader, "EMM"));
	}
}

sealed class CusEntryHeaderForTest(BusinessObjectFactory factory, DataRow row) : CusEntryHeader(factory, row)
{
	public ZString EntryNumberType_Exposed => base.EntryNumberType;
}
