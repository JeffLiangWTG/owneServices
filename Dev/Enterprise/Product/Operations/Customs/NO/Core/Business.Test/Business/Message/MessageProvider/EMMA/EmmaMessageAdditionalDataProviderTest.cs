using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EmmaMessageAdditionalDataProvider))]
sealed class EmmaMessageAdditionalDataProviderTest : TestCaseWithFactory
{
	public void TestGetContainerMode() => CombineAssertions(() =>
	{
		foreach (var containerMode in transportInContainerModes)
		{
			declaration.JE_ContainerMode = containerMode;
			AssertEquals($"When Container Mode: {containerMode}", "1", GetNewDataProvider().GetContainerMode(entryHeader));
		}

		declaration.JE_ContainerMode = Core.Constants.ContainerModes.Other;
		AssertEquals($"When Container Mode: OTH", "0", GetNewDataProvider().GetContainerMode(entryHeader));
	});

	public void TestGetTotalInvoiceOrLinesAmount() => CombineAssertions(() =>
	{
		var invLine1 = AddNewInvoiceLine();
		CurrencyConverterTestHelper.SetExchangeRate(Factory, "EUR", 0.6m, ZDateTime.Today, "CUS");

		invoiceHeader.JZ_InvoiceAmount = 17m;
		invLine1.JI_LinePrice = 12m;
		AssertEquals("When one or no invoice lines added", 17m, GetNewDataProvider().GetTotalInvoiceOrLinesAmount(entryHeader));

		var invHeader2 = declaration.Invoices.AddNew();
		var invLine2 = invHeader2.InvoiceLines.AddNew();
		invLine2.JI_CL = entryLine.PK;
		new LineMerger(declaration).DoMerge();

		invoiceHeader.JZ_RX_NKInvoice_Currency = "NOK";
		invHeader2.JZ_RX_NKInvoice_Currency = "NOK";
		invLine2.JI_LinePrice = 19m;
		AssertEquals("When more than one invoice lines added, but with single currency", 31m, GetNewDataProvider().GetTotalInvoiceOrLinesAmount(entryHeader));

		invHeader2.JZ_RX_NKInvoice_Currency = "EUR";
		AssertEquals("When more than one invoice lines added, but with multiple currency", 43.67m, GetNewDataProvider().GetTotalInvoiceOrLinesAmount(entryHeader));
	});

	[TestDate(2025, 1, 1)]
	public void TestGetDocuments()
	{
		entryHeader.SetEntryReleaseNumber("12553343");
		var docManager = entryHeader.DocManagerInfo();
		_ = docManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub SADH NO document 2"), "sadh-file.pdf", Core.Constants.RefDocTypes.EntryPrint);
		_ = docManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub Misc document"), "misc-file.png", Core.Constants.RefDocTypes.MiscellaneousDocument);

		var declarationDocManager = declaration.DocManagerInfo();
		_ = declarationDocManager.AddFileOrDocument(Encoding.ASCII.GetBytes("stub Invoice document"), "inv-file.txt", Core.Constants.RefDocTypes.Invoice);

		var attachedDocs = GetNewDataProvider().GetDocuments(entryHeader);
		CombineAssertions("When Entry Release Number is present", () =>
		{
			AssertEquals("Total attached document", 2, attachedDocs.Count);
			AssertFileInfo("INV-12553343-inv-file.txt", "Invoice");
			AssertFileInfo("EPR-12553343-sadh-file.pdf", "Entry Print/ Customs Declaration Documents");
		});

		entryHeader.SetEntryReleaseNumber(ZString.Empty);
		attachedDocs = GetNewDataProvider().GetDocuments(entryHeader);
		CombineAssertions("When Entry Release Number is not present", () =>
		{
			AssertEquals("Total attached document", 2, attachedDocs.Count);
			AssertFileInfo("INV-inv-file.txt", "Invoice");
			AssertFileInfo("EPR-sadh-file.pdf", "Entry Print/ Customs Declaration Documents");
		});
		void AssertFileInfo(string fileName, string description)
		{
			var pdfFileInfo = attachedDocs.FirstOrDefault(f => f.Filnavn == fileName);
			AssertNotNull($"{fileName}", pdfFileInfo);
			AssertEquals("Date", ZDateTime.Today.ToShortDateString(), pdfFileInfo.DokumentDato);
			AssertEquals("Description", description, pdfFileInfo.Beskrivelse);
		}
	}

	public void TestGetGNONumberFirstPart()
	{
		var entryNum = "CEN1234;00123";
		CreateEntryReleaseNumber(declaration, entryNum, CusEntryNumberTypes.Norway.GoodsNumber);
		AssertEquals("CEN1234", GetNewDataProvider().GetGNONumberFirstPart(entryHeader));
	}

