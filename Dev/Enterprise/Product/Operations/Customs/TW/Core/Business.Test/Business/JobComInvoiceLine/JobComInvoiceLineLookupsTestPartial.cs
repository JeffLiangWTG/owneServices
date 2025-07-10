using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineLookups))]
	partial class JobComInvoiceLineLookupsTest
	{
		public void TestDutyOrTaxPaymentMethodList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.JI_Procedure = Constants.ProcedureCodes._38;
			AssertEquals("CAS, DEF, ROR", invLine.Lookups.DutyOrTaxPaymentMethodList.CodesAsString);

			invLine.JI_Procedure = Constants.ProcedureCodes._37;
			AssertEquals("CAS, DEF", invLine.Lookups.DutyOrTaxPaymentMethodList.CodesAsString);
		}

		public void TestTextileWidthUQList()
		{
			var lookupsList = InvoiceLine.Lookups.TextileWidthUQList;
			AssertSame(Factory.GetCachedValue<TextileWidthUQList>(), lookupsList);
			AssertEquals("FT, IN, CM, MM, M, YD", lookupsList.CodesAsString);
		}

		public void TestCarConditionCodeList()
		{
			var lookupsList = InvoiceLine.Lookups.CarConditionCodeList;
			AssertSame(Factory.GetCachedValue<CarConditionCodeList>(), lookupsList);
			AssertEquals(3, lookupsList.Count);
		}

		public void TestCatalystConverterPrintModeList()
		{
			var lookupsList = InvoiceLine.Lookups.CatalystConverterPrintModeList;
			AssertSame(Factory.GetCachedValue<CatalystConverterPrintModeList>(), lookupsList);
			AssertEquals(2, lookupsList.Count);
		}

		public void TestEngineTypeCodeList()
		{
			var lookupsList = InvoiceLine.Lookups.EngineTypeCodeList;
			AssertSame(Factory.GetCachedValue<EngineTypeCodeList>(), lookupsList);
			AssertEquals(11, lookupsList.Count);
		}

		public void TestEquipmentPrintModeList()
		{
			var lookupsList = InvoiceLine.Lookups.EquipmentPrintModeList;
			AssertSame(Factory.GetCachedValue<EquipmentPrintModeList>(), lookupsList);
			AssertEquals(3, lookupsList.Count);
		}

		public void TestLeftSideSteeringCodeList()
		{
			var lookupsList = InvoiceLine.Lookups.LeftSideSteeringCodeList;
			AssertSame(Factory.GetCachedValue<LeftSideSteeringCodeList>(), lookupsList);
			AssertEquals(2, lookupsList.Count);
		}

		public void TestTransmissionCodeList()
		{
			var lookupsList = InvoiceLine.Lookups.TransmissionCodeList;
			AssertSame(Factory.GetCachedValue<TransmissionCodeList>(), lookupsList);
			AssertEquals(4, lookupsList.Count);
		}

		public void TestGoodsTypeList()
		{
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "DN" });
			var header = jobDeclartion.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			var lookupsList = line.Lookups.GoodsTypeList;
			AssertContainsExactElementsInAnyOrder(new CPT_107_301_GoodsTypeList(), lookupsList);
			AssertEquals(21, lookupsList.Count);
			jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "IF" });
			header = jobDeclartion.Invoices.AddNew();
			line = header.JobComInvoiceLines.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			lookupsList = line.Lookups.GoodsTypeList;
			AssertContainsExactElementsInAnyOrder(new CPT_107_601_GoodsTypeList(), lookupsList);
			AssertEquals(3, lookupsList.Count);
			jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "DH" });
			header = jobDeclartion.Invoices.AddNew();
			line = header.JobComInvoiceLines.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			lookupsList = line.Lookups.GoodsTypeList;
			AssertContainsExactElementsInAnyOrder(new CPT_107_601_GoodsTypeList(), lookupsList);
			AssertEquals(3, lookupsList.Count);
			jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "DN", "IF", "DH" });
			header = jobDeclartion.Invoices.AddNew();
			line = header.JobComInvoiceLines.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().ForEach(x => x.IsLinkedCMHeader = true);
			lookupsList = line.Lookups.GoodsTypeList;
			AssertEquals(24, lookupsList.Count);
		}

		public void TestBondedGoodsCodeList()
		{
			var lookupsList = InvoiceLine.Lookups.BondedGoodsCodeList;
			AssertSame(Factory.GetCachedValue<BondedGoodsCodeList>(), lookupsList);
			AssertEquals(3, lookupsList.Count);
		}

		public void TestCarTypeCodeList()
		{
			var lookupsList = InvoiceLine.Lookups.CarTypeCodeList;
			AssertSame(Factory.GetCachedValue<CarTypeCodeList>(), lookupsList);
			AssertEquals(16, lookupsList.Count);
		}

		public void TestNewOwnerProducts()
		{
			var supplierOrgHeader = Factory.New<OrgHeader>();
			supplierOrgHeader.OH_Code = "S01";
			supplierOrgHeader.OH_FullName = "Supplier 001";
			var ownerOrgHeader = Factory.New<OrgHeader>();
			ownerOrgHeader.OH_Code = "O01";
			ownerOrgHeader.OH_FullName = "Owner 001";
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "Part No 1";
			part1.OP_Desc = "PART 1";
			part1.RelatedOrganisations.AddOwner(ownerOrgHeader);
			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "Part No 2";
			part2.OP_Desc = "PART 2";
			part2.RelatedOrganisations.AddSupplier(supplierOrgHeader);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplierOrgHeader.PK;
			var instruction = declaration.CusEntryInstruction;
			instruction.CEI_OH_Owner = ownerOrgHeader.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var declaration2 = Factory.New<JobDeclaration>();
			var instruction2 = declaration2.CusEntryInstruction;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_NewOwnerPartNo = part1.OP_PartNum;
			Factory.Save();
			var newOwnerProducts = invoiceLine.Lookups.NewOwnerProducts;
			AssertEquals("Importer/Supplier:Property1", ownerOrgHeader.PK, newOwnerProducts.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
			var filter = newOwnerProducts.CompleteFilter;
			Assert("Part2", !part2.MatchesFilter(filter));
			Assert("OwnerPart", part1.MatchesFilter(filter));
			newOwnerProducts = invoiceLine2.Lookups.NewOwnerProducts;
			AssertEquals("Product Code:Property", part1.OP_PartNum, newOwnerProducts.FilterBusinessObjectDefaults["Product Code:Property"].Value);
			filter = newOwnerProducts.CompleteFilter;
			Assert("OwnerPart", part1.MatchesFilter(filter));
		}

		public void TestInnerPackageTypeList()
		{
			var lookupsList = InvoiceLine.Lookups.InnerPackageTypeList;
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<CPT_115_InnerPackageTypeList>(), lookupsList);
			AssertEquals(7, lookupsList.Count);
		}

		public void TestInnerPackingMaterialList()
		{
			var lookupsList = InvoiceLine.Lookups.InnerPackingMaterialList;
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<CPT_114_InnerPackingMaterialList>(), lookupsList);
			AssertEquals(26, lookupsList.Count);
		}

		public void TestContainerCapacityList()
		{
			var lookupsList = InvoiceLine.Lookups.ContainerCapacityList;
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<ContainerCapacityList>(), lookupsList);
			AssertEquals(10, lookupsList.Count);
		}

		public void TestContainerMaterialList()
		{
			var lookupsList = InvoiceLine.Lookups.ContainerMaterialList;
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<ContainerMaterialList>(), lookupsList);
			AssertEquals(13, lookupsList.Count);
		}

		public void TestContainerMaterialNumberList()
		{
			var lookupsList = InvoiceLine.Lookups.ContainerMaterialNumberList;
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<ContainerMaterialNumberList>(), lookupsList);
			AssertEquals(6, lookupsList.Count);
		}

		public void TestRAPRORCurrencyList()
		{
			var lookupsList = InvoiceLine.Lookups.RAPRORCurrencyList;
			AssertEquals(1, lookupsList.Count);
			AssertEquals("TWD", lookupsList.CodesAsString);
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var lookups = invoiceLine.Lookups;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "TWD";
			AssertEquals(1, lookups.RAPRORCurrencyList.Count);
			AssertEquals("TWD", lookups.RAPRORCurrencyList.CodesAsString);
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			AssertEquals(2, lookups.RAPRORCurrencyList.Count);
			AssertEquals("TWD, USD", lookups.RAPRORCurrencyList.CodesAsString);
		}

		public void TestTariffPrintLengthList()
		{
			var lookupsList = InvoiceLine.Lookups.TariffPrintLengthList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", Factory.GetCachedValue<TariffPrintLengthList>(), lookupsList);
				AssertEquals("N, 1, 2, 3", lookupsList.CodesAsString);
			});
		}

		public void TestCPT_124_OriginCriteriaCodeList_EN()
		{
			var lookupsList = InvoiceLine.Lookups.CPT_124_OriginCriteriaCodeList_EN;
			CombineAssertions(() =>
			{
				AssertSame("Cached", Factory.GetCachedValue<CPT_124_OriginCriteriaCodeList_EN>(), lookupsList);
				AssertEquals("01, 02, 03, 04, 05, 06", lookupsList.CodesAsString);
			});
		}

		public void TestManufacturerRelationshipCodes()
		{
			var expectedList09_11 = Factory.GetCachedValue<CPT_126_09_11_ManufacturerRelationship>();
			var expectedList13_14 = Factory.GetCachedValue<CPT_126_13_14_18_19_ManufacturerRelationship>();

			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

			controllingMessageHeader.TW1_ControllingMessageType = "NX101";
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			AssertContainsExactElementsInAnyOrder(expectedList09_11, line.Lookups.ManufacturerRelationshipCodes);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			AssertContainsExactElementsInAnyOrder(expectedList09_11, line.Lookups.ManufacturerRelationshipCodes);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			AssertContainsExactElementsInAnyOrder(expectedList13_14, line.Lookups.ManufacturerRelationshipCodes);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			AssertContainsExactElementsInAnyOrder(expectedList13_14, line.Lookups.ManufacturerRelationshipCodes);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			AssertContainsExactElementsInAnyOrder(expectedList13_14, line.Lookups.ManufacturerRelationshipCodes);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			AssertContainsExactElementsInAnyOrder(expectedList13_14, line.Lookups.ManufacturerRelationshipCodes);

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
			AssertEquals(0, line.Lookups.ManufacturerRelationshipCodes.Count);
		}

		public void TestPTCriteriaCodes()
		{
			var expectedList09 = Factory.GetCachedValue<CPT_125_09_PTCriteria>();
			var expectedList11 = Factory.GetCachedValue<CPT_125_11_PTCriteria>();
			var expectedList13 = Factory.GetCachedValue<CPT_125_13_PTCriteria>();
			var expectedList14 = Factory.GetCachedValue<CPT_125_14_PTCriteria>();
			var expectedList15 = Factory.GetCachedValue<CPT_125_15_PTCriteria>();
			var expectedList19 = Factory.GetCachedValue<CPT_125_19_PTCriteria>();

			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var line = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

			controllingMessageHeader.TW1_ControllingMessageType = "NX101";
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			AssertContainsExactElementsInAnyOrder(expectedList09, line.Lookups.PTCriteriaCodes);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			AssertContainsExactElementsInAnyOrder(expectedList11, line.Lookups.PTCriteriaCodes);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			AssertContainsExactElementsInAnyOrder(expectedList13, line.Lookups.PTCriteriaCodes);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			AssertContainsExactElementsInAnyOrder(expectedList14, line.Lookups.PTCriteriaCodes);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			AssertContainsExactElementsInAnyOrder(expectedList15, line.Lookups.PTCriteriaCodes);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			AssertContainsExactElementsInAnyOrder(expectedList19, line.Lookups.PTCriteriaCodes);

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
			AssertEquals(0, line.Lookups.PTCriteriaCodes.Count);
		}

		public void TestPTCriteria2List()
		{
			var expectedList09_11_13 = Factory.GetCachedValue<CPT_125_09_11_13OtherPTCriteria>();
			var expectedList14 = Factory.GetCachedValue<CPT_125_14OtherPTCriteria>();
			var expectedList15 = Factory.GetCachedValue<CPT_125_15OtherPTCriteria>();
			var expectedList18 = Factory.GetCachedValue<CPT_125_18OtherPTCriteria>();
			var expectedList19 = Factory.GetCachedValue<CPT_125_19OtherPTCriteria>();

			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = decl.CusEntryInstruction;
			var line = (JobComInvoiceLine)decl.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			AssertContainsExactElementsInAnyOrder(expectedList09_11_13, line.Lookups.PTCriteria2List);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			AssertContainsExactElementsInAnyOrder(expectedList09_11_13, line.Lookups.PTCriteria2List);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			AssertContainsExactElementsInAnyOrder(expectedList09_11_13, line.Lookups.PTCriteria2List);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			AssertContainsExactElementsInAnyOrder(expectedList14, line.Lookups.PTCriteria2List);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			AssertContainsExactElementsInAnyOrder(expectedList15, line.Lookups.PTCriteria2List);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			AssertContainsExactElementsInAnyOrder(expectedList18, line.Lookups.PTCriteria2List);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			AssertContainsExactElementsInAnyOrder(expectedList19, line.Lookups.PTCriteria2List);

			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = false;
			AssertEquals(0, line.Lookups.PTCriteria2List.Count);
		}
	}
}
