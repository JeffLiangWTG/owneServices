using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

abstract class AESExportOperationProviderTestBase<TProvider, TInterface> : DataProviderTestCase<TProvider, TInterface>
	where TInterface : class, IExportOperation
	where TProvider : AESExportOperationProvider, TInterface
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new AESExportOperationProvider(null));

		var sendingObject = new BaseMessageSendingObject(Factory.New<CusEntryHeader>());
		AssertExceptionThrown<ArgumentNullException>("Null Related Declaration", "Value cannot be null.\r\nParameter name: EntryHeader.Declaration", () => new AESExportOperationProvider(sendingObject));

		sendingObject = new BaseMessageSendingObject((CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew());
		AssertExceptionThrown<ArgumentNullException>("Null Related Instruction", "Value cannot be null.\r\nParameter name: EntryHeader.EntryInstruction", () => new AESExportOperationProvider(sendingObject));
	});

	public void TestDeclarationType() => AssertEquals("AB", GetProvider().DeclarationType);

	public void TestAdditionalDeclarationType() => AssertEquals("C", GetProvider().AdditionalDeclarationType);

	public virtual void TestPresentationOfTheGoodsDateAndTime() => CombineAssertions(() =>
	{
		var testDate = DateTime.UtcNow.Date;
		Declaration.ZG_PresentationStartDate = testDate;
		AssertEquals("ZG_PresentationStartDate is not empty", testDate, GetProvider().PresentationOfTheGoodsDateAndTime);

		Declaration.ZG_PresentationStartDate = ZDateTime.Empty;
		AssertNull("ZG_PresentationStartDate is not defined", GetProvider().PresentationOfTheGoodsDateAndTime);
	});

	public void TestSecurity() => CombineAssertions(() =>
	{
		AssertEquals("Security is empty", string.Empty, GetProvider().Security);

		SendingObject.Security = ExportSecurityTypeList.Codes.EXS;
		AssertEquals("Security is 2", "2", GetProvider().Security);

		SendingObject.Security = "A";
		AssertEquals("Security is A", "A", GetProvider().Security);
	});

	public virtual void TestSpecificCircumstanceIndicator() => CombineAssertions(() =>
	{
		AssertNull("Empty SpecificCircumstanceIndicator", GetProvider().SpecificCircumstanceIndicator);

		Declaration.ZG_SpecificCircumstanceIndicator = "a";
		AssertEquals("Not empty SpecificCircumstanceIndicator", "a", GetProvider().SpecificCircumstanceIndicator);
	});

	public virtual void TestStorage() => CombineAssertions(() =>
	{
		AssertNull("Export Manifest doesn't exist", GetProvider().Storage);

		Declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.V;
		Declaration.JE_CustomsOffice = "asd";
		Declaration.JE_OfficeOfEntryExit = "asd";
		EntryInstruction.ZG_ExportManifest = false;
		AssertEquals("Export Manifest is false", false, GetProvider().Storage);

		EntryInstruction.ZG_ExportManifest = true;
		AssertEquals("Export Manifest is true", true, GetProvider().Storage);

		Declaration.JE_OfficeOfEntryExit = "zxc";
		AssertNull("Export Manifest different JE_OfficeOfEntryExit and GoodsLocationCustomsOffice", GetProvider().Storage);

		Declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.W;
		AssertNull("Export Manifest JE_LocationQualifier is not office type", GetProvider().Storage);
	});

	public virtual void TestEadPrint() => CombineAssertions(() =>
	{
		EntryInstruction.ZG_EADPrintOut = "S";
		AssertEquals("Not numeric", null, GetProvider().EadPrint);

		EntryInstruction.ZG_EADPrintOut = null;
		AssertEquals("Null", null, GetProvider().EadPrint);

		EntryInstruction.ZG_EADPrintOut = "4";
		AssertEquals("Numeric", (byte)4, GetProvider().EadPrint);
	});

	protected override void SetUp()
	{
		base.SetUp();
		Declaration = Factory.New<JobDeclaration>();
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		Declaration.JE_EntryStyle = "AB";
		EntryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		EntryInstruction.CEI_SubStyle = "C";
		EntryHeader = Declaration.CustomsEntryHeaders.AddNew();
		EntryHeader.CH_BGMReference = "ReferenceNumberTest";
		EntryHeader.CH_CEI_Instruction = EntryInstruction.PK;
		EntryLine = EntryHeader.AllEntryLines.AddNew();
		Invoice = Declaration.Invoices.AddNew();
		InvoiceLine = Invoice.InvoiceLines.AddNew();
		InvoiceLine.JI_CEI = EntryInstruction.PK;
		InvoiceLine.JI_CL = EntryLine.PK;
		InvoiceLine2 = Invoice.InvoiceLines.AddNew();
		InvoiceLine2.JI_CEI = EntryInstruction.PK;
		InvoiceLine2.JI_CL = EntryLine.PK;

		SendingObject = new BaseMessageSendingObject(EntryHeader);
	}

	class InvoiceChargeWithOverridenJ7Amount : InvoiceCharge
	{
		public InvoiceChargeWithOverridenJ7Amount(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
		{
		}

		public override ZDecimal J7_Amount { get; set; }
	}

	protected JobDeclaration Declaration { get; set; }
	protected CusEntryInstruction EntryInstruction { get; set; }
	protected JobComInvoiceHeader Invoice { get; set; }
	protected JobComInvoiceLine InvoiceLine { get; set; }
	protected JobComInvoiceLine InvoiceLine2 { get; set; }
	protected CusEntryHeader EntryHeader { get; set; }
	protected CusEntryLine EntryLine { get; set; }
	protected BaseMessageSendingObject SendingObject { get; set; }
}