	public void TestGetGNOorGSPNumberSecondPart() => CombineAssertions(() =>
	{
		var entryNum = "CEN1234;00123";
		CreateEntryReleaseNumber(declaration, entryNum, CusEntryNumberTypes.Norway.GoodsNumber);
		AssertEquals("When GSP EntryNum type is not created", "00123", GetNewDataProvider().GetGNOorGSPNumberSecondPart(entryHeader));

		var invLine = AddNewInvoiceLine();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invLine.JI_CEI = entryInstruction.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		CreateEntryReleaseNumber(entryInstruction, "ST987", CusEntryNumberTypes.Norway.GoodsNumberSubPosition);
		AssertEquals("When GSP EntryNum type is present", "00123/ST987", GetNewDataProvider().GetGNOorGSPNumberSecondPart(entryHeader));
	});

	public void TestGetGoodsMarksAndNos() => CombineAssertions(() =>
	{
		var invLine = AddNewInvoiceLine();
		AssertEquals("When JI_GoodsMarks is empty", "ADR", GetNewDataProvider().GetGoodsMarksAndNos(entryLine));

		invLine.JI_GoodsMarks = "NO12345";
		AssertEquals("When JI_GoodsMarks is not empty", "NO12345", GetNewDataProvider().GetGoodsMarksAndNos(entryLine));
	});

	public void TestGetGoodsDescription() => CombineAssertions(() =>
	{
		var expectedVareslag = string.Empty;

		for (var i = 1; i <= 5; ++i)
		{
			var description = $"INV LINE{i} DESCRIPTION 12345678910";
			var invLine = AddNewInvoiceLine();
			invLine.JI_Description = description;
			expectedVareslag += description.Substring(0, 31);

			AssertEquals($"When {i} Inv Lines added", expectedVareslag, GetNewDataProvider().GetGoodsDescription(entryLine));
		}

		var invLineWithDupDesc = AddNewInvoiceLine();
		invLineWithDupDesc.JI_Description = "INV LINE1 DESCRIPTION 12345678910";
		AssertEquals($"When another Inv Line with duplicate description added", expectedVareslag, GetNewDataProvider().GetGoodsDescription(entryLine));
	});

	public void TestGetContainer1()
	{
		AddContainers(1);
		AssertEquals("CONTNUM1", GetNewDataProvider().GetContainer1(entryLine));
	}

	public void TestGetContainer2()
	{
		AddContainers(2);
		AssertEquals("CONTNUM2", GetNewDataProvider().GetContainer2(entryLine));
	}

	public void TestGetContainer3()
	{
		AddContainers(3);
		AssertEquals("CONTNUM3", GetNewDataProvider().GetContainer3(entryLine));
	}

	public void TestGetContainer4()
	{
		AddContainers(4);
		AssertEquals("CONTNUM4", GetNewDataProvider().GetContainer4(entryLine));
	}

	public void TestGetContainer5()
	{
		AddContainers(5);
		AssertEquals("CONTNUM5", GetNewDataProvider().GetContainer5(entryLine));
	}

	public void TestGetContainer6()
	{
		AddContainers(6);
		AssertEquals("CONTNUM6", GetNewDataProvider().GetContainer6(entryLine));
	}

	public void TestGetContainer7()
	{
		AddContainers(7);
		AssertEquals("CONTNUM7", GetNewDataProvider().GetContainer7(entryLine));
	}

	public void TestGetContainer8()
	{
		AddContainers(8);
		AssertEquals("CONTNUM8", GetNewDataProvider().GetContainer8(entryLine));
	}

	public void TestGetGrossWeight()
	{
		var invLine1 = AddNewInvoiceLine();
		invLine1.JI_Weight = 12m;
		invLine1.JI_WeightUQ = "KG";

		var invLine2 = AddNewInvoiceLine();
		invLine2.JI_Weight = 1265m;
		invLine2.JI_WeightUQ = "G";

		AssertEquals(13.265m, GetNewDataProvider().GetGrossWeight(entryLine));
	}

	public void TestGetProcedureCode() => CombineAssertions(() =>
	{
		var invLine = AddNewInvoiceLine();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invLine.JI_CEI = entryInstruction.PK;
		invLine.JI_Procedure = "4007";

		AssertEquals("When Invoice Line Procedure code is not empty", "4007", GetNewDataProvider().GetProcedureCode(entryLine));

		invLine.JI_Procedure = string.Empty;
		entryInstruction.CEI_Procedure = "4006";
		AssertEquals("When Invoice Line Procedure code is empty, fetch from CusEntryInstruction", "4006", GetNewDataProvider().GetProcedureCode(entryLine));
	});

