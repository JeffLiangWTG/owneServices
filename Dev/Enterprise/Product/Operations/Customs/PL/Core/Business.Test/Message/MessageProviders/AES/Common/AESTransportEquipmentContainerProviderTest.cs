using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESTransportEquipmentContainerProviderTest : DataProviderTestCase<AESTransportEquipmentContainerProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null Container", "Value cannot be null.\r\nParameter name: container",
			() => new AESTransportEquipmentContainerProvider(1, null, false));
	}

	public void TestSequenceNumber()
	{
		AssertEquals(1, Provider.SequenceNumber);
	}

	public void TestContainerIdentificationNumber()
	{
		container.CO_ContainerNumber = "123";
		AssertEquals("123", Provider.ContainerIdentificationNumber);
	}

	public void TestNumberOfSeals()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Container - empty CO_Seal, empty CO_SecondSeal", 0, GetProvider().NumberOfSeals);
			container.CO_Seal = "A";
			AssertEquals("Container - not empty CO_Seal, empty CO_SecondSeal", 1, GetProvider().NumberOfSeals);
			container.CO_SecondSeal = "B";
			AssertEquals("Container - not empty CO_Seal, not empty CO_SecondSeal", 2, GetProvider().NumberOfSeals);
			container.CO_Seal = ZString.Empty;
			AssertEquals("Container - empty CO_Seal, not empty CO_SecondSeal", 1, GetProvider().NumberOfSeals);
		});
	}

	public void TestSeals()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Container - empty CO_Seal, empty CO_SecondSeal - count", 0, GetProvider().Seals.Count);
			container.CO_Seal = "A";
			AssertEquals("Container - not empty CO_Seal, empty CO_SecondSeal - count", 1, GetProvider().Seals.Count);
			AssertEquals("Container - not empty CO_Seal, empty CO_SecondSeal - identifiers", "A", string.Join(",", GetProvider().Seals.Select(x => x.Identifier)));
			AssertEquals("Container - not empty CO_Seal, empty CO_SecondSeal - sequence numbers", "1", string.Join(",", GetProvider().Seals.Select(x => x.SequenceNumber)));
			container.CO_SecondSeal = "B";
			AssertEquals("Container - not empty CO_Seal, not empty CO_SecondSeal - count", 2, GetProvider().Seals.Count);
			AssertEquals("Container - not empty CO_Seal, not empty CO_SecondSeal - identifiers", "A,B", string.Join(",", GetProvider().Seals.Select(x => x.Identifier)));
			AssertEquals("Container - not empty CO_Seal, not empty CO_SecondSeal - sequence numbers", "1,2", string.Join(",", GetProvider().Seals.Select(x => x.SequenceNumber)));
			container.CO_Seal = ZString.Empty;
			AssertEquals("Container - empty CO_Seal, not empty CO_SecondSeal - count", 1, GetProvider().Seals.Count);
			AssertEquals("Container - empty CO_Seal, not empty CO_SecondSeal - identifiers", "B", string.Join(",", GetProvider().Seals.Select(x => x.Identifier)));
			AssertEquals("Container - empty CO_Seal, not empty CO_SecondSeal - sequence numbers", "1", string.Join(",", GetProvider().Seals.Select(x => x.SequenceNumber)));
		});
	}

	public void TestGoodsReferences()
	{
		var merger = new Declaration.LineMerger(declaration);
		merger.DoMerge();
		CombineAssertions(() =>
		{
			AssertEquals("Container not assigned - count", 0, GetProvider().GoodsReferences.Count);
			invoiceLine1.ContainersPivot.AddPivotFor(container);
			container.InvoiceLinePivotCollection.Load();
			AssertEquals("Container assigned to one invoice line - count", 1, GetProvider().GoodsReferences.Count);
			invoiceLine2.ContainersPivot.AddPivotFor(container);
			container.InvoiceLinePivotCollection.Load();
			AssertEquals("Container assigned to one two line - count", 2, GetProvider().GoodsReferences.Count);
			AssertEquals("Container assigned to one two line, everyInvoiceLineInSameContainer true - count", 0,
				GetProvider(true).GoodsReferences.Count);
		});
	}

	AESTransportEquipmentContainerProvider GetProvider(bool everyInvoiceLineInSameContainer) =>
		new AESTransportEquipmentContainerProvider(1, container, everyInvoiceLineInSameContainer);

	protected override AESTransportEquipmentContainerProvider GetProvider() => GetProvider(false);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = cusEntryInstruction.PK;
		invoiceLine1.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Poland;
		invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = cusEntryInstruction.PK;
		invoiceLine2.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Germany;
		container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = "123";
	}

	JobDeclaration declaration;
	BaseCusContainer container;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceLine invoiceLine2;
}
