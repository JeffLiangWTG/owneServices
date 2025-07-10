using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Testing
{
	using System.Linq;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.TradeSingleWindow;
	using NUnit.Framework;

	public class TSWEntryHeaderWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new TSWEntryHeaderWrapper(null, null);
		}

		public void TestTSWEntryHeaderWrapper()
		{
			var testHeader = GetNewCusEntryHeader();
			var wrappedEntry = new TSWEntryHeaderWrapper(testHeader, null);
			AssertNotNull("TSWEntryHeaderWrapper", wrappedEntry);
		}

		public void TestConsignmentItemIsGeneratedFromDeclaration()
		{
			var testHeader = GetNewCusEntryHeader();
			var wrappedEntry = new TSWEntryHeaderWrapper(testHeader, null);
			ICargoReportExport cREHeader = wrappedEntry;
			AssertNotNull(cREHeader.Consignments);
			foreach (var creConsignment in cREHeader.Consignments)
			{
				AssertNotNull(creConsignment.ConsignmentItems);
				foreach (var creConsignmentItem in creConsignment.ConsignmentItems)
				{
					AssertEquals("GoodsDescription", "LOW VALUE GOODS", creConsignmentItem.GoodsDescription);
					AssertEquals("CommodityValue", 100m, creConsignmentItem.Value);
					AssertEquals("CommodityValueCurrency", "NZD", creConsignmentItem.Currency);
					AssertEquals("ItemGrossWeightInKGM", 5m, creConsignmentItem.GrossWeightInKg);
					AssertEquals("NoOfPackages", 15, creConsignmentItem.PackageQty);
					AssertEquals("PackageType", "CT", creConsignmentItem.PackageType);
					AssertEquals("OriginCountry", "NZ", creConsignmentItem.GoodsOriginCountry);
				}
			}
		}

		public void TestConsignmentItemsGeneratedFromMultiInvoiceDeclaration()
		{
			const string expectedConsignmentItem1 = @"<ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>";
			const string expectedConsignmentItem2 = @"<ConsignmentItem>
      <SequenceNumeric>2</SequenceNumeric>";
			const string expectedConsignmentItem3 = @"<ConsignmentItem>
      <SequenceNumeric>3</SequenceNumeric>";
			const string expectedConsignmentItem4 = @"<ConsignmentItem>
      <SequenceNumeric>4</SequenceNumeric>";

			var declaration = GetNewJobDeclaration();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "Inv-1";
			invoice1.JZ_InvoiceAmount = 100m;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "Inv-2";
			invoice2.JZ_InvoiceAmount = 20m;
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "Inv-3";
			invoice3.JZ_InvoiceAmount = 33m;

			var invoice1Line1 = invoice1.InvoiceLines.AddNew();
			invoice1Line1.JI_LinePrice = 100m;
			invoice1Line1.JI_InvoiceQuantity = 10;
			invoice1Line1.JI_InvoiceUQ = "PK";
			AssertEquals("Pre-condition - JI_LineNo", (ZShort)1, invoice1Line1.JI_LineNo);

			var invoice2Line1 = invoice2.InvoiceLines.AddNew();
			invoice2Line1.JI_LinePrice = 20m;
			invoice2Line1.JI_InvoiceQuantity = 5;
			invoice2Line1.JI_InvoiceUQ = "BX";
			AssertEquals("Pre-condition - JI_LineNo - each new invoice sequences from line 1", (ZShort)1, invoice2Line1.JI_LineNo);

			var invoice3Line1 = invoice3.InvoiceLines.AddNew();
			invoice3Line1.JI_LinePrice = 10m;
			invoice3Line1.JI_InvoiceQuantity = 1;
			invoice3Line1.JI_InvoiceUQ = "CT";
			AssertEquals("Pre-condition - Invoice 3 line 1: JI_LineNo - each new invoice sequences from line 1", (ZShort)1, invoice3Line1.JI_LineNo);
			var invoice3Line2 = invoice3.InvoiceLines.AddNew();
			invoice3Line2.JI_LinePrice = 23m;
			invoice3Line2.JI_InvoiceQuantity = 1;
			invoice3Line2.JI_InvoiceUQ = "CT";
			AssertEquals("Pre-condition - Invoice 3 line 2: JI_LineNo", (ZShort)2, invoice3Line2.JI_LineNo);

			var entryHeader = (CusEntryHeader)declaration.CusEntryHeader;
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var additionalMessageInformation = new AdditionalMessageInformation(null, declaration.eDocsForSelection, TSWTransactionTypes.Original, declaration.Factory, "ICR");
			var wrapper = new TSWEntryHeaderWrapper(entryHeader, additionalMessageInformation);
			var icrBuilder = new ICRMessageBuilder(wrapper, TSWTransactionTypes.Original, "00009908C");
			var icrMessageString = icrBuilder.GetXMLMessage();

			AssertEquals("Consignment Item 1", true, icrMessageString.Contains(expectedConsignmentItem1));
			AssertEquals("Consignment Item 2 sequence", true, icrMessageString.Contains(expectedConsignmentItem2));
			AssertEquals("Consignment Item 3", true, icrMessageString.Contains(expectedConsignmentItem3));
			AssertEquals("Consignment Item 4", true, icrMessageString.Contains(expectedConsignmentItem4));
		}

		public void TestConsignmentItemsGeneratedFromMultiInvoiceSeaDeclarationWithContainers()
		{
			var declaration = GetNewJobDeclaration();
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "ABC123";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "XYZ987";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "MNO567";
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "Inv-1";
			invoice1.JZ_InvoiceAmount = 100m;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "Inv-2";
			invoice2.JZ_InvoiceAmount = 20m;
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "Inv-3";
			invoice3.JZ_InvoiceAmount = 33m;

			var invoice1Line1 = invoice1.InvoiceLines.AddNew();
			invoice1Line1.JI_Description = "INVOICE 1 LINE 1";
			invoice1Line1.JI_LinePrice = 100m;
			invoice1Line1.JI_InvoiceQuantity = 3;
			invoice1Line1.JI_InvoiceUQ = "PK";
			var pivot = invoice1Line1.ContainersForInvoiceLinesForBindingOnly.AddNew(container1);
			pivot.IsForInvoiceLine = true;
			AssertEquals("Pre-condition - JI_LineNo", (ZShort)1, invoice1Line1.JI_LineNo);

			var invoice2Line1 = invoice2.InvoiceLines.AddNew();
			invoice2Line1.JI_Description = "INVOICE 2 LINE 1";
			invoice2Line1.JI_LinePrice = 20m;
			invoice2Line1.JI_InvoiceQuantity = 6;
			invoice2Line1.JI_InvoiceUQ = "PK";
			invoice2Line1.ContainersForInvoiceLinesForBindingOnly.AddNew(container2).IsForInvoiceLine = true;
			AssertEquals("Pre-condition - JI_LineNo - each new invoice sequences from line 1", (ZShort)1, invoice2Line1.JI_LineNo);

			var invoice3Line1 = invoice3.InvoiceLines.AddNew();
			invoice3Line1.JI_Description = "INVOICE 3 LINE 1";
			invoice3Line1.JI_LinePrice = 10m;
			invoice3Line1.JI_InvoiceQuantity = 9;
			invoice3Line1.JI_InvoiceUQ = "PK";
			invoice3Line1.ContainersForInvoiceLinesForBindingOnly.AddNew(container3).IsForInvoiceLine = true;
			AssertEquals("Pre-condition - Invoice 3 line 1: JI_LineNo - each new invoice sequences from line 1", (ZShort)1, invoice3Line1.JI_LineNo);

			var invoice3Line2 = invoice3.InvoiceLines.AddNew();
			invoice3Line2.JI_Description = "INVOICE 3 LINE 2";
			invoice3Line2.JI_LinePrice = 23m;
			invoice3Line2.JI_InvoiceQuantity = 8;
			invoice3Line2.JI_InvoiceUQ = "PK";
			invoice3Line2.ContainersForInvoiceLinesForBindingOnly.AddNew(container3).IsForInvoiceLine = true;
			AssertEquals("Pre-condition - Invoice 3 line 2: JI_LineNo", (ZShort)2, invoice3Line2.JI_LineNo);

			var entryHeader = (CusEntryHeader)declaration.CusEntryHeader;
			entryHeader.PopulateCH_BGMReferenceIfNeeded();
			var additionalMessageInformation = new AdditionalMessageInformation(null, declaration.eDocsForSelection, TSWTransactionTypes.Original, declaration.Factory, MessageTypeList.Codes.ICR);
			var wrapper = new TSWEntryHeaderWrapper(entryHeader, additionalMessageInformation);
			var icrBuilder = new ICRMessageBuilder(wrapper, TSWTransactionTypes.Original, "00009908C");
			var icrMessageString = icrBuilder.GetXMLMessage();

			const string expectedConsignmentItem1 = @"<ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>INVOICE 1 LINE 1</CargoDescription>
        <ValueAmount currencyID=""NZD"">100</ValueAmount>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">0</GrossMassMeasure>
      </GoodsMeasure>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>3</QuantityQuantity>
        <TypeCode>PK</TypeCode>
      </Packaging>
      <TransportEquipment>
        <ID>ABC123</ID>
      </TransportEquipment>
    </ConsignmentItem>";
			const string expectedConsignmentItem2 = @"<ConsignmentItem>
      <SequenceNumeric>2</SequenceNumeric>
      <Commodity>
        <CargoDescription>INVOICE 2 LINE 1</CargoDescription>
        <ValueAmount currencyID=""NZD"">20</ValueAmount>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">0</GrossMassMeasure>
      </GoodsMeasure>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>6</QuantityQuantity>
        <TypeCode>PK</TypeCode>
      </Packaging>
      <TransportEquipment>
        <ID>XYZ987</ID>
      </TransportEquipment>
    </ConsignmentItem>";
			const string expectedConsignmentItem3 = @"<ConsignmentItem>
      <SequenceNumeric>3</SequenceNumeric>
      <Commodity>
        <CargoDescription>INVOICE 3 LINE 1</CargoDescription>
        <ValueAmount currencyID=""NZD"">10</ValueAmount>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">0</GrossMassMeasure>
      </GoodsMeasure>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>9</QuantityQuantity>
        <TypeCode>PK</TypeCode>
      </Packaging>
      <TransportEquipment>
        <ID>MNO567</ID>
      </TransportEquipment>
    </ConsignmentItem>";
			const string expectedConsignmentItem4 = @"<ConsignmentItem>
      <SequenceNumeric>4</SequenceNumeric>
      <Commodity>
        <CargoDescription>INVOICE 3 LINE 2</CargoDescription>
        <ValueAmount currencyID=""NZD"">23</ValueAmount>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">0</GrossMassMeasure>
      </GoodsMeasure>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>8</QuantityQuantity>
        <TypeCode>PK</TypeCode>
      </Packaging>
      <TransportEquipment>
        <ID>MNO567</ID>
      </TransportEquipment>
    </ConsignmentItem>";

			AssertContains("Consignment Item 1", expectedConsignmentItem1, icrMessageString);
			AssertContains("Consignment Item 2", expectedConsignmentItem2, icrMessageString);
			AssertContains("Consignment Item 3", expectedConsignmentItem3, icrMessageString);
			AssertContains("Consignment Item 4", expectedConsignmentItem4, icrMessageString);
		}

		public void TestEmptyContainersCreateSeparateConsignmentItems()
		{
			var declaration = GetNewJobDeclaration();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "FLMU0392832";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "FLMU0394328";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;

			var testHeader = (CusEntryHeader)declaration.CusEntryHeader;
			var wrappedEntry = new TSWEntryHeaderWrapper(testHeader, null);
			ICargoReportExport cREHeader = wrappedEntry;
			AssertNotNull(cREHeader.Consignments);
			var containerLine1 = ZString.Empty;
			var containerLine2 = ZString.Empty;
			foreach (var creConsignment in cREHeader.Consignments)
			{
				AssertNotNull(creConsignment.ConsignmentItems);
				int consignmentItemCount = 0;
				foreach (var creConsignmentItem in creConsignment.ConsignmentItems)
				{
					consignmentItemCount++;
					if (consignmentItemCount == 1)
					{
						containerLine1 = creConsignmentItem.ContainerNumber;
					}
					else
					{
						containerLine2 = creConsignmentItem.ContainerNumber;
					}
				}

				AssertEquals("Should be two Consignments item here - 1 for the empty container1, 1 for the empty container 2", 2, consignmentItemCount);
				AssertEquals("Empty container must be reported as a separate consignment item", "FLMU0392832", containerLine1);
				AssertEquals("Empty container must be reported as a separate consignment item", "FLMU0394328", containerLine2);
			}
		}

		public void TestHasEmptyContainersOnly()
		{
			var declaration = GetEmptyContainerDec();
			var testHeader = (CusEntryHeader)declaration.CusEntryHeader;
			var wrappedEntry = new TSWEntryHeaderWrapper(testHeader, null);
			ICargoReportExport cREHeader = wrappedEntry;
			AssertEquals("HasEmptyContainersOnly", true, cREHeader.HasEmptyContainersOnly);

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "FLMU0884914";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;

			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "FLMU0394328";
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;
			AssertEquals("HasEmptyContainersOnly", true, cREHeader.HasEmptyContainersOnly);

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 200m;
			invoiceLine1.JI_InvoiceQuantity = 20;
			invoiceLine1.JI_InvoiceUQ = "BX";
			AssertEquals("HasEmptyContainersOnly", false, cREHeader.HasEmptyContainersOnly);
		}

		public void TestEmptyContainersOnNonContainerisedDec()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			var testHeader = (CusEntryHeader)declaration.CusEntryHeader;
			var wrappedEntry = new TSWEntryHeaderWrapper(testHeader, null);
			ICargoReportExport cREHeader = wrappedEntry;
			AssertEquals("HasContainersAndTheyreAllEmpty - Declaration does not have ANY containers", false, cREHeader.HasEmptyContainersOnly);
		}

		public void TestManifestConsignmentsOrderedCorrectly()
		{
			var manifestEntryHeader = Factory.New<Manifesting.CusEntryHeader>();
			manifestEntryHeader.CH_BGMReference = "M00001136";
			manifestEntryHeader.Declarations.AddNew();
			manifestEntryHeader.Declarations.AddNew();
			manifestEntryHeader.Declarations.AddNew();
			manifestEntryHeader.Declarations.AddNew();

			var additionalMessageInfo = new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory);
			additionalMessageInfo.AM_SendManifest = true;

			manifestEntryHeader.Declarations[0].JE_DeclarationReference = "M00001136-1";
			manifestEntryHeader.Declarations[0].JE_MasterBill = "08187215295";
			manifestEntryHeader.Declarations[0].JE_HouseBill = "190611HBILL1";
			manifestEntryHeader.Declarations[0].JE_MessageSubType = "ECI";

			manifestEntryHeader.Declarations[1].JE_DeclarationReference = "M00001136-4";
			manifestEntryHeader.Declarations[1].JE_MasterBill = "08187215295";
			manifestEntryHeader.Declarations[1].JE_HouseBill = "190611HBILL4";
			manifestEntryHeader.Declarations[1].JE_MessageSubType = "ECI";

			manifestEntryHeader.Declarations[2].JE_DeclarationReference = "M00001136-2";
			manifestEntryHeader.Declarations[2].JE_MasterBill = "08187215295";
			manifestEntryHeader.Declarations[2].JE_HouseBill = "190611HBILL2";
			manifestEntryHeader.Declarations[2].JE_MessageSubType = "ECI";

			manifestEntryHeader.Declarations[3].JE_DeclarationReference = "M00001136-3";
			manifestEntryHeader.Declarations[3].JE_MasterBill = "08187215295";
			manifestEntryHeader.Declarations[3].JE_HouseBill = "190611HBILL3";
			manifestEntryHeader.Declarations[3].JE_MessageSubType = "ECI";

			var wrappedManifestEntry = new TSWEntryHeaderWrapper(manifestEntryHeader, additionalMessageInfo);
			IInwardCargoReport icrHeader = wrappedManifestEntry;
			AssertNotNull(icrHeader.Consignments);
			var consignmentBillNo = ZString.Empty;
			int consignmentsInWrapper = 0;
			int expectedConsignments = 4;
			foreach (var icrConsignment in icrHeader.Consignments)
			{
				Assert("Consignments must be in ascending order (HouseBill number used as reference value for this test)", icrConsignment.BillNumber > consignmentBillNo);
				consignmentBillNo = icrConsignment.BillNumber;
				consignmentsInWrapper++;
			}

			AssertEquals("expectedConsignments in this manifest", expectedConsignments, consignmentsInWrapper);
		}

		public void TestSupportingDocuments()
		{
			var testEntryHeader = Factory.NewWithValidTestData<Manifesting.CusEntryHeader>();
			ICargoReportExport creHeaderWrapper = new TSWEntryHeaderWrapper(testEntryHeader, null);
			AssertEquals(0, creHeaderWrapper.SupportingDocuments.Count());

			IInwardCargoReport icrHeaderWrapper = new TSWEntryHeaderWrapper(testEntryHeader, null);
			AssertEquals(0, icrHeaderWrapper.SupportingDocuments.Count());
		}

		#region Implementation

		CusEntryHeader GetNewCusEntryHeader()
		{
			var declaration = GetNewJobDeclaration();
			return (CusEntryHeader)declaration.CusEntryHeader;
		}

		JobDeclaration GetEmptyContainerDec()
		{
			var declaration = GetNewJobDeclaration();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "FLMU0392832";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;
			return declaration;
		}

		JobDeclaration GetNewJobDeclaration()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			declaration.JE_HouseBill = "LV0001";
			declaration.JE_GoodsDescription = "Low value goods";
			declaration.JE_ECI_InvoiceAmount = 100m;
			declaration.JE_TotalWeight = 5m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_RL_NKOrigin = "NZAKL";
			declaration.JE_TotalNoOfPacks = 15;
			declaration.JE_TotalNoOfPacksPackType = "CT";
			return declaration;
		}

		#endregion
	}
}