	public void TestGetNetWeight()
	{
		var invLine1 = AddNewInvoiceLine();
		invLine1.JI_NetWeight = 12m;
		invLine1.JI_NetWeightUQ = "KG";

		var invLine2 = AddNewInvoiceLine();
		invLine2.JI_NetWeight = 1265m;
		invLine2.JI_NetWeightUQ = "G";

		AssertEquals(13.265m, GetNewDataProvider().GetNetWeight(entryLine));
	}

	public void TestGetCustomsSecondQuantity()
	{
		new RefCusTariffTestHelper(Factory).SetupGenericTariffData();
		var invLine = AddNewInvoiceLine();

		CombineAssertions(() =>
		{
			invLine.JI_Tariff = "22222222";
			invLine.JI_CustomsSecondQuantity = 5;
			invLine.JI_CustomsSecondUnitQty = "NMB";
			AssertEquals("Customs quantity no CU2", null, GetNewDataProvider().GetCustomsSecondQuantity(entryLine));

			invLine.JI_Tariff = "55555555";
			AssertEquals("Customs quantity in integer with CU2", 5m, GetNewDataProvider().GetCustomsSecondQuantity(entryLine));

			invLine.JI_Tariff = "11111111";
			invLine.JI_CustomsSecondQuantity = 1.123;
			invLine.JI_CustomsSecondUnitQty = "LTR";
			AssertEquals("Customs quantity with decimals with CU2", 1.123m, GetNewDataProvider().GetCustomsSecondQuantity(entryLine));
		});
	}

	public void TestGetValuationCodeOrMethod() => CombineAssertions(() =>
	{
		var invLine = AddNewInvoiceLine();
		invLine.JI_ValuationCode = "VD";
		AssertEquals("When InvLine ValuationCode is not emoty", "VD", GetNewDataProvider().GetValuationCodeOrMethod(entryLine));

		invLine.JI_ValuationCode = string.Empty;
		invoiceHeader.JZ_ValuationMethod = "1";
		AssertEquals("When InvLine ValuationCode is emoty, fetch from linked JobComInvoiceHeader.JZ_ValuationMethod", "1", GetNewDataProvider().GetValuationCodeOrMethod(entryLine));
	});

	public void TestGetVatCode() => CombineAssertions(() =>
	{
		var invLine = AddNewInvoiceLine();
		invLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;
		AssertEquals("When all entry line's VATCode != MV2", null, GetNewDataProvider().GetVatCode(entryLine));
		invLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV2;
		AssertEquals("When all entry line's VATCode == MV2", "M2", GetNewDataProvider().GetVatCode(entryLine));
	});

	public void TestGetCustomsValueWithoutLinesPrice() => CombineAssertions(() =>
	{
		AssertEquals("When null BizObj passed", null, GetNewDataProvider().GetCustomsValueWithoutLinesPrice(null));

		entryLine.CL_CustomsValue = 10m;
		var invLine1 = AddNewInvoiceLine();
		invLine1.JI_LinePrice = 2m;
		var invLine2 = AddNewInvoiceLine();
		invLine2.JI_LinePrice = 3.5m;
		AssertEquals("When all values are present", 4.5m, GetNewDataProvider().GetCustomsValueWithoutLinesPrice(entryLine));
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceHeader = declaration.Invoices.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
	JobComInvoiceHeader invoiceHeader;

	static IEmmaSystemsFortollingAdditionalDataProvider GetNewDataProvider() => new EmmaMessageAdditionalDataProvider();

	JobComInvoiceLine AddNewInvoiceLine()
	{
		var newInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		newInvoiceLine.JI_CL = entryLine.PK;
		entryLine.InvoiceLines.Add(newInvoiceLine);
		return newInvoiceLine;
	}

	void CreateEntryReleaseNumber(BusinessObject parent, string number, string type)
	{
		var entryNum = CusEntryNumber.LoadOrCreate(parent, type, GlbCompany.CurrentCompany.Country.Code, false);
		entryNum.CE_EntryNum = number;
	}

	void AddContainers(int number)
	{
		for (var containerIdx = 1; containerIdx <= number; containerIdx++)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = $"CONTNUM{containerIdx}";
			AddNewInvoiceLine();
			invoiceHeader.AssignContainerToInvoiceLines(container.CO_ContainerNumber);
		}
	}

	static readonly ImmutableHashSet<string> transportInContainerModes = new HashSet<string>()
	{
		Core.Constants.ContainerModes.FCL,
		Core.Constants.ContainerModes.LCL,
		Core.Constants.ContainerModes.ULD,
		Core.Constants.ContainerModes.Containerised
	}.ToImmutableHashSet();
}
