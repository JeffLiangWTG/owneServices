using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobComInvoiceHeaderLookups))]
	abstract class JobComInvoiceHeaderLookupsAbstractTest : BusinessObjectLookupsTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			invoice = jobDeclaration.Invoices.AddNew();
			lookups = GetLookups();
		}
		protected JobDeclaration jobDeclaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceHeaderLookups lookups;

		protected abstract string MessageType { get; }
		protected abstract JobComInvoiceHeaderLookups GetLookups();
	}
}
