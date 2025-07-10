using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public abstract class PGALineStatusTest : TestCaseWithFactory
	{
		public void TestPGALineStatusMembers()
		{
			var master = Master;
			Assert(ZString.Format("{0} Government Agency Code {1}", PGAType, GovernmentAgencyCode), master.PGALineStatusAgencyCode == GovernmentAgencyCode);
			AssertPGALineNumber(master);

			var dispositionParent = master as ICusDispositionParent;
			AssertEquals(ZString.Format("{0} CusDisposition Type", PGAType), dispositionParent.Type, CusDispositionTypeCodeList.Codes.USPGALineStatus);
			AssertEquals(ZString.Format("{0} CusDisposition ParentTableCode", PGAType), dispositionParent.ParentTableCode, ParentTableCode);
			AssertEquals(ZString.Format("{0} CusDisposition CollectionMaster", PGAType), dispositionParent.CollectionMaster, CollectionMaster);
		}

		protected virtual void AssertPGALineNumber(IPGALineStatus master)
		{
			Assert(ZString.Format("{0} Government Agency Code", PGAType), master.PGALineNumber > 0);
		}

		public abstract IPGALineStatus Master { get; }
		public abstract ZString GovernmentAgencyCode { get; }
		public abstract ZString ProgramCode { get; }
		public abstract ZString ParentTableCode { get; }
		public abstract BusinessObject CollectionMaster { get; }
		public abstract ZString PGAType { get; }

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		public JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;
	}
}
