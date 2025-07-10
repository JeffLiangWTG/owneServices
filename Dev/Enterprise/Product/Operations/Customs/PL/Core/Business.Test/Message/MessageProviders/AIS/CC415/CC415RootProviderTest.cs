using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC415RootProviderTest : Customs.Business.Testing.DataProviderTestCase<CC415RootProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new CC415RootProvider(sendingObject: null));
		AssertExceptionThrown<ArgumentNullException>("Null Related Declaration", "Value cannot be null.\r\nParameter name: EntryHeader.Declaration", () => new CC415RootProvider(new BaseMessageSendingObject(Factory.New<CusEntryHeader>())));
		AssertExceptionThrown<ArgumentNullException>("Null Related Instruction", "Value cannot be null.\r\nParameter name: EntryHeader.EntryInstruction", () => new CC415RootProvider(new BaseMessageSendingObject(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew())));
		AssertNoExceptionThrown("Valid data", () => new CC415RootProvider(new BaseMessageSendingObject(entryHeader)));
	});

	public void TestOfficeIdentifier() => AssertNull(GetProvider().OfficeIdentifier);

	public void TestLRN() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("empty", GetProvider().LRN);

		sendingObject.LocalReferenceNumber = "asd";
		AssertEquals("not empty", "asd", GetProvider().LRN);
	});

	public void TestDeclarationType() => CombineAssertions(() =>
	{
		declaration.JE_EntryStyle = "A";
		AssertEquals("not empty", "A", GetProvider().DeclarationType);
	});

	public void TestAdditionalDeclarationType() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("empty", GetProvider().AdditionalDeclarationType);
		instruction.CEI_SubStyle = "B";
		AssertEquals("not empty", "B", GetProvider().AdditionalDeclarationType);
	});

	public void TestEmailOfTemporaryStorageFacilityOperator() => AssertNull(GetProvider().EmailOfTemporaryStorageFacilityOperator);

	public void TestEMailOfWarehouseAuthorisationHolder() => AssertNull(GetProvider().EMailOfWarehouseAuthorisationHolder);

	public void TestInternalCurrencyUnit() => AssertNull(GetProvider().InternalCurrencyUnit);

	public void TestTotalAmountInvoiced() => AssertNull(GetProvider().TotalAmountInvoiced);

	public void TestExchangeRate() => AssertNull(GetProvider().ExchangeRate);

	public void TestDeferredPayments() => AssertNotNull(GetProvider().DeferredPayments);

	public void TestAuthorisations() => AssertNotNull(GetProvider().Authorisations);

	public void TestApplicationAndAuthorisationForSpecialProcedures() => AssertNull(GetProvider().ApplicationAndAuthorisationForSpecialProcedures);

	public void TestImporter() => AssertNull(GetProvider().Importer);

	public void TestDeclarant() => AssertNull(GetProvider().Declarant);

	public void TestRepresentative() => AssertNull(GetProvider().Representative);

	public void TestCustomsOfficeOfPresentation() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("does not exist", GetProvider().CustomsOfficeOfPresentation);
		var office = declaration.CustomsOfficesForBinding.AddNew();
		office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		office.CY_Data = ZString.Empty;
		AssertEquals("Empty CustomsOfficeOfPresentation", string.Empty, GetProvider().CustomsOfficeOfPresentation);
		office.CY_Data = "asd";
		AssertEquals("Not Empty CustomsOfficeOfPresentation", "asd", GetProvider().CustomsOfficeOfPresentation);
	});

	public void TestSupervisingCustomsOffice() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("does not exist", GetProvider().SupervisingCustomsOffice);
		var office = declaration.CustomsOfficesForBinding.AddNew();
		office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
		office.CY_Data = ZString.Empty;
		AssertEquals("Empty CustomsOfficeOfSupervising", string.Empty, GetProvider().SupervisingCustomsOffice);
		office.CY_Data = "asd";
		AssertEquals("Not Empty CustomsOfficeOfSupervising", "asd", GetProvider().SupervisingCustomsOffice);
	});

	public void TestGuarantees() => AssertNotNull(GetProvider().Guarantees);

	public void TestGoodsShipments() => AssertNotNull(GetProvider().GoodsShipments);

	public void TestOperatorsEmails() => AssertNotNull(GetProvider().OperatorsEmails);

	public void TestPersonPayingCustomsDutyIdentificationNumber() => AssertNull(GetProvider().PersonPayingCustomsDutyIdentificationNumber);

	public void TestPersonProvidingGuaranteeIdentificationNumber() => AssertNull(GetProvider().PersonProvidingGuaranteeIdentificationNumber);

	public void TestCustomsOfficeOfDeclarationReferenceNumber()
	{
		AssertNullOrEmpty("empty", GetProvider().CustomsOfficeOfDeclarationReferenceNumber);
		declaration.JE_CustomsOffice = "Office123";
		AssertEquals("not empty", "Office123", GetProvider().CustomsOfficeOfDeclarationReferenceNumber);
	}

	protected override CC415RootProvider GetProvider() => new CC415RootProvider(sendingObject);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		entryHeader = declaration.CustomsEntryHeaders.Single();

		sendingObject = new BaseMessageSendingObject(entryHeader);
	}
	CusEntryHeader entryHeader;
	JobDeclaration declaration;
	CusEntryInstruction instruction;
	BaseMessageSendingObject sendingObject;
}
