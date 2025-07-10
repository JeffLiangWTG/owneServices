using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(LicenceAndPermitCollection))]
	sealed class LicenceAndPermitCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<LicenceAndPermit>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<LicenceAndPermit> GetCusCodeDataCollection() => InvoiceLine.LicenceAndPermits;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<LicenceAndPermit>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = "JI";
			return result;
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.Invoices.AddNew();
					invoiceLine = declaration.InvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}
	}
}
