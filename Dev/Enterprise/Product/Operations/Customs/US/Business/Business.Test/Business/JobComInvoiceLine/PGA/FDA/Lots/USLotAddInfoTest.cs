using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USLotAddInfo))]
	public class USLotAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLot()
		{
			var lot = Factory.New<Lot>();
			var addInfo = new USLotAddInfo(lot.B7_AddInfoDataInfo);
			AssertEquals(lot.PK, addInfo.Lot.PK);
		}

		public void TestGetNewValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			var lot = fda.Lots.AddNew();

			var addInfo = new USLotAddInfo(lot.B7_AddInfoDataInfo);
			AssertNoExceptionThrown(() =>
			{
				var validation = addInfo.Validation;
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var lot = Factory.New<Lot>();
			var addInfo = new USLotAddInfo(lot.B7_AddInfoDataInfo);
			return addInfo;
		}
	}
}
