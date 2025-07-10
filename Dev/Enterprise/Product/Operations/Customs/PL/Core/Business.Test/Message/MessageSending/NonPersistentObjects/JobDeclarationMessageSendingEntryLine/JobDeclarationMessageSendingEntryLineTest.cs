using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(JobDeclarationMessageSendingEntryLine))]
sealed class JobDeclarationMessageSendingEntryLineTest : NonPersistentBusinessObjectTestCase
{
	public void TestLine()
	{
		var line = Factory.New<CusEntryLine>();
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(line, collection);
		AssertEquals(line, messageSendingEntryLine.Line);
	}

	public void TestSend_InitialValue()
	{
		var line = Factory.New<CusEntryLine>();
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(line, collection);
		AssertEquals(false, messageSendingEntryLine.Send);
	}

	public void TestLineNumber_InitialValue()
	{
		var line = Factory.New<CusEntryLine>();
		line.CL_LineNumber = 41;
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(line, collection);
		AssertEquals(new ZShort(41), messageSendingEntryLine.LineNumber);
	}

	public void TestLineNumber_ReadOnly()
	{
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(Factory.New<CusEntryLine>(), collection);
		AssertEquals(true, messageSendingEntryLine.LineNumberInfo.ReadOnly);
	}

	public void TestTariffCode_InitialValue()
	{
		var line = Factory.New<CusEntryLine>();
		line.CL_AdValoremTariff = "8714101000";
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(line, collection);
		AssertEquals("8714.10.10 00", messageSendingEntryLine.TariffCode);
	}

	public void TestTariffCode_ReadOnly()
	{
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(Factory.New<CusEntryLine>(), collection);
		AssertEquals(true, messageSendingEntryLine.TariffCodeInfo.ReadOnly);
	}

	public void TestDescription_InitialValue()
	{
		var line = Factory.New<CusEntryLine>();
		line.EffectiveDescription = "Description 123";
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(line, collection);
		AssertEquals("Description 123", messageSendingEntryLine.Description);
	}

	public void TestDescription_ReadOnly()
	{
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(Factory.New<CusEntryLine>(), collection);
		AssertEquals(true, messageSendingEntryLine.DescriptionInfo.ReadOnly);
	}

	public void TestQuotaOrdNo_InitialValue()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var line = Factory.New<CusEntryLine>();
		invoiceLine.JI_CL = line.PK;
		invoiceLine.JI_ConcessionOrder = "A1S";
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(line, collection);
		AssertEquals("A1S", messageSendingEntryLine.QuotaOrdNo);
	}

	public void TestQuotaOrdNo_ReadOnly()
	{
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(Factory.New<CusEntryLine>(), collection);
		AssertEquals(true, messageSendingEntryLine.QuotaOrdNoInfo.ReadOnly);
	}

	public void TestSupUq_InitialValue()
	{
		const string unit = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		const string concessionOrder = "111";
		UniversalReferenceTestDataHelper.CreateOrFindExistingRefCusQuota(Factory, Core.Constants.CountryCodes.Poland, 10, 5, concessionOrder, unit, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var line = Factory.New<CusEntryLine>();
		invoiceLine.JI_CL = line.PK;
		entryInstruction.CEI_DateForDuty = ZDateTime.Today;
		invoiceLine.JI_ConcessionOrder = concessionOrder;
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(line, collection);
		AssertEquals(new ZString(unit), messageSendingEntryLine.SupUq);
	}

	public void TestSupUq_ReadOnly()
	{
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(Factory.New<CusEntryLine>(), collection);
		AssertEquals(true, messageSendingEntryLine.SupUqInfo.ReadOnly);
	}

	public void TestQuotaQuantity_InitialValue()
	{
		const string unit = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		const string concessionOrder = "111";
		UniversalReferenceTestDataHelper.CreateOrFindExistingRefCusQuota(Factory, Core.Constants.CountryCodes.Poland, 10, 5, concessionOrder, unit, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		var invoiceLine4 = invoice.InvoiceLines.AddNew();
		var line = Factory.New<CusEntryLine>();
		invoiceLine1.JI_CL = line.PK;
		invoiceLine2.JI_CL = line.PK;
		invoiceLine3.JI_CL = line.PK;
		invoiceLine4.JI_CL = line.PK;
		entryInstruction.CEI_DateForDuty = ZDateTime.Today;
		invoiceLine1.JI_ConcessionOrder = concessionOrder;
		invoiceLine2.JI_ConcessionOrder = concessionOrder;
		invoiceLine3.JI_ConcessionOrder = concessionOrder;
		invoiceLine4.JI_ConcessionOrder = concessionOrder;
		invoiceLine1.JI_CustomsQuantity = 1;
		invoiceLine1.JI_CustomsUnitQty = unit;
		invoiceLine2.JI_CustomsSecondQuantity = 2;
		invoiceLine2.JI_CustomsSecondUnitQty = unit;
		invoiceLine2.JI_CustomsUnitQty = ZString.Empty;
		invoiceLine3.JI_CustomsThirdQuantity = 3;
		invoiceLine3.JI_CustomsThirdUnitQty = unit;
		invoiceLine3.JI_CustomsUnitQty = ZString.Empty;
		invoiceLine4.JI_CustomsFourthQuantity = 4;
		invoiceLine4.JI_CustomsFourthUnitQty = unit;
		invoiceLine4.JI_CustomsUnitQty = ZString.Empty;
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(line, collection);
		AssertEquals(10M, messageSendingEntryLine.QuotaQuantity);
	}

	public void TestQuotaQuantity_ReadOnly()
	{
		var messageSendingEntryLine = new JobDeclarationMessageSendingEntryLine(Factory.New<CusEntryLine>(), collection);
		AssertEquals(true, messageSendingEntryLine.QuotaQuantityInfo.ReadOnly);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var line = Factory.New<CusEntryLine>();
		return new JobDeclarationMessageSendingEntryLine(line, collection);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var parent = new RetrospectiveQuotaRequestMessageSendingObjectParent(declaration);
		collection = new JobDeclarationMessageSendingEntryLineCollection(Factory, parent);
	}

	JobDeclarationMessageSendingEntryLineCollection collection;
}
