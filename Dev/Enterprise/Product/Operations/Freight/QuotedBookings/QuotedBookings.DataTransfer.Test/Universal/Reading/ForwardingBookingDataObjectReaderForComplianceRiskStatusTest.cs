using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	public class ForwardingBookingDataObjectReaderForComplianceRiskStatusTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestComplianceRiskRegistryFalseWhenAddedQuickBookingAndComplianceRiskStatusHasNotCreated()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(QuotedBookingMessage);
				var (shipment, complianceRiskStatus) = GetDataObject("S00001000");

				CombineAssertions(() =>
				{
					AssertNull("Compliance risk status should not be created.", complianceRiskStatus);
					AssertMultilineASCIIEquals("Import successful log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).".Trim(), serviceTaskLog.ToString());
				});
			}
		}

		public void TestComplianceRiskRegistryTrueWhenAddedQuickBookingAndComplianceRiskStatusHasCreated()
		{
			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(QuotedBookingMessage);
			var (shipment, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions(() =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertMultilineASCIIEquals("Import successful log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).".Trim(), serviceTaskLog.ToString());
			});
		}

		public void TestComplianceRiskRegistryTrueWhenAddedQuickBookingAndCompliancePartyRiskStatusHasPotentialRisk()
		{
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABABEU"));
			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AASDRA"));
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			Factory.SaveForTesting();

			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(QuotedBookingMessage);
			var (shipment, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions(() =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertEquals(shipment.PK, complianceRiskStatus.COR_ParentID);
				AssertEquals(shipment.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
				AssertEquals("Job compliance status", "HLD", complianceRiskStatus.COR_OverallRisk);
				AssertEquals("Parties risk status", "HSK", complianceRiskStatus.COR_PartyRisk);
				AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
				AssertEquals("Commodities risk status", "NAP", complianceRiskStatus.COR_CommodityRisk);
				AssertMultilineASCIIEquals("Import successful log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).".Trim(), serviceTaskLog.ToString());
			});
		}

		public void TestComplianceRiskRegistryTrueWhenAddedQuickBookingAndCompliancePartyRiskStatusHasClear()
		{
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABABEU"));
			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AASDRA"));
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			Factory.SaveForTesting();

			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(QuotedBookingMessage);
			var (shipment, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions("Precondition:", () =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertEquals(shipment.PK, complianceRiskStatus.COR_ParentID);
				AssertEquals(shipment.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
				AssertEquals("Job compliance status", "HLD", complianceRiskStatus.COR_OverallRisk);
				AssertEquals("Parties risk status", "HSK", complianceRiskStatus.COR_PartyRisk);
				AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
				AssertEquals("Commodities risk status", "NAP", complianceRiskStatus.COR_CommodityRisk);
				AssertMultilineASCIIEquals("Import successful log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).".Trim(), serviceTaskLog.ToString());
			});

			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			Factory.SaveForTesting();

			(ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(QuotedBookingMessageWithKey);
			(shipment, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions(() =>
			{
				AssertEquals("Job compliance status", "HLD", complianceRiskStatus.COR_OverallRisk);
				AssertEquals("Parties risk status", "CLR", complianceRiskStatus.COR_PartyRisk);
				AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
				AssertEquals("Commodities risk status", "UNK", complianceRiskStatus.COR_CommodityRisk);
				AssertMultilineASCIIEquals("Import successful log", @"
Successfully loaded matching QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Matching 'SendersLocalClient':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Warning - Unknown Address Type [SendersLocalClient] found. Job Document Address not imported.
Updated Quick Booking - Booking (S00001000) from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).".Trim(), ediMessage.GetLogNoteText());
			});
		}

		public void TestComplianceRiskRegistryTrueWhenAddedQuickBookingAndComplianceCommodityRiskStatusHasNotApplicable()
		{
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABABEU"));
			consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AASDRA"));
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			Factory.SaveForTesting();

			var tariffView = LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "090121", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			CombineAssertions("Precondition :", () =>
			{
				AssertEquals("WCO", tariffView.ZZ1_ZZZ_NKDataGrouping);
				AssertEquals("HSN", tariffView.ZZ1_ZZI_NKTariffType);
				AssertEquals("090121", tariffView.ZZ1_TariffCode);
			});
			tariffView.Factory.Save();

			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(QuotedBookingMessageWithCommodity);
			var (shipment, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions(() =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertEquals(shipment.PK, complianceRiskStatus.COR_ParentID);
				AssertEquals(shipment.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
				AssertEquals("Job compliance status", "CLR", complianceRiskStatus.COR_OverallRisk);
				AssertEquals("Parties risk status", "CLR", complianceRiskStatus.COR_PartyRisk);
				AssertEquals("Locations risk status", "CLR", complianceRiskStatus.COR_LocationRisk);
				AssertEquals("Commodities risk status", "NAP", complianceRiskStatus.COR_CommodityRisk);
				AssertMultilineASCIIEquals("Import successful log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000) with 1 x ForwardingPackLine.".Trim(), serviceTaskLog.ToString());
				AssertMultilineASCIIEquals("Import successful log", @"
No matching QuotedBooking found, creating new QuotedBooking.
Populating QuotedBooking...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Matching 'ConsignorPickupDeliveryAddress':- Matched to 'ABABEU' by code, address 'Pick Up Address' by short code.
Matching 'ConsigneePickupDeliveryAddress':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Matching 'SendersLocalClient':- Matched to 'AASDRA' by code, address 'PST: UNIT A1, 4TH FLOOR,' by short code.
Warning - Unknown Address Type [SendersLocalClient] found. Job Document Address not imported.
No matching ForwardingPackLine found, creating new ForwardingPackLine.
Populating ForwardingPackLine...
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000) with 1 x ForwardingPackLine.".Trim(), ediMessage.GetLogNoteText());
			});
		}

		public void TestComplianceRiskRegistryTrueWhenAddedDomesticBooking()
		{
			var domesticImportFile = InternationalQuotedBookingMessageWithCommodity.Replace("<Code>VAVAT</Code>\r\n      <Name>Vatican City</Name>", "<Code>USTSB</Code>\r\n      <Name>Petersburg</Name>");

			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(domesticImportFile);
			var (shipment, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions(() =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertEquals("NAP", complianceRiskStatus.COR_CommodityRisk);
			});
		}

		public void TestComplianceRiskRegistryTrueWhenAddedInternationalBookingAndNoHSCodes()
		{
			var importFileWithoutHsCode = InternationalQuotedBookingMessageWithCommodity.Replace("<HarmonisedCode>090121</HarmonisedCode>", "");

			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(importFileWithoutHsCode);
			var (shipment, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions(() =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertEquals("UNK", complianceRiskStatus.COR_CommodityRisk);
			});
		}

		public void TestComplianceRiskRegistryTrueWhenAddedInternationalBookingAndWithHSCodes()
		{
			var tariffView = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(new BusinessObjectFactory(), "090121");
			tariffView.Factory.Save();

			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(InternationalQuotedBookingMessageWithCommodity);
			var (shipment, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions(() =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertEquals("UNK", complianceRiskStatus.COR_CommodityRisk);
			});
		}

		public void TestComplianceRiskRegistryTrueWhenAddedInternationalBookingAndWithHSCodesWithConditions()
		{
			var tariffView = ComplianceRiskTariffTestDataHelper.CreateTariffWithConditions(new BusinessObjectFactory(), "090121", "Test Conditions");
			tariffView.Factory.Save();

			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(InternationalQuotedBookingMessageWithCommodity);
			var (shipment, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions(() =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertEquals("UNK", complianceRiskStatus.COR_CommodityRisk);
			});
		}

		public void TestComplianceRiskRegistryTrueWhenAddedQuickBookingCORParentTableCodeIsJS()
		{
			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(InternationalQuotedBookingMessageWithCommodity);
			var (shipment, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions(() =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertContains(@"Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking", ediMessage.GetLogNoteText());
				AssertEquals("JS", complianceRiskStatus.COR_ParentTableCode);
			});
		}

		public void TestComplianceRiskRegistryTrueWhenAddedQuoteBookingCORParentTableCodeIsTH()
		{
			var importFileWithQuoteNumber = InternationalQuotedBookingMessageWithCommodity.Replace(
				"</PortOfOrigin>\r\n", "</PortOfOrigin>\r\n    " +
				"<QuoteNumber></QuoteNumber>\r\n" +
				"<ServiceLevel>\r\n      " +
				"<Code>EXP</Code>\r\n    " +
				"<Description>Express</Description>\r\n    " +
				"</ServiceLevel>");

			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(importFileWithQuoteNumber);
			var complianceRiskStatus = GetRatingHeaderDataObject("00001000");

			CombineAssertions(() =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertContains(@"Added Booking with Quote from UniversalShipment.
Successfully saved Booking with Quote", ediMessage.GetLogNoteText());
				AssertEquals("TH", complianceRiskStatus.COR_ParentTableCode);
			});
		}

		public void TestJobMaterialChanges_ShouldResetAllCommodityRiskStatus()
		{
			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(QuotedBookingMessage);
			var (bookingBO, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions("Pre-Condition:", () =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertMultilineASCIIEquals("Import successful log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000).".Trim(), serviceTaskLog.ToString());
			});

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";
			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "23456";
			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var eventLog = Factory.NewWithValidTestData<StmComplianceEvent>();
			eventLog.SCE_EventType = "CRI";
			eventLog.SCE_EventSubType = "CAI";
			eventLog.SCE_ParentID = bookingBO.PK;

			complianceRiskStatus.Factory.Save();

			(ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(QuotedBookingMessageWithKey);
			(bookingBO, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions(() =>
			{
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertEquals("Commodities risk status NCH", 2, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
			});
		}

		public void TestCommodityMaterialChangesPackLine_ShouldResetCommoditiyRiskStatus()
		{
			var importFile = QuotedBookingPackLineWithKey.Replace("<Key>S00001000</Key>", "<Key></Key>");
			var (ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(importFile);
			var (bookingBO, complianceRiskStatus) = GetDataObject("S00001000");

			CombineAssertions("Pre-Condition:", () =>
			{
				AssertEquals("Pack line JL_HarmonisedCode - 090121", "090121", bookingBO.OuterPackLines[0].JL_HarmonisedCode);
				AssertEquals("Pack line JL_RN_NKOrigin - US", "US", bookingBO.OuterPackLines[0].JL_RN_NKOrigin);
				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertMultilineASCIIEquals("Import successful log", @"
Added Quick Booking from UniversalShipment.
Successfully saved Quick Booking - Booking (S00001000) with 1 x ForwardingPackLine.".Trim(), serviceTaskLog.ToString());
			});

			bookingBO.OuterPackLines[0].JL_RN_NKOrigin = "AU";

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "123456";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var existingCommodity = complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Single(u => u.CCD_HarmonizedCode == "090121");
			existingCommodity.CCD_RN_NKOrigin = "AU";
			existingCommodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.Factory.Save();

			(ediMessage, serviceTaskLog) = ProcessUniversalShipmentMessage(QuotedBookingPackLineWithKey);
			(bookingBO, complianceRiskStatus) = GetDataObject("S00001000");

			AssertContainsExactElementsInAnyOrder("Commodities risk status of 090121 reset to NCH", new[]
			{
					("090121", ComplianceRiskStatusCodeList.Codes.NotChecked),
					("123456", ComplianceRiskStatusCodeList.Codes.Blocked),
				}, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Select(u => (u.CCD_HarmonizedCode.ToString(), u.CCD_RiskStatus.ToString())));
		}

		#region Implementation

		(Messaging.Integration.IEDIMessage EDIMessage, ServiceTaskLogForTesting ServiceTaskLog) ProcessUniversalShipmentMessage(string xmlMessage)
		{
			var ediMessage = GetQueuedUniversalShipmentMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(ediMessage);

			return (ediMessage, serviceTaskLog);
		}

		(ForwardingShipment Shipment, ComplianceRiskStatus ComplianceRiskStatus) GetDataObject(string shipmentKey)
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentKey));
			var complianceRiskStatus = factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK));

			return (shipment, complianceRiskStatus);
		}

		ComplianceRiskStatus GetRatingHeaderDataObject(string quoteNumber)
		{
			var factory = new BusinessObjectFactory();
			var ratingHeader = factory.LoadTop1<RatingHeader>(new ZQuery(RatingHeaderSchema.TH_QuoteNumber, quoteNumber));
			var complianceRiskStatus = factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, ratingHeader.PK));

			return complianceRiskStatus;
		}

		TariffView LoadOrCreateNewTariff(string dataGrouping, string tariffCode, string nkTariffType)
		{
			var helper = new UniversalReferenceTestDataHelper(new BusinessObjectFactory());
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia, "Australia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, nkTariffType);
			var tariffView = helper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			return tariffView;
		}

		const string InternationalQuotedBookingMessageWithCommodity = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingBooking</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>DEX</Code>
        <Description>Data Export</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2023-05-03T23:22:28.48</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>
    </DataContext>

	<PortOfDestination>
      <Code>VAVAT</Code>
      <Name>Vatican City</Name>
    </PortOfDestination>
    <PortOfDischarge>
      <Code>VAVAT</Code>
      <Name>Vatican City</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfLoading>
    <PortOfOrigin>
      <Code>USSWQ</Code>
      <Name>Swanton</Name>
    </PortOfOrigin>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <Address1>DIESLSTR 11</Address1>
        <Address2>57439 ATTENDORN, GERMANY</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
        <City></City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>DEFRA</Code>
          <Name>Frankfurt am Main</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <Address1>DIESLSTR 11</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>Pick Up Address</AddressShortCode>
        <City>ATTENDORN?, GERMANY</City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode>57439</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
        <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
        <City></City>
        <CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>AASDRA</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
        <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
        <City></City>
        <CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>AASDRA</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendersLocalClient</AddressType>
        <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
        <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
        <City></City>
        <CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>AASDRA</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection Content=""Complete"">
      <PackingLine>
        <Commodity>
          <Code>GEN</Code>
          <Description>General</Description>
        </Commodity>
        <ContainerPackingOrder>0</ContainerPackingOrder>
        <CountryOfOrigin>
          <Code></Code>
        </CountryOfOrigin>
        <DetailedDescription></DetailedDescription>
        <EndItemNo>0</EndItemNo>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription></GoodsDescription>
        <HarmonisedCode>090121</HarmonisedCode>
        <Height>0</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <ItemNo>0</ItemNo>
        <LastKnownCFSStatus>
          <Code></Code>
        </LastKnownCFSStatus>
        <LastKnownCFSStatusDate></LastKnownCFSStatusDate>
        <Length>0</Length>
        <LengthUnit>
          <Code>M</Code>
          <Description>Meters</Description>
        </LengthUnit>
        <LinePrice>0</LinePrice>
        <LoadingMeters>0</LoadingMeters>
        <MarksAndNos></MarksAndNos>
        <OutturnComment></OutturnComment>
        <OutturnDamagedQty>0</OutturnDamagedQty>
        <OutturnedHeight>0</OutturnedHeight>
        <OutturnedLength>0</OutturnedLength>
        <OutturnedVolume>0</OutturnedVolume>
        <OutturnedWeight>0</OutturnedWeight>
        <OutturnedWidth>0</OutturnedWidth>
        <OutturnPillagedQty>0</OutturnPillagedQty>
        <OutturnQty>0</OutturnQty>
        <PackingLineID>EDIDAT00000009</PackingLineID>
        <PackQty>0</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <ReferenceNumber></ReferenceNumber>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>13.000</Volume>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </VolumeUnit>
        <Weight>12340.000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
        <Width>0</Width>
        <PackedItemCollection>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

		const string QuotedBookingMessageWithCommodity = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingBooking</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>DEX</Code>
        <Description>Data Export</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2023-05-03T23:22:28.48</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>
    </DataContext>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <Address1>DIESLSTR 11</Address1>
        <Address2>57439 ATTENDORN, GERMANY</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
        <City></City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>DEFRA</Code>
          <Name>Frankfurt am Main</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <Address1>DIESLSTR 11</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>Pick Up Address</AddressShortCode>
        <City>ATTENDORN?, GERMANY</City>
        <CompanyName>ABA BEUL</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>ABABEU</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode>57439</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
        <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
        <City></City>
        <CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>AASDRA</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
        <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
        <City></City>
        <CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>AASDRA</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendersLocalClient</AddressType>
        <Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
        <Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
        <City></City>
        <CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
        <Country>
          <Code>HK</Code>
          <Name>Hong Kong</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>AASDRA</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>HKHKG</Code>
          <Name>Hong Kong</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection Content=""Complete"">
      <PackingLine>
        <Commodity>
          <Code>GEN</Code>
          <Description>General</Description>
        </Commodity>
        <ContainerPackingOrder>0</ContainerPackingOrder>
        <CountryOfOrigin>
          <Code></Code>
        </CountryOfOrigin>
        <DetailedDescription></DetailedDescription>
        <EndItemNo>0</EndItemNo>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription></GoodsDescription>
        <HarmonisedCode>090121</HarmonisedCode>
        <Height>0</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <ItemNo>0</ItemNo>
        <LastKnownCFSStatus>
          <Code></Code>
        </LastKnownCFSStatus>
        <LastKnownCFSStatusDate></LastKnownCFSStatusDate>
        <Length>0</Length>
        <LengthUnit>
          <Code>M</Code>
          <Description>Meters</Description>
        </LengthUnit>
        <LinePrice>0</LinePrice>
        <LoadingMeters>0</LoadingMeters>
        <MarksAndNos></MarksAndNos>
        <OutturnComment></OutturnComment>
        <OutturnDamagedQty>0</OutturnDamagedQty>
        <OutturnedHeight>0</OutturnedHeight>
        <OutturnedLength>0</OutturnedLength>
        <OutturnedVolume>0</OutturnedVolume>
        <OutturnedWeight>0</OutturnedWeight>
        <OutturnedWidth>0</OutturnedWidth>
        <OutturnPillagedQty>0</OutturnPillagedQty>
        <OutturnQty>0</OutturnQty>
        <PackingLineID>EDIDAT00000009</PackingLineID>
        <PackQty>0</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <ReferenceNumber></ReferenceNumber>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>13.000</Volume>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </VolumeUnit>
        <Weight>12340.000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
        <Width>0</Width>
        <PackedItemCollection>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

		const string QuotedBookingMessageWithKey = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingBooking</Type>
		  <Key>S00001000</Key>
		</DataTarget>
	  </DataTargetCollection>

	  <Company>
		<Code>EDI</Code>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Name>Eagle Datamation International</Name>
	  </Company>
	  <DataProvider>EDIDATEDI</DataProvider>
	  <EnterpriseID>EDI</EnterpriseID>
	  <EventBranch>
		<Code>BNE</Code>
		<Name>BN - AUBNE</Name>
	  </EventBranch>
	  <EventDepartment>
		<Code>BRN</Code>
		<Name>Branch</Name>
	  </EventDepartment>
	  <EventType>
		<Code>DEX</Code>
		<Description>Data Export</Description>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <ServerID>DAT</ServerID>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDate>2023-05-03T23:22:28.48</TriggerDate>
	  <TriggerDescription></TriggerDescription>
	  <TriggerType>Manual</TriggerType>
	</DataContext>

    <PortOfDestination>
      <Code>HKHKG</Code>
      <Name>Hong Kong</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>DEFRA</Code>
      <Name>Frankfurt am Main</Name>
    </PortOfOrigin>

	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<City></City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<City>ATTENDORN?, GERMANY</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code></Code>
		</Port>
		<Postcode>57439</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneePickupDeliveryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>SendersLocalClient</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";

		const string QuotedBookingMessage = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingBooking</Type>
		</DataTarget>
	  </DataTargetCollection>

	  <Company>
		<Code>EDI</Code>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Name>Eagle Datamation International</Name>
	  </Company>
	  <DataProvider>EDIDATEDI</DataProvider>
	  <EnterpriseID>EDI</EnterpriseID>
	  <EventBranch>
		<Code>BNE</Code>
		<Name>BN - AUBNE</Name>
	  </EventBranch>
	  <EventDepartment>
		<Code>BRN</Code>
		<Name>Branch</Name>
	  </EventDepartment>
	  <EventType>
		<Code>DEX</Code>
		<Description>Data Export</Description>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <ServerID>DAT</ServerID>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDate>2023-05-03T23:22:28.48</TriggerDate>
	  <TriggerDescription></TriggerDescription>
	  <TriggerType>Manual</TriggerType>
	</DataContext>

	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<City></City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<City>ATTENDORN?, GERMANY</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code></Code>
		</Port>
		<Postcode>57439</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneePickupDeliveryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>SendersLocalClient</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>A&amp;S FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";

		const string QuotedBookingPackLineWithKey = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingBooking</Type>
		  <Key>S00001000</Key>
		</DataTarget>
	  </DataTargetCollection>

	  <Company>
		<Code>EDI</Code>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Name>Eagle Datamation International</Name>
	  </Company>
	  <DataProvider>EDIDATEDI</DataProvider>
	  <EnterpriseID>EDI</EnterpriseID>
	  <EventBranch>
		<Code>BNE</Code>
		<Name>BN - AUBNE</Name>
	  </EventBranch>
	  <EventDepartment>
		<Code>BRN</Code>
		<Name>Branch</Name>
	  </EventDepartment>
	  <EventType>
		<Code>DEX</Code>
		<Description>Data Export</Description>
	  </EventType>
	  <EventUser>
		<Code>E</Code>
		<Name>CargoWise Support</Name>
	  </EventUser>
	  <ServerID>DAT</ServerID>
	  <TriggerCount>1</TriggerCount>
	  <TriggerDate>2023-05-03T23:22:28.48</TriggerDate>
	  <TriggerDescription></TriggerDescription>
	  <TriggerType>Manual</TriggerType>
	</DataContext>

    <PortOfDestination>
      <Code>HKHKG</Code>
      <Name>Hong Kong</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>DEFRA</Code>
      <Name>Frankfurt am Main</Name>
    </PortOfOrigin>

	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<City></City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorPickupDeliveryAddress</AddressType>
		<Address1>DIESLSTR 11</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<City>ATTENDORN?, GERMANY</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code></Code>
		</Port>
		<Postcode>57439</Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsigneePickupDeliveryAddress</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>SendersLocalClient</AddressType>
		<Address1>UNIT A1, 4TH FLOOR, PIONEER IND BLDG</Address1>
		<Address2>213 WAI YIP STREET, KWUN TONG, KOWLOON  HONG KONG</Address2>
		<AddressOverride>false</AddressOverride>
		<AddressShortCode>PST: UNIT A1, 4TH FLOOR,</AddressShortCode>
		<City></City>
		<CompanyName>FURNISHING CO LTD</CompanyName>
		<Country>
		  <Code>HK</Code>
		  <Name>Hong Kong</Name>
		</Country>
		<Email></Email>
		<Fax></Fax>
		<OrganizationCode>AASDRA</OrganizationCode>
		<Phone></Phone>
		<Port>
		  <Code>HKHKG</Code>
		  <Name>Hong Kong</Name>
		</Port>
		<Postcode></Postcode>
		<ScreeningStatus>
		  <Code>UNK</Code>
		  <Description>Unknown</Description>
		</ScreeningStatus>
		<State></State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
    <PackingLineCollection Content=""Complete"">
      <PackingLine>
        <Commodity>
          <Code>GEN</Code>
          <Description>General</Description>
        </Commodity>
        <ContainerPackingOrder>0</ContainerPackingOrder>
        <CountryOfOrigin>
          <Code>US</Code>
        </CountryOfOrigin>
        <DetailedDescription></DetailedDescription>
        <EndItemNo>0</EndItemNo>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription></GoodsDescription>
        <HarmonisedCode>090121</HarmonisedCode>
        <Height>0</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <ItemNo>0</ItemNo>
        <LastKnownCFSStatus>
          <Code></Code>
        </LastKnownCFSStatus>
        <LastKnownCFSStatusDate></LastKnownCFSStatusDate>
        <Length>0</Length>
        <LengthUnit>
          <Code>M</Code>
          <Description>Meters</Description>
        </LengthUnit>
        <LinePrice>0</LinePrice>
        <LoadingMeters>0</LoadingMeters>
        <MarksAndNos></MarksAndNos>
        <OutturnComment></OutturnComment>
        <OutturnDamagedQty>0</OutturnDamagedQty>
        <OutturnedHeight>0</OutturnedHeight>
        <OutturnedLength>0</OutturnedLength>
        <OutturnedVolume>0</OutturnedVolume>
        <OutturnedWeight>0</OutturnedWeight>
        <OutturnedWidth>0</OutturnedWidth>
        <OutturnPillagedQty>0</OutturnPillagedQty>
        <OutturnQty>0</OutturnQty>
        <PackingLineID>EDIDAT00000009</PackingLineID>
        <PackQty>0</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <ReferenceNumber></ReferenceNumber>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>13.000</Volume>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </VolumeUnit>
        <Weight>12340.000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
        <Width>0</Width>
        <PackedItemCollection>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

		#endregion

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;
		IDisposable setAllowComplianceCommodityRiskAssessmentToTrue;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			setAllowComplianceCommodityRiskAssessmentToTrue = OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
			setAllowComplianceCommodityRiskAssessmentToTrue.Dispose();
		}
	}
}
