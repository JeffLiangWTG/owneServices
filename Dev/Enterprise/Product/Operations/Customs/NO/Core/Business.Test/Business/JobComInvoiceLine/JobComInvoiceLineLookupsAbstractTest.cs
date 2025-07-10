using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestsSubclassesOf(typeof(JobComInvoiceLineLookups))]
abstract class JobComInvoiceLineLookupsAbstractTest<T> : BusinessObjectLookupsTestCase
	where T : JobComInvoiceLineLookups
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		_ = AssertArgumentExceptionThrown<ArgumentNullException>("parent", () => _ = new JobComInvoiceLineLookups(null));
		AssertNoExceptionThrown(() => _ = new JobComInvoiceLineLookups(invoiceLine));
	});

	public void TestLookupsType()
	{
		AssertType<T>(invoiceLine.Lookups);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		header = declaration.Invoices.AddNew();
		invoiceLine = header.JobComInvoiceLines.AddNew();
		lookups = invoiceLine.Lookups as T;
	}

	protected JobDeclaration declaration;
	protected JobComInvoiceHeader header;
	protected JobComInvoiceLine invoiceLine;
	protected T lookups;

	protected abstract string MessageType { get; }
}
