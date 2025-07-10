using CargoWise.EntityFramework;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(BillCollection))]
	sealed class BillCollectionTest : Customs.Business.Testing.BaseHouseBillCollectionTest
	{
		[ExpectNoExceptions]
		public void TestSetDefaultsForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "MB2111";
			declaration.JE_HouseBill = "XX1111";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.ContainerNote;
			var bill2 = declaration.Bills.AddNew();
			NUnit.Framework.Assert.That(bill2.CU_BillType, NUnit.Framework.Is.EqualTo(BillTypeList.Codes.ContainerNote).Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return declaration.Bills;
		}
	}
}
