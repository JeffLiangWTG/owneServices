using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class InvoiceLineCustomsChargeTypeListTest : TestCaseWithFactory
	{
		public void TestImvoiceLineCustomsChargeTypeList_Import()
		{
			var testList = new ImportIncoTermAndCustomsChargeFactory().GetChargeList(Common.ChargeParentTypes.InvoiceLine);
			AssertEquals(2, testList.Count());
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.Discount));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue));
		}

		public void TestImvoiceLineCustomsChargeTypeList_NonImport()
		{
			var testList = new IncoTermAndCustomsChargeFactory().GetChargeList(Common.ChargeParentTypes.InvoiceLine);
			AssertEquals(1, testList.Count());
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.Discount));
		}

		public void TestImvoiceLineCustomsChargeTypeList_Complete()
		{
			var testList = new ImportIncoTermAndCustomsChargeFactory().GetChargeList(Common.ChargeParentTypes.GroupInvoice);
			AssertEquals(11, testList.Count());
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.AdditionCharge));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.Commission));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.DeductionCharge));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.ExWorks));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.ForeignInlandFreight));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.LandingCharges));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.OtherCharges));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.OverseasFreight));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.OverseasInsurance));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.PackingCost));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.Discount));
			testList = new ImportIncoTermAndCustomsChargeFactory().GetChargeList(Common.ChargeParentTypes.Invoice);
			AssertEquals(11, testList.Count());
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.AdditionCharge));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.Commission));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.DeductionCharge));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.ExWorks));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.ForeignInlandFreight));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.LandingCharges));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.OtherCharges));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.OverseasFreight));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.OverseasInsurance));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.PackingCost));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.Discount));
			testList = new ImportIncoTermAndCustomsChargeFactory().GetChargeList(Common.ChargeParentTypes.InvoiceLine);
			AssertEquals(2, testList.Count());
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue));
			Assert(testList.Any(x => x.Code == InvoiceLineCustomsChargeTypeList.Codes.Discount));
		}
	}
}
