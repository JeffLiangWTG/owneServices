using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FTZ214EntryLine))]
	sealed class FTZ214EntryLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFTZWarehousePackageQtyAndUnit()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "OO11";
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "O212";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test@#";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";
			product.OP_StockKeepingUnit = "UNT";
			product.OP_NetWeight = 0.1m;
			product.OP_WeightUQ = "T";
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "Tes2@#";
			var relation2 = product.RelatedOrganisations.AddNew();
			relation2.OU_OH = importer2.PK;
			relation2.OU_Relationship = "OWN";
			product2.OP_StockKeepingUnit = "BOX";
			product2.OP_NetWeight = 0.1m;
			product2.OP_WeightUQ = "T";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 20.51m;
			invoiceLine.JI_OP = product.PK;
			invoiceLine.JI_BondedWhsQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = "PCS";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_BondedWhsQuantity = 7m;
			invoiceLine2.JI_LinePrice = 33m;
			invoiceLine2.JI_OP = product2.PK;
			invoiceLine2.JI_InvoiceUQ = "CS";
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 50.61m;
			invoiceLine3.JI_OP = product2.PK;
			invoiceLine3.JI_InvoiceUQ = "NO";
			AssertEquals("UQ loaded from Part", "BOX", invoiceLine3.PartStockTakeUnit);
			invoiceLine3.JI_BondedWhsQuantity = 9m;
			invoiceLine3.JI_BondedWhsUnitQty = "CS";
			AssertEquals("Manually changed", "CS", invoiceLine3.JI_BondedWhsUnitQty);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoiceLine", "5 PCS", invoiceLine.CusEntryLine.FTZWarehousePackageQtyAndUnit);
			AssertEquals("invoiceLine2", "7 CS", invoiceLine2.CusEntryLine.FTZWarehousePackageQtyAndUnit);
			AssertEquals("invoiceLine3", "9 NO", invoiceLine3.CusEntryLine.FTZWarehousePackageQtyAndUnit);
		}

		public void TestFTZWarehousePackageQtyAndUnitWhenMerged()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "OO11";
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "O212";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test@#";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";
			product.OP_StockKeepingUnit = "UNT";
			product.OP_NetWeight = 0.1m;
			product.OP_WeightUQ = "T";
			var classification = Factory.NewWithValidTestData<CusClassification>();
			var aPivot = Factory.New<CusClassPartPivot>();
			aPivot.CI_CC = classification.PK;
			aPivot.CI_OP = product.PK;
			aPivot.CI_PartPivotUOM = "NO";
			classification.CC_TariffNum = "4415103003";
			classification.CC_LookupCode = "GRANNY";
			classification.CC_ClassificationType = "BTH";
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "Tes2@#";
			var relation2 = product.RelatedOrganisations.AddNew();
			relation2.OU_OH = importer2.PK;
			relation2.OU_Relationship = "OWN";
			product2.OP_StockKeepingUnit = "BOX";
			product2.OP_NetWeight = 0.1m;
			product2.OP_WeightUQ = "T";
			var classification2 = Factory.NewWithValidTestData<CusClassification>();
			var aPivot2 = Factory.New<CusClassPartPivot>();
			aPivot2.CI_CC = classification.PK;
			aPivot2.CI_OP = product.PK;
			aPivot2.CI_PartPivotUOM = "KGM";
			classification2.CC_TariffNum = "2215102002";
			classification2.CC_LookupCode = "NANNY";
			classification2.CC_ClassificationType = "BTH";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 20.51m;
			invoiceLine.JI_OP = product.PK;
			invoiceLine.JI_BondedWhsQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = "PCS";
			invoiceLine.US_ZoneStatus = "N";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_BondedWhsQuantity = 7m;
			invoiceLine2.JI_LinePrice = 33m;
			invoiceLine2.JI_OP = product.PK;
			invoiceLine2.JI_InvoiceUQ = "PCS";
			invoiceLine2.US_ZoneStatus = "N";
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 50.61m;
			invoiceLine3.JI_OP = product2.PK;
			invoiceLine3.JI_InvoiceUQ = "NO";
			AssertEquals("UQ loaded from Part", "BOX", invoiceLine3.PartStockTakeUnit);
			invoiceLine3.JI_BondedWhsQuantity = 9m;
			invoiceLine3.JI_BondedWhsUnitQty = "CS";
			AssertEquals("Manually changed", "CS", invoiceLine3.JI_BondedWhsUnitQty);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Lines merged by Tariff", 2, declaration.ActiveEntryHeaders[0].MergedLines.Count);
			var ftzLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals("FTZ entry line number 1", (ZShort)1, ftzLine1.CL_LineNumber);
			AssertEquals("FTZ entry Line WarehousePkg.Qty", "12 PCS", ftzLine1.FTZWarehousePackageQtyAndUnit);
			var ftzLine2 = declaration.ActiveEntryHeaders[0].MergedLines[1];
			AssertEquals("FTZ entry Line2 WarehousePkg.Qty", "9 NO", ftzLine2.FTZWarehousePackageQtyAndUnit);
		}

		public void TestFTZPackQuantityAndType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "0012365489";
			bill.US_UI_NKBillIssuerSCAC = "ABC";
			bill.CU_NoOfPacks = 15;
			bill.CU_PackType = ABIUnitOfMeasureList.Codes.Packs;
			var ftzEntryLine = new FTZ214EntryLineForTest(Factory, bill);
			AssertEquals("15 PK", ftzEntryLine.FTZPackQuantityAndType);
			AssertEquals("EntryLine is Null : FTZWarehousePackageQtyAndUnit", ZString.Empty, ftzEntryLine.FTZWarehousePackageQtyAndUnit);
			AssertEquals("EntryLine is Null : FTZCountryOfOrigin", ZString.Empty, ftzEntryLine.FTZCountryOfOrigin);
			AssertEquals("EntryLine is Null : FTZForeignPortOfLadingCode", ZString.Empty, ftzEntryLine.FTZForeignPortOfLadingCode);
			AssertEquals("EntryLine is Null : FTZForeignPortOfLadingName", ZString.Empty, ftzEntryLine.FTZForeignPortOfLadingName);
			AssertEquals("EntryLine is Null : FTZDescription", ZString.Empty, ftzEntryLine.FTZDescription);
			AssertEquals("EntryLine is Null : FTZTariff", ZString.Empty, ftzEntryLine.FTZTariff);
			AssertEquals("EntryLine is Null : FTZQuantityAndUnit", ZString.Empty, ftzEntryLine.FTZQuantityAndUnit);
			AssertEquals("EntryLine is Null : FTZSecondQuantityAndUnit", ZString.Empty, ftzEntryLine.FTZSecondQuantityAndUnit);
			AssertEquals("EntryLine is Null : FTZWeightAndUnit", ZString.Empty, ftzEntryLine.FTZWeightAndUnit);
			AssertEquals("EntryLine is Null : FTZCharges", ZDecimal.Zero, ftzEntryLine.Charges);
			AssertEquals("EntryLine is Null : FTZCustomsValue", ZDecimal.Zero, ftzEntryLine.FTZCustomsValue);
		}

		public void TestFTZBillAndITNumberWithSelectedContainer()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "OO11";
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "O212";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test@#";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";
			product.OP_StockKeepingUnit = "UNT";
			product.OP_NetWeight = 0.1m;
			product.OP_WeightUQ = "T";
			var classification = Factory.NewWithValidTestData<CusClassification>();
			var aPivot = Factory.New<CusClassPartPivot>();
			aPivot.CI_CC = classification.PK;
			aPivot.CI_OP = product.PK;
			aPivot.CI_PartPivotUOM = "NO";
			classification.CC_TariffNum = "4415103003";
			classification.CC_LookupCode = "GRANNY";
			classification.CC_ClassificationType = "BTH";
			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "Tes2@#";
			var relation2 = product.RelatedOrganisations.AddNew();
			relation2.OU_OH = importer2.PK;
			relation2.OU_Relationship = "OWN";
			product2.OP_StockKeepingUnit = "BOX";
			product2.OP_NetWeight = 0.1m;
			product2.OP_WeightUQ = "T";
			var classification2 = Factory.NewWithValidTestData<CusClassification>();
			var aPivot2 = Factory.New<CusClassPartPivot>();
			aPivot2.CI_CC = classification.PK;
			aPivot2.CI_OP = product.PK;
			aPivot2.CI_PartPivotUOM = "KGM";
			classification2.CC_TariffNum = "2215102002";
			classification2.CC_LookupCode = "NANNY";
			classification2.CC_ClassificationType = "BTH";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 20.51m;
			invoiceLine.JI_OP = product.PK;
			invoiceLine.JI_BondedWhsQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = "PCS";
			invoiceLine.US_ZoneStatus = "N";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_BondedWhsQuantity = 7m;
			invoiceLine2.JI_LinePrice = 33m;
			invoiceLine2.JI_OP = product.PK;
			invoiceLine2.JI_InvoiceUQ = "PCS";
			invoiceLine2.US_ZoneStatus = "N";
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 50.61m;
			invoiceLine3.JI_OP = product2.PK;
			invoiceLine3.JI_InvoiceUQ = "NO";
			AssertEquals("UQ loaded from Part", "BOX", invoiceLine3.PartStockTakeUnit);
			invoiceLine3.JI_BondedWhsQuantity = 9m;
			invoiceLine3.JI_BondedWhsUnitQty = "CS";
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OON1111111";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OON2222222";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OON3333333";
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "0012365489";
			bill.US_UI_NKBillIssuerSCAC = "ABC";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "HOUSEBILL1";
			houseBill.US_UI_NKBillIssuerSCAC = "ABC";
			houseBill.CU_CU_ParentBill = bill.PK;
			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "HOUSEBILL2";
			houseBill2.US_UI_NKBillIssuerSCAC = "DEF";
			houseBill2.CU_CU_ParentBill = bill.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var nullEntryLinePrintAllNumsFromBill = new FTZ214EntryLineForTest(Factory, bill);
			AssertEquals("MB: ABC0012365489\r\nHB: ABCHOUSEBILL1 HB: DEFHOUSEBILL2\r\nCNR: OON1111111, OON2222222", nullEntryLinePrintAllNumsFromBill.FTZBillAndITNumber);
			var ftzLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			var ftzEntryLine = new FTZ214EntryLineForTest(Factory, ftzLine1, bill);
			AssertEquals("MB: ABC0012365489\r\nHB: ABCHOUSEBILL1 HB: DEFHOUSEBILL2", ftzEntryLine.FTZBillAndITNumber);
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var pivot = invoiceLine.ContainersPivot[0];
			AssertEquals("ContainerNumber", "OON1111111", pivot.ContainerNumber);
			AssertEquals("MB: ABC0012365489\r\nHB: ABCHOUSEBILL1 HB: DEFHOUSEBILL2\r\nCNR: OON1111111", ftzEntryLine.FTZBillAndITNumber);
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[2].IsForInvoiceLine = true;
			AssertEquals("MB: ABC0012365489\r\nHB: ABCHOUSEBILL1 HB: DEFHOUSEBILL2\r\nCNR: OON1111111, OON3333333", ftzEntryLine.FTZBillAndITNumber);
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = false;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[2].IsForInvoiceLine = false;
			AssertEquals("MB: ABC0012365489\r\nHB: ABCHOUSEBILL1 HB: DEFHOUSEBILL2", ftzEntryLine.FTZBillAndITNumber);
		}

		public void TestFTZBillAndITNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OON1111111";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OON2222222";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OON3333333";
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "0012365489";
			bill.US_UI_NKBillIssuerSCAC = "ABC";
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "HOUSEBILL1";
			houseBill.US_UI_NKBillIssuerSCAC = "ABC";
			houseBill.CU_CU_ParentBill = bill.PK;
			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "HOUSEBILL2";
			houseBill2.US_UI_NKBillIssuerSCAC = "DEF";
			houseBill2.CU_CU_ParentBill = bill.PK;
			var ftzEntryLine = new FTZ214EntryLineForTest(Factory, bill);
			AssertEquals("MB: ABC0012365489\r\nHB: ABCHOUSEBILL1 HB: DEFHOUSEBILL2", ftzEntryLine.FTZBillAndITNumber);
			var itDetails = houseBill.ITAndSplitDetails.AddNew();
			itDetails.US_ITNumber = "1239402";
			AssertEquals("IT: 1239402 MB: ABC0012365489\r\nHB: ABCHOUSEBILL1 HB: DEFHOUSEBILL2", ftzEntryLine.FTZBillAndITNumber);
			var itDetails2 = houseBill2.ITAndSplitDetails.AddNew();
			itDetails2.US_ITNumber = "1564898";
			AssertEquals("IT: 1239402, 1564898 MB: ABC0012365489\r\nHB: ABCHOUSEBILL1 HB: DEFHOUSEBILL2", ftzEntryLine.FTZBillAndITNumber);
			var itDetails3 = houseBill2.ITAndSplitDetails.AddNew();
			itDetails3.US_ITNumber = "2840192";
			houseBill.Containers.Add(container1);
			houseBill2.Containers.Add(container2);
			AssertEquals("IT: MULTI MB: ABC0012365489\r\nHB: ABCHOUSEBILL1 HB: DEFHOUSEBILL2\r\nCNR: OON1111111, OON2222222", ftzEntryLine.FTZBillAndITNumber);
			var ftzEntryLine2 = new FTZ214EntryLineForTest(Factory, houseBill);
			AssertEquals("IT: 1239402 MB: ABC0012365489\r\nHB: ABCHOUSEBILL1\r\nCNR: OON1111111", ftzEntryLine2.FTZBillAndITNumber);
			var itDetails21 = houseBill.ITAndSplitDetails.AddNew();
			itDetails21.US_ITNumber = "7777777";
			AssertEquals("IT: 1239402, 7777777 MB: ABC0012365489\r\nHB: ABCHOUSEBILL1\r\nCNR: OON1111111", ftzEntryLine2.FTZBillAndITNumber);
			var itDetails22 = houseBill.ITAndSplitDetails.AddNew();
			itDetails22.US_ITNumber = "1564898";
			AssertEquals("IT: MULTI MB: ABC0012365489\r\nHB: ABCHOUSEBILL1\r\nCNR: OON1111111", ftzEntryLine2.FTZBillAndITNumber);
			var itDetails23 = houseBill.ITAndSplitDetails.AddNew();
			itDetails23.US_ITNumber = "2840192";
			houseBill.Containers.Add(container2);
			houseBill.Containers.Add(container1);
			AssertContains("OON1111111", ftzEntryLine2.FTZBillAndITNumber);
			AssertContains("OON2222222", ftzEntryLine2.FTZBillAndITNumber);
		}

		protected override BusinessObject GetNewBusinessObject() => new FTZ214EntryLine(Factory);

		sealed class FTZ214EntryLineForTest : FTZ214EntryLine
		{
			public FTZ214EntryLineForTest(BusinessObjectFactory factory, Bill bill) : base(factory, null, bill, true)
			{
			}

			public FTZ214EntryLineForTest(BusinessObjectFactory factory, CusEntryLine entryLine, Bill bill) : base(factory, entryLine, bill, true)
			{
			}
		}
	}
}
