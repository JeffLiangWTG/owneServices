using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobComInvoiceHeaderValidation))]
	abstract class JobComInvoiceHeaderValidationAbstractTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			invoice = jobDeclaration.Invoices.AddNew();
			validation = GetValidation();
		}
		protected JobDeclaration jobDeclaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceHeaderValidation validation;

		protected abstract string MessageType { get; }

		protected abstract JobComInvoiceHeaderValidation GetValidation();
	}
}
