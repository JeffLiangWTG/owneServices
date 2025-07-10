using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class JobDeclarationMessageSendingEntryLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckSend()
	{
		const string message = "The ZCX05 message must have at least 1 Entry Line selected.";
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var line1 = entryHeader.AllEntryLines.AddNew();
		var line2 = entryHeader.AllEntryLines.AddNew();
		var invoiceLine1 = line1.InvoiceLines.AddNew();
		var invoiceLine2 = line2.InvoiceLines.AddNew();
		invoiceLine1.JI_ConcessionOrder = "123";
		invoiceLine2.JI_ConcessionOrder = "123";
		var parent = new RetrospectiveQuotaRequestMessageSendingObjectParent(declaration);
		var collection = new JobDeclarationMessageSendingEntryLineCollection(Factory, parent);
		collection.Load();
		var messageSendingEntryLine1 = collection[0];
		var messageSendingEntryLine2 = collection[1];

		CombineAssertions(() =>
		{
			messageSendingEntryLine1.Validation.ValidateSend();
			messageSendingEntryLine2.Validation.ValidateSend();
			AssertHasError("Initial state, messageSendingEntryLine1", messageSendingEntryLine1.SendInfo, message);
			AssertHasError("Initial state, messageSendingEntryLine2", messageSendingEntryLine2.SendInfo, message);
			messageSendingEntryLine1.Send = true;
			AssertNoError("MessageSendingEntryLine1 changed send to true, messageSendingEntryLine1", messageSendingEntryLine1.SendInfo, message);
			AssertNoError("MessageSendingEntryLine1 changed send to true, messageSendingEntryLine2", messageSendingEntryLine2.SendInfo, message);
		});
	}
}
