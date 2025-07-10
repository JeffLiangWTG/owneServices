using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(Bill))]
	sealed class BillTest : Customs.Business.Testing.BaseHouseBillTest<Bill, JobDeclaration>
	{
		[ExpectNoExceptions]
		public void TestTypeDecider()
		{
			NUnit.Framework.Assert.That(Factory.New(typeof(Customs.Business.Bill)).GetType(), NUnit.Framework.Is.EqualTo(GetExpectedBusinessObjectType()), "Update Customs.Business.BaseHouseBill to include a decider for this class");
		}

		[ExpectNoExceptions]
		public void TestCU_BillType()
		{
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_ParentBillUniqueCode = "XXX";
			bill.CU_BillType = BillTypeList.Codes.ContainerNote;
			NUnit.Framework.Assert.That(bill.CU_ParentBillUniqueCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "XX1112";
			return houseBill;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "MB2111";
			declaration.JE_HouseBill = "XX1111";
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
	}
}
