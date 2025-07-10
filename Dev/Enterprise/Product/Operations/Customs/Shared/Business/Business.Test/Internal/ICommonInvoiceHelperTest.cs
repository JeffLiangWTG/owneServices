using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ICommonInvoiceHelperTest : TestCaseWithFactory
	{
		public void TestGetParentWithThisChargeType()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvHeaderCharge topGroupOFT = topGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");

			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvHeaderCharge subGroupONS = subGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, "AUD");

			BaseJobComInvoiceHeader invoice = subGroup.JobComInvoiceHeaders.AddNew();
			BaseJobComInvHeaderCharge invOFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 25m, "AUD");

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("GetParent With OFT from InvoiceLine", invoice, invoiceLine.GetParentWithThisChargeType(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true)));
			AssertEquals("GetParent with ONS from InvoiceLine", subGroup, invoiceLine.GetParentWithThisChargeType(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasInsurance, false, true)));
			AssertEquals("GetParent with ADD from InvoiceLine", null, invoiceLine.GetParentWithThisChargeType(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.AdditionCharge, true, true)));

			AssertEquals("GetParent with OFT from SubGroup", topGroup, subGroup.GetParentWithThisChargeType(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true)));
			AssertEquals("GetParent with OFT from Invoice", topGroup, invoice.GetParentWithThisChargeType(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true)));

			AssertEquals("GetParent With OFT from InvoiceLine", invoice, invoiceLine.GetParentWithThisChargeType(invOFT.ApportionChargeKey));
			AssertEquals("GetParent with ONS from InvoiceLine", subGroup, invoiceLine.GetParentWithThisChargeType(subGroupONS.ApportionChargeKey));
			AssertEquals("GetParent with ADD from InvoiceLine", null, invoiceLine.GetParentWithThisChargeType(new ApportionChargeKey(CustomsChargeTypeList.Codes.AdditionCharge, true, true, ApportionmentTypeList.Codes.PartialApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m, false, "", false)));

			AssertEquals("GetParent with OFT from SubGroup", topGroup, subGroup.GetParentWithThisChargeType(topGroupOFT.ApportionChargeKey));
			AssertEquals("GetParent with OFT from Invoice", topGroup, invoice.GetParentWithThisChargeType(topGroupOFT.ApportionChargeKey));
		}

		public void TestHasParentChargeDistributedByThisField()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvHeaderCharge topGroupOFT = topGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			topGroupOFT.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;

			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvHeaderCharge subGroupONS = subGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 50m, "AUD");
			subGroupONS.J7_DistributeBy = ChargeDistributeByList.Codes.Volume;

			BaseJobComInvoiceHeader invoice = subGroup.JobComInvoiceHeaders.AddNew();
			BaseJobComInvHeaderCharge invOFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 25m, "AUD");
			invOFT.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Has Parent distributed by Weight from InvoiceLine", true, invoiceLine.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Weight));
			AssertEquals("Has Parent distributed by Value from InvoiceLine", true, invoiceLine.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Value));
			AssertEquals("Has Parent distributed by Volume from InvoiceLine", true, invoiceLine.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Volume));

			AssertEquals("Has Parent distributed by Weight from Invoice", true, invoice.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Weight));
			AssertEquals("Has Parent distributed by Value from Invoice", false, invoice.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Value));
			AssertEquals("Has Parent distributed by Volume from Invoice", true, invoice.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Volume));

			AssertEquals("Has Parent distributed by Weight from SubGroup", true, subGroup.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Weight));
			AssertEquals("Has Parent distributed by Value from SubGroup", false, subGroup.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Value));
			AssertEquals("Has Parent distributed by Volume from SubGroup", false, subGroup.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Volume));

			AssertEquals("Has Parent distributed by Weight from TopGroup", false, topGroup.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Weight));
			AssertEquals("Has Parent distributed by Value from TopGroup", false, subGroup.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Value));
			AssertEquals("Has Parent distributed by Volume from TopGroup", false, subGroup.HasParentChargeDistributedByThisField(ChargeDistributeByList.Codes.Volume));
		}
	}
}
