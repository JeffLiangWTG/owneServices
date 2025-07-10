using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class BaseJobComInvoiceLineApportionTest : TestCaseWithFactory
	{
		public void TestMarkApportionmentDirty()
		{
			var testDec = BaseJobDeclaration.New(Factory);

			var invoice = testDec.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();

			testDec.ApportionmentDirty = false;
			AssertEquals("PreCondition:Apportionement is not dirty", false, testDec.ApportionmentDirty);

			line.JI_LinePrice = 1000m;
			AssertEquals("Apportionment is dirty now", true, testDec.ApportionmentDirty);

			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;

			testDec.ApportionmentDirty = false;
			AssertEquals("PreCondition:Apportionement is not dirty", false, testDec.ApportionmentDirty);
			line.JI_Weight = 10m;
			line.JI_WeightUQ = "kg";
			AssertEquals("Apportionment is dirty now as there is a charge distributed by weight", true, testDec.ApportionmentDirty);

			testDec.ApportionmentDirty = false;
			AssertEquals("PreCondition:Apportionement is not dirty", false, testDec.ApportionmentDirty);
			line.JI_Volume = 10m;
			line.JI_VolumeUQ = "m3";
			AssertEquals("Apportionement is not dirty as there are no charges distributed by volume", false, testDec.ApportionmentDirty);

			line.JI_InvoiceQuantity = 12.30m;
			AssertEquals("Apportionement is not dirty as there are no charges distributed by invoice quantity", false, testDec.ApportionmentDirty);
			line.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Apportionement is not dirty as there are no charges distributed by invoice quantity", false, testDec.ApportionmentDirty);

			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Quantity;
			line.JI_InvoiceQuantity = 20m;
			AssertEquals("Apportionement is dirty as there is a charge distributed by invoice quantity", true, testDec.ApportionmentDirty);
			testDec.ApportionmentDirty = false;
			line.JI_InvoiceUQ = Core.Constants.Weight.Pounds;
			AssertEquals("Apportionement is not dirty because Invoice UQ is not relevant", false, testDec.ApportionmentDirty);
		}

		public void TestChangingLinePriceReapportionAllCharges()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
				oTH.J7_IsIncludedInITOT = true;

				var eXW = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 500m, invoice.JobDeclaration.LocalCurrencyCode);
				eXW.J7_IsIncludedInITOT = true;

				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 6000m;

				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 4000m;
				baseJobDeclaration.ResumeApportionment();
				AssertEquals("Apportioned charges for line1", 600m, invoiceLine1.ApportionedCharges.GetCharge(oTH.ChargeKey).Amount);
				AssertEquals("Apportioned charges for line1", 300m, invoiceLine1.ApportionedCharges.GetCharge(eXW.ChargeKey).Amount);
				AssertEquals("Apportioned charges for line2", 400m, invoiceLine2.ApportionedCharges.GetCharge(oTH.ChargeKey).Amount);
				AssertEquals("Apportioned charges for line2", 200m, invoiceLine2.ApportionedCharges.GetCharge(eXW.ChargeKey).Amount);
			}
		}

		public void TestChangingInvoiceReapportionAllCharges()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				invoice.JZ_InvoiceNumber = "INV1";

				var oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
				oTH.J7_IsIncludedInITOT = true;
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 6000m;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 4000m;
				baseJobDeclaration.ResumeApportionment();
				AssertEquals("Apportioned charge for InvoiceLine1", 1, invoiceLine1.ApportionedCharges.Count);
				AssertEquals("Apportioned charge for InvoiceLine2", 1, invoiceLine2.ApportionedCharges.Count);

				var anotherInvoice = baseJobDeclaration.Invoices.AddNew();
				anotherInvoice.JZ_InvoiceNumber = "INV2";
				anotherInvoice.JZ_InvoiceAmount = 10000m;
				anotherInvoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;

				var eXW = anotherInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
				eXW.J7_IsIncludedInITOT = true;

				invoiceLine2.JI_Calc_Invoice = anotherInvoice.JZ_InvoiceNumber;
				baseJobDeclaration.ResumeApportionment();
				AssertEquals("Apportioned charge", 1, invoiceLine2.ApportionedCharges.Count);
				AssertEquals("Apportioned charge for InvoiceLine2 is EXW now", CustomsChargeTypeList.Codes.ExWorks, invoiceLine2.ApportionedCharges[0].J7_ChargeType);
				AssertEquals("Apportioned charge for invoice line 1 amount", 1000m, invoiceLine1.ApportionedCharges[0].J7_Amount);
			}
		}

		public void TestChangingInvoiceReapportionAllChargesFromGroupIfNecessary()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var testDec = Factory.New<BaseJobDeclaration>();
				var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
				var topOTH = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 2000m, testDec.LocalCurrencyCode);

				var sub1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
				var sub1OFT = sub1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 700m, testDec.LocalCurrencyCode);

				var sub1Invoice = sub1.JobComInvoiceHeaders.AddNew();
				sub1Invoice.JZ_InvoiceAmount = 10000m;
				sub1Invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				sub1Invoice.JZ_IncoTerm = "FOB";
				sub1Invoice.JZ_InvoiceNumber = "INV1";
				var line1 = sub1Invoice.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 10000;

				var sub2 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
				var sub2Invoice = sub2.JobComInvoiceHeaders.AddNew();
				sub2Invoice.JZ_InvoiceAmount = 10000m;
				sub2Invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				sub2Invoice.JZ_IncoTerm = "FOB";
				sub2Invoice.JZ_InvoiceNumber = "INV2";
				var line2 = sub2Invoice.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 10000m;
				var lineOth = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1500m, testDec.LocalCurrencyCode);
				lineOth.J7_IsNotIncludedInInvoice = true;
				testDec.ResumeApportionment();
				AssertEquals("PreCondition:Apportioned OTH for Sub1Invoice", 500m, sub1Invoice.GroupCharges.GetCharge(topOTH.ChargeKey).Amount);
				AssertEquals("PreCondition:Apportioned OFT for Sub1Invoice", 700m, sub1Invoice.GroupCharges.GetCharge(sub1OFT.ChargeKey).Amount);
				AssertEquals("PreCondition:Apportioned OTH for Sub2Invoice", 1500m, sub2Invoice.GroupCharges.GetCharge(topOTH.ChargeKey).Amount);
				AssertEquals("PreCondition:Apportioned OFT for Sub2Invoice", 0m, sub2Invoice.GroupCharges.GetCharge(sub1OFT.ChargeKey).Amount);
				AssertEquals("PreCondition:Line 1 apportioned", 500m, line1.ApportionedCharges.GetCharge(topOTH.ChargeKey).Amount);
				AssertEquals("PreCondition:Apportioned OFT for Line1", 700m, line1.ApportionedCharges.GetCharge(sub1OFT.ChargeKey).Amount);
				AssertEquals("PreCondition:Line 2 own charge", 1500m, line2.Charges.GetCharge(topOTH.ChargeKey).Amount);
				AssertEquals("PreCondition:Apportioned OFT for Line2", 0m, line2.ApportionedCharges.GetCharge(sub1OFT.ChargeKey).Amount);

				line1.JI_LinePrice = 5000;
				line2.JI_LinePrice = 5000;
				line2.JI_Calc_Invoice = sub1Invoice.JZ_InvoiceNumber;
				testDec.ResumeApportionment();
				AssertEquals("Apportioned OTH for Sub1Invoice", 1666.67m, sub1Invoice.GroupCharges.GetCharge(topOTH.ChargeKey).Amount);
				AssertEquals("Apportioned OFT for Sub1Invoice", 700m, sub1Invoice.GroupCharges.GetCharge(sub1OFT.ChargeKey).Amount);
				AssertEquals("Apportioned OTH for Sub2Invoice", 333.33m, sub2Invoice.GroupCharges.GetCharge(topOTH.ChargeKey).Amount);
				AssertEquals("Apportioned OFT for Sub2Invoice", 0m, sub2Invoice.GroupCharges.GetCharge(sub1OFT.ChargeKey).Amount);
				AssertEquals("Line 1 apportioned", 166.67m, line1.ApportionedCharges.GetCharge(topOTH.ChargeKey).Amount);
				AssertEquals("Apportioned OFT for Line1", 350m, line1.ApportionedCharges.GetCharge(sub1OFT.ChargeKey).Amount);
				AssertEquals("Line 2 OTH", 1500m, line2.Charges.GetCharge(topOTH.ChargeKey).Amount);
				AssertEquals("Apportioned OFT for Line2", 350m, line2.ApportionedCharges.GetCharge(sub1OFT.ChargeKey).Amount);
			}
		}

		public void TestSettingEmptyCustomsUQClearsCustomsQuantity()
		{
			var line = invoice.JobComInvoiceLines.AddNew();

			line.JI_CustomsQuantity = 100m;
			line.JI_CustomsUnitQty = "KG";

			line.JI_CustomsUnitQty = ZString.Empty;
			AssertEquals("Unit is not cleared -> Qty is not cleared", 100m, line.JI_CustomsQuantity);
		}

		public void TestConcurrencyPolicySet()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals("Default strategy - not set yet", ConcurrencyPolicy.Default, line.JI_CLInfo.ConcurrencyPolicy);
			Factory.Save();
			line.OnSaving();
			AssertEquals("No merge", ConcurrencyPolicy.Strict, line.JI_CLInfo.ConcurrencyPolicy);
		}

		public void TestNonPersistentContainer()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var container = dec.CusContainers.AddNew();
			container.CO_ContainerNumber = "OLCU0000000";
			container.CO_Seal = "CUCKOO SQUEAKER";
			var nonPersistentContainer = line.ContainersForInvoiceLinesForBindingOnly[0];
			nonPersistentContainer.IsForInvoiceLine = true;
			AssertEquals("OLCU0000000", nonPersistentContainer.ContainerNumber);
			AssertEquals("CUCKOO SQUEAKER", nonPersistentContainer.Seal);
			Assert("Should be selected", nonPersistentContainer.IsForInvoiceLine);
		}

		public void TestContainersForInvoiceLines()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var container1 = dec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OLCU0000001";
			var container2 = dec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OLCU0000002";
			var container3 = dec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OLCU0000003";
			AssertEquals("Should have all three containers here", 3, line.ContainersForInvoiceLinesForBindingOnly.Count);
		}

		BaseJobDeclaration baseJobDeclaration;
		BaseJobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			baseJobDeclaration = Factory.New<BaseJobDeclaration>();
			invoice = baseJobDeclaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = baseJobDeclaration.LocalCurrencyCode;
		}
	}
}
