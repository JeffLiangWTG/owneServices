using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USFSISLotAddInfo))]
	class USFSISLotAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new USFSISLotAddInfo(Lot.B7_AddInfoDataInfo);
		}

		USFSISLot Lot
		{
			get { return fLot ?? (fLot = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew().Lots.AddNew()); }
		}
		USFSISLot fLot;
	}
}
