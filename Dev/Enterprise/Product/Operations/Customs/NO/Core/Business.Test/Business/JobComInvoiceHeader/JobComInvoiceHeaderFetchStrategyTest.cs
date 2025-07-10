using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(JobComInvoiceHeaderFetchStrategy))]
sealed class JobComInvoiceHeaderFetchStrategyTest : BusinessObjectFetchStrategyTestCase
{
	public void TestFetchForView_EffectiveValuationDate()
	{
		FetchStrategy.FetchForView([new("", JobComInvoiceHeader.Schema.EffectiveValuationDate)]);
		AssertCollectionContains(JobComInvoiceLine.Schema.TableName, Factory.GetAllFetchHintedTableNames());
	}

	protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory) => Declaration.Invoices;

	JobComInvoiceHeaderFetchStrategy FetchStrategy => fetchStrategy ??= (JobComInvoiceHeaderFetchStrategy)Declaration.Invoices.AddNew().FetchStrategy;
	JobComInvoiceHeaderFetchStrategy fetchStrategy;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
