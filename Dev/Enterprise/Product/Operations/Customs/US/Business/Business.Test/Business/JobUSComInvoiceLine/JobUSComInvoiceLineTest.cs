using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(JobUSComInvoiceLine))]
	sealed class JobUSComInvoiceLineTest : IAddInfoChildUniqueIndexFailureHandlerSupporterTestCase<JobUSComInvoiceLine>
	{
		public void TestIJobUSComInvoiceLineIsCorrectlySetup()
		{
			var data = (BusinessObject)Factory.New<Integration.Customs.US.IJobUSComInvoiceLine>();
			AssertType<JobUSComInvoiceLine>(data);
			AssertType<JobUSComInvoiceLine>(Factory.Load(data.TablePrefix, data.PK));
		}

		public void TestValidation()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var addInfoChild = invoiceLine.AddInfoChild;
			AssertType<JobUSComInvoiceLineValidation>(addInfoChild.Validation);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine.WipeJzForTesting();
			invoiceLine.JI_JZ = invoice.PK;
			AssertType<DrawbackJobUSComInvoiceLineValidation>(addInfoChild.Validation);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertType<JobUSComInvoiceLineValidation>(addInfoChild.Validation);
		}

		protected override void SetUpSystemLastEditOnParent(EnterpriseBusinessObject parent, ZString systemLastEditUser, ZDateTime systemLastEditTimeUtc)
		{
			var invoiceLine = (JobComInvoiceLine)parent;
			invoiceLine.JI_SystemLastEditUser = systemLastEditUser;
			invoiceLine.JI_SystemLastEditTimeUtc = systemLastEditTimeUtc;
		}

		protected override string ExpectedUniqueIndexName => ZArchitecture.Schema.JobUSComInvoiceLineSchema.Constants.Indexes.FK_UX__USI_JI;

		protected override EnterpriseBusinessObject GetParent(JobUSComInvoiceLine bizObj) => bizObj.InvoiceLine;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.AddInfoChild;
		}
	}
}
