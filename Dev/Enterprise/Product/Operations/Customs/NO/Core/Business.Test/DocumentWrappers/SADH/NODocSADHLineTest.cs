using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSADHLine))]
sealed class NODocSADHLineTest : DocBaseWrapperTest
{
	public void TestConstructor()
	{
		_ = AssertArgumentExceptionThrown<ArgumentNullException>("entryLine", () => NODocSADHLine.New(null, Factory));
	}

	NODocSADHLine CreateNewDocSADHLineWrapper() => NODocSADHLine.New(entryLine, Factory);

	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return NODocSADHLine.New(Factory.New<CusEntryLine>(), Factory);
	}

	public void TestBox31GoodsDescription()
	{
		AssertEquals("[PRE-CONDITION] GoodsDescription", ZString.Empty, CreateNewDocSADHLineWrapper().Box31GoodsDescription);
		invoiceLine.JI_Description = "DescriptionText";
		AssertEquals("GoodsDescription", "DescriptionText", CreateNewDocSADHLineWrapper().Box31GoodsDescription);
	}

	public void TestBox31MarksAndNumbers()
	{
		var marks = CreateNewDocSADHLineWrapper().Box31MarksAndNumbers;
		AssertContainsExactElementsInAnyOrder("[PRE-CONDITION] MarksAndNumbers", Array.Empty<ZString>(), marks);

		declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;
		invoiceLine.JI_GoodsMarks = "Marks line1";

		var invoiceLine2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
		invoiceLine2.JI_CL = entryLine.PK;
		invoiceLine2.JI_GoodsMarks = "Marks line2";

		var invoiceLine3 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
		invoiceLine3.JI_CL = entryLine.PK;
		invoiceLine3.JI_GoodsMarks = "Marks line2\r\nMarks line3";

		_ = declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER1";
		foreach (var container in invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<Customs.Business.NonPersistentCusContainer>())
		{
			container.IsForInvoiceLine = true;
		}
		_ = declaration.CusContainers.AddNew().CO_ContainerNumber = "ForAnotherInvoice";

		var pack1 = declaration.Packages.AddNew();
		pack1.CW_PackType = "VN";
		pack1.CW_MarksAndNos = "VehicleID";

		var pack2 = declaration.Packages.AddNew();
		pack2.CW_PackType = "XX";
		pack2.CW_MarksAndNos = "NotAVehicleID";

		invoiceLine.PackagesForInvoiceLinesForBindingOnly
			.Cast<Customs.Business.BaseCusLinkPackage>()
			.ForEach(x => x.IsLinked = true);

		_ = declaration.DoMerge();
		marks = CreateNewDocSADHLineWrapper().Box31MarksAndNumbers;

		CombineAssertions(() =>
		{
			IEnumerable<ZString> expectedMarks = new ZString[] { "Marks line1", "Marks line2", "Marks line3", "CONTAINER1", "VehicleID" };
			AssertContainsExactElementsInAnyOrder("MarksAndNumbers", expectedMarks, marks.Cast<NODocSADHMarksAndNumbers>().Select(x => x.Value));
			AssertEquals("No default marks", ZString.Empty, CreateNewDocSADHLineWrapper().Box31MarksAndNumbers_NoInfo);
		});
	}

	public void TestBox31MarksAndNumbersFromLine27() => CombineAssertions(() =>
	{
		for (var i = 1; i < 40; i++)
		{
			AddNewPackage($"PackId_{i}");
		}
		invoiceLine.PackagesForInvoiceLinesForBindingOnly
			.Cast<Customs.Business.BaseCusLinkPackage>()
			.ForEach(x => x.IsLinked = true);

		AssertEquals("Marks1", "PackId_27", CreateNewDocSADHLineWrapper().Box31MarksAndNumbersFromLine27[0].ToString());
		AssertEquals("Extra lines", 13, CreateNewDocSADHLineWrapper().Box31MarksAndNumbersFromLine27.Count);
		AssertEquals("No default marks", ZString.Empty, CreateNewDocSADHLineWrapper().Box31MarksAndNumbers_NoInfo);

		void AddNewPackage(ZString marks)
		{
			var pack = declaration.Packages.AddNew();
			pack.CW_PackType = "VN";
			pack.CW_MarksAndNos = marks;
		}
	});

	public void TestBox31MarksAndNumbers_NoInfo() => CombineAssertions(() =>
	{
		AssertEquals("No marks", "ADR", CreateNewDocSADHLineWrapper().Box31MarksAndNumbers_NoInfo);
		invoiceLine.JI_GoodsMarks = "Marks line1";
		AssertEquals("Have marks", ZString.Empty, CreateNewDocSADHLineWrapper().Box31MarksAndNumbers_NoInfo);
	});

	public void TestBox32LineNumber()
	{
		_ = declaration.DoMerge();
		AssertEquals("[PRE-CONDITION] LineNumber", (short)1, CreateNewDocSADHLineWrapper().Box32LineNumber);

		entryLine.CL_LineNumber = 9;
		AssertEquals("LineNumber", (short)9, CreateNewDocSADHLineWrapper().Box32LineNumber);
	}

	public void TestBox33Tariff()
	{
		AssertEquals("[PRE-CONDITION] Tariff", ZString.Empty, CreateNewDocSADHLineWrapper().Box33Tariff);
		invoiceLine.JI_Tariff = "12345678";
		_ = declaration.DoMerge();
		AssertEquals("Tariff", "12345678", CreateNewDocSADHLineWrapper().Box33Tariff);
	}

	public void TestBox33ReducedCustoms()
	{
		AssertEquals("[PRE-CONDITION] ReducedCustoms", ZString.Empty, CreateNewDocSADHLineWrapper().Box33ReducedCustoms);
		invoiceLine.JI_ReducedCustomsFlag = "S";
		AssertEquals("ReducedCustoms", "S", CreateNewDocSADHLineWrapper().Box33ReducedCustoms);
	}

	public void TestBox34CountryOfOrigin()
	{
		AssertEquals("[PRE-CONDITION] Country of Origin", Constants.CountryCodes.Norway, CreateNewDocSADHLineWrapper().Box34CountryOfOrigin);
		invoiceLine.JI_CountryOfOrigin = Constants.CountryCodes.Germany;
		AssertEquals("Country of Origin", Constants.CountryCodes.Germany, CreateNewDocSADHLineWrapper().Box34CountryOfOrigin);
	}

	public void TestBox35GrossWeight()
	{
		AssertEquals("[PRE-CONDITION] GrossWeight", "0K", CreateNewDocSADHLineWrapper().Box35GrossWeight);
		CombineAssertions(() =>
		{
			invoiceLine.JI_Weight = 10.5;
			AssertEquals("GrossWeight", "10,5K", CreateNewDocSADHLineWrapper().Box35GrossWeight);
			invoiceLine.JI_Weight = 7;
			AssertEquals("GrossWeight", "7K", CreateNewDocSADHLineWrapper().Box35GrossWeight);
		});
	}

	public void TestBox36Preference()
	{
		AssertEquals("[PRE-CONDITION] Preference", ZString.Empty, CreateNewDocSADHLineWrapper().Box36Preference);
		invoiceLine.JI_PrimaryPreference = "P";
		AssertEquals("Preference", "P", CreateNewDocSADHLineWrapper().Box36Preference);
	}

	public void TestBox37Procedure()
	{
		AssertEquals("[PRE-CONDITION] Procedure", ZString.Empty, CreateNewDocSADHLineWrapper().Box37Procedure);
		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = "4100";
			AssertEquals("Procedure from entryInstruction", "4100", CreateNewDocSADHLineWrapper().Box37Procedure);
			invoiceLine.JI_Procedure = "4155";
			AssertEquals("Procedure from InvoiceLine", "4155", CreateNewDocSADHLineWrapper().Box37Procedure);
		});
	}

	public void TestBox38NetWeight()
	{
		AssertEquals("[PRE-CONDITION] NetWeight", "0K", CreateNewDocSADHLineWrapper().Box38NetWeight);
		CombineAssertions(() =>
		{
			invoiceLine.JI_NetWeight = 5.5;
			AssertEquals("NetWeight", "5,5K", CreateNewDocSADHLineWrapper().Box38NetWeight);
			invoiceLine.JI_NetWeight = 1;
			AssertEquals("NetWeight", "1K", CreateNewDocSADHLineWrapper().Box38NetWeight);
		});
	}

	public void TestBox41CusQuantityAndType()
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.SetupGenericTariffData();

		AssertEquals("[PRE-CONDITION] Customs quantity", ZString.Empty, CreateNewDocSADHLineWrapper().Box41CusQuantityAndType);
		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = "22222222";
			invoiceLine.JI_CustomsSecondQuantity = 5;
			invoiceLine.JI_CustomsSecondUnitQty = "NMB";
			AssertEquals("Customs quantity no CU2", ZString.Empty, CreateNewDocSADHLineWrapper().Box41CusQuantityAndType);

			invoiceLine.JI_Tariff = "55555555";
			AssertEquals("Customs quantity in integer with CU2", "5 NMB", CreateNewDocSADHLineWrapper().Box41CusQuantityAndType);

			invoiceLine.JI_Tariff = "11111111";
			invoiceLine.JI_CustomsSecondQuantity = 1.123;
			invoiceLine.JI_CustomsSecondUnitQty = "LTR";
			AssertEquals("Customs quantity with decimals with CU2", "1,123 LTR", CreateNewDocSADHLineWrapper().Box41CusQuantityAndType);
		});
	}

	public void TestBox43ValuationCode()
	{
		AssertEquals("[PRE-CONDITION] ValuationCode", "1", CreateNewDocSADHLineWrapper().Box43ValuationCode);
		invoiceLine.JI_ValuationCode = "08";
		AssertEquals("ValuationCode", "08", CreateNewDocSADHLineWrapper().Box43ValuationCode);
	}

	public void TestBox44SupportingDocuments()
	{
		AssertEquals("[PRE-CONDITION] No documents", 0, CreateNewDocSADHLineWrapper().Box44SupportingDocuments.Count);
		CombineAssertions(() =>
		{
			var document1 = invoiceLine.SupportingDocuments.AddNew();
			document1.CSI_Code = "XXX";
			document1.CSI_ReferenceNumber = "Reference1";
			var document2 = invoiceLine.SupportingDocuments.AddNew();
			document2.CSI_Code = "ZZZ";
			document2.CSI_ReferenceNumber = "Reference2";
			AssertEquals("Doc1 type", "XXX", CreateNewDocSADHLineWrapper().Box44SupportingDocuments[0].ReferenceType);
			AssertEquals("Doc1 ref", "Reference1", CreateNewDocSADHLineWrapper().Box44SupportingDocuments[0].ReferenceNumber);
			AssertEquals("Doc2 type", "ZZZ", CreateNewDocSADHLineWrapper().Box44SupportingDocuments[1].ReferenceType);
			AssertEquals("Doc2 ref", "Reference2", CreateNewDocSADHLineWrapper().Box44SupportingDocuments[1].ReferenceNumber);
			AssertEquals("Document count", 2, CreateNewDocSADHLineWrapper().Box44SupportingDocuments.Count);
		});
	}

	public void TestAdjustments()
	{
		AssertEquals("[PRE-CONDITION] No adjustments", ZString.Empty, CreateNewDocSADHLineWrapper().Adjustments);
		entryLine.CL_CustomsValue = 100m;

		CombineAssertions(() =>
		{
			entryLine.CL_InvoiceAmount = 12.34m;
			AssertEquals("Adjustment should be present and rounded down", "88", CreateNewDocSADHLineWrapper().Adjustments);

			entryLine.CL_InvoiceAmount = 56.78m;
			AssertEquals("Adjustment should be present and rounded up", "43", CreateNewDocSADHLineWrapper().Adjustments);
		});
	}

	public void TestStatisticalValue()
	{
		AssertEquals("[PRE-CONDITION] No statistical values", ZString.Empty, CreateNewDocSADHLineWrapper().Adjustments);

		CombineAssertions(() =>
		{
			entryLine.CL_StatisticalValue = 12.34m;
			AssertEquals("Statistical value should be present and rounded down", "12", CreateNewDocSADHLineWrapper().StatisticalValue);

			entryLine.CL_StatisticalValue = 56.78m;
			AssertEquals("Statistical value should be present and rounded up", "57", CreateNewDocSADHLineWrapper().StatisticalValue);
		});
	}

	public void TestBox47TaxesSort()
	{
		AddNewEntryLineFee("GA200");
		AddNewEntryLineFee("MV1");
		AddNewEntryLineFee("TL1");

		var feeTypes = CreateNewDocSADHLineWrapper().Box47Taxes;
		var expected = new[] { "TL", "GA", "MV" };
		AssertContainsExactElementsInExactOrder("Duties types should be correct sorted", expected, feeTypes.Cast<NODocSADHLineTax>().Select(x => x.Type).ToArray());

		void AddNewEntryLineFee(ZString rateCode)
		{
			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_ChargeType = rateCode;
			fee.CF_ChargeAmount = 1m;
			entryLine.Fees.Add(fee);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = "BLT";
		declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
	}
	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CusEntryInstruction entryInstruction;
	CusEntryLine entryLine;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
}


