using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class IPIMessageBuilderTest : TSWMessageBuilderTest
	{
		public void TestIPIEntryDoesNotSendDeclarantElement()
		{
			var elementNotExpected = @"<Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>johnathon.tester@testcompany.com.nz</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>0419687522</ID>
      <TypeID>AL</TypeID>
    </Communication>
  </Declarant>";
			CreateImportSeaJob(MessageTypeList.Codes.IPI);
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IPIMessageBuilder(declaration, TSWTransactionTypes.Original, null);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IPI Entry - Declarant element is NOT to be included", false, im1MessageString.Contains(elementNotExpected));
		}

		public void TestBillNumbersAreNotTruncated()
		{
			var expectedMBElement = "<TransportContractDocument>\r\n        <ID>OB293042-24902Y2992203928</ID>\r\n        <TypeCode>MB</TypeCode>";
			var expectedHBElement = "<TransportContractDocument>\r\n        <ID>123456789B123456789C123456789D12345</ID>\r\n        <TypeCode>BM</TypeCode>";
			CreateImportSeaJob();
			JobDeclaration.JE_MasterBill = "OB293042-24902Y2992203928";
			JobDeclaration.JE_HouseBill = "123456789B123456789C123456789D12345";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IPIMessageBuilder(declaration, TSWTransactionTypes.Original, null);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IPI Message - MasterBill number element should not be truncated", true, im1MessageString.Contains(expectedMBElement));
			AssertEquals("IPI Message - HouseBill number element should not be truncated", true, im1MessageString.Contains(expectedHBElement));
		}

		public void TestVoyageIsTruncated()
		{
			var expectedResult = @"<JourneyID>PO227WES</JourneyID>";
			CreateImportSeaJob();
			JobDeclaration.JE_VoyageFlightNo = "PO227WEST";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IPIMessageBuilder(declaration, TSWTransactionTypes.Original, null);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IPI Message - voyage number element should be truncated to 8 characters", true, im1MessageString.Contains(expectedResult));
		}

		public void TestFlightNoIsCapitalized()
		{
			var expectedResult = @"<BorderTransportMeans>
    <Name>QF007</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>";
			CreateImportAirJob();
			JobDeclaration.JE_VoyageFlightNo = "qf007";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IM1MessageBuilder(declaration, TSWTransactionTypes.Original);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IPI Message - Flight Number should be in Uppercase", true, im1MessageString.Contains(expectedResult));
		}

		public void TestCraftAndVoyageNumberIsCapitalized()
		{
			var expectedResult = @"<BorderTransportMeans>
    <Name>HYOGO PRINCESS</Name>
    <ID />
    <TypeCode>1</TypeCode>
    <JourneyID>182S</JourneyID>
  </BorderTransportMeans>";
			CreateImportSeaJob();
			JobDeclaration.JE_VoyageFlightNo = "182s";
			JobDeclaration.JE_VesselName = "Hyogo Princess";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IPIMessageBuilder(declaration, TSWTransactionTypes.Original, null);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("IPI Message - voyage number element should be Uppercase", true, im1MessageString.Contains(expectedResult));
		}

		public void TestIPIMessageWithContactDetails()
		{
			CreateImportSeaJob();
			var importer = JobDeclaration.Importer;
			var importerContact = importer.Contacts.AddNew();
			importerContact.OC_ContactName = "Bill Brown";
			importerContact.OC_Email = "bill.brown@importer.com.au";
			var allocatedContact = importerContact.Allocations.AddNew();
			allocatedContact.PC_Type = "NZC";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SMITH AND SONS";
			supplier.MainAddress.OA_Email = "info@smithandson.org.au";
			var supplierContact = supplier.Contacts.AddNew();
			supplierContact.OC_ContactName = "Wendy Smith";
			var supplierAllocatedContact = supplierContact.Allocations.AddNew();
			supplierAllocatedContact.PC_Type = "NZC";
			JobDeclaration.JE_OH_Supplier = supplier.PK;
			var invoice = JobDeclaration.Invoices[0];
			invoice.JZ_OH_Supplier = supplier.PK;
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IPIMessageBuilder(declaration, TSWTransactionTypes.Original, null);
			var im1MessageString = im1Builder.GetXMLMessage();
			var importerDetailsExpected = @"<Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
    <Contact>
      <Name>Bill Brown</Name>
      <Communication>
        <ID>bill.brown@importer.com.au</ID>
        <TypeID>EM</TypeID>
      </Communication>
    </Contact>
  </Importer>";
			AssertEquals("Importer contact", true, im1MessageString.Contains(importerDetailsExpected));
			var supplierDetailsExpected = @"<Supplier>
      <Name>SMITH AND SONS</Name>
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
      <Contact>
        <Name>Wendy Smith</Name>
        <Communication>
          <ID>info@smithandson.org.au</ID>
          <TypeID>EM</TypeID>
        </Communication>
      </Contact>
    </Supplier>";
			AssertEquals("Supplier contact", true, im1MessageString.Contains(supplierDetailsExpected));
		}

		public void TestIPISupplierWithNoContactDetails()
		{
			CreateImportSeaJob();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SMITH AND SONS";
			supplier.MainAddress.OA_Email = "info@smithandson.org.au";
			JobDeclaration.JE_OH_Supplier = supplier.PK;
			var invoice = JobDeclaration.Invoices[0];
			invoice.JZ_OH_Supplier = supplier.PK;
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IPIMessageBuilder(declaration, TSWTransactionTypes.Original, null);
			var im1MessageString = im1Builder.GetXMLMessage();
			var supplierDetailsExpected = @"<Supplier>
      <Name>SMITH AND SONS</Name>
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
    </Supplier>";
			AssertEquals("Supplier details when no Contact information is available", true, im1MessageString.Contains(supplierDetailsExpected));
		}

		public void TestContactDetailsIncludesFAXIfPresent()
		{
			CreateImportSeaJob();
			var importer = JobDeclaration.Importer;
			var importerContact = importer.Contacts.AddNew();
			importerContact.OC_ContactName = "Bill Brown";
			importerContact.OC_Email = "bill.brown@importer.com.au";
			importerContact.OC_Fax = "+61 2 7082 4901";
			var allocatedContact = importerContact.Allocations.AddNew();
			allocatedContact.PC_Type = "NZC";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SMITH AND SONS";
			supplier.MainAddress.OA_Email = "info@smithandson.org.au";
			supplier.MainAddress.OA_Fax = "+61 2 8760 9278";
			var supplierContact = supplier.Contacts.AddNew();
			supplierContact.OC_ContactName = "Wendy Smith";
			var supplierAllocatedContact = supplierContact.Allocations.AddNew();
			supplierAllocatedContact.PC_Type = "NZC";
			JobDeclaration.JE_OH_Supplier = supplier.PK;
			var invoice = JobDeclaration.Invoices[0];
			invoice.JZ_OH_Supplier = supplier.PK;
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IPIMessageBuilder(declaration, TSWTransactionTypes.Original, null);
			var im1MessageString = im1Builder.GetXMLMessage();
			var importerDetailsExpected = @"<Importer>
    <Name />
    <Address>
      <CityName />
      <CountryCode />
      <CountrySubDivisionName />
      <Line>1</Line>
      <PostcodeID />
    </Address>
    <Contact>
      <Name>Bill Brown</Name>
      <Communication>
        <ID>bill.brown@importer.com.au</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>61270824901</ID>
        <TypeID>FX</TypeID>
      </Communication>
    </Contact>
  </Importer>";
			AssertEquals("Importer contact includes Fax number", true, im1MessageString.Contains(importerDetailsExpected));
			var supplierDetailsExpected = @"<Supplier>
      <Name>SMITH AND SONS</Name>
      <Address>
        <CityName />
        <CountryCode />
        <CountrySubDivisionName />
        <Line>1</Line>
        <PostcodeID />
      </Address>
      <Contact>
        <Name>Wendy Smith</Name>
        <Communication>
          <ID>info@smithandson.org.au</ID>
          <TypeID>EM</TypeID>
        </Communication>
        <Communication>
          <ID>61287609278</ID>
          <TypeID>FX</TypeID>
        </Communication>
      </Contact>
    </Supplier>";
			AssertEquals("Supplier contact includes FAX number", true, im1MessageString.Contains(supplierDetailsExpected));
		}

		public void TestGoodsLocation()
		{
			var expectedElement = @"<GoodsLocation>
        <ID>NZWSZ</ID>
      </GoodsLocation>";
			CreateImportSeaJob();
			JobDeclaration.JE_RL_NKPortOfArrival = "NZWSZ";
			var entryHeader = JobDeclaration.CustomsEntryHeaders[0];
			var declaration = new MockCusEntryHeaderWrapper(entryHeader);
			var im1Builder = new IPIMessageBuilder(declaration, TSWTransactionTypes.Original, null);
			var im1MessageString = im1Builder.GetXMLMessage();
			AssertEquals("This IPI Message should contain the Port of Arrival as the GoodsLocation", true, im1MessageString.Contains(expectedElement));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			var broker = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			broker.GS_FullName = "JOHNATHON TESTER";
			broker.GS_MobilePhone = "0419687522";
			broker.GS_EmailAddress = "johnathon.tester@testcompany.com.nz";
			var wrapper = broker.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "40006206E";
			Factory.Save();
		}

		protected void Create2HB1ContainerJob()
		{
			JobDeclaration.JE_MessageType = "IMP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BA00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I10;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_MasterBill = "OB93428378";
			JobDeclaration.JE_HouseBill = "H458239-1";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 1;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
			var houseBill2 = JobDeclaration.Bills.AddNew();
			houseBill2.CU_BillType = "HB";
			houseBill2.CU_BillNum = "B942042-2";
			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "YKKU9388747";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";
			packLine1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			var packLine2 = JobDeclaration.Packages[1];
			packLine2.CW_PackQty = 8;
			packLine2.CW_PackType = "BX";
			packLine2.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected void CreateExampleAJob()
		{
			JobDeclaration.JE_MessageType = "IMP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BA00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I10;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_MasterBill = "123456";
			JobDeclaration.JE_HouseBill = "COS12345678";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_VesselName = "Hyogo Maru";
			JobDeclaration.JE_TotalNoOfPacks = 1;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CAXU2968920";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";
			packLine1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected void CreateBondedWarehouseJob()
		{
			var nz = Factory.Load<RefCountry>(Constants.CountryGuids.NewZealand);
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "QANTAS AIRFREIGHT";
			var controlledWarehouse = Factory.NewWithValidTestData<OrgHeader>();
			controlledWarehouse.OH_FullName = "Auckland Bond";
			controlledWarehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "7198J", nz);
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I10;
			JobDeclaration.JE_TransportMode = "AIR";
			JobDeclaration.JE_TotalWeight = 150.752m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "QF108";
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2016, 11, 9);
			JobDeclaration.JE_DeclarationReference = "BIS00002309";
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 7, 19);
			JobDeclaration.JE_GoodsDescription = "NEWS PRINT";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "0810049584";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = carrier.PK;
			JobDeclaration.WarehouseDocAddress.OrganisationPK = controlledWarehouse.PK;
			JobDeclaration.WarehouseDocAddress.E2_OA_Address = controlledWarehouse.MainAddress.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PCS";
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2016, 10, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		#endregion
		protected ForwardingConsol JobConsol
		{
			get
			{
				if (jobConsol == null)
				{
					jobConsol = Factory.NewWithValidTestData<ForwardingConsol>();
				}

				return jobConsol;
			}
		}

		ForwardingConsol jobConsol;
	}
}
