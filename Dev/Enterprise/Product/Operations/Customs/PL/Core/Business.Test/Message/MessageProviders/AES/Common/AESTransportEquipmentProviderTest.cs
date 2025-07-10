using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.PL.Business.Declaration.CusEntryLine;
using CusEquipment = Enterprise.Customs.EU.Business.Declaration.CusEquipment;
using JobComInvoiceLine = Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine;
using JobDeclaration = Enterprise.Customs.PL.Business.Declaration.JobDeclaration;
using LineMerger = Enterprise.Customs.PL.Business.Declaration.LineMerger;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESTransportEquipmentProviderTest : DataProviderTestCase<AESTransportEquipmentProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null EntryInstruction", "Value cannot be null.\r\nParameter name: cusEntryInstruction",
				() => new AESTransportEquipmentProvider(1, null, null, false));

			AssertExceptionThrown<ArgumentNullException>("Null CusEquipment", "Value cannot be null.\r\nParameter name: cusEquipment",
				() => new AESTransportEquipmentProvider(1, cusEntryInstruction, null, false));
		});
	}

	public void TestSequenceNumber()
	{
		AssertEquals(1, Provider.SequenceNumber);
	}

	public void TestContainerIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals(String.Empty, GetProvider().ContainerIdentificationNumber);

			var provider = new AESTransportEquipmentProvider(1, cusEntryInstruction, equipment, true);
			AssertEquals("asd", provider.ContainerIdentificationNumber);
		});
	}

	public void TestNumberOfSeals()
	{
		var seal1 = equipment.Seals.AddNew();
		seal1.BK_SealNumber = "asd1";
		var seal2 = equipment.Seals.AddNew();
		seal2.BK_SealNumber = "asd2";
		AssertEquals(2, GetProvider().NumberOfSeals);
	}

	public void TestSeals()
	{
		var seal1 = equipment.Seals.AddNew();
		var seal2 = equipment.Seals.AddNew();
		seal1.BK_SealNumber = "Seal1";
		seal2.BK_SealNumber = "Seal2";
		CombineAssertions(() =>
		{
			AssertEquals("Non container - count", 2, GetProvider().Seals.Count);
			AssertEquals("Non container - identifiers", "Seal1,Seal2", string.Join(",", GetProvider().Seals.Select(x => x.Identifier)));
			TestHelper.TestCollectionHasCorrectIndexOrder(GetProvider().Seals, (item) => item.SequenceNumber);
		});
	}

	public void TestGoodsReferences()
	{
		var merger = new LineMerger(declaration);
		merger.DoMerge();
		var entryHeader = cusEntryInstruction.EntryHeader;
		var firstLineNumber = entryHeader.AllEntryLines.Cast<CusEntryLine>().First().CL_LineNumber;
		CombineAssertions(() =>
		{
			AssertEquals("Non container - count", 2, GetProvider().GoodsReferences.Count);
			AssertEquals("Non container - DeclarationGoodsItemNumber", firstLineNumber,
				GetProvider().GoodsReferences.First().DeclarationGoodsItemNumber);
		});
	}

	protected override AESTransportEquipmentProvider GetProvider() => new AESTransportEquipmentProvider(1, cusEntryInstruction, equipment, false);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		equipment = declaration.Equipments.AddNew();
		equipment.CEQ_IdentificationNumber = "asd";

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = cusEntryInstruction.PK;
		invoiceLine1.ZG_CountryOfSupply = Core.Constants.CountryCodes.Poland;
		invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = cusEntryInstruction.PK;
		invoiceLine2.ZG_CountryOfSupply = Core.Constants.CountryCodes.Germany;
	}

	JobDeclaration declaration;
	CusEntryInstruction cusEntryInstruction;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceLine invoiceLine2;
	CusEquipment equipment;
}
