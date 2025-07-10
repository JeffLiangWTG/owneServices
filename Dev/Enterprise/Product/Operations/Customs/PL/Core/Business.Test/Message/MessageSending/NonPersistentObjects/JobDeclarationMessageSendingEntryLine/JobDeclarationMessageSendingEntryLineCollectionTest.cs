using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(JobDeclarationMessageSendingEntryLineCollection))]
sealed class JobDeclarationMessageSendingEntryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDeclarationMessageSendingEntryLineCollection>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new JobDeclarationMessageSendingEntryLineCollection(Factory, null));
	}

	public void TestAnyToSend()
	{
		var collection = GetCollectionToTest();
		var elementToAdd = (JobDeclarationMessageSendingEntryLine)GetNewElementToAddToTheCollection();
		collection.Add(elementToAdd);
		CombineAssertions(() =>
		{
			AssertEquals("Nothing to send", false, collection.AnyToSend());
			elementToAdd.Send = true;
			AssertEquals("One to send", true, collection.AnyToSend());
		});
	}

	public void TestLoad()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var parent = new RetrospectiveQuotaRequestMessageSendingObjectParent(declaration);
		var collection = new JobDeclarationMessageSendingEntryLineCollection(Factory, parent);
		CombineAssertions(() =>
		{
			collection.Load();
			AssertEquals("Empty", 0, collection.Count);
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_ConcessionOrder = "123";
			entryHeader.AllEntryLines.AddNew();
			entryHeader.AllEntryLines.AddNew();
			collection.Load();
			AssertEquals("Not empty", 1, collection.Count);
		});
	}

	protected override JobDeclarationMessageSendingEntryLineCollection GetCollectionToTest() => collection;

	protected override BusinessObject GetNewElementToAddToTheCollection()
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
