using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AMSLotCodeCollection))]
	public class AMSLotCodeCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<AMSLotCode>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<AMSLotCode> GetCusCodeDataCollection()
		{
			return AMSLotCodes;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return AMSLotCodes.AddNew();
		}

		AMSLotCodeCollection AMSLotCodes
		{
			get
			{
				if (amsLotCodeCollection == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var ams = invoiceLine.AMSLines.AddNew();
					ams.US_Program = AMSProgramList.Codes.PN1;
					var amsLine = ams.AMSLines.AddNew();
					amsLotCodeCollection = amsLine.LotCodes;
				}
				return amsLotCodeCollection;
			}
		}
		AMSLotCodeCollection amsLotCodeCollection;
	}
}
