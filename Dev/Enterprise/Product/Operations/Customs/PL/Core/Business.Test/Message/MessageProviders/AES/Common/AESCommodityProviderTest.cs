using System.Linq;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class AESCommodityProviderTest : Customs.Business.Testing.DataProviderTestCase<AESCommodityProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertNull("Null entryLine", AESCommodityProvider.NewOrNull(null));
		AssertNotNull("EntryLine is not null", AESCommodityProvider.NewOrNull(EntryLine));
	});

	public void TestDescriptionOfGoods() => CombineAssertions(() =>
	{
		invoiceLine.JI_Description = string.Empty;
		AssertEquals("Empty JI_NDescription and JI_Description", string.Empty, GetProvider().DescriptionOfGoods);

		invoiceLine.JI_Description = "Description";
		AssertEquals("Empty JI_NDescription", "Description", GetProvider().DescriptionOfGoods);

		invoiceLine.JI_NDescription = "NDescription";
		AssertEquals("Not Empty JI_NDescription", "NDescription", GetProvider().DescriptionOfGoods);
	});

	public void TestDescriptionOfGoodsMaxLength() => CombineAssertions(() =>
	{
		AssertEquals("Not in UCC6 transition period", 512, Provider.DescriptionOfGoodsMaxLength);
		using (declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AssertEquals("UCC6 transition period", 280, AESCommodityProvider.NewOrNull(EntryLine).DescriptionOfGoodsMaxLength);
		}
	});

	public void TestCusCode() => CombineAssertions(() =>
	{
		AssertEquals("Empty ZG_CusNumber", string.Empty, GetProvider().CusCode);
		invoiceLine.AddInfo.ZG_CusNumber = "123";
		AssertEquals("ZG_CusNumber", "123", GetProvider().CusCode);
	});

	public void TestCommodityCode() => AssertNotNull(Provider.CommodityCode);

	public virtual void TestDangerousGoods()
	{
		var undg1 = invoiceLine.UNDGs.AddNew();
		var substance = Factory.New<UNDGSubstance>();
		undg1.DI_DG = substance.PK;
		invoiceLine.UNDGs.AddNew();
		AssertEquals(1, Provider.DangerousGoods.Count);
	}

	public void TestGoodsMeasure() => AssertNotNull(Provider.CommodityCode);

	public void TestCalculationOfTaxes() => AssertNotNull(Provider.CalculationOfTaxes);

	protected override AESCommodityProvider GetProvider() => AESCommodityProvider.NewOrNull(EntryLine);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		Instruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "123";
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = Instruction.PK;
		invoiceLine.JI_Description = "invoice line 1";

		lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		entryHeader = declaration.CustomsEntryHeaders.First();
		EntryLine = entryHeader.AllEntryLines.FirstOrDefault();
	}

	LineMerger lineMerger;
	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	protected CusEntryLine EntryLine { get; set; }
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice;
	protected CusEntryInstruction Instruction { get; set; }
}
