using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEAffirmationCodeCollection))]
	public class ACEAffirmationCodeCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<ACEAffirmationCode>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<ACEAffirmationCode> GetCusCodeDataCollection()
		{
			return AffirmationCodes;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return AffirmationCodes.AddNew();
		}

		ACEAffirmationCodeCollection AffirmationCodes
		{
			get
			{
				if (affirmationCodes == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var fda = invoiceLine.ACE_FDALines.AddNew();
					affirmationCodes = fda.AffirmationCodes;
				}
				return affirmationCodes;
			}
		}
		ACEAffirmationCodeCollection affirmationCodes;
	}
}
